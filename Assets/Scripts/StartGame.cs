﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

public class StartGame : MonoBehaviour 
{
	enum State { Initialized, Menu, ClassicMode, SurvivalMode, Gameover, Starting };
    State _currentState = State.Starting;
    State _prevState = State.Starting;
    enum Difficulty { Easy = 12, Normal = 10, Pro = 7 }; // Divide value by 10 to get initial duration
    Difficulty _difficulty = Difficulty.Normal;
	Action _stateEnder;

	List<GameColor> _mainColors = new List<GameColor>(); // Stores the colors of the buttons: green, red, blue, yellow
	List<GameColor> _extraColors = new List<GameColor>(); // Stores the extra colours that 
	GameColor _currentColor = new GameColor();
	GameColor _prevColor = new GameColor();
	
	GameObject _menuLayout;
	GameObject _gameplayLayout;
	GameObject _gameoverLayout;
	GameObject _prevTappedColor;

    TickingTimer _timer;
	float _duration = 1;
	int _score = 0;
	bool _tapped; // Flag for when the player taps the current color
	bool _firstColor; // Flag to check if the current color is the first color
    int _remainingColors = 0; //Used in classic mode
    float _classicTimeLeft = 0;
	
	void Start ()
	{
        toInitialized();
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_currentState == State.Menu)
            {
                Application.Quit();
            }
            else if (_currentState == State.ClassicMode || _currentState == State.SurvivalMode)
            {
                toGameOver();
            }
            else if (_currentState == State.Gameover)
            {
                toMenu();
            }
        }
    }

	void enterState(State state, Action stateEnder)
	{
		if (_stateEnder != null)
		{
			_stateEnder();
		}

        _prevState = _currentState;
        _currentState = state;
		_stateEnder = stateEnder;
	}


    /* 
	 * STATE METHODS
	*/
    void toInitialized()
    {
        enterState(State.Initialized, null);
        PlayGamesPlatform.Activate();
        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                Debug.Log("You have successfully logged in.");
            }
            else
            {
                Debug.Log("Login has failed.");
            }
        });
        _menuLayout = transform.Find("Menu").gameObject;
        _gameplayLayout = transform.Find("Game").gameObject;
        _gameoverLayout = transform.Find("Gameover").gameObject;
        _gameoverLayout.SetActive(false);
        addMainColors();
        toMenu();
    }

    void toMenu()
    {
        enterState(State.Menu, endMenu);
        _menuLayout.SetActive(true);
        _gameplayLayout.GetComponent<CanvasGroup>().alpha = 0.8f;
        _menuLayout.transform.Find("HighScore").GetComponent<Text>().text = "High Score: " + PlayerPrefs.GetInt("highScore" + _difficulty.ToString());
        _menuLayout.transform.Find("TimeLeft").GetComponent<Text>().text = "Best Time: " + PlayerPrefs.GetFloat("timeLeft" + _difficulty.ToString()) + "s";
    }
    void endMenu()
    {
        _menuLayout.SetActive(false);
    }

    void toSurvivalMode()
    {
        enterState(State.SurvivalMode, endSurvivalMode);
        _score = 0;
        _firstColor = true;
        _currentColor = null;
        _prevColor = null;
        _prevTappedColor = null;
        _extraColors.Clear();

        _gameplayLayout.GetComponent<CanvasGroup>().alpha = 1;
        Text scoreText = transform.Find("Score").GetComponent<Text>();
        scoreText.gameObject.SetActive(true);
        scoreText.text = "Score: 0";
        scoreText.color = Color.black;
        _timer = new TickingTimer(_duration, 0, timerTick, this);
        chooseColor();
        setSurvivalModeDifficulty();
    }
    void endSurvivalMode()
    {
        _timer.destroy();
        
    }

    void toClassicMode()
    {
        enterState(State.ClassicMode, endClassicMode);
        _score = 0;
        _firstColor = true;
        _currentColor = null;
        _prevColor = null;
        _prevTappedColor = null;
        _extraColors.Clear();

        _gameplayLayout.GetComponent<CanvasGroup>().alpha = 1;
        _gameplayLayout.transform.Find("instructions").gameObject.SetActive(true);
        Text scoreText = transform.Find("Score").GetComponent<Text>();
        scoreText.text = "Score: 0";
        scoreText.color = Color.black;


        //Set Difficulty
        if (_difficulty == Difficulty.Easy)
        {
            _classicTimeLeft = 10f;
            _remainingColors = 10;
        }
        else if (_difficulty == Difficulty.Normal)
        {
            _classicTimeLeft = 15f;
            _remainingColors = 20;
        }
        else if (_difficulty == Difficulty.Pro)
        {
            _classicTimeLeft = 20f;
            _remainingColors = 40;
        }

        Text timeLeft = transform.Find("TimeLeft").GetComponent<Text>();
        timeLeft.gameObject.SetActive(true);
        timeLeft.text = _classicTimeLeft + "s";
        timeLeft.color = Color.black;
        timeLeft.fontSize = 25;
        Text remaining = transform.Find("Remaining").GetComponent<Text>();
        remaining.gameObject.SetActive(true);
        remaining.fontSize = 25;
        remaining.color = Color.black;
        remaining.text = _remainingColors + " Remaining";
        
        chooseColor(false, true);
        new TickingTimer(1.5f, 1, startClassicMode, this); // Wait 1 second for first color to change
    }
    void endClassicMode()
    {
        _timer.destroy();
        
    }

    void toGameOver()
    {
        enterState(State.Gameover, endGameover);
        _gameoverLayout.SetActive(true);
        _gameplayLayout.GetComponent<CanvasGroup>().alpha = 0.8f;
        transform.Find("Score").GetComponent<Text>().color = Color.white;
        Text text = _gameoverLayout.transform.Find("text").GetComponent<Text>();
        text.text = "GAME OVER";

        // Update high score
        if (_prevState== State.SurvivalMode)
        {
            int currentHighScore = PlayerPrefs.GetInt("highScore" + _difficulty.ToString());
            if (_score > currentHighScore)
            {
                PlayerPrefs.SetInt("highScore" + _difficulty.ToString(), _score);
                postToGoogleLeaderboard();
            }
        }
        else if (_prevState == State.ClassicMode && _remainingColors == 0)
        {
            text.text = "SUCCESS";
            Text timeLeft = transform.Find("TimeLeft").GetComponent<Text>();
            timeLeft.color = Color.white;
            timeLeft.fontSize = 40;
            float currentBestTimeLeft = PlayerPrefs.GetFloat("timeLeft" + _difficulty.ToString());

            if (_classicTimeLeft > currentBestTimeLeft)
            {
                text.text = "NEW BEST!";
                PlayerPrefs.SetFloat("timeLeft" + _difficulty.ToString(), Mathf.Round(_classicTimeLeft*100)/100);
                postToGoogleLeaderboard();
            }
        }
        else if (_prevState == State.ClassicMode && _remainingColors > 0)
        {
            Text remaining = transform.Find("Remaining").GetComponent<Text>();
            remaining.color = Color.white;
            remaining.fontSize = 40;
            text.text = "FAILURE";
        }

        // Reset checkmkarks
        foreach (GameColor color in _mainColors)
        {
            GameObject checkMark = _gameplayLayout.transform.Find("button" + color.name).Find("check").gameObject;
            checkMark.GetComponent<Image>().CrossFadeAlpha(1, 0, false);
			checkMark.SetActive(false);
        }
    }
    void endGameover()
    {
        transform.Find("TimeLeft").GetComponent<Text>().gameObject.SetActive(false);
        transform.Find("Remaining").GetComponent<Text>().gameObject.SetActive(false);
        transform.Find("Score").GetComponent<Text>().gameObject.SetActive(false);
        _gameoverLayout.SetActive(false);
    }


	/* 
	 * EVENTS
	*/
	void timerTick()
	{
		// Resets alpha of checkmarks
		if (_prevTappedColor != null)
		{
			_prevTappedColor.transform.Find("check").gameObject.GetComponent<Image>().CrossFadeAlpha(1, 0, false);
			_prevTappedColor.transform.Find("check").gameObject.SetActive(false);
		}

        if (_tapped || _prevColor == null || !(_prevColor.isMain))
		{
            if (_firstColor) { _firstColor = false; }
			chooseColor();
		}else
		{
			toGameOver();
		}
	}

    void classicTimerTick()
    {
        Text timeLeft = transform.Find("TimeLeft").GetComponent<Text>();
        _classicTimeLeft = _classicTimeLeft - 0.01f;
        timeLeft.text = _classicTimeLeft.ToString("0.00") + "s";
        
        if (_classicTimeLeft <= 0)
        {
            toGameOver();
        }
    }

    public void tapDifficultyButton(GameObject difficulty)
    {
        // Reset color of previously selected difficulty button
        Button oldDifficultyButton = _menuLayout.transform.Find(_difficulty.ToString() + "Button").GetComponent<Button>();
        var temp1 = oldDifficultyButton.colors;
        temp1.normalColor = new Color(254f / 255f, 171f / 255f, 67f / 255);
        oldDifficultyButton.colors = temp1;

        // Change color of selected button
        _difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), difficulty.tag);
        Button difficultyButton = difficulty.GetComponent<Button>();
        var temp2 = difficultyButton.colors;
        temp2.normalColor = new Color(195f / 255f, 94f / 255f, 0f);
        difficultyButton.colors = temp2;

        toMenu(); // Update high score depending on difficulty
    }

	public void tapColor(GameObject colorButton)
	{
        if (!_firstColor)
        {
            if (colorButton.tag.Equals(_prevColor.name)) // If tapped color is correct
            {
                if (!_tapped || _currentState == State.ClassicMode) // Ensures only first tap of color counts
                {
           
                    if (_currentState == State.SurvivalMode)
                    {
                        _score++;
                        transform.Find("Score").GetComponent<Text>().text = "Score: " + _score;
                        colorButton.transform.Find("check").gameObject.SetActive(true);
                        colorButton.transform.Find("check").gameObject.GetComponent<Image>().CrossFadeAlpha(0, 0.25f, false);
                        _prevTappedColor = colorButton;
                        setSurvivalModeDifficulty();
                        checkAchievement();
                    }
                    else if (_currentState == State.ClassicMode)
                    {
                        Text remaining = transform.Find("Remaining").GetComponent<Text>();
                        remaining.text = --_remainingColors + " Remaining";

                        // Reset checkmark and fade it away
                        if (_prevTappedColor != null)
                        {
                            _prevTappedColor.transform.Find("check").gameObject.GetComponent<Image>().CrossFadeAlpha(1, 0, false);
                            _prevTappedColor.transform.Find("check").gameObject.SetActive(false);
                        }
                        colorButton.transform.Find("check").gameObject.SetActive(true);
                        colorButton.transform.Find("check").gameObject.GetComponent<Image>().CrossFadeAlpha(0, 0.25f, false);
                        _prevTappedColor = colorButton;

                        if (_remainingColors <= 0)
                        {
                            toGameOver();
                        }
                        else
                        {
                            chooseColor(false, true);
                        }
                    }
                }
                _tapped = true;
            }
            else
            {
                toGameOver();
            }
        }
	}

    public void tapAchievements()
    {
        Social.ShowAchievementsUI();
    }

    public void tapLeaderboards()
    {
        Social.ShowLeaderboardUI();
    }

    public void tapClassicModeButton()
    {
        toClassicMode();
    }

    public void tapSurvivalModeButton()
    {
        toSurvivalMode();
    }

    public void tapMenu()
    {
        toMenu();
    }

    public void tapRestart()
    {
        if (_prevState == State.ClassicMode)
        {
            toClassicMode();
        }
        else if (_prevState == State.SurvivalMode)
        {
            toSurvivalMode();
        }        
    }


	/* 
	 * HELPERS
	*/
	void addMainColors()
	{
		_mainColors.Add(new GameColor(Color.green, "Green", 0, true));
		_mainColors.Add(new GameColor(Color.red, "Red", 0, true));
		_mainColors.Add(new GameColor(Color.blue, "Blue", 0, true));
		_mainColors.Add(new GameColor(Color.yellow, "Yellow", 0, true));
	}

	// Sets the duration for survival Mode of how long a color stays on the screen. Duration decreases as the player's score increases.
	void setSurvivalModeDifficulty()
	{
		switch(_score)
		{
		case 0: 
			_duration = ((float)_difficulty)/10f; // Normal = 1
            Debug.Log("Duration: " + (_duration));
			break;
		case 10:
			_duration -= 0.1f; // Normal = 0.9
            _extraColors.Add(new GameColor(Color.gray, "Gray", 0, false));
            Debug.Log("Duration: " + _duration);
			break;
		case 15:
            _duration -= 0.05f; // Normal = 0.85
            Debug.Log("Duration: " + _duration);
			break;
		case 20:
			_duration -= 0.05f; // Normal = 0.8
            Debug.Log("Duration: " + _duration);
            _extraColors.Add(new GameColor(Color.cyan, "Cyan", 0, false));
			break;
		case 30:
			_duration -= 0.1f; // Normal = 0.7
            Debug.Log("Duration: " + _duration);
			break;
		case 40:
			_duration -= 0.05f;
			break;
		case 50:
			_duration -= 0.1f;
            _extraColors.Add(new GameColor(Color.magenta, "Magenta", 0, false));
			break;
		case 100:
			_duration -= 0.1f;
			break;
		}

		_timer.setDuration(_duration);
	}

	// Chooses the next color to display
	void chooseColor(bool rechoose = false, bool classicMode = false)
	{
        _prevColor = _currentColor; // Store the previous color to check if player is correct	

        // % chance of choosing a main color
		System.Random rnd  = new System.Random();
		int die = rnd.Next(100); // TODO: change to proper probabiblity method

        if (classicMode)
        {
            die = 0;
        }
		
		if (die < 90 || _extraColors.Count == 0)
		{
			_currentColor = _mainColors [rnd.Next (0, _mainColors.Count)];
		}else 
		{
			_currentColor = _extraColors [rnd.Next (0, _extraColors.Count)];
		}

        if (!_firstColor && (_currentColor.value == _prevColor.value)) // Never choose the previous color
		{
			chooseColor(true); 
		}else
		{
			_tapped = false; // Reset flag for new color to be tapped
			transform.Find ("Game").Find("swapper").Find("background").gameObject.GetComponent<Image>().color = _currentColor.value;
		}
	}

    void startClassicMode()
    {
        _firstColor = false;
        _gameplayLayout.transform.Find("instructions").gameObject.SetActive(false);
        chooseColor(false, true);
        _timer = new TickingTimer(0.01f, 0, classicTimerTick, this);
    }

    void checkAchievement()
    {
        if (_difficulty.ToString() == "Easy")
        {
            if (_score == 50)
            {
                Social.ReportProgress("CgkIr_Hh_8oTEAIQAg", 100.0f, (bool success) =>
                {
                    if (success) { Debug.Log("Not Bad"); }
                });
            }
            else if (_score == 100)
            {
                Social.ReportProgress("CgkIr_Hh_8oTEAIQAw", 100.0f, (bool success) =>
                {
                    if (success) { Debug.Log("Pretty Good!"); }
                });
            }
        }
        else if (_difficulty.ToString()== "Normal")
        {
            if (_score == 100)
            {
                Social.ReportProgress("CgkIr_Hh_8oTEAIQBA", 100.0f, (bool success) =>
                {
                    if (success) { Debug.Log("Wow!!"); }
                });
            }
        }
        else if (_difficulty.ToString() == "Pro")
        {
            if (_score == 100)
            {
                Social.ReportProgress("CgkIr_Hh_8oTEAIQBQ", 100.0f, (bool success) =>
                {
                    if (success) { Debug.Log("No waaay!!!"); }
                });
            }
            else if (_score == 101) 
            {
                Social.ReportProgress("CgkIr_Hh_8oTEAIQBg", 100.0f, (bool success) =>
                {
                    if (success) { Debug.Log("Are you even human?..."); }
                });
            }
        }
    }

    void postToGoogleLeaderboard()
    {
        if (_currentState == State.SurvivalMode)
        {
            if (_difficulty.ToString() == "Easy")
            {
                Social.ReportScore(_score, "CgkIr_Hh_8oTEAIQBw", (bool success) =>
                {
                    if (success) { Debug.Log("Posted to Easy Survival Leaderboard!"); }
                });
            }
            else if (_difficulty.ToString() == "Normal")
            {
                Social.ReportScore(_score, "CgkIr_Hh_8oTEAIQCA", (bool success) =>
                {
                    if (success) { Debug.Log("Posted to Normal Survival Leaderboard!"); }
                });
            }
            else if (_difficulty.ToString() == "Pro")
            {
                Social.ReportScore(_score, "CgkIr_Hh_8oTEAIQCQ", (bool success) =>
                {
                    if (success) { Debug.Log("Posted to Pro Survival Leaderboard!"); }
                });
            }
        }
        else if (_currentState == State.ClassicMode)
        {
            if (_difficulty.ToString() == "Easy")
            {
                Social.ReportScore(_score, "CgkIr_Hh_8oTEAIQCg", (bool success) =>
                {
                    if (success) { Debug.Log("Posted to Easy Classic Leaderboard!"); }
                });
            }
            else if (_difficulty.ToString() == "Normal")
            {
                Social.ReportScore(_score, "CgkIr_Hh_8oTEAIQCw", (bool success) =>
                {
                    if (success) { Debug.Log("Posted to Normal Classic Leaderboard!"); }
                });
            }
            else if (_difficulty.ToString() == "Pro")
            {
                Social.ReportScore(_score, "CgkIr_Hh_8oTEAIQDA", (bool success) =>
                {
                    if (success) { Debug.Log("Posted to Pro Classic Leaderboard!"); }
                });
            }
        }
    }
}
