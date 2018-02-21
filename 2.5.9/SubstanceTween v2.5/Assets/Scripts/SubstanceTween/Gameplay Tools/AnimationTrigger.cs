using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour {

    public GameObject animatedPrefab;
    public bool stopAnimationOnTriggerExit;
	// Use this for initialization
	void Start ()
    {
		
	}

    void OnTriggerEnter(Collider other)
    {
        //animatedPrefab.GetComponent<PrefabProperties>().animationToggle = true;
        StartCoroutine(animatedPrefab.GetComponent<PrefabProperties>().Prewarm());
        //animatedPrefab.GetComponent<PrefabProperties>().Prewarm();
    }
    void OnTriggerExit(Collider other)
    {
        if (stopAnimationOnTriggerExit)
            animatedPrefab.GetComponent<PrefabProperties>().animationToggle = false;
            
    }
}