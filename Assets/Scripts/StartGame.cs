using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;

public class StartGame : MonoBehaviour 
{
	enum State { Initialized, Menu, Gameplay, Gameover, Disposed };
	State _currentState;
	Action _stateEnder;

	List<GameColor> _mainColors = new List<GameColor>(); // Stores the colors of the buttons: green, red, blue, yellow
	List<GameColor> _extraColors = new List<GameColor>(); // Stores the extra colours that 
	GameColor _currentColor = new GameColor(); 
	GameColor _prevColor = new GameColor();
	TickingTimer _timer;

	GameObject _menuLayout;
	GameObject _gameplayLayout;
	GameObject _gameoverLayout;
	GameObject _prevTappedColor; // The previously tapped color by the player

    int _difficultyOffset = 0;
	float _duration = 1;
	int _score = 0;
	bool _tapped; // Flag for when the player taps the current color
	bool _firstColor; // Flag to check if the current color is the first color
	int _initialDuration = 1; //sets difficulty
	
	void Start ()
	{
		toInitialized();
	}

	void enterState(State state, Action stateEnder)
	{
		if (_stateEnder != null)
		{
			_stateEnder();
		}

		_currentState = state;
		_stateEnder = stateEnder;
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

		if (_firstColor || _tapped || !(_prevColor.isMain))
		{
			chooseColor();
			if (_firstColor) { _firstColor = false; }
		}else
		{
			toGameOver();
		}
	}

	public void tapPlay()
	{
		toGameplay();
	}

	public void tapMenu()
	{
		toMenu();
	}

	public void tapRestart()
	{
		toGameplay();
	}

	public void tapColor(GameObject colorButton)
	{
		if (colorButton.tag.Equals(_prevColor.name)) // If tapped color is correct
		{
			if (!_tapped) // Ensures only first tap of color counts
			{
				_score++;
				transform.Find("Score").GetComponent<Text>().text = "Score: " + _score;
				colorButton.transform.Find("check").gameObject.SetActive(true);
				colorButton.transform.Find("check").gameObject.GetComponent<Image>().CrossFadeAlpha(0, 0.25f, false);
				_prevTappedColor = colorButton;
				increaseDifficulty();
			}
            _tapped = true;
		}else
		{
			toGameOver();
		}
	}

	/* 
	 * STATE METHODS
	*/
	void toInitialized()
	{
		enterState(State.Initialized, endInitialized);
		_menuLayout = transform.Find("Menu").gameObject;
		_gameplayLayout = transform.Find("Game").gameObject;
		_gameoverLayout = transform.Find("Gameover").gameObject;
		_gameoverLayout.SetActive(false);
		setColors();
		toMenu();
	}
	void endInitialized()
	{

	}

	void toMenu()
	{
		enterState(State.Menu, endMenu);
		_menuLayout.SetActive(true);
		_gameplayLayout.GetComponent<CanvasGroup>().alpha = 0.2f;

		transform.Find("Score").GetComponent<Text>().text = "High Score: " + PlayerPrefs.GetInt("highScore");
	}
	void endMenu()
	{
		_menuLayout.SetActive(false);
	}

	void toGameplay()
	{
		enterState(State.Gameplay, endGameplay);
		_score = 0;
		_duration = _initialDuration;
		_firstColor = true;

		_gameplayLayout.GetComponent<CanvasGroup>().alpha = 1;
		transform.Find("Score").GetComponent<Text>().text = "Score: 0";
		transform.Find("Score").GetComponent<Text>().color = Color.black;
		_timer = new TickingTimer(_duration, 0, timerTick, this);
		chooseColor();
	}
	void endGameplay()
	{
		_timer.destroy();
	}

	void toGameOver()
	{
		enterState(State.Gameover, endGameover);
		_gameoverLayout.SetActive(true);
		_gameplayLayout.GetComponent<CanvasGroup>().alpha = 0.2f;
		transform.Find("Score").GetComponent<Text>().color = Color.white;

		int currentHighScore = PlayerPrefs.GetInt("highScore");
		if (_score > currentHighScore)
		{
			PlayerPrefs.SetInt("highScore", _score);
		}
	}
	void endGameover()
	{
		_gameoverLayout.SetActive(false);
	}

	void toDisposed()
	{
	
	}
	void endDisposed()
	{

	}

	/* 
	 * HELPERS
	*/
    void setInitialDifficulty(int difficultyOffset)
    {
        _difficultyOffset = difficultyOffset;
    }

	void setColors()
	{
		_mainColors.Add(new GameColor(Color.green, "Green", 0, true));
		_mainColors.Add(new GameColor(Color.red, "Red", 0, true));
		_mainColors.Add(new GameColor(Color.blue, "Blue", 0, true));
		_mainColors.Add(new GameColor(Color.yellow, "Yellow", 0, true));
		_extraColors.Add(new GameColor(Color.gray, "Gray", 0, false));
	}

	/// <summary>
	/// Sets the duration of how long a color stays on the screen. Duration decreases as the player's score increases.
	/// </summary>
	void increaseDifficulty()
	{
		switch(_score + _difficultyOffset)
		{
		case 0:
			_duration = 1;
			break;
		case 5:
			_duration = 1f;
			break;
		case 10:
			_duration = 0.9f;
			break;
		case 15:
			_duration = 0.8f;
			break;
		case 20:
			_duration = 0.7f;
            _extraColors.Add(new GameColor(Color.cyan, "Cyan", 0, false));
			break;
		case 30:
			_duration = 0.6f;
			break;
		case 35:
			_duration = 0.5f;
			break;
		case 40:
			_duration = 0.45f;
			_extraColors.Add(new GameColor(Color.magenta, "Magenta", 0, false));
			break;
		case 45:
			_duration = 0.4f;
			break;
		case 50:
			_duration = 0.3f;
			break;
		case 80:
			_duration = 0.2f;
			break;
		}

		_timer.setDuration(_duration);
	}

	/// <summary>
	/// Chooses the next color. 90% chance of main color.
	/// </summary>
	void chooseColor(bool rechoose = false)
	{
        _prevColor = _currentColor; // Store the previous color to check if player is correct	

        // % chance of choosing a main color
		System.Random rnd  = new System.Random();
		int die = rnd.Next(100);						
		if (die < 90)
		{
			_currentColor = _mainColors [rnd.Next (1, _mainColors.Count)];
		}else 
		{
			_currentColor = _extraColors [rnd.Next (0, _extraColors.Count)];
		}

        if (!_firstColor && (_currentColor.value == _prevColor.value)) // Never choose the previous color
		{
			chooseColor(true); 
		}else
		{
			_tapped = false; //reset flag for new color to be tapped
			transform.Find ("Game").Find("swapper").Find("background").gameObject.GetComponent<Image>().color = _currentColor.value;
		}
	}
}
