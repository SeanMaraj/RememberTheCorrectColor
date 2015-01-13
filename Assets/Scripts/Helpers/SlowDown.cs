using UnityEngine;
using System.Collections;

public class SlowDown : MonoBehaviour {

	public GameObject cube;

	// Use this for initialization
	void Start () 
	{
		for (float i = 0; i < 1000f; i++)
		{
			GameObject thisCube = (GameObject)Instantiate(cube);
			thisCube.transform.position = new Vector3(0, 1.5f + i - i*0.5f, 0);
		}


	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
