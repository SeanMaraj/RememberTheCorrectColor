using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;

public class StartGame : MonoBehaviour 
{
	enum State { Initialized = 1, Menu, Gameplay, Gameover, Disposed };
	State _state;
	Action _stateEnder;

	//JSONArray _colors = new SimpleJSON.JSONArray();
	Dictionary<string, GameColor> _colors = new Dictionary<string, GameColor>();
	Color _currentColor;
	Color _prevColor;
	TickingTimer _timer;



	GameObject _menuLayout;
	GameObject _gameplayLayout;
	GameObject _gameoverLayout;
	GameObject _prevTappedColor;

	float _duration = 1;
	int _score = 0;
	int _count = 0;
	bool _tapped;
	bool _firstColor;
	bool _primaryColor;
	bool _prevPrimaryColor;
	
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

		_state = state;
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

		if (_firstColor || _tapped || !_prevPrimaryColor)
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
		if (_colors[colorButton.tag].Equals(_prevColor))
		{
			if (!_tapped)
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
		_duration = 1;
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

		_colors.Add("Green",new GameColor(Color.green, 10));
		_colors.Add("Red",new GameColor(Color.red, 10));
		_colors.Add("Blue",new GameColor(Color.blue, 10));
		_colors.Add("Yellow",new GameColor(Color.yellow, 10));
		_colors.Add("Gray", new GameColor(Color.gray, 10));

		//_colors.Add("C
	}
	void setDuration()
	{
		Debug.Log (_count);
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
			break;
		case 15:
			_duration = 0.8f;
			break;
		case 20:
			_duration = 0.7f;
			_colors.Add("Magenta",new GameColor(Color.magenta, 10));
			break;
		case 30:
			_duration = 0.6f;
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
	
	void chooseColor()
	{
		System.Random rnd  = new System.Random();
		int rnum = rnd.Next(1,_colors.Count+1);
		_prevColor = _currentColor;
		_prevPrimaryColor = _primaryColor;

		switch(rnum)
		{
			case 1:
				_currentColor = _colors["Green"].color;
				_primaryColor = true;
				break;
			case 2:
				_currentColor = _colors["Red"].color;
				_primaryColor = true;
				break;
			case 3:
				_currentColor = _colors["Blue"].color;
				_primaryColor = true;
				break;
			case 4:
				_currentColor = _colors["Yellow"].color;
				_primaryColor = true;
				break;
			case 5:
				_currentColor = _colors["Gray"].color;
				_primaryColor = false;
				break;
			case 6:
				_currentColor = _colors["Gray"].color;
				_primaryColor = false;
				break;
		}

		if (_currentColor == _prevColor)
		{
			chooseColor(); 
		}else
		{
			//if (_duration > 0.2) { _duration -= 0.01f; }
			//_timer.setDuration(_duration);
			_tapped = false;

			transform.Find ("Game").Find("swapper").Find("background").gameObject.GetComponent<Image>().color = _currentColor;
		}
	}

	void test()
	{

	}
}
