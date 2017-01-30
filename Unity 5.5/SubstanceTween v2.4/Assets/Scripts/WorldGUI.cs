using UnityEngine;
using System.Collections;

public class WorldGUI : MonoBehaviour {

	public  float updateInterval = 0.5F;
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	public string format;
	float xAxis = 0.0f;
	float yAxis = 0.0f;
	float zAxis = 0.0f;

	void Start()
	{
		timeleft = updateInterval;  
	}
		
	void Update() 
	{
		transform.eulerAngles = new Vector3(xAxis, yAxis, zAxis);
		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;
		if( timeleft <= 0.0 )
		{
			float fps = accum/frames;
			 format = System.String.Format("{0:F2} FPS",fps);
			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}
	}

	void OnGUI () 
	{
		GUI.Label(new Rect(Screen.width/2, 0, 200, 30), "Sun Controls" );
		xAxis= GUI.HorizontalSlider (new Rect (Screen.width/2, 25, 200, 30), xAxis, -90, 90.0f);
		yAxis= GUI.HorizontalSlider (new Rect (Screen.width/2, 50, 200, 30), yAxis, -90, 90.0f);
		zAxis= GUI.HorizontalSlider (new Rect (Screen.width/2, 75, 200, 30), zAxis, -90, 90.0f);
		GUI.Label(new Rect(Screen.width/2, 80, 200, 30), format );
	}
}