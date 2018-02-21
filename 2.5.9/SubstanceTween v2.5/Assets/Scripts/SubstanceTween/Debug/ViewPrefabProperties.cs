using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ViewPrefabProperties : MonoBehaviour {

    public GameObject prefabObject;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void OnGUI()
    {
        if (prefabObject && prefabObject.GetComponent<PrefabProperties>())
        {
            GUI.Label(new Rect(0, 0, 200, 20),"Prefab animationToggle: " + prefabObject.GetComponent<PrefabProperties>().animationToggle.ToString());

            if (prefabObject.GetComponent<SubstanceTool>() )
            {
                GUI.Label(new Rect(0, 20, 200, 20), "Has SubstanceTool script attached" + prefabObject.GetComponent<SubstanceTool>());
            }
            GUI.Label(new Rect(0, 40, 200, 20), "prefab DictionaryCount" + prefabObject.GetComponent<PrefabProperties>().MaterialVariableKeyframeDictionaryList.Count);
            GUI.Label(new Rect(0, 60, 200, 20), "prefab current index" + prefabObject.GetComponent<PrefabProperties>().currentKeyframeIndex);
            GUI.Label(new Rect(0, 80, 200, 20), "prefab end keyframe " + prefabObject.GetComponent<PrefabProperties>().animationEndKeyframe);
            GUI.Label(new Rect(0, 100, 200, 20), "prefab current keyframe time " + prefabObject.GetComponent<PrefabProperties>().currentAnimationTime);
           // GUI.Label(new Rect(0, 120, 200, 20), "prefab lerp " + prefabObject.GetComponent<PrefabProperties>().lerp);
            GUI.Label(new Rect(0, 120, 200, 20), "prefab lerp " + prefabObject.GetComponent<PrefabProperties>().lerp);
            GUI.Label(new Rect(0, 140, 200, 20), "Animated Variables " + prefabObject.GetComponent<PrefabProperties>().animatedMaterialVariables.Count);
            GUI.Label(new Rect(0, 160, 200, 20), "Animation time restart end: " + prefabObject.GetComponent<PrefabProperties>().animationTimeRestartEnd);


        }
    }

}
