using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class StartGame : MonoBehaviour 
{
	enum State { Initialized, Menu, Gameplay, Gameover, Disposed };
	State _state;
	Action _stateEnder;

	Dictionary<string, Color> _colors = new Dictionary<string, Color>();
	Color _currentColor;
	Color _prevColor;
	TickingTimer _timer;

	GameObject _menuLayout;
	GameObject _gameplayLayout;
	GameObject _gameoverLayout;
	GameObject _prevTappedColor;

	float _duration = 1;
	int _score = 0;
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
			chooseColor();
			if (_firstColor)
			{
				_firstColor = false;
			}
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
		_duration = 1;
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
	void setColors()
	{
		_colors.Add("Green",Color.green);
		_colors.Add("Red",Color.red);
		_colors.Add("Blue",Color.blue);
		_colors.Add("Yellow",Color.yellow);
		_colors.Add("Magenta",Color.magenta);
		_colors.Add("Gray",Color.cyan);
	}
	
	void chooseColor()
	{
		System.Random rnd  = new System.Random();
		int rnum = rnd.Next(1,_colors.Count);
		_prevColor = _currentColor;
		_prevPrimaryColor = _primaryColor;

		switch(rnum)
		{
			case 1:
				_currentColor = _colors["Green"];
				_primaryColor = true;
				break;
			case 2:
				_currentColor = _colors["Red"];
				_primaryColor = true;
				break;
			case 3:
				_currentColor = _colors["Blue"];
				_primaryColor = true;
				break;
			case 4:
				_currentColor = _colors["Yellow"];
				_primaryColor = true;
				break;
			case 5:
				_currentColor = _colors["Magenta"];
				_primaryColor = false;
				break;
			case 6:
				_currentColor = _colors["Gray"];
				_primaryColor = false;
				break;
		}

		if (_currentColor == _prevColor)
		{
			chooseColor(); 
		}else
		{
			if (_duration > 0.2) { _duration -= 0.01f; }
			_timer.setDuration(_duration);
			_tapped = false;

			transform.Find ("Game").Find("swapper").Find("background").gameObject.GetComponent<Image>().color = _currentColor;
		}
	}
}
