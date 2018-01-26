using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Diagnostics;

[System.Serializable]
//[ExecuteInEditMode]
public class PrefabProperties : MonoBehaviour
{ // This script automatically gets added to the prefab once it is created
    public ProceduralPropertyDescription[] materialVariables;
    [SerializeField]
    public List<ProceduralPropertyDescription> animatedMaterialVariables = new List<ProceduralPropertyDescription>();

    public bool animateBackwards;
    public bool animateOnStart;
    public bool animateBasedOnTime;
    public enum AnimateObjectBasedOnPosition { None, FasterOnApproach, SlowerOnApproach };
    public AnimateObjectBasedOnPosition animateObjectBasedOnPosition = AnimateObjectBasedOnPosition.None;
    public float distanceLerpCalc = 1;
    public GameObject customObject;
    public float playerDistanceToObject;
    public float objectMinDistance = 3;
    public float objectMaxDistance = 20;
    public bool animationToggle,playOnce, useSharedMaterial = true, rebuildSubstanceImmediately, cacheAtStartup, animateOutputParameters;
    public float animationDelay = 1f;
    //[HideInInspector]
    public Renderer rend;
    public ProceduralMaterial substance;
   // [HideInInspector]
    public int keyFrames, currentKeyframeIndex;
    //[HideInInspector]
    public float currentAnimationTime;
    public List<float> keyFrameTimes = new List<float>();
    [HideInInspector]
    public List<float> keyFrameTimesOriginal = new List<float>();
    public List<MaterialVariableListHolder> MaterialVariableKeyframeList = new List<MaterialVariableListHolder>();
    [SerializeField]
    public List<MaterialVariableDictionaryHolder> MaterialVariableKeyframeDictionaryList = new List<MaterialVariableDictionaryHolder>();

    public enum AnimationType { Loop, BackAndForth };
    public AnimationType animationType;

    public AnimationCurve prefabAnimationCurve;
    [HideInInspector]
    public AnimationCurve prefabAnimationCurveBackup;

    public float currentKeyframeAnimationTime;
    public float lerp = 0, lerpCalc, curveFloat = 0, animationTimeRestartEnd = 0, keyframeSum = 0;
    public bool flickerEnabled, flickerToggle, flickerFloatToggle, flickerColor3Toggle, flickerColor4Toggle, flickerVector2Toggle, flickerVector3Toggle, flickerVector4Toggle, flickerBooleanToggle, flickerEmissionToggle ;
    float flickerCalc = 1, flickerFloatCalc = 1, flickerColor3Calc = 1, flickerColor4Calc = 1, flickerVector2Calc = 1, flickerVector3Calc = 1, flickerVector4Calc = 1, flickerEmissionCalc = 1;
    int flickerBooleanCalc;
    public float flickerMin = 0.2f, flickerMax = 1.0f;
    public enum MySubstanceProcessorUsage { Unsupported, One, Half, All };
    public MySubstanceProcessorUsage mySubstanceProcessorUsage;
    public enum MyProceduralCacheSize { Medium = 0, Heavy = 1, None = 2, NoLimit = 3, Tiny = 4 };
    public MyProceduralCacheSize myProceduralCacheSize;
    public int AnimationStartKeyframe = 0;
    public int AnimationEndKeyframe = 0;
    public bool deleteOldListValuesOnStart;
    public Color emissionInput;
    
    // Use this for initialization
    public void Awake()
    {
       
        rend = GetComponent<Renderer>();
      
     
        if (useSharedMaterial && this.GetComponent<Renderer>() != null)
            substance = rend.sharedMaterial as ProceduralMaterial;
        else if (!useSharedMaterial && this.GetComponent<Renderer>() != null)
            substance = rend.material as ProceduralMaterial;
        else
            Debug.LogWarning("No Renderer on " + this.name);
        
        Debug.Log("Awake: " + emissionInput.ToString() + " " + rend.sharedMaterial.GetColor("_EmissionColor").ToString());
        emissionInput = rend.sharedMaterial.GetColor("_EmissionColor");
        if (substance)
        {
            substance.cacheSize = (ProceduralCacheSize)myProceduralCacheSize;
            ProceduralMaterial.substanceProcessorUsage = (ProceduralProcessorUsage)mySubstanceProcessorUsage;
            substance.enableInstancing = true;
            materialVariables = substance.GetProceduralPropertyDescriptions();
            if (MaterialVariableKeyframeList.Count > 0)
            {
                Debug.Log("MaterialVariableKeyframeList.Count: " + MaterialVariableKeyframeList.Count);
                for (int i = 0; i <= MaterialVariableKeyframeList.Count - 1; i++)
                {
                    MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                    MaterialVariableKeyframeDictionaryList[i].PropertyMaterialName = MaterialVariableKeyframeList[0].PropertyMaterialName;
                    for (int j = 0; j < materialVariables.Length; j++)
                    {
                        ProceduralPropertyDescription materialVariable = materialVariables[j];
                        ProceduralPropertyType propType = materialVariables[j].type;
                        if (propType == ProceduralPropertyType.Float)
                        {
                            for (int k = 0; k < MaterialVariableKeyframeList[i].myValues.Count; k++)
                            {
                                if (MaterialVariableKeyframeList[i].myKeys[k] == materialVariable.name)
                                {
                                    MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, float.Parse(MaterialVariableKeyframeList[i].myValues[k]));
                                    MaterialVariableKeyframeDictionaryList[i].PropertyFloatDictionary.Add(materialVariable.name, float.Parse(MaterialVariableKeyframeList[i].myValues[k]));
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Color3)
                        {
                            for (int k = 0; k < MaterialVariableKeyframeList[i].myKeys.Count; k++)
                            {
                                if (MaterialVariableKeyframeList[i].myKeys[k] == materialVariable.name)
                                {
                                    Color curColor;
                                    ColorUtility.TryParseHtmlString(MaterialVariableKeyframeList[i].myValues[k], out curColor);
                                    MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curColor);
                                    MaterialVariableKeyframeDictionaryList[i].PropertyColorDictionary.Add(materialVariable.name, curColor);
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Color4)
                        {
                            for (int k = 0; k < MaterialVariableKeyframeList[i].myKeys.Count; k++)
                            {
                                if (MaterialVariableKeyframeList[i].myKeys[k] == materialVariable.name)
                                {
                                    Color curColor;
                                    ColorUtility.TryParseHtmlString(MaterialVariableKeyframeList[i].myValues[k], out curColor);
                                    MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curColor);
                                    MaterialVariableKeyframeDictionaryList[i].PropertyColorDictionary.Add(materialVariable.name, curColor);
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
                        {
                            if (propType == ProceduralPropertyType.Vector4)
                            {
                                for (int k = 0; k < MaterialVariableKeyframeList[i].myKeys.Count; k++)
                                {
                                    if (MaterialVariableKeyframeList[i].myKeys[k] == materialVariable.name)
                                    {
                                        Vector4 curVector4 = StringToVector(MaterialVariableKeyframeList[i].myValues[k], 4);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curVector4);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyVector4Dictionary.Add(materialVariable.name, curVector4);
                                    }
                                }
                            }
                            else if (propType == ProceduralPropertyType.Vector3)
                            {
                                for (int k = 0; k < MaterialVariableKeyframeList[i].myKeys.Count; k++)
                                {
                                    if (MaterialVariableKeyframeList[i].myKeys[k] == materialVariable.name)
                                    {
                                        Vector3 curVector3 = StringToVector(MaterialVariableKeyframeList[i].myValues[k], 3);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curVector3);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyVector3Dictionary.Add(materialVariable.name, curVector3);
                                    }
                                }
                            }
                            else if (propType == ProceduralPropertyType.Vector2)
                            {
                                for (int k = 0; k < MaterialVariableKeyframeList[i].myKeys.Count; k++)
                                {
                                    if (MaterialVariableKeyframeList[i].myKeys[k] == materialVariable.name)
                                    {
                                        Vector2 curVector2 = StringToVector(MaterialVariableKeyframeList[i].myValues[k], 2);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curVector2);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyVector2Dictionary.Add(materialVariable.name, curVector2);
                                    }
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Enum)
                        {
                            for (int k = 0; k < MaterialVariableKeyframeList[i].myKeys.Count; k++)
                            {
                                if (MaterialVariableKeyframeList[i].myKeys[k] == materialVariable.name)
                                {
                                    int curEnum = int.Parse(MaterialVariableKeyframeList[i].myValues[k]);
                                    MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curEnum);
                                    MaterialVariableKeyframeDictionaryList[i].PropertyEnumDictionary.Add(materialVariable.name, curEnum);
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Boolean)
                        {
                            for (int k = 0; k < MaterialVariableKeyframeList[i].myKeys.Count; k++)
                            {
                                if (MaterialVariableKeyframeList[i].myKeys[k] == materialVariable.name)
                                {
                                    bool curBool = bool.Parse(MaterialVariableKeyframeList[i].myValues[k]);
                                    MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curBool);
                                    MaterialVariableKeyframeDictionaryList[i].PropertyBoolDictionary.Add(materialVariable.name, curBool);
                                }
                            }
                        }
                    }
                    MaterialVariableKeyframeDictionaryList[i].MainTex = MaterialVariableKeyframeList[i].MainTex;
                    MaterialVariableKeyframeDictionaryList[i].emissionColor = MaterialVariableKeyframeList[i].emissionColor;
                    substance.RebuildTextures();
                }
            }
        }
       
        if(deleteOldListValuesOnStart)
        MaterialVariableKeyframeList.Clear();
        //Debug.Log("Awake2: " + emissionInput.ToString() + " "+ rend.sharedMaterial.GetColor("_EmissionColor").ToString());
    }
    void Start()
    {
        if (substance)
        materialVariables = substance.GetProceduralPropertyDescriptions();
        emissionInput = rend.sharedMaterial.GetColor("_EmissionColor");
            Debug.Log("Start: " + emissionInput);
         rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
        customObject = GameObject.FindGameObjectWithTag("Player");
        substance.RebuildTexturesImmediately();
        substance.RebuildTextures();
        if (MaterialVariableKeyframeDictionaryList.Count > 0)
        {// if you have 1 keyframe set the substance parameters from that keyframe if you have more than 1 it will animate in Update()
            for (int i = 0; i < materialVariables.Length; i++)
            {
                ProceduralPropertyDescription MaterialVariable = materialVariables[i];
                ProceduralPropertyType propType = materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float)
                {
                    foreach (KeyValuePair<string, float> keyValue in MaterialVariableKeyframeDictionaryList[0].PropertyFloatDictionary)
                        if (keyValue.Key == materialVariables[i].name)
                            substance.SetProceduralFloat(keyValue.Key, keyValue.Value);
                }
                else if (propType == ProceduralPropertyType.Color3)
                {
                    foreach (KeyValuePair<string, Color> keyValue in MaterialVariableKeyframeDictionaryList[0].PropertyColorDictionary)
                        if (keyValue.Key == materialVariables[i].name)
                            substance.SetProceduralColor(keyValue.Key, keyValue.Value);
                }
                else if (propType == ProceduralPropertyType.Color4)
                {
                    foreach (KeyValuePair<string, Color> keyValue in MaterialVariableKeyframeDictionaryList[0].PropertyColorDictionary)
                        if (keyValue.Key == materialVariables[i].name)
                            substance.SetProceduralColor(keyValue.Key, keyValue.Value);
                }
                else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
                {
                    if (propType == ProceduralPropertyType.Vector4)
                    {
                        foreach (KeyValuePair<string, Vector4> keyValue in MaterialVariableKeyframeDictionaryList[0].PropertyVector4Dictionary)
                            if (keyValue.Key == materialVariables[i].name)
                                substance.SetProceduralVector(keyValue.Key, keyValue.Value);
                    }
                    else if (propType == ProceduralPropertyType.Vector3)
                    {
                        foreach (KeyValuePair<string, Vector3> keyValue in MaterialVariableKeyframeDictionaryList[0].PropertyVector3Dictionary)
                            if (keyValue.Key == materialVariables[i].name)
                                substance.SetProceduralVector(keyValue.Key, keyValue.Value);
                    }
                    else if (propType == ProceduralPropertyType.Vector2)
                    {
                        foreach (KeyValuePair<string, Vector2> keyValue in MaterialVariableKeyframeDictionaryList[0].PropertyVector2Dictionary)
                        {
                            if (keyValue.Key == "$outputSize" && !animateOutputParameters)
                                substance.SetProceduralVector(keyValue.Key, keyValue.Value);
                            if (keyValue.Key == materialVariables[i].name)
                                substance.SetProceduralVector(keyValue.Key, keyValue.Value);
                        }
                    }
                }
                else if (propType == ProceduralPropertyType.Enum)
                {
                    foreach (KeyValuePair<string, int> keyValue in MaterialVariableKeyframeDictionaryList[0].PropertyEnumDictionary)
                        if (keyValue.Key == materialVariables[i].name)
                            substance.SetProceduralEnum(keyValue.Key, keyValue.Value);
                }
                else if (propType == ProceduralPropertyType.Boolean)
                {
                    foreach (KeyValuePair<string, bool> keyValue in MaterialVariableKeyframeDictionaryList[0].PropertyBoolDictionary)
                        if (keyValue.Key == materialVariables[i].name)
                            substance.SetProceduralBoolean(keyValue.Key, keyValue.Value);
                }
                //}
            }
            if (useSharedMaterial && rend)
                rend.sharedMaterial.SetColor("_EmissionColor", MaterialVariableKeyframeDictionaryList[0].emissionColor);
            else if (!useSharedMaterial && rend)
                rend.material.SetColor("_EmissionColor", MaterialVariableKeyframeDictionaryList[0].emissionColor);
            List<string> PropertyFloatKeysToRemove = new List<string>();
            List<string> PropertyColorKeysToRemove = new List<string>();
            List<string> PropertyVector4KeysToRemove = new List<string>();
            List<string> PropertyVector3KeysToRemove = new List<string>();
            List<string> PropertyVector2KeysToRemove = new List<string>();
            List<string> PropertyEnumKeysToRemove = new List<string>();
            List<string> PropertyBoolKeysToRemove = new List<string>();

            for (int i = 0; i < materialVariables.Length; i++)
            {// search for variables that never change in animation and delete them from the dictionary.
                ProceduralPropertyDescription MaterialVariable = materialVariables[i];
                ProceduralPropertyType propType = materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float)
                {
                    float propertyFloatAnimationCheck = 0;
                    for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                    {
                        foreach (KeyValuePair<string, float> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyFloatDictionary)
                        {
                            if (keyValue.Key == materialVariables[i].name)
                            {
                                if (j == 0)
                                    propertyFloatAnimationCheck = keyValue.Value;
                                if (j > 0 && cacheAtStartup && keyValue.Value != propertyFloatAnimationCheck)
                                {
                                    substance.CacheProceduralProperty(keyValue.Key, true);
                                    animatedMaterialVariables.Add(MaterialVariable);
                                }
                                if (j == keyFrameTimes.Count - 1 && keyValue.Value == propertyFloatAnimationCheck)
                                {
                                    for (int k = 0; k <= keyFrameTimes.Count; k++)
                                        PropertyFloatKeysToRemove.Add(keyValue.Key);
                                }
                            }
                        }
                        foreach (string item in PropertyFloatKeysToRemove)
                            if (deleteOldListValuesOnStart)
                                MaterialVariableKeyframeDictionaryList[j].PropertyFloatDictionary.Remove(item);
                    }
                }
                else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
                {
                    Color propertyColorAnimationCheck = Color.white;
                    for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                    {
                        foreach (KeyValuePair<string, Color> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyColorDictionary)
                        {
                            if (keyValue.Key == materialVariables[i].name)
                            {
                                if (j == 0)
                                    propertyColorAnimationCheck = keyValue.Value;
                                if (j > 0 && cacheAtStartup && keyValue.Value != propertyColorAnimationCheck)
                                {
                                    substance.CacheProceduralProperty(keyValue.Key, true);
                                    animatedMaterialVariables.Add(MaterialVariable);
                                }
                                if (j == keyFrameTimes.Count - 1 && keyValue.Value == propertyColorAnimationCheck)
                                {
                                    for (int k = 0; k <= keyFrameTimes.Count; k++)
                                        PropertyColorKeysToRemove.Add(keyValue.Key);
                                }
                            }
                        }
                        foreach (string item in PropertyColorKeysToRemove)
                            if (deleteOldListValuesOnStart)
                                MaterialVariableKeyframeDictionaryList[j].PropertyColorDictionary.Remove(item);
                    }
                }
                else if (propType == ProceduralPropertyType.Vector4)
                {
                    Vector4 propertyVector4AnimationCheck = Vector4.zero;
                    for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                    {
                        foreach (KeyValuePair<string, Vector4> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyVector4Dictionary)
                        {
                            if (keyValue.Key == materialVariables[i].name)
                            {
                                if (j == 0)
                                    propertyVector4AnimationCheck = keyValue.Value;
                                if (j > 0 && cacheAtStartup && keyValue.Value != propertyVector4AnimationCheck)
                                {
                                    substance.CacheProceduralProperty(keyValue.Key, true);
                                    animatedMaterialVariables.Add(MaterialVariable);
                                }
                                if (j == keyFrameTimes.Count - 1 && keyValue.Value == propertyVector4AnimationCheck)
                                {
                                    for (int k = 0; k <= keyFrameTimes.Count; k++)
                                        PropertyVector4KeysToRemove.Add(keyValue.Key);
                                }
                            }
                        }
                        foreach (string item in PropertyVector4KeysToRemove)
                            if (deleteOldListValuesOnStart)
                                MaterialVariableKeyframeDictionaryList[j].PropertyVector4Dictionary.Remove(item);
                    }
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    Vector3 propertyVector3AnimationCheck = Vector3.zero;
                    for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                    {
                        foreach (KeyValuePair<string, Vector3> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyVector3Dictionary)
                        {
                            if (keyValue.Key == materialVariables[i].name)
                            {
                                if (j == 0)
                                    propertyVector3AnimationCheck = keyValue.Value;
                                if (j > 0 && cacheAtStartup && keyValue.Value != propertyVector3AnimationCheck)
                                {
                                    substance.CacheProceduralProperty(keyValue.Key, true);
                                    animatedMaterialVariables.Add(MaterialVariable);
                                }
                                if (j == keyFrameTimes.Count - 1 && keyValue.Value == propertyVector3AnimationCheck)
                                {
                                    for (int k = 0; k <= keyFrameTimes.Count; k++)
                                        PropertyVector3KeysToRemove.Add(keyValue.Key);
                                }
                            }
                        }
                        foreach (string item in PropertyVector3KeysToRemove)
                            if (deleteOldListValuesOnStart)
                                MaterialVariableKeyframeDictionaryList[j].PropertyVector3Dictionary.Remove(item);
                    }
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    Vector2 propertyVector2AnimationCheck = Vector2.zero;
                    for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                    {
                        foreach (KeyValuePair<string, Vector2> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyVector2Dictionary)
                        {
                            if (keyValue.Key == materialVariables[i].name)
                            {
                                if (j == 0)
                                    propertyVector2AnimationCheck = keyValue.Value;
                                if (j > 0 && cacheAtStartup && keyValue.Value != propertyVector2AnimationCheck && (keyValue.Key[0] != '$' || (keyValue.Key[0] == '$' && animateOutputParameters)))
                                {
                                    substance.CacheProceduralProperty(keyValue.Key, true);
                                    animatedMaterialVariables.Add(MaterialVariable);
                                }
                                if (j == keyFrameTimes.Count - 1 && keyValue.Value == propertyVector2AnimationCheck || (keyValue.Key[0] == '$' && !animateOutputParameters))
                                {
                                    for (int k = 0; k <= keyFrameTimes.Count; k++)
                                        PropertyVector2KeysToRemove.Add(keyValue.Key);
                                }
                            }
                        }
                        foreach (string item in PropertyVector2KeysToRemove)
                            if (deleteOldListValuesOnStart)
                            MaterialVariableKeyframeDictionaryList[j].PropertyVector2Dictionary.Remove(item);
                    }
                }
                else if (propType == ProceduralPropertyType.Enum)
                {
                    int propertyEnumAnimationCheck = 9999;
                    for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                    {
                        foreach (KeyValuePair<string, int> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyEnumDictionary)
                        {
                            if (keyValue.Key == materialVariables[i].name)
                            {
                                if (j == 0)
                                    propertyEnumAnimationCheck = keyValue.Value;
                                if (j > 0 && cacheAtStartup && keyValue.Value != propertyEnumAnimationCheck)
                                {
                                    substance.CacheProceduralProperty(keyValue.Key, true);
                                    animatedMaterialVariables.Add(MaterialVariable);
                                }
                                if (j == keyFrameTimes.Count - 1 && keyValue.Value == propertyEnumAnimationCheck)
                                {
                                    for (int k = 0; k <= keyFrameTimes.Count; k++)
                                    {
                                        PropertyEnumKeysToRemove.Add(keyValue.Key);
                                    }
                                }
                            }
                        }
                        foreach (string item in PropertyEnumKeysToRemove)
                            if (deleteOldListValuesOnStart)
                                MaterialVariableKeyframeDictionaryList[j].PropertyEnumDictionary.Remove(item);
                    }
                }
                else if (propType == ProceduralPropertyType.Boolean)
                {
                    bool propertyBoolAnimationCheck = false;
                    for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                    {
                        foreach (KeyValuePair<string, bool> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyBoolDictionary)
                        {
                            if (keyValue.Key == materialVariables[i].name)
                            {
                                if (j == 0)
                                    propertyBoolAnimationCheck = keyValue.Value;
                                if (j > 0 && cacheAtStartup && keyValue.Value != propertyBoolAnimationCheck)
                                {
                                    substance.CacheProceduralProperty(keyValue.Key, true);
                                    animatedMaterialVariables.Add(MaterialVariable);
                                }
                                if (j == keyFrameTimes.Count - 1 && keyValue.Value == propertyBoolAnimationCheck)
                                {
                                    for (int k = 0; k <= keyFrameTimes.Count; k++)
                                    {
                                        PropertyBoolKeysToRemove.Add(keyValue.Key);
                                    }
                                }
                            }
                        }
                        foreach (string item in PropertyEnumKeysToRemove)
                            if (deleteOldListValuesOnStart)
                                MaterialVariableKeyframeDictionaryList[j].PropertyBoolDictionary.Remove(item);
                    }
                }
            }
        }
        if (AnimationEndKeyframe == 0)
            AnimationEndKeyframe = keyFrameTimes.Count - 1;
        currentKeyframeIndex = AnimationStartKeyframe;
        animationTimeRestartEnd = CalculateAnimationStartTime(AnimationStartKeyframe);
        if (animateOnStart)
            StartCoroutine(Prewarm()); // gives animation time to load
    }

    public IEnumerator Prewarm()
    {
        yield return new WaitForSeconds(animationDelay);
        if (MaterialVariableKeyframeDictionaryList.Count >= 2 /*&& animateOnStart*/ )
            animationToggle = true;
        else
            animationToggle = false;
    }
    private void OnGUI()
    {
       // GUI.Label(new Rect(0, 0, 100, 100), lerp.ToString());
        //GUI.Label(new Rect(0, 20, 400, 100), "Current animation Time between keyframes: " + currentAnimationTime.ToString());
        //GUI.Label(new Rect(0, 40, 300, 100),"Current Keyframe Index: " + currentKeyframeIndex.ToString());
        //GUI.Label(new Rect(0, 60, 300, 100), "Animation Time between current/next keyframe" + currentKeyframeAnimationTime.ToString());
        //GUI.Label(new Rect(0, 80, 300, 100), "Current animation time total: " + animationTimeRestartEnd.ToString());
        //GUI.Label(new Rect(0, 100, 300, 100), "Curve value " + curveFloat.ToString());
        rebuildSubstanceImmediately = GUI.Toggle(new Rect(0, 120, 300, 100), rebuildSubstanceImmediately, "Rebuild Immediately");
    }
    // Update is called once per frame
    void Update()
    {
        if (animationToggle)
        {
            

            substance.cacheSize = (ProceduralCacheSize)myProceduralCacheSize;
            if (flickerEnabled)
                ChangeFlicker();
            currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
            if (animationType == AnimationType.BackAndForth && animateBackwards && currentKeyframeIndex <= AnimationEndKeyframe && currentKeyframeIndex > 0)
                currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex - 1];

            if (animateObjectBasedOnPosition != AnimateObjectBasedOnPosition.None)
            {
                playerDistanceToObject = Vector3.Distance(customObject.transform.position, this.transform.position);
                if (animateObjectBasedOnPosition == AnimateObjectBasedOnPosition.SlowerOnApproach)
                {
                    if (playerDistanceToObject < objectMaxDistance && playerDistanceToObject > objectMinDistance - 0.1f)
                        distanceLerpCalc = Mathf.InverseLerp(objectMaxDistance, objectMinDistance, playerDistanceToObject);
                }
                if (animateObjectBasedOnPosition == AnimateObjectBasedOnPosition.FasterOnApproach)
                {
                    if (playerDistanceToObject < objectMaxDistance && playerDistanceToObject > objectMinDistance + 0.1f)
                        distanceLerpCalc = Mathf.InverseLerp(objectMinDistance, objectMaxDistance, playerDistanceToObject);
                    else if (playerDistanceToObject < objectMinDistance)
                        distanceLerpCalc = Mathf.InverseLerp(objectMinDistance, objectMaxDistance, objectMinDistance + 0.1f);
                    else if (playerDistanceToObject > objectMaxDistance)
                        distanceLerpCalc = Mathf.InverseLerp(objectMinDistance, objectMaxDistance, objectMaxDistance - 0.1f);
                }
            }
            if (animationType == AnimationType.Loop)
            {
                currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
                if (animateBasedOnTime)
                {
                    currentAnimationTime += Time.deltaTime;
                    animationTimeRestartEnd += Time.deltaTime;

                    if (animateObjectBasedOnPosition != AnimateObjectBasedOnPosition.None)
                    {
                        currentAnimationTime /= distanceLerpCalc;
                        animationTimeRestartEnd /= Time.deltaTime;
                    }
                }
                else
                {
                    if (animateObjectBasedOnPosition == AnimateObjectBasedOnPosition.SlowerOnApproach)
                    {
                        currentAnimationTime += distanceLerpCalc / Time.deltaTime;
                        animationTimeRestartEnd += distanceLerpCalc / Time.deltaTime;
                    }
                    else if (animateObjectBasedOnPosition == AnimateObjectBasedOnPosition.FasterOnApproach)
                    {
                        currentAnimationTime += Mathf.Lerp(1, 0, distanceLerpCalc) / 5;
                        animationTimeRestartEnd += Mathf.Lerp(1, 0, distanceLerpCalc) / 5;
                    }
                }
                curveFloat = prefabAnimationCurve.Evaluate(animationTimeRestartEnd);
                if (keyFrameTimes.Count > 2 && currentAnimationTime > currentKeyframeAnimationTime && currentKeyframeIndex <= AnimationEndKeyframe - 2)
                {
                    currentAnimationTime = 0;
                    currentKeyframeIndex++;
                }
                else if (keyFrameTimes.Count > 2 && currentKeyframeIndex >= AnimationEndKeyframe - 1 && currentAnimationTime >= currentKeyframeAnimationTime)
                {
                    currentAnimationTime = 0;
                    animationTimeRestartEnd = CalculateAnimationStartTime(AnimationStartKeyframe);
                    currentKeyframeIndex = AnimationStartKeyframe;
                    curveFloat = prefabAnimationCurve.Evaluate(animationTimeRestartEnd);
                    lerp = Mathf.InverseLerp(prefabAnimationCurve.keys[currentKeyframeIndex].time, prefabAnimationCurve.keys[currentKeyframeIndex + 1].time, curveFloat);
                    if (playOnce)
                        animationToggle = false;
                }
                else if (keyFrameTimes.Count == 2 && currentAnimationTime > currentKeyframeAnimationTime)
                {
                    currentAnimationTime = 0;
                    animationTimeRestartEnd = 0;
                    currentKeyframeIndex = 0;
                }
                if (currentKeyframeIndex <= AnimationEndKeyframe - 1)
                    lerp = Mathf.InverseLerp(prefabAnimationCurve.keys[currentKeyframeIndex].time, prefabAnimationCurve.keys[currentKeyframeIndex + 1].time, curveFloat);
            }

            else if (animationType == AnimationType.BackAndForth)
            {
                if (animateBasedOnTime)
                {
                    if (!animateBackwards)
                    {
                        currentAnimationTime += Time.deltaTime;
                        animationTimeRestartEnd += Time.deltaTime;
                    }
                    else if (animateBackwards)
                    {
                        currentAnimationTime -= Time.deltaTime;
                        animationTimeRestartEnd -= Time.deltaTime;
                    }

                    if (animateObjectBasedOnPosition != AnimateObjectBasedOnPosition.None)
                    {
                        currentAnimationTime /= distanceLerpCalc;
                        animationTimeRestartEnd /= Time.deltaTime;
                    }
                }

                else
                {
                    if (animateObjectBasedOnPosition == AnimateObjectBasedOnPosition.SlowerOnApproach)
                    {
                        if (!animateBackwards)
                        {
                            currentAnimationTime += distanceLerpCalc / Time.deltaTime;
                            animationTimeRestartEnd += distanceLerpCalc / Time.deltaTime;
                        }

                        else if (animateBackwards)
                        {
                            currentAnimationTime -= distanceLerpCalc / Time.deltaTime;
                            animationTimeRestartEnd -= distanceLerpCalc / Time.deltaTime;
                        }

                    }
                    else if (animateObjectBasedOnPosition == AnimateObjectBasedOnPosition.FasterOnApproach)
                    {
                        if (!animateBackwards)
                        {
                            currentAnimationTime += Mathf.Lerp(1, 0, distanceLerpCalc) / 5;
                            animationTimeRestartEnd += Mathf.Lerp(1, 0, distanceLerpCalc) / 5;

                        }
                        if (animateBackwards)
                        {
                            currentAnimationTime -= Mathf.Lerp(1, 0, distanceLerpCalc) / 5;
                            animationTimeRestartEnd -= Mathf.Lerp(1, 0, distanceLerpCalc) / 5;
                        }
                    }
                }

                curveFloat = prefabAnimationCurve.Evaluate(animationTimeRestartEnd);
                if (keyFrameTimes.Count > 2 && AnimationEndKeyframe - AnimationStartKeyframe > 1 && !animateBackwards && currentAnimationTime > currentKeyframeAnimationTime && currentKeyframeIndex < AnimationEndKeyframe) // reach next keyframe when going forwards
                {
                    if (currentKeyframeIndex == AnimationEndKeyframe - 1) 
                        animateBackwards = true;
                    else
                    {
                        currentKeyframeIndex++;
                        currentAnimationTime = 0;
                    }
                }
                else if (keyFrameTimes.Count > 2 && animateBackwards && currentAnimationTime <= 0 && currentKeyframeIndex <= AnimationEndKeyframe && currentKeyframeIndex > AnimationStartKeyframe) // reach next keyframe when going backwards
                {
                    currentAnimationTime = currentKeyframeAnimationTime;
                    currentKeyframeIndex--;
                    curveFloat = prefabAnimationCurve.Evaluate(animationTimeRestartEnd);
                    lerp = (Mathf.Lerp(1, 0, Mathf.InverseLerp(prefabAnimationCurve.keys[currentKeyframeIndex + 1].time, prefabAnimationCurve.keys[currentKeyframeIndex].time, curveFloat)));
                }
                else if ((keyFrameTimes.Count == 2 || AnimationEndKeyframe - AnimationStartKeyframe == 1) && currentAnimationTime >= currentKeyframeAnimationTime)
                {
                    animateBackwards = true;
                    currentAnimationTime = currentKeyframeAnimationTime;
                }
                if (animateBackwards && currentKeyframeIndex == AnimationStartKeyframe && currentAnimationTime < 0) // if you reach the last keyframe when going backwards go forwards.
                {
                    animateBackwards = false;
                    currentAnimationTime = 0;
                    animationTimeRestartEnd = CalculateAnimationStartTime(AnimationStartKeyframe);
                    if (playOnce)
                        animationToggle = false;
                }
                if (!animateBackwards && currentKeyframeIndex < AnimationEndKeyframe)
                {
                    lerp = Mathf.InverseLerp(prefabAnimationCurve.keys[currentKeyframeIndex].time, prefabAnimationCurve.keys[currentKeyframeIndex + 1].time, curveFloat);
                }
                else if (keyFrameTimes.Count > 2 && AnimationEndKeyframe - AnimationStartKeyframe > 1  && animateBackwards && currentAnimationTime != currentKeyframeAnimationTime && currentKeyframeIndex != AnimationEndKeyframe)
                {
                    lerp = (Mathf.Lerp(1, 0, Mathf.InverseLerp(prefabAnimationCurve.keys[currentKeyframeIndex + 1].time, prefabAnimationCurve.keys[currentKeyframeIndex].time, curveFloat)));
                }
                else if ((keyFrameTimes.Count == 2 || AnimationEndKeyframe - AnimationStartKeyframe == 1) && animateBackwards && currentAnimationTime != currentKeyframeAnimationTime)
                    lerp = curveFloat / currentKeyframeAnimationTime;
            }
            if (materialVariables != null)
                for (int i = 0; i < animatedMaterialVariables.Count; i++)
                {
                    ProceduralPropertyDescription materialVariable = animatedMaterialVariables[i];
                    ProceduralPropertyType propType = animatedMaterialVariables[i].type;
                    switch (propType)
                    {
                        case ProceduralPropertyType.Float:
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                {
                                    substance.SetProceduralFloat(materialVariable.name, Mathf.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyFloatDictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyFloatDictionary[materialVariable.name], lerp * flickerFloatCalc));
                                }
                            }
                            break;
                        case ProceduralPropertyType.Color3:
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                {
                                    substance.SetProceduralColor(materialVariable.name, Color.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyColorDictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyColorDictionary[materialVariable.name], lerp * flickerColor3Calc));
                                }
                            }
                            break;
                        case ProceduralPropertyType.Color4:
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                {
                                    substance.SetProceduralColor(materialVariable.name, Color.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyColorDictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyColorDictionary[materialVariable.name], lerp * flickerColor4Calc));
                                }
                            }
                            break;
                        case ProceduralPropertyType.Vector4:
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                {
                                    substance.SetProceduralVector(materialVariable.name, Vector4.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector4Dictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyVector4Dictionary[materialVariable.name], lerp * flickerVector4Calc));
                                }
                            }
                            break;
                        case ProceduralPropertyType.Vector3:
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                {
                                    substance.SetProceduralVector(materialVariable.name, Vector3.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector3Dictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyVector3Dictionary[materialVariable.name], lerp * flickerVector3Calc));
                                }
                            }
                            break;
                        case ProceduralPropertyType.Vector2:
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                {
                                    substance.SetProceduralVector(materialVariable.name, Vector2.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector2Dictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyVector2Dictionary[materialVariable.name], lerp * flickerVector2Calc));
                                }
                            }
                            break;
                        case ProceduralPropertyType.Enum:
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (currentAnimationTime == 0 || currentAnimationTime == currentKeyframeAnimationTime))
                                {
                                    substance.SetProceduralEnum(materialVariable.name, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyEnumDictionary[materialVariable.name]);
                                }
                            }
                            break;
                        case ProceduralPropertyType.Boolean:
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (currentAnimationTime == 0 || currentAnimationTime == currentKeyframeAnimationTime))
                                {
                                    substance.SetProceduralBoolean(materialVariable.name, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyBoolDictionary[materialVariable.name]);
                                }
                            }
                            break;
                    }
                }
            if (rend.sharedMaterial.HasProperty("_EmissionColor") && currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
            {
                Color curlerp1Emission = MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].emissionColor;
                Color curlerp2Emission = MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].emissionColor;
                if (curlerp1Emission != curlerp2Emission)
                    rend.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(curlerp1Emission, curlerp2Emission, lerp * flickerEmissionCalc));
            }
            if (rend.sharedMaterial.HasProperty("_MainTex") && currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
            {
                Vector2 curlerp1MainTex = MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].MainTex;
                Vector2 curlerp2MainTex = MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].MainTex;
                if (curlerp1MainTex != curlerp2MainTex)
                    rend.sharedMaterial.SetTextureOffset("_MainTex", Vector2.Lerp(curlerp1MainTex, curlerp2MainTex, lerp * flickerCalc));
            }
            if (!rebuildSubstanceImmediately)
                substance.RebuildTextures();
            else
                substance.RebuildTexturesImmediately();
        }
    }

    void OnValidate() // runs when anything in the inspector or graph changes.
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorWindow.focusedWindow || (UnityEditor.EditorWindow.focusedWindow.ToString() != " (UnityEditor.CurveEditorWindow)" && UnityEditor.EditorWindow.focusedWindow.ToString() != " (SubstanceToolWindow)")) // if you delete or add a keyframe in the curve editor, Reset keys.
        {
            if (UnityEditor.EditorWindow.focusedWindow)
            if (keyFrameTimes.Count > keyFrameTimesOriginal.Count) // if the keyframe size is larger than the original number of keyframes delete the extra keyframes.
            {
                int numOfAddedKeyframes = (keyFrameTimes.Count - keyFrameTimesOriginal.Count);
                for (int i = 0; i < numOfAddedKeyframes; i++)
                {
                    keyFrameTimes.RemoveAt(keyFrameTimes.Count - 1);
                }
            }
            List<Keyframe> tmpKeyframes = new List<Keyframe>();
            if (prefabAnimationCurve != null)
            {
                tmpKeyframes = prefabAnimationCurve.keys.ToList();
                keyframeSum = 0;
                if (prefabAnimationCurve.keys.Count() >= 1)
                {
                    for (int j = keyFrameTimes.Count() - 1; j > 0; j--)// remove all keys
                        prefabAnimationCurve.RemoveKey(j);
                }
                for (int j = 0; j < keyFrameTimes.Count(); j++)//rewrite keys with changed times
                {
                    prefabAnimationCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, tmpKeyframes[j].inTangent, tmpKeyframes[j].outTangent));
                    if (j == 0)
                        UnityEditor.AnimationUtility.SetKeyBroken(prefabAnimationCurve, 0, true);
                    keyframeSum += keyFrameTimes[j];
                }
                if (keyFrameTimes.Count > 1)
                    keyframeSum -= keyFrameTimes[keyFrameTimes.Count - 1];

                if (animationTimeRestartEnd > keyframeSum) //Reset animation variables
                {
                    currentAnimationTime = 0;
                    currentKeyframeIndex = 0;
                    animationTimeRestartEnd = 0;
                }
                prefabAnimationCurveBackup.keys = prefabAnimationCurve.keys;
            }
          
        }
        if (UnityEditor.EditorWindow.focusedWindow && UnityEditor.EditorWindow.focusedWindow.ToString() == " (UnityEditor.CurveEditorWindow)" && prefabAnimationCurve.keys.Count() != prefabAnimationCurveBackup.keys.Count()) // if you delete or add a keyframe in the curve editor, Reset keys.
        {
            prefabAnimationCurve.keys = prefabAnimationCurveBackup.keys;
        }
        CalculateAnimationStartTime(AnimationStartKeyframe);
#endif
    }

    public static Vector4 StringToVector(string startVector, int VectorAmount)
    {
        if (startVector.StartsWith("(") && startVector.EndsWith(")")) // Remove "( )" fram the string 
            startVector = startVector.Substring(1, startVector.Length - 2);
        string[] sArray = startVector.Split(',');
        if (VectorAmount == 2)
        {
            Vector2 result = new Vector2(float.Parse(sArray[0]), float.Parse(sArray[1]));
            return result;
        }
        else if (VectorAmount == 3)
        {
            Vector3 result = new Vector3(float.Parse(sArray[0]), float.Parse(sArray[1]), float.Parse(sArray[2]));
            return result;
        }
        else if (VectorAmount == 4)
        {
            Vector4 result = new Vector4(float.Parse(sArray[0]), float.Parse(sArray[1]), float.Parse(sArray[2]), float.Parse(sArray[3]));
            return result;
        }
        else
            return new Vector4(0, 0, 0, 0);
    }
    public float ChangeFlicker()
    {
        if (flickerEnabled)
        {
            if (flickerToggle || flickerFloatToggle || flickerColor3Toggle || flickerColor4Toggle || flickerVector2Toggle || flickerVector3Toggle || flickerVector4Toggle)
                flickerCalc = UnityEngine.Random.Range(flickerMin, flickerMax);
            if (flickerToggle)
                flickerEmissionCalc = flickerCalc;
            if (flickerFloatToggle)
                flickerFloatCalc = flickerCalc;
            else
                flickerFloatCalc = 1;
            if (flickerColor3Toggle)
                flickerColor3Calc = flickerCalc;
            else
                flickerColor3Calc = 1;
            if (flickerColor4Toggle)
                flickerColor4Calc = flickerCalc;
            else
                flickerColor4Calc = 1;
            if (flickerVector2Toggle)
                flickerVector2Calc = flickerCalc;
            else
                flickerVector2Calc = 1;
            if (flickerVector3Toggle)
                flickerVector3Calc = flickerCalc;
            else
                flickerVector3Calc = 1;
            if (flickerVector4Toggle)
                flickerVector4Calc = flickerCalc;
            else
                flickerVector4Calc = 1;
            if (flickerBooleanToggle)
                flickerBooleanCalc = (int)UnityEngine.Random.Range(0, 1);
            return flickerCalc;
        }
        else
        {
            flickerCalc = 1;
            return flickerCalc;
        }
    }

    public float CalculateTimeAtKeyframe(bool loop)
    {
        float timeCalc = 0;
        if(loop)
        {
            for (int i = 0; i < AnimationStartKeyframe; i++)
            {
                timeCalc += keyFrameTimes[i];
            }
          
        }
        else
        {
            for (int i = 0; i < AnimationEndKeyframe; i++)
            {
                timeCalc += keyFrameTimes[i];
            }

            for (int i = AnimationEndKeyframe; i > AnimationStartKeyframe; i++)
            {
                timeCalc += keyFrameTimes[i];
            }


        }
        return timeCalc;
    }

    

    public float CalculateAnimationStartTime(int index)
    {
        float startTimeCalc = 0;
        for(int i = 0; i < index; i++)
        {
            startTimeCalc += keyFrameTimes[i];
        }
        return startTimeCalc;
    }
}