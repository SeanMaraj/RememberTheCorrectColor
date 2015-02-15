using UnityEngine;
using System.Collections;

public class GameColor{

	//TODO set these to private?
	public Color value { get; set; }
	public int weight { get; set; }
	public string name { get; set; }
	public bool isMain { get; set; }
	public bool isFirst { get; set; }

	public GameColor(){}

	public GameColor (Color value, string name, int weight, bool isMain, bool isFirst = false)
	{
		this.value = value;
		this.name = name;
		this.weight = weight;
		this.isMain = isMain;
		this.isFirst = isFirst;
	}
}
