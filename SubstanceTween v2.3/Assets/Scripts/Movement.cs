using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{

		float xAxisValue = Input.GetAxis("Horizontal");
		float zAxisValue = Input.GetAxis("Vertical");
		float yAxisValue = Input.GetAxis("Horizontal 2");
		if(Camera.current != null)
		{
			Camera.current.transform.Translate(new Vector3(xAxisValue, yAxisValue, zAxisValue));
		}

	
	}
}
