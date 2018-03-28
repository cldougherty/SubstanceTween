using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// script that goes on object when tool starts and in the build 
/// </summary>

[System.Serializable]
public class PrefabProperties : MonoBehaviour
{ // This script automatically gets added to the prefab once it is created
    public ProceduralPropertyDescription[] materialVariables;
    [SerializeField]
    public List<ProceduralPropertyDescription> animatedMaterialVariables = new List<ProceduralPropertyDescription>();

    public bool animateBackwards;
    public bool animateOnStart;
    
    public enum AnimationType { Loop, BackAndForth };
    public AnimationType animationType;
    public bool playOnce;
    public float animationDelay = 1f;
    public int animationStartKeyframe = 0;
    public int animationEndKeyframe = 0;

    public bool animationToggle;
    public bool animateBasedOnTime;

    public AnimationCurve prefabAnimationCurve;
    [HideInInspector]
    public AnimationCurve prefabAnimationCurveBackup;

    public GameObject customObject;
    public enum AnimateObjectBasedOnPosition { None, FasterOnApproach, SlowerOnApproach };
    public AnimateObjectBasedOnPosition animateObjectBasedOnPosition = AnimateObjectBasedOnPosition.None;
    public float objectMinDistance = 3;
    public float objectMaxDistance = 20;
    float distanceLerpCalc = 1;
    float playerDistanceToObject;
   public bool isCustomInstance; // for the spawner object, after certain amount of objects spawned i use shared materials;
    
    public bool useSharedMaterial, rebuildSubstanceImmediately, cacheAtStartup, animateOutputParameters;
    public enum MySubstanceProcessorUsage { Unsupported, One, Half, All };
    public MySubstanceProcessorUsage mySubstanceProcessorUsage;
    public enum MyProceduralCacheSize { Medium = 0, Heavy = 1, None = 2, NoLimit = 3, Tiny = 4 };
    public MyProceduralCacheSize myProceduralCacheSize;
    public float LodNearDistance = 10;
    public float LodMidDistance = 20;
    public float LodFarDistance = 30;
    public bool deleteOldListValuesOnStart;

    public bool flickerEnabled, flickerFloatToggle, flickerColor3Toggle, flickerColor4Toggle, flickerVector2Toggle, flickerVector3Toggle, flickerVector4Toggle, flickerEmissionToggle;
    public float flickerMin = 0.2f, flickerMax = 1.0f;
    float flickerCalc = 1, flickerFloatCalc = 1, flickerColor3Calc = 1, flickerColor4Calc = 1, flickerVector2Calc = 1, flickerVector3Calc = 1, flickerVector4Calc = 1, flickerEmissionCalc = 1;
    
    public Renderer rend;
    public ProceduralMaterial substance;
    public int keyFrames, currentKeyframeIndex;
    public float currentAnimationTime;
    public List<float> keyFrameTimes = new List<float>();
    [HideInInspector]
    public List<float> keyFrameTimesOriginal = new List<float>();
    public List<MaterialVariableListHolder> MaterialVariableKeyframeList = new List<MaterialVariableListHolder>();
    [SerializeField]
    public List<MaterialVariableDictionaryHolder> MaterialVariableKeyframeDictionaryList = new List<MaterialVariableDictionaryHolder>();
    float currentKeyframeAnimationTime;
    [HideInInspector]
    public float lerp = 0, curveFloat = 0, animationTimeRestartEnd = 0, keyframeSum = 0;
    public Color emissionInput;
    public Vector2 originalOutputSize; // texture size when starting
    public List<string> animatedParameterNames = new List<string>();

    // Use this for initialization
    public void Awake()
    {
        if (MaterialVariableKeyframeDictionaryList.Count > 0)
            MaterialVariableKeyframeDictionaryList.Clear();
        ConvertAnimatedListToDictionaryandSet();
    }
    void Start()
    {
        if (substance)
        {
            materialVariables = substance.GetProceduralPropertyDescriptions();
            if (substance.HasProceduralProperty("$outputsize"))
            {
               
                originalOutputSize = substance.GetProceduralVector("$outputsize");
            }
        }
        emissionInput = rend.sharedMaterial.GetColor("_EmissionColor");
        rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
        //if (GameObject.FindGameObjectWithTag("Player") != null)
        customObject = GameObject.FindGameObjectWithTag("Player");
        if (animationEndKeyframe == 0)
            animationEndKeyframe = keyFrameTimes.Count;
        currentKeyframeIndex = animationStartKeyframe;
        animationTimeRestartEnd = CalculateAnimationStartTime(animationStartKeyframe);
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

    void Update()
    {
        if (animationToggle && !isCustomInstance)
        {
            substance.cacheSize = (ProceduralCacheSize)myProceduralCacheSize;
            if (flickerEnabled)
                ChangeFlicker();
            currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
            if (animationType == AnimationType.BackAndForth && animateBackwards && currentKeyframeIndex <= animationEndKeyframe && currentKeyframeIndex > 0)
                currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex - 1];

            if (animateObjectBasedOnPosition != AnimateObjectBasedOnPosition.None && customObject)
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
                if (keyFrameTimes.Count > 2 && currentAnimationTime > currentKeyframeAnimationTime && currentKeyframeIndex <= animationEndKeyframe-2)
                {
                    currentAnimationTime = 0;
                    currentKeyframeIndex++;
                }
                else if (keyFrameTimes.Count > 2 && currentKeyframeIndex >= animationEndKeyframe -1  )
                {
                    currentAnimationTime = 0;
                    animationTimeRestartEnd = CalculateAnimationStartTime(animationStartKeyframe);
                    currentKeyframeIndex = animationStartKeyframe;
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
                if (currentKeyframeIndex <= animationEndKeyframe - 1)
                {
                    lerp = Mathf.InverseLerp(prefabAnimationCurve.keys[currentKeyframeIndex].time, prefabAnimationCurve.keys[currentKeyframeIndex + 1].time, curveFloat);
                }
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
                if (keyFrameTimes.Count > 2 && (animationEndKeyframe - 1) - animationStartKeyframe > 1 && !animateBackwards && currentAnimationTime > currentKeyframeAnimationTime && currentKeyframeIndex < animationEndKeyframe) // reach next keyframe when going forwards
                {
                    if (currentKeyframeIndex == animationEndKeyframe - 2)
                        animateBackwards = true;
                    else
                    {
                        currentKeyframeIndex++;
                        currentAnimationTime = 0;
                    }
                }
                else if (keyFrameTimes.Count > 2 && animateBackwards && currentAnimationTime <= 0 && currentKeyframeIndex <= animationEndKeyframe -1 && currentKeyframeIndex > animationStartKeyframe) // reach next keyframe when going backwards
                {
                    currentAnimationTime = currentKeyframeAnimationTime;
                    currentKeyframeIndex--;
                    curveFloat = prefabAnimationCurve.Evaluate(animationTimeRestartEnd);
                    lerp = (Mathf.Lerp(1, 0, Mathf.InverseLerp(prefabAnimationCurve.keys[currentKeyframeIndex + 1].time, prefabAnimationCurve.keys[currentKeyframeIndex].time, curveFloat)));
                }
                else if ((keyFrameTimes.Count == 2 || (animationEndKeyframe - 1) - animationStartKeyframe == 1) )
                {
                    animateBackwards = true;
                    currentAnimationTime = currentKeyframeAnimationTime;
                }
                if (animateBackwards && currentKeyframeIndex == animationStartKeyframe && currentAnimationTime <= 0) // if you reach the last keyframe when going backwards go forwards.
                {
                    animateBackwards = false;
                    currentAnimationTime = 0;
                    animationTimeRestartEnd = CalculateAnimationStartTime(animationStartKeyframe);
                    if (playOnce)
                        animationToggle = false;
                }
                if (!animateBackwards && currentKeyframeIndex < animationEndKeyframe - 1)
                {
                    lerp = Mathf.InverseLerp(prefabAnimationCurve.keys[currentKeyframeIndex].time, prefabAnimationCurve.keys[currentKeyframeIndex + 1].time, curveFloat);
                }
                else if (keyFrameTimes.Count > 2 && (animationEndKeyframe - 1) - animationStartKeyframe > 1 && animateBackwards && currentAnimationTime != currentKeyframeAnimationTime && currentKeyframeIndex != animationEndKeyframe - 1)
                {
                    lerp = (Mathf.Lerp(1, 0, Mathf.InverseLerp(prefabAnimationCurve.keys[currentKeyframeIndex + 1].time, prefabAnimationCurve.keys[currentKeyframeIndex].time, curveFloat)));
                }
                else if ((keyFrameTimes.Count == 2 || (animationEndKeyframe - 1) - animationStartKeyframe == 1) && animateBackwards && currentAnimationTime != currentKeyframeAnimationTime)
                {
                    lerp = curveFloat / currentKeyframeAnimationTime;
                }
            }

            if (materialVariables != null)
            {
                if (playerDistanceToObject <= LodFarDistance)
                {
                    for (int i = 0; i < animatedMaterialVariables.Count; i++)
                    {
                        ProceduralPropertyDescription materialVariable = animatedMaterialVariables[i];
                        ProceduralPropertyType propType = animatedMaterialVariables[i].type;
                        switch (propType)
                        {
                            case ProceduralPropertyType.Float:
                                {
                                    if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (materialVariable.name[0] != '$' || (materialVariable.name[0] == '$' && animateOutputParameters)))
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
                                    if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (materialVariable.name[0] != '$' || (materialVariable.name[0] == '$' && animateOutputParameters)))
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
                                        substance.SetProceduralBoolean(materialVariable.name, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyBoolDictionary[materialVariable.name])  ;
                                    }
                                }
                                break;
                        }
                    }
                }
                if (rend.sharedMaterial.HasProperty("_EmissionColor") && currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                {
                    rend.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].emissionColor, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].emissionColor, lerp * flickerEmissionCalc));
                }
                if (rend.sharedMaterial.HasProperty("_MainTex") && currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                {
                        rend.sharedMaterial.SetTextureOffset("_MainTex", Vector2.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].MainTex, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].MainTex, lerp * flickerCalc));
                }

                if (playerDistanceToObject <= LodNearDistance)
                    substance.SetProceduralVector("$outputsize", originalOutputSize);
                else if (playerDistanceToObject >= LodNearDistance && playerDistanceToObject <= LodMidDistance)
                    substance.SetProceduralVector("$outputsize", new Vector2(originalOutputSize.x - 1, originalOutputSize.y - 1));
                else if (playerDistanceToObject >= LodMidDistance && playerDistanceToObject <= LodFarDistance)
                    substance.SetProceduralVector("$outputsize", new Vector2(originalOutputSize.x - 2, originalOutputSize.y - 2));
                if ( !rebuildSubstanceImmediately && (animateObjectBasedOnPosition == AnimateObjectBasedOnPosition.None || animateObjectBasedOnPosition != AnimateObjectBasedOnPosition.None && rend.isVisible))
                    substance.RebuildTextures();
                else if (rebuildSubstanceImmediately && (animateObjectBasedOnPosition == AnimateObjectBasedOnPosition.None || animateObjectBasedOnPosition != AnimateObjectBasedOnPosition.None && rend.isVisible))
                    substance.RebuildTexturesImmediately();
            }
        }
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
            if (flickerFloatToggle || flickerColor3Toggle || flickerColor4Toggle || flickerVector2Toggle || flickerVector3Toggle || flickerVector4Toggle || flickerEmissionToggle)
                flickerCalc = UnityEngine.Random.Range(flickerMin, flickerMax);
            if (flickerEmissionToggle)
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
        if (loop)
        {
            for (int i = 0; i < animationStartKeyframe; i++)
            {
                timeCalc += keyFrameTimes[i];
            }
        }
        else // back and forth is selected as the animationType
        {
            for (int i = 0; i < animationEndKeyframe; i++)
            {
                timeCalc += keyFrameTimes[i];
            }

            for (int i = animationEndKeyframe; i > animationStartKeyframe; i++)
            {
                timeCalc += keyFrameTimes[i];
            }
        }
        return timeCalc;
    }

    public float CalculateAnimationStartTime(int index)
    {
        float startTimeCalc = 0;
        for (int i = 0; i < index; i++)
        {
            startTimeCalc += keyFrameTimes[i];
        }
        return startTimeCalc;
    }

    public void ConvertAnimatedListToDictionaryandSet()
    {
        if (gameObject.GetComponent<PrefabProperties>().enabled)
        {
            rend = GetComponent<Renderer>();
#if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject != null && UnityEditor.Selection.activeGameObject.GetComponent<SubstanceTool>() != null)
            useSharedMaterial = true;
            if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
            {
                Debug.Log ("object is in project view");
                return;
            }
#endif
            if (useSharedMaterial && this.GetComponent<Renderer>() != null)
                substance = rend.sharedMaterial as ProceduralMaterial;
            else if (!useSharedMaterial && this.GetComponent<Renderer>() != null)
                substance = rend.material as ProceduralMaterial;
            else
                Debug.LogWarning("No Renderer on " + this.name);
            if (useSharedMaterial)
                emissionInput = rend.sharedMaterial.GetColor("_EmissionColor");
            else
                emissionInput = rend.material.GetColor("_EmissionColor");
            if (substance)
            {
                substance.cacheSize = (ProceduralCacheSize)myProceduralCacheSize;
                ProceduralMaterial.substanceProcessorUsage = (ProceduralProcessorUsage)mySubstanceProcessorUsage;
                substance.enableInstancing = true;
                materialVariables = substance.GetProceduralPropertyDescriptions();
                if (MaterialVariableKeyframeList.Count > 0)
                {
                    for (int i = 0; i <= MaterialVariableKeyframeList.Count - 1; i++)
                    {
                        int index;
                        MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                        MaterialVariableKeyframeDictionaryList[i].PropertyMaterialName = MaterialVariableKeyframeList[0].PropertyMaterialName;
                        for (int j = 0; j < materialVariables.Length; j++)
                        {
                            ProceduralPropertyDescription materialVariable = materialVariables[j];
                            ProceduralPropertyType propType = materialVariables[j].type;

                            if (i == 0 && animatedParameterNames.Contains(materialVariable.name))
                                substance.CacheProceduralProperty(materialVariable.name, true);
                            if (propType == ProceduralPropertyType.Float)
                            {
                                if (MaterialVariableKeyframeList[i].myFloatKeys.Contains(materialVariable.name))
                                {
                                    if (animatedParameterNames.Contains(materialVariable.name))
                                    {
                                        index = MaterialVariableKeyframeList[i].myFloatKeys.IndexOf(materialVariable.name);
                                        float curFloat = MaterialVariableKeyframeList[i].myFloatValues[index];
                                        MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curFloat);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyFloatDictionary.Add(materialVariable.name, curFloat);
                                        if (i == 0)
                                            animatedMaterialVariables.Add(materialVariable);
                                    }
                                    if (i == 0)
                                    {
                                        substance.SetProceduralFloat(materialVariable.name, MaterialVariableKeyframeList[0].myFloatValues[MaterialVariableKeyframeList[0].myFloatKeys.IndexOf(materialVariable.name)]);
                                    }
                                }
                            }
                            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
                            {
                                if (MaterialVariableKeyframeList[i].myColorKeys.Contains(materialVariable.name))
                                {
                                    if (animatedParameterNames.Contains(materialVariable.name))
                                    {
                                        index = MaterialVariableKeyframeList[i].myColorKeys.IndexOf(materialVariable.name);
                                        Color curColor = MaterialVariableKeyframeList[i].myColorValues[index];
                                        MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curColor);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyColorDictionary.Add(materialVariable.name, curColor);
                                        if (i == 0)
                                            animatedMaterialVariables.Add(materialVariable);
                                    }
                                    if (i == 0)
                                    {
                                        substance.SetProceduralColor(materialVariable.name, MaterialVariableKeyframeList[0].myColorValues[MaterialVariableKeyframeList[0].myColorKeys.IndexOf(materialVariable.name)]);
                                    }
                                }
                            }
                            else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
                            {
                                if (propType == ProceduralPropertyType.Vector4)
                                {
                                    if (MaterialVariableKeyframeList[i].myVector4Keys.Contains(materialVariable.name))
                                    {
                                        if (animatedParameterNames.Contains(materialVariable.name))
                                        {
                                            index = MaterialVariableKeyframeList[i].myVector4Keys.IndexOf(materialVariable.name);
                                            Vector4 curVector4 = MaterialVariableKeyframeList[i].myVector4Values[index];
                                            MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curVector4);
                                            MaterialVariableKeyframeDictionaryList[i].PropertyVector4Dictionary.Add(materialVariable.name, curVector4);
                                            if (i == 0)
                                                animatedMaterialVariables.Add(materialVariable);
                                        }
                                    }
                                    if (i == 0)
                                    {
                                        substance.SetProceduralVector(materialVariable.name, MaterialVariableKeyframeList[0].myVector4Values[MaterialVariableKeyframeList[0].myVector4Keys.IndexOf(materialVariable.name)]);
                                    }
                                }
                                else if (propType == ProceduralPropertyType.Vector3)
                                {
                                    if (MaterialVariableKeyframeList[i].myVector3Keys.Contains(materialVariable.name))
                                    {
                                        if (animatedParameterNames.Contains(materialVariable.name))
                                        {
                                            index = MaterialVariableKeyframeList[i].myVector3Keys.IndexOf(materialVariable.name);
                                            Vector3 curVector3 = MaterialVariableKeyframeList[i].myVector3Values[index];
                                            MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curVector3);
                                            MaterialVariableKeyframeDictionaryList[i].PropertyVector3Dictionary.Add(materialVariable.name, curVector3);
                                            if (i == 0)
                                                animatedMaterialVariables.Add(materialVariable);
                                        }
                                    }
                                    if (i == 0)
                                    {
                                        substance.SetProceduralVector(materialVariable.name, MaterialVariableKeyframeList[0].myVector3Values[MaterialVariableKeyframeList[0].myVector3Keys.IndexOf(materialVariable.name)]);
                                    }
                                }
                                else if (propType == ProceduralPropertyType.Vector2)
                                {
                                    if (MaterialVariableKeyframeList[i].myVector2Keys.Contains(materialVariable.name))
                                    {
                                        if (animatedParameterNames.Contains(materialVariable.name))
                                        {
                                            index = MaterialVariableKeyframeList[i].myVector2Keys.IndexOf(materialVariable.name);
                                            Vector2 curVector2 = MaterialVariableKeyframeList[i].myVector2Values[index];
                                            MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curVector2);
                                            MaterialVariableKeyframeDictionaryList[i].PropertyVector2Dictionary.Add(materialVariable.name, curVector2);
                                            if (i == 0)
                                                animatedMaterialVariables.Add(materialVariable);
                                        }
                                    }
                                    if (i == 0)
                                    {
                                        substance.SetProceduralVector(materialVariable.name, MaterialVariableKeyframeList[0].myVector2Values[MaterialVariableKeyframeList[0].myVector2Keys.IndexOf(materialVariable.name)]);
                                    }
                                }
                            }
                            else if (propType == ProceduralPropertyType.Enum)
                            {
                                if (MaterialVariableKeyframeList[i].myEnumKeys.Contains(materialVariable.name))
                                {
                                    if (animatedParameterNames.Contains(materialVariable.name))
                                    {
                                        index = MaterialVariableKeyframeList[i].myEnumKeys.IndexOf(materialVariable.name);
                                        int curEnum = MaterialVariableKeyframeList[i].myEnumValues[index];
                                        MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curEnum);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyEnumDictionary.Add(materialVariable.name, curEnum);
                                        if (i == 0)
                                            animatedMaterialVariables.Add(materialVariable);
                                    }
                                }
                                if (i == 0)
                                {
                                    substance.SetProceduralEnum(materialVariable.name, MaterialVariableKeyframeList[0].myEnumValues[MaterialVariableKeyframeList[0].myEnumKeys.IndexOf(materialVariable.name)]);
                                }
                            }
                            else if (propType == ProceduralPropertyType.Boolean)
                            {
                                if (MaterialVariableKeyframeList[i].myBooleanKeys.Contains(materialVariable.name))
                                {
                                    if (animatedParameterNames.Contains(materialVariable.name))
                                    {
                                        index = MaterialVariableKeyframeList[i].myBooleanKeys.IndexOf(materialVariable.name);
                                        bool curBool = MaterialVariableKeyframeList[i].myBooleanValues[index];
                                        MaterialVariableKeyframeDictionaryList[i].PropertyDictionary.Add(materialVariable.name, curBool);
                                        MaterialVariableKeyframeDictionaryList[i].PropertyBoolDictionary.Add(materialVariable.name, curBool);
                                        if (i == 0)
                                            animatedMaterialVariables.Add(materialVariable);
                                    }
                                }
                                if (i == 0)
                                {
                                    substance.SetProceduralBoolean(materialVariable.name, MaterialVariableKeyframeList[0].myBooleanValues[MaterialVariableKeyframeList[0].myBooleanKeys.IndexOf(materialVariable.name)]);
                                }
                            }
                        }
                        MaterialVariableKeyframeDictionaryList[i].MainTex = MaterialVariableKeyframeList[i].MainTex;
                        MaterialVariableKeyframeDictionaryList[i].emissionColor = MaterialVariableKeyframeList[i].emissionColor;
                    }
                }
            }
            if (deleteOldListValuesOnStart)
                MaterialVariableKeyframeList.Clear();
        }
    }
}