using UnityEngine;
using System.Collections;

public class GameColor : MonoBehaviour {

	public int weight { get; set; }
	public Color color { get; set; }

	public GameColor (Color color, int weight)
	{
		this.color = color;
		this.weight = weight;
	}
}
