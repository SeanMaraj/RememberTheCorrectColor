using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;

public class StartGame : MonoBehaviour 
{
	enum State { Initialized, Menu, Gameplay, Gameover, Disposed };
	enum Colors { Green = 1, Red = 2, Blue = 3, Yellow = 4, Gray = 5, Cyan = 6, Magenta = 7};
	State _currentState;
	Action _stateEnder;

	Dictionary<string, GameColor> _mainColors = new Dictionary<string, GameColor>();
	Dictionary<string, GameColor> _extraColors = new Dictionary<string, GameColor>();
	GameColor _currentColor = new GameColor(); 
	GameColor _prevColor = new GameColor();
	TickingTimer _timer;

	GameObject _menuLayout;
	GameObject _gameplayLayout;
	GameObject _gameoverLayout;
	GameObject _prevTappedColor; // The previously tapped color by the player

	float _duration = 1;
	int _score = 0;
	int _count = 0;
	bool _tapped; //flag for when the player taps the current color
	bool _firstColor;
	bool _primaryColor;
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
		// resets alpha of checkmarks
		if (_prevTappedColor != null)
		{
			_prevTappedColor.transform.Find("check").gameObject.GetComponent<Image>().CrossFadeAlpha(1, 0, false);
			_prevTappedColor.transform.Find("check").gameObject.SetActive(false);
		}

		if (_firstColor || _tapped || !(_prevColor.isMain))
		{
			setDuration();
			chooseColor();
			if (_firstColor)
			{
				_firstColor = false;
			}
		}else
		{
			toGameOver();
		}
		_count++;
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
		if (_mainColors[colorButton.tag].value.Equals(_prevColor.value)) // If tapped color is correct
		{
			if (!_tapped) // Ensures only first tap of color counts
			{
				_score++;
				transform.Find("Score").GetComponent<Text>().text = "Score: " + _score;
				colorButton.transform.Find("check").gameObject.SetActive(true);
				colorButton.transform.Find("check").gameObject.GetComponent<Image>().CrossFadeAlpha(0, 0.25f, false);
				_prevTappedColor = colorButton;
			}
		}else
		{
			toGameOver();
		}
		_tapped = true;
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
		_count = 0;
		_duration = _initialDuration;
		_firstColor = true;

		_gameplayLayout.GetComponent<CanvasGroup>().alpha = 1;
		transform.Find("Score").GetComponent<Text>().text = "Score: 0";
		transform.Find("Score").GetComponent<Text>().color = Color.black;
		_timer = new TickingTimer(_duration, 0, timerTick, this);
		chooseColor();
		setDuration();
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
	void setColors()
	{

		_mainColors.Add("Green",new GameColor(Color.green, "Green", 0, true));
		_mainColors.Add("Red",new GameColor(Color.red, "Red", 0, true));
		_mainColors.Add("Blue",new GameColor(Color.blue, "Blue", 0, true));
		_mainColors.Add("Yellow",new GameColor(Color.yellow, "Yellow", 0, true));

		_extraColors.Add("Gray", new GameColor(Color.gray, "Gray", 0, false));
	}

	/// <summary>
	/// Sets the duration of how long a color stays on the screen. Duration decreases as the player's score increases.
	/// </summary>
	void setDuration()
	{
		//Debug.Log (_count);
		switch(_score)
		{
		case 0:
			_duration = 1;
			break;
		case 5:
			_duration = 1f;
			break;
		case 10:
			_duration = 0.9f;
			Debug.Log ("ADDING CYAN"); //TODO remove
			if (!_extraColors.ContainsKey("Cyan")) { _extraColors.Add("Cyan",new GameColor(Color.cyan, "Cyan", 0, false)); }
			break;
		case 15:
			_duration = 0.8f;
			break;
		case 20:
			_duration = 0.7f;
			break;
		case 30:
			_duration = 0.6f;
			Debug.Log ("ADDING MAGENTA"); //TODO remove
			if (!_extraColors.ContainsKey("Magenta")) {_extraColors.Add("Magenta",new GameColor(Color.magenta, "Magenta", 0, false)); }
			break;
		case 35:
			_duration = 0.5f;
			break;
		case 40:
			_duration = 0.45f;
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
		System.Random rnd  = new System.Random();
		int die = rnd.Next(100);
 
		_prevColor = _currentColor; // Store the previous color to check if player is correct							

		if (die < 90) 
		{
			_currentColor = _mainColors [((Colors)rnd.Next (1, 5)).ToString ()];
		}else 
		{
			_currentColor = _extraColors [((Colors)rnd.Next (5, 5 + _extraColors.Count)).ToString ()];
		}

		// Never choose the previous color.
		if (!_firstColor && (_currentColor.value == _prevColor.value))
		{
			chooseColor(true); 
		}else
		{
			_tapped = false; //reset flag for new color to be tapped
			transform.Find ("Game").Find("swapper").Find("background").gameObject.GetComponent<Image>().color = _currentColor.value;
		}
	}

	void test()
	{

	}
}
