using UnityEngine;
using System.Collections;

public class GameColor : MonoBehaviour {

	//TODO set these to private?
	public Color value { get; set; }
	public int weight { get; set; }
	public string name { get; set; }
	public bool isMain { get; set; }
	public bool isFirst { get; set; }
	public bool isTapped { get; set; }

	/*
	public GameColor (Color value)
	{
		this.value = value;
	}

	public GameColor (Color value, int weight)
	{
		this.value = value;
		this.weight = weight;
	}
	*/

	public GameColor (Color value, string name, int weight, bool isMain, bool isFirst = false, bool isTapped = false)
	{
		this.value = value;
		this.name = name;
		this.weight = weight;
		this.isMain = isMain;
		this.isFirst = isFirst;
		this.isTapped = isTapped;
	}
}
