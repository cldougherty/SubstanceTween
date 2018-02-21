using UnityEngine;
using System.Collections;

public class WorldGUI : MonoBehaviour {



	float xAxis = 0.0f;
	float yAxis = 0.0f;

	float zAxis = 0.0f;

	void Update() {
		transform.eulerAngles = new Vector3(xAxis, yAxis, zAxis);
	}

	void OnGUI () {
		GUI.Label(new Rect(Screen.width/2, 0, 200, 30), "To Challenge the Sun" );
		xAxis= GUI.HorizontalSlider (new Rect (Screen.width/2, 25, 200, 30), xAxis, -90, 90.0f);
		yAxis= GUI.HorizontalSlider (new Rect (Screen.width/2, 50, 200, 30), yAxis, -90, 90.0f);
		zAxis= GUI.HorizontalSlider (new Rect (Screen.width/2, 75, 200, 30), zAxis, -90, 90.0f);
	
	}
}
