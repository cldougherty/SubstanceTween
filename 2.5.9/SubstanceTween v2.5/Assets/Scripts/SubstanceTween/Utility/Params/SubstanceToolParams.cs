using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubstanceToolParams
{

    public GameObject currentSelection;
    public bool gameIsPaused, showEnumDropdown, showKeyframes, reWriteAllKeyframeTimes, curveDebugFoldout, emissionFlickerFoldout;
    public bool showVariableInformationToggle = false;
    public Vector2 scrollVal;

    public bool mousePressedInCurveEditor = false;
    public PrefabProperties selectedPrefabScript;

    public string lastAction;

    public List<string> DebugStrings = new List<string>();


    public bool testingCurve; // GUI will not repaint when this is true. this is so the unit testing will not cause errors when adding new animation cuvre keys.
}
