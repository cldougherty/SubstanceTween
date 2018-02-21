using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class SubstanceTweenLastSelectionUtility : MonoBehaviour { // Since i cannot use OnSelectionChange with the inspector script i use this so that i always know what the last selected object was

    static SubstanceTweenLastSelectionUtility()
    {
        EditorApplication.update += () => {
            if (Selection.activeGameObject != null || Selection.activeGameObject != LastActiveGameObject)
                LastActiveGameObject = Selection.activeGameObject;
        };
    }

    public static GameObject LastActiveGameObject { get; private set; }
}
