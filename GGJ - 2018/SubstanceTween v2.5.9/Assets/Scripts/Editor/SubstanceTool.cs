using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;
[CustomEditor(typeof(MeshRenderer))]
[CanEditMultipleObjects]
public class SubstanceTool : Editor
{
    //SubstanceTween Ver 2.5 - 10/21/2011
    //Written by: Chris Dougherty
    //https://www.linkedin.com/in/archarchaic
    //chris.ll.dougherty@gmail.com
    //https://www.artstation.com/artist/archarchaic
    public SubstanceToolWindow window;
    private GameObject currentSelection;
    public Renderer rend;
    public bool UpdatingStartVariables = true, saveDefaultSubstanceVars = true, rebuildSubstanceImmediately, cacheSubstance = true, gameIsPaused, substanceLerp, saveOutputParameters = true, animateOutputParameters = true, resettingValuesToDefault = true, showEnumDropdown, showKeyframes, reWriteAllKeyframeTimes, curveDebugFoldout, emissionFlickerFoldout, animateBackwards;
    public ProceduralMaterial substance, defaultSubstance;
    private ProceduralPropertyDescription[] materialVariables;
    public List<ProceduralPropertyDescription> animatedMaterialVariables = new List<ProceduralPropertyDescription>();
    MaterialVariableListHolder xmlDescription, jsonDescription;
    public List<MaterialVariableDictionaryHolder> MaterialVariableKeyframeDictionaryList = new List<MaterialVariableDictionaryHolder>();
    public List<MaterialVariableListHolder> MaterialVariableKeyframeList = new List<MaterialVariableListHolder>();
    public List<MaterialVariableListHolder> defaultSubstanceObjProperties = new List<MaterialVariableListHolder>();
    public List<float> defaultMatFloatValues = new List<float>();
    public List<Vector2> defaultMatVector2Values = new List<Vector2>();
    public List<Vector3> defaultMatVector3Values = new List<Vector3>();
    public List<Vector4> defaultMatVector4Values = new List<Vector4>();
    public List<Color> defaultMatColorValues = new List<Color>();
    public List<string> DebugStrings = new List<string>();
    public List<ProceduralMaterial> selectedStartupMaterials = new List<ProceduralMaterial>();
    public List<GameObject> selectedStartupGameObjects = new List<GameObject>();
    public List<float> keyFrameTimes = new List<float>();
    public Color emissionInput;
    public Vector2 scrollVal, MainTexOffset;
    public int defaultSubstanceIndex, keyFrames, currentKeyframeIndex;
    public float defaultAnimationTime = 5, currentAnimationTime = 0, lerp, lerpCalc, keyframeSum = 0, curveFloat, animationTimeRestartEnd, currentKeyframeAnimationTime;
    public enum AnimationType { Loop, BackAndForth };
    public AnimationType animationType;
    public enum MySubstanceProcessorUsage { Unsupported, One, Half, All };
    public MySubstanceProcessorUsage mySubstanceProcessorUsage;
    public enum MyProceduralCacheSize { Medium = 0, Heavy = 1, None = 2, NoLimit = 3, Tiny = 4 };
    public MyProceduralCacheSize myProceduralCacheSize;
    public AnimationCurve substanceCurve = AnimationCurve.Linear(0, 0, 0, 0);
    public AnimationCurve substanceCurveBackup = AnimationCurve.Linear(0, 0, 0, 0);

    public bool flickerEnabled, flickerToggle, flickerFloatToggle, flickerColor3Toggle, flickerColor4Toggle, flickerVector2Toggle, flickerVector3Toggle, flickerVector4Toggle, flickerEmissionToggle, flickerBoolToggle;
    public float flickerCalc = 1, flickerMin = 0.2f, flickerMax = 1.0f, flickerEmissionCalc = 1, flickerFloatCalc = 1, flickerColor3Calc = 1, flickerColor4Calc = 1, flickerVector2Calc = 1, flickerVector3Calc = 1, flickerVector4Calc = 1;
    public int flickerBoolCalc;
    public string substanceAssetName;
    public int[] textureSizeValues = { 3, 4, 5, 6, 7, 8, 9, 10 };
    public string[] textureSizeStrings = { "16", "32", "64", "128", "256", "512", "1024", "2048" };
    public int substanceHeight;
    public int substanceWidth;
    public int substanceTextureFormat;
    public int substanceLoadBehavior;
    SubstanceImporter substanceImporter;
    public bool ProcessorUsageAllMenuToggle = true, ProcessorUsageHalfMenuToggle, ProcessorUsageOneMenuToggle, ProcessorUsageUnsupportedMenuToggle;
    public bool ProceduralCacheSizeNoLimitMenuToggle = true, ProceduralCacheSizeHeavyMenuToggle, ProceduralCacheSizeMediumMenuToggle, ProceduralCacheSizeTinyMenuToggle, ProceduralCacheSizeNoneMenuToggle;
    public bool randomizeProceduralFloat = true, randomizeProceduralColorRGB = true, randomizeProceduralColorRGBA = true, randomizeProceduralVector2 = true, randomizeProceduralVector3 = true, randomizeProceduralVector4 = true, randomizeProceduralEnum = true, randomizeProceduralBoolean = true;
    public bool showVariableInformationToggle = false;
    public float desiredAnimationTime;
    public float totalAnimationLength;
    public string lastAction;
    bool chooseOverwriteVariables;
    bool saveOverwriteVariables;
    public List<ProceduralPropertyDescription> variablesToOverwrite = new List<ProceduralPropertyDescription>();
    public Dictionary<string, string> VariableValuesToOverwrite = new Dictionary<string, string>();
    public PrefabProperties prefabScript;
    bool mousePressedInCurveEditor = false;
    int objectsSelected = 0;
    public bool gamePlayingOnSelectionChange;
    public GUIContent generateAllOutputsContent = new GUIContent("Generate all outputs", "Force the generation of all Substance outputs.");
    public GUIContent mipmapContent = new GUIContent("Generate Mip Maps");
    public bool generateAllOutputsBool;
    GUIStyle toolbarStyle ;
    GUIStyle toolbarDropdownStyle;
    GUIStyle toolbarButtonStyle;
    GUIStyle toolbarPopupStyle ;
    GUIStyle styleAnimationButton;
    GUIStyle rightAnchorTextStyle;
    GUIStyle styleMainHeader;
    GUIStyle errorTextStyle;

    //[MenuItem("Window/SubstanceTween")]

    public void OnEnable()
    {
        if (EditorApplication.isPlaying && currentSelection == null && Selection.activeGameObject && Selection.activeGameObject.GetComponent<Renderer>() && Selection.activeGameObject.GetComponent<Renderer>().sharedMaterial && Selection.activeGameObject.activeSelf)
        {
            toolbarStyle = new GUIStyle(EditorStyles.toolbar);
           toolbarDropdownStyle = new GUIStyle(EditorStyles.toolbarDropDown);
            toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
        toolbarPopupStyle = new GUIStyle(EditorStyles.toolbarPopup);
           styleAnimationButton = new GUIStyle(EditorStyles.toolbarButton);
            rightAnchorTextStyle = new GUIStyle();
            rightAnchorTextStyle.alignment = TextAnchor.MiddleRight;

            styleMainHeader = new GUIStyle();
            styleMainHeader.fontSize = 25;
            styleMainHeader.alignment = TextAnchor.UpperCenter;
#if UNITY_PRO_LICENSE
            styleMainHeader.normal.textColor = Color.white;
#endif
            errorTextStyle = new GUIStyle();
            errorTextStyle.fontSize = 15;
            errorTextStyle.wordWrap = true;

            Debug.Log("Enable");
            objectsSelected++;
            Debug.Log(objectsSelected);

            if (currentSelection != Selection.activeGameObject)
                Debug.Log(currentSelection + " " + Selection.activeGameObject.name);

            currentSelection = Selection.activeGameObject;
            if (currentSelection)
                rend = currentSelection.GetComponent<Renderer>();
            if (currentSelection.GetComponent<PrefabProperties>() != null && currentSelection.GetComponent<PrefabProperties>().isActiveAndEnabled) // if object has the predab script get all of the keyframe/animation information from that script
            {
                prefabScript = currentSelection.GetComponent<PrefabProperties>();
                rend = prefabScript.rend;
                substance = rend.sharedMaterial as ProceduralMaterial;
                if (substance)
                {
                    materialVariables = substance.GetProceduralPropertyDescriptions();
                }
                prefabScript.useSharedMaterial = true;
                prefabScript.animateBasedOnTime = false;
                prefabScript.animateOnStart = false;
                prefabScript.animationToggle = false;
                keyFrameTimes = prefabScript.keyFrameTimes;
                keyFrames = keyFrameTimes.Count;
                substanceCurve = prefabScript.prefabAnimationCurve;
                substanceCurveBackup = substanceCurve;
                MaterialVariableKeyframeDictionaryList = prefabScript.MaterialVariableKeyframeDictionaryList;
                MaterialVariableKeyframeList = prefabScript.MaterialVariableKeyframeList;

                Debug.Log(MaterialVariableKeyframeDictionaryList.Count());
                Debug.Log(MaterialVariableKeyframeList.Count());

                if (MaterialVariableKeyframeList.Count > 2)
                {
                    CacheProceduralVariables();
                    keyframeSum = 0;
                    for (int i = 0; i <= MaterialVariableKeyframeList.Count() - 1; i++)
                    {
                        keyframeSum += keyFrameTimes[i];
                    }
                }
                if (rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    emissionInput = rend.sharedMaterial.GetColor("_EmissionColor");
                    prefabScript.emissionInput = emissionInput;
                    rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
                }
            }
            if (!Selection.activeGameObject.GetComponent<PrefabProperties>())
            {
                DebugStrings.Add("Opened Tool");
                Selection.activeGameObject.AddComponent<PrefabProperties>();
                prefabScript = Selection.activeGameObject.GetComponent<PrefabProperties>();
                prefabScript.useSharedMaterial = true;
                prefabScript.animateBasedOnTime = false;
                prefabScript.animateOnStart = false;
                prefabScript.animationToggle = false;
                prefabScript.rend = rend;
                substance = rend.sharedMaterial as ProceduralMaterial;
                prefabScript.substance = substance; // Makes it so anytime i edit substance it will affect prefabScript.substance.
                keyFrameTimes = prefabScript.keyFrameTimes;
                prefabScript.keyFrameTimes = keyFrameTimes;
                keyFrames = keyFrameTimes.Count;
                prefabScript.prefabAnimationCurve = new AnimationCurve();
                substanceCurve = prefabScript.prefabAnimationCurve;
                substanceCurveBackup = substanceCurve;
                MaterialVariableKeyframeDictionaryList = prefabScript.MaterialVariableKeyframeDictionaryList;
                MaterialVariableKeyframeList = prefabScript.MaterialVariableKeyframeList;
                myProceduralCacheSize = (MyProceduralCacheSize)ProceduralCacheSize.NoLimit;
                if (substance)
                    substance.cacheSize = ProceduralCacheSize.NoLimit;
                mySubstanceProcessorUsage = (MySubstanceProcessorUsage)ProceduralProcessorUsage.All;
                ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;
                if (rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    emissionInput = rend.sharedMaterial.GetColor("_EmissionColor");
                    prefabScript.emissionInput = emissionInput;
                    Debug.Log(emissionInput);
                    rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
                }
                UpdatingStartVariables = false;
                selectedStartupMaterials.Add(substance); // First selected object material 
                selectedStartupGameObjects.Add(currentSelection); // First selected game object
                DebugStrings.Add("First object selected: " + currentSelection + " Selected objects material name: " + rend.sharedMaterial.name);
            }

            if (substance)
            {
                substance.RebuildTextures();
                substanceAssetName = AssetDatabase.GetAssetPath(substance);
                if (substanceAssetName != String.Empty)
                {
                    substanceImporter = AssetImporter.GetAtPath(substanceAssetName) as SubstanceImporter;
                    substanceImporter.GetPlatformTextureSettings(substanceAssetName, "", out substanceWidth, out substanceHeight, out substanceTextureFormat, out substanceLoadBehavior);
                }
                substanceHeight = 8; // 8 is 512x512 by default if the right output settings are chosen in substance designer.
                substanceWidth = 8;
                myProceduralCacheSize = MyProceduralCacheSize.NoLimit;
                substance.cacheSize = ProceduralCacheSize.NoLimit;
                CalculateAnimationLength();
            }
            ResetProceduralValuesOnUndo();
            Repaint();
            EditorApplication.update += Update; // allows me to use Update() as it appears by default in a MonoBehavior script.
        }
    }

    // All toggles and settings for the menu bar.
    public void SaveOutputMenuFunction()
    {
        if (saveOutputParameters)
            saveOutputParameters = false;
        else
            saveOutputParameters = true;
    }

    public void AnimateOutputParamMenuFunction()
    {
        if (animateOutputParameters)
            animateOutputParameters = false;
        else
            animateOutputParameters = true;
    }

    public void RebuildImmediatelyMenuFunction()
    {
        if (rebuildSubstanceImmediately)
            rebuildSubstanceImmediately = false;
        else
            rebuildSubstanceImmediately = true;
    }
    public void CacheMaterialMenuFunction()
    {
        if (cacheSubstance)
            cacheSubstance = false;
        else
            cacheSubstance = true;
    }
    public void ProcessorUsageAllMenuFunction()
    {
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;
        mySubstanceProcessorUsage = (MySubstanceProcessorUsage)ProceduralProcessorUsage.All;
        ProcessorUsageAllMenuToggle = true;
        ProcessorUsageHalfMenuToggle = false;
        ProcessorUsageOneMenuToggle = false;
        ProcessorUsageUnsupportedMenuToggle = false;
    }
    public void ProcessorUsageOneMenuFunction()
    {
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.One;
        mySubstanceProcessorUsage = (MySubstanceProcessorUsage)ProceduralProcessorUsage.One;
        ProcessorUsageOneMenuToggle = true;
        ProcessorUsageAllMenuToggle = false;
        ProcessorUsageHalfMenuToggle = false;
        ProcessorUsageUnsupportedMenuToggle = false;
    }
    public void ProcessorUsageHalfMenuFunction()
    {
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.Half;
        mySubstanceProcessorUsage = (MySubstanceProcessorUsage)ProceduralProcessorUsage.Half;
        ProcessorUsageHalfMenuToggle = true;
        ProcessorUsageOneMenuToggle = false;
        ProcessorUsageAllMenuToggle = false;
        ProcessorUsageUnsupportedMenuToggle = false;
    }
    public void ProcessorUsageUnsupportedMenuFunction()
    {
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.Unsupported;
        mySubstanceProcessorUsage = (MySubstanceProcessorUsage)ProceduralProcessorUsage.Unsupported;
        ProcessorUsageUnsupportedMenuToggle = true;
        ProcessorUsageHalfMenuToggle = false;
        ProcessorUsageOneMenuToggle = false;
        ProcessorUsageAllMenuToggle = false;
    }
    public void ProceduralCacheSizeNoLimitMenuFunction()
    {
        myProceduralCacheSize = MyProceduralCacheSize.NoLimit;
        substance.cacheSize = ProceduralCacheSize.NoLimit;
        ProceduralCacheSizeNoLimitMenuToggle = true;
        ProceduralCacheSizeHeavyMenuToggle = false;
        ProceduralCacheSizeMediumMenuToggle = false;
        ProceduralCacheSizeTinyMenuToggle = false;
        ProceduralCacheSizeNoneMenuToggle = false;
    }
    public void ProceduralCacheSizeHeavyMenuFunction()
    {
        myProceduralCacheSize = MyProceduralCacheSize.Heavy;
        substance.cacheSize = ProceduralCacheSize.Heavy;
        ProceduralCacheSizeNoLimitMenuToggle = false;
        ProceduralCacheSizeHeavyMenuToggle = true;
        ProceduralCacheSizeMediumMenuToggle = false;
        ProceduralCacheSizeTinyMenuToggle = false;
        ProceduralCacheSizeNoneMenuToggle = false;
    }
    public void ProceduralCacheSizeMediumMenuFunction()
    {
        myProceduralCacheSize = MyProceduralCacheSize.Medium;
        substance.cacheSize = ProceduralCacheSize.Medium;
        ProceduralCacheSizeNoLimitMenuToggle = false;
        ProceduralCacheSizeHeavyMenuToggle = false;
        ProceduralCacheSizeMediumMenuToggle = true;
        ProceduralCacheSizeTinyMenuToggle = false;
        ProceduralCacheSizeNoneMenuToggle = false;
    }
    public void ProceduralCacheSizeTinyMenuFunction()
    {
        myProceduralCacheSize = MyProceduralCacheSize.Tiny;
        substance.cacheSize = ProceduralCacheSize.Tiny;
        ProceduralCacheSizeNoLimitMenuToggle = false;
        ProceduralCacheSizeHeavyMenuToggle = false;
        ProceduralCacheSizeMediumMenuToggle = false;
        ProceduralCacheSizeTinyMenuToggle = true;
        ProceduralCacheSizeNoneMenuToggle = false;
    }
    public void ProceduralCacheSizeNoneMenuFunction()
    {
        myProceduralCacheSize = MyProceduralCacheSize.None;
        substance.cacheSize = ProceduralCacheSize.None;
        ProceduralCacheSizeNoLimitMenuToggle = false;
        ProceduralCacheSizeHeavyMenuToggle = false;
        ProceduralCacheSizeMediumMenuToggle = false;
        ProceduralCacheSizeTinyMenuToggle = false;
        ProceduralCacheSizeNoneMenuToggle = true;
    }

    public void RandomizeFloatToggleFunction()
    {
        if (randomizeProceduralFloat)
            randomizeProceduralFloat = false;
        else
            randomizeProceduralFloat = true;
    }
    public void RandomizeColorRGBToggleFunction()
    {
        if (randomizeProceduralColorRGB)
            randomizeProceduralColorRGB = false;
        else
            randomizeProceduralColorRGB = true;
    }
    public void RandomizeColorRGBAToggleFunction()
    {
        if (randomizeProceduralColorRGBA)
            randomizeProceduralColorRGBA = false;
        else
            randomizeProceduralColorRGBA = true;
    }
    public void RandomizeVector2ToggleFunction()
    {
        if (randomizeProceduralVector2)
            randomizeProceduralVector2 = false;
        else
            randomizeProceduralVector2 = true;
    }
    public void RandomizeVector3ToggleFunction()
    {
        if (randomizeProceduralVector3)
            randomizeProceduralVector3 = false;
        else
            randomizeProceduralVector3 = true;
    }
    public void RandomizeVector4ToggleFunction()
    {
        if (randomizeProceduralVector4)
            randomizeProceduralVector4 = false;
        else
            randomizeProceduralVector4 = true;
    }
    public void RandomizeEnumToggleFunction()
    {
        if (randomizeProceduralEnum)
            randomizeProceduralEnum = false;
        else
            randomizeProceduralEnum = true;
    }
    public void RandomizeBooleanToggleFunction()
    {
        if (randomizeProceduralBoolean)
            randomizeProceduralBoolean = false;
        else
            randomizeProceduralBoolean = true;
    }
    public void ShowVariableInformationFunction()
    {
        if (showVariableInformationToggle)
            showVariableInformationToggle = false;
        else
            showVariableInformationToggle = true;
    }
    public void DisplayAboutDialog()
    {
        EditorUtility.DisplayDialog("About", "SubstanceTween Ver 2.5 - 10/21/2017 \n Written by: Chris Dougherty \n  https://www.linkedin.com/in/archarchaic \n chris.ll.dougherty@gmail.com \n https://www.artstation.com/artist/archarchaic \n \n What does this tool do? \n This tool takes exposed parameters from substance designer files(SBAR) and allows you to create multiple key frames by manipulating the exposed Variables, creating transitions and animating them.\n " +
            "You can Write variables to XML files and read from them as well.When you are done creating your animated object you can save the object as a Prefab for future use. \n" +
     "Hotkeys: \n G - Repeat last action \n R = Randomize Variables \n CTRL + Z = Undo \n CTRL + Y = Redo ", "OK");
    }
    // - End of menu toggles and settings

    public override void OnInspectorGUI() // where content gets displayed.
    {
        if (!Application.isPlaying || (Selection.activeGameObject.GetComponent<Renderer>() && Selection.activeGameObject.GetComponent<Renderer>().sharedMaterial.GetType() != typeof(ProceduralMaterial)) || (AssetDatabase.GetAssetPath(Selection.activeObject) != String.Empty && Selection.gameObjects[0].GetComponent<Renderer>() && Selection.gameObjects[0].GetComponent<Renderer>().sharedMaterial.GetType() == typeof(ProceduralMaterial)))
            DrawDefaultInspector();


        EditorGUI.BeginChangeCheck();
        if (Application.isPlaying && Selection.activeGameObject && Selection.activeGameObject.GetComponent<Renderer>() && Selection.activeGameObject.GetComponent<Renderer>().sharedMaterial && Selection.activeGameObject.activeSelf)
        {
            if ((Event.current.type == EventType.ExecuteCommand || Event.current.type == EventType.ValidateCommand) && Event.current.commandName == "UndoRedoPerformed")
            {
                ResetProceduralValuesOnUndo();
            }
            if (Event.current.type != EventType.ExecuteCommand || Event.current.commandName == "PopupMenuChanged" || Event.current.commandName == "ColorPickerChanged" || Event.current.commandName == "DelayedControlShouldCommit") //Drawing GUI while adding keys in the curve editor will give an error
            {
                if (prefabScript && prefabScript.animationToggle)
                    prefabScript.animationToggle = false;
              
                if (MaterialVariableKeyframeList.Count >= 2 && substanceLerp)
                    styleAnimationButton.normal.textColor = Color.green; // green for animating
                else if (MaterialVariableKeyframeList.Count >= 2 && !substanceLerp)
                    styleAnimationButton.normal.textColor = new Color(255, 204, 0); // yellow for pause/standby
                else
                    styleAnimationButton.normal.textColor = Color.red; // red for not ready to animate (needs 2 keyframes)
              
                if (!gameIsPaused && currentSelection != null && rend && substance && AssetDatabase.GetAssetPath(Selection.activeObject) == String.Empty)//Check if you are in play mode, game is not paused, a object is selected and active , it has a renderer and the object is not in the project view
                {
                    GUILayout.BeginHorizontal(toolbarStyle, GUILayout.Width(EditorGUIUtility.currentViewWidth));
                    EditorStyles.toolbar.hover.textColor = Color.red;
                    if (GUILayout.Button("Save-Load", toolbarDropdownStyle))
                    {
                        GenericMenu toolsMenu = new GenericMenu();
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Write XML File"), false, WriteXML);
                        toolsMenu.AddItem(new GUIContent("Read XML File"), false, ReadXML);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Write All Keyframes To XML Files"), false, WriteAllXML);
                        toolsMenu.AddItem(new GUIContent("Create Keyframes From XML Files"), false, ReadAllXML);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Write JSON File"), false, WriteJSON);
                        //toolsMenu.AddItem(new GUIContent("Read JSON File"), false, ReadJSON);
                        toolsMenu.AddSeparator("");
                        //toolsMenu.AddItem(new GUIContent("Write All Keyframes To JSON Files"), false, WriteAllJSON);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Save Prefab"), false, SavePrefab);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Save Debug Text File"), false, WriteDebugText);
                        toolsMenu.DropDown(new Rect(Event.current.mousePosition.x, 0, 0, 16));
                        EditorGUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button("Set-Modify", toolbarDropdownStyle))
                    {
                        GenericMenu toolsMenu = new GenericMenu();
                        GUILayout.BeginHorizontal(EditorStyles.toolbar);
                        EditorStyles.toolbar.hover.textColor = Color.red;
                        toolsMenu.AddItem(new GUIContent("Set All Procedural Values To Minimum"), false, SetAllProceduralValuesToMin);
                        toolsMenu.AddItem(new GUIContent("Set All Procedural Values To Maximum"), false, SetAllProceduralValuesToMax);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Randomize All Procedural Values(R)"), false, RandomizeProceduralValues);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Floats"), randomizeProceduralFloat, RandomizeFloatToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize RGB Colors"), randomizeProceduralColorRGB, RandomizeColorRGBToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Vector2"), randomizeProceduralVector2, RandomizeVector2ToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Vector3"), randomizeProceduralVector3, RandomizeVector3ToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Vector4"), randomizeProceduralVector4, RandomizeVector4ToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Enums"), randomizeProceduralEnum, RandomizeEnumToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Booleans"), randomizeProceduralBoolean, RandomizeBooleanToggleFunction);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Reset All Procedural Values To Default"), false, ResetAllProceduralValues);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("DELETE ALL KEYFRAMES"), false, DeleteAllKeyframes);
                        toolsMenu.DropDown(new Rect(Event.current.mousePosition.x, 0, 0, 16));
                    }
                    if (GUILayout.Button("Performance-Misc", toolbarDropdownStyle))
                    {
                        GenericMenu toolsMenu = new GenericMenu();
                        toolsMenu.AddItem(new GUIContent("About"), false, DisplayAboutDialog);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Save Output Paramaters($randomSeed-$outputSize)"), saveOutputParameters, SaveOutputMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Animate Output Paramaters($randomSeed-$outputSize)"), animateOutputParameters, AnimateOutputParamMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Rebuild Substance Immediately(SLOW)"), rebuildSubstanceImmediately, RebuildImmediatelyMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Cache Substance"), cacheSubstance, CacheMaterialMenuFunction);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Procedural Processor Usage/All"), ProcessorUsageAllMenuToggle, ProcessorUsageAllMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Processor Usage/Half"), ProcessorUsageHalfMenuToggle, ProcessorUsageHalfMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Processor Usage/One"), ProcessorUsageOneMenuToggle, ProcessorUsageOneMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Processor Usage/Unsupported"), ProcessorUsageUnsupportedMenuToggle, ProcessorUsageUnsupportedMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/No Limit"), ProceduralCacheSizeNoLimitMenuToggle, ProceduralCacheSizeNoLimitMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/Heavy"), ProceduralCacheSizeHeavyMenuToggle, ProceduralCacheSizeHeavyMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/Medium"), ProceduralCacheSizeMediumMenuToggle, ProceduralCacheSizeMediumMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/Tiny"), ProceduralCacheSizeTinyMenuToggle, ProceduralCacheSizeTinyMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/None"), ProceduralCacheSizeNoneMenuToggle, ProceduralCacheSizeNoneMenuFunction);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Show Variable Information"), showVariableInformationToggle, ShowVariableInformationFunction);
                        toolsMenu.DropDown(new Rect(Event.current.mousePosition.x, 0, 0, 16));
                        EditorGUIUtility.ExitGUI();
                    }
                    if (!gameIsPaused)
                    {
                        if (GUILayout.Button("Toggle Animation On/Off", styleAnimationButton)) //Pauses-Unpauses animation
                        {
                            if (currentSelection.GetComponent<PrefabProperties>() != null)
                                ToggleAnimation(prefabScript.MaterialVariableKeyframeDictionaryList);
                            else
                                ToggleAnimation(MaterialVariableKeyframeDictionaryList);
                        }
                    }
                    else if (gameIsPaused)
                        EditorGUILayout.LabelField("GAME IS PAUSED", styleAnimationButton);
                    if (GUILayout.Button("Create keyframe: " + (keyFrameTimes.Count + 1), toolbarButtonStyle))
                    {
                        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Create Keyframe " + MaterialVariableKeyframeDictionaryList.Count().ToString());
                        CreateKeyframe(MaterialVariableKeyframeDictionaryList, MaterialVariableKeyframeList, keyFrameTimes);
                        EditorGUIUtility.ExitGUI();
                    }
                    EditorGUIUtility.labelWidth = 100f;
                    animationType = (AnimationType)EditorGUILayout.EnumPopup("Animation Type:", animationType, toolbarPopupStyle);
                    EditorGUIUtility.labelWidth = 0;
                    GUILayout.EndHorizontal();

                    EditorGUILayout.BeginVertical();
                    scrollVal = GUILayout.BeginScrollView(scrollVal);
                    EditorGUILayout.LabelField("SubstanceTween", styleMainHeader); EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Currently selected Material:", styleMainHeader); EditorGUILayout.Space();
                    if (substance)
                        EditorGUILayout.LabelField(substance.name, styleMainHeader); EditorGUILayout.Space();
                    if (substance && saveDefaultSubstanceVars) // saves the selected substance variables on start if you need to reset to default
                    {
                        defaultSubstanceObjProperties.Add(new MaterialVariableListHolder());
                        defaultSubstance = rend.sharedMaterial as ProceduralMaterial;
                        materialVariables = substance.GetProceduralPropertyDescriptions();
                        defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName = defaultSubstance.name;
                        AddProceduralVariablesToList(defaultSubstanceObjProperties[defaultSubstanceIndex]);
                        defaultSubstanceObjProperties[defaultSubstanceIndex].MainTex = MainTexOffset;
                        defaultSubstanceObjProperties[defaultSubstanceIndex].emissionColor = emissionInput;
                        defaultSubstanceIndex++;
                        saveDefaultSubstanceVars = false;
                    }
                    if (substance)//if object has a procedural material, loop through properties and create sliders/fields based on type
                    {
                        materialVariables = substance.GetProceduralPropertyDescriptions();
                        List<string> variableGroups = new List<string>(); // List of different group names
                        for (int i = 0; i < materialVariables.Length; i++)//Finds group names and adds them to a list. if current paramter has no group then display the control
                        {
                            ProceduralPropertyDescription materialVariable = materialVariables[i];
                            if (substance.IsProceduralPropertyVisible(materialVariable.name)) // checks the current variable for any $visibleIf paramters(Set in Substance Designer)
                            {
                                string variableGroupName = materialVariable.group; // Gets the group that the current parameter is in 
                                if (!variableGroups.Contains(variableGroupName) && variableGroupName != String.Empty)
                                    variableGroups.Add(variableGroupName);
                                else if (variableGroupName == String.Empty) // variable has no group so display it 
                                    DisplayControlForParameter(materialVariable);
                            }
                        }
                        foreach (string curGroupName in variableGroups) // Go through each group and display controls by group order
                        {
                            bool showGroup = true;
                            string name = substance.name;
                            string key = name + curGroupName;
                            GUILayout.Space(5f);
                            bool showGroupDropdown = EditorPrefs.GetBool(key, true); // show dropdown for group if is flagged to be visible($visible if in Substance Designer)
                            EditorGUI.BeginChangeCheck();
                            showGroupDropdown = EditorGUILayout.Foldout(showGroupDropdown, curGroupName, true);
                            if (EditorGUI.EndChangeCheck())
                                EditorPrefs.SetBool(key, showGroupDropdown);
                            if (showGroupDropdown)
                            {

                                EditorGUI.indentLevel += 1;
                                if (!substanceLerp || (substanceLerp && animatedMaterialVariables.Count == 0))
                                {
                                    for (int i = 0; i < materialVariables.Length; i++)
                                    {
                                        ProceduralPropertyDescription materialVariable = materialVariables[i];
                                        if (showGroup && materialVariable.group == curGroupName) // if current group can be displayed and the current variable group matches the one in the current  list 
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.Space((float)(EditorGUI.indentLevel * 40));
                                            DisplayControlForParameter(materialVariable);
                                            EditorGUILayout.EndHorizontal();
                                        }
                                    }
                                }
                                else if (substanceLerp)
                                {
                                    for (int i = 0; i < animatedMaterialVariables.Count; i++)
                                    {
                                        ProceduralPropertyDescription animatedMaterialVariable = animatedMaterialVariables[i];
                                        if (showGroup && animatedMaterialVariable.group == curGroupName ) // if current group can be displayed and the current variable group matches the one in the current  list 
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.Space((float)(EditorGUI.indentLevel * 40));
                                            DisplayControlForParameter(animatedMaterialVariable);
                                            EditorGUILayout.EndHorizontal();
                                        }
                                    }
                                }
                                EditorGUI.indentLevel -= 1;
                            }
                        }
                        if (rend && rend.sharedMaterial.HasProperty("_MainTex")) // If object has a Main Texture Offset field
                        {
                            GUILayout.Label("_MainTex");
                            Vector2 oldOffset = MainTexOffset;
                            MainTexOffset.x = EditorGUILayout.Slider(MainTexOffset.x, -10f, 10.0f);
                            MainTexOffset.y = EditorGUILayout.Slider(MainTexOffset.y, -10f, 10.0f);
                            if (MainTexOffset != oldOffset)
                            {
                                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "_MainTex Changed");
                                if (!gameIsPaused && substanceLerp)
                                    rend.sharedMaterial.SetTextureOffset("_MainTex", MainTexOffset);
                                else
                                    rend.sharedMaterial.SetTextureOffset("_MainTex", MainTexOffset);
                                DebugStrings.Add("_MainTex" + " Was " + oldOffset + " is now: " + MainTexOffset);
                                if (chooseOverwriteVariables)
                                {
                                    if (VariableValuesToOverwrite.ContainsKey("_MainTex") == false) //if the value is changed once
                                        VariableValuesToOverwrite.Add("_MainTex", MainTexOffset.ToString());
                                    else // when the value is changed more than once 
                                        VariableValuesToOverwrite["_MainTex"] = MainTexOffset.ToString();
                                }
                            }
                        }
                        if (rend && rend.sharedMaterial.HasProperty("_EmissionColor")) // if object has a field for the EmissionColor
                        {
                            EditorGUI.BeginChangeCheck();
                            GUILayout.Label("Emission:");
                            if (emissionInput != prefabScript.emissionInput) // emission gets reset do default when i add the prefabProperties Script.
                            {
                                emissionInput = prefabScript.emissionInput;
                                rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
                            }
                            emissionInput = EditorGUILayout.ColorField(GUIContent.none, value: emissionInput, showEyedropper: true, showAlpha: true, hdr: true, hdrConfig: null);
                            Color oldEmissionInput = emissionInput;
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Emission Changed");
                                rend.sharedMaterial.EnableKeyword("_EMISSION");
                                rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
                                prefabScript.emissionInput = emissionInput;
                                DebugStrings.Add("_EmissionColor" + " Was " + oldEmissionInput + " is now: " + emissionInput);
                                if (chooseOverwriteVariables)
                                {
                                    if (VariableValuesToOverwrite.ContainsKey("_EmissionColor") == false) //if the value is changed once
                                        VariableValuesToOverwrite.Add("_EmissionColor", "#" + ColorUtility.ToHtmlStringRGBA(emissionInput));
                                    else // when the value is changed more than once 
                                        VariableValuesToOverwrite["_EmissionColor"] = "#" + ColorUtility.ToHtmlStringRGBA(emissionInput);
                                }
                            }
                            flickerEnabled = EditorGUILayout.Toggle("Enable Flicker", flickerEnabled);
                            if (flickerEnabled)
                            {
                                emissionFlickerFoldout = EditorGUILayout.Foldout(emissionFlickerFoldout, "Emission Flicker");
                                EditorGUILayout.LabelField("FlickerCalc: " + flickerCalc);
                                if (flickerMin >= 0 && flickerMin <= 1)
                                    flickerMin = EditorGUILayout.DelayedFloatField(flickerMin);
                                else
                                    flickerMin = EditorGUILayout.DelayedFloatField(1);
                                if (flickerMax >= 0 && flickerMax <= 1)
                                    flickerMax = EditorGUILayout.DelayedFloatField(flickerMax);
                                else
                                    flickerMax = EditorGUILayout.DelayedFloatField(1);
                                flickerFloatToggle = EditorGUILayout.Toggle(" Float Flicker: ", flickerFloatToggle);
                                flickerColor3Toggle = EditorGUILayout.Toggle(" Color3(RGB) Flicker: ", flickerColor3Toggle);
                                flickerColor4Toggle = EditorGUILayout.Toggle(" Color4(RGBA) Flicker: ", flickerColor4Toggle);
                                flickerVector2Toggle = EditorGUILayout.Toggle(" Vector2 Flicker: ", flickerVector2Toggle);
                                flickerVector3Toggle = EditorGUILayout.Toggle(" Vector3 Flicker: ", flickerVector3Toggle);
                                flickerVector4Toggle = EditorGUILayout.Toggle(" Vector4 Flicker: ", flickerVector4Toggle);
                                flickerBoolToggle = EditorGUILayout.Toggle("Boolean Toggle", flickerBoolToggle);
                                flickerEmissionToggle = EditorGUILayout.Toggle(" Emission Flicker: ", flickerEmissionToggle);
                                if (flickerEnabled)
                                    ChangeFlicker();
                            }
                        }

                        if (currentSelection.GetComponent<PrefabProperties>() != null) // Selected object already has a script attached for animating variables
                            DisplayKeyframeControls(prefabScript.MaterialVariableKeyframeDictionaryList, prefabScript.MaterialVariableKeyframeList, prefabScript.keyFrameTimes, prefabScript.prefabAnimationCurve);
                        else
                            DisplayKeyframeControls(MaterialVariableKeyframeDictionaryList, MaterialVariableKeyframeList, keyFrameTimes, substanceCurve);
                        EditorGUI.BeginChangeCheck();

                        if (MaterialVariableKeyframeList.Count >= 2)
                        {
                            GUILayout.Label("Animation Scrub");
                            desiredAnimationTime = GUILayout.HorizontalSlider(animationTimeRestartEnd, 0, totalAnimationLength);
                            if (EditorGUI.EndChangeCheck() && !substanceLerp)
                            {
                                SetProceduralMaterialBasedOnAnimationTime(desiredAnimationTime);
                            }
                            EditorGUILayout.LabelField("Animation length: " + totalAnimationLength.ToString());
                        }

                        if (showVariableInformationToggle)
                        {
                            curveDebugFoldout = EditorGUILayout.Foldout(curveDebugFoldout, "Curve Keyframes"); //Drop down list for animation curve keys
                            if (curveDebugFoldout && substanceCurve.keys.Count() >= 1)
                            {
                                for (int i = 0; i <= substanceCurve.keys.Count() - 1; i++)// list of current keys on curve 
                                {
                                    EditorGUILayout.LabelField("Keyframe: " + i + " Time:" + substanceCurve.keys[i].time + " Value:" + substanceCurve.keys[i].value + " In Tangent: " + substanceCurve.keys[i].inTangent + " Out Tangent: " + substanceCurve.keys[i].outTangent + " Tangent Mode: " + substanceCurve[i].tangentMode);
                                }
                                EditorGUILayout.LabelField("Animation time length: " + keyframeSum);
                            }
                            GUILayout.Label("CacheSize:" + myProceduralCacheSize.ToString());
                            EditorGUILayout.LabelField("Percentage between keyframes: " + lerp.ToString());
                            EditorGUILayout.LabelField("Current animation Time between keyframes: " + currentAnimationTime.ToString());
                            EditorGUILayout.LabelField("Curve keyframe count: " + substanceCurve.keys.Length.ToString());
                            EditorGUILayout.LabelField("Curve backup keyframe count: " + substanceCurveBackup.keys.Length.ToString());
                            EditorGUILayout.LabelField("Curve value " + curveFloat.ToString());
                            EditorGUILayout.LabelField("Current Keyframe Index: " + currentKeyframeIndex.ToString());
                            EditorGUILayout.LabelField("Animation Time between current/next keyframe" + currentKeyframeAnimationTime.ToString());
                            EditorGUILayout.LabelField("Current animation time total: " + animationTimeRestartEnd.ToString());
                            EditorGUILayout.LabelField("Animation length: " + keyframeSum.ToString());
                            EditorGUILayout.LabelField("Animating Backwards?: " + animateBackwards.ToString());
                            EditorGUILayout.LabelField("Keyframe Count: " + keyFrameTimes.Count());
                            EditorGUILayout.LabelField("Overwriting Variables?: " + chooseOverwriteVariables);
                            EditorGUILayout.LabelField("Current Selection: " + currentSelection);
                            EditorGUILayout.LabelField("substance: " + substance);
                            //EditorGUILayout.LabelField("animation rate." + substanceImporter.GetAnimationUpdateRate(substance));
                        }
                        EditorGUILayout.EndScrollView();
                        EditorGUILayout.EndVertical();
                        if (Event.current.keyCode == KeyCode.G && Event.current.type == EventType.KeyUp && lastAction != null) //repeat last action
                        {
                            MethodInfo lastUsedMethod = this.GetType().GetMethod(lastAction, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            Action myAction = (Action)Delegate.CreateDelegate(typeof(Action), this, lastUsedMethod);
                            myAction();
                        }
                        if (Event.current.keyCode == KeyCode.R && Event.current.type == EventType.KeyUp) // randomize values
                            RandomizeProceduralValues();
                    }
                }
                else if (selectedStartupMaterials.Count < 1) // when opening the tool for the first time and not having something thats required.
                {
                    if (!Selection.activeGameObject.activeSelf)
                    {
                        EditorGUILayout.LabelField("Selected object is not visible/enabled");
                        return;
                    }
                    else if (!rend)
                    {
                        EditorGUILayout.LabelField("No renderer!");
                        return;
                    }
                    EditorGUILayout.LabelField("Select a game object in the Hierarchy that has a Substance material first, then select this window.(Make sure that you are in play mode)A", errorTextStyle);
                }
                if (EditorGUI.EndChangeCheck() || substanceLerp)
                {
                    Repaint();
                }
#if UNITY_5_3
			if (substance && !substanceLerp)
				substance.RebuildTexturesImmediately();
#endif
#if UNITY_5_4_OR_NEWER // In Unity 5.4 and above I am able to use RebuildTextures() which is faster but it is not compatible with 5.3 
                if (substance && !substanceLerp)
                    substance.RebuildTextures();
#endif
            }
        }  
    }

   void Update()
    {
        if (EditorApplication.isPlaying)
        {
            if (substance && (substanceLerp) && currentSelection == Selection.activeGameObject)
            {
                currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
                if (animationType == AnimationType.BackAndForth && animateBackwards && currentKeyframeIndex <= keyFrameTimes.Count - 1 && currentKeyframeIndex > 0)
                    currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex - 1];
                if (animationType == AnimationType.Loop)// animate through every keyframe and repeat at the beginning
                {
                    currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
                    if (substanceLerp)
                    {
                        currentAnimationTime += Time.deltaTime;
                        animationTimeRestartEnd += Time.deltaTime;
                    }
                    curveFloat = substanceCurve.Evaluate(animationTimeRestartEnd);
                    if (keyFrameTimes.Count > 2 && currentAnimationTime > currentKeyframeAnimationTime && currentKeyframeIndex <= keyFrameTimes.Count - 3)//goto next keyframe
                    {
                        currentAnimationTime = 0;
                        currentKeyframeIndex++;
                    }
                    else if (keyFrameTimes.Count > 2 && currentKeyframeIndex >= keyFrameTimes.Count - 2 && currentAnimationTime >= currentKeyframeAnimationTime)// reached last keyframe. Reset animation
                    {
                        currentAnimationTime = 0;
                        animationTimeRestartEnd = 0;
                        currentKeyframeIndex = 0;
                        curveFloat = substanceCurve.Evaluate(animationTimeRestartEnd);
                        lerp = Mathf.InverseLerp(substanceCurve.keys[currentKeyframeIndex].time, substanceCurve.keys[currentKeyframeIndex + 1].time, curveFloat);
                    }
                    else if (keyFrameTimes.Count == 2 && currentAnimationTime > currentKeyframeAnimationTime)// If you have 2 keyframes and it reaches the end, reset animation
                    {
                        currentAnimationTime = 0;
                        animationTimeRestartEnd = 0;
                        currentKeyframeIndex = 0;
                    }
                    if (currentKeyframeIndex <= keyFrameTimes.Count - 2)
                    {
                        lerp = Mathf.InverseLerp(substanceCurve.keys[currentKeyframeIndex].time, substanceCurve.keys[currentKeyframeIndex + 1].time, curveFloat);
                    }
                }
                else if (animationType == AnimationType.BackAndForth) //animate through every keyframe and then repeat backwards.
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
                    curveFloat = substanceCurve.Evaluate(animationTimeRestartEnd);
                    if (keyFrameTimes.Count > 2 && !animateBackwards && currentAnimationTime > currentKeyframeAnimationTime && currentKeyframeIndex < keyFrameTimes.Count - 1) // reach next keyframe when going forwards
                    {
                        currentKeyframeIndex++;
                        currentAnimationTime = 0;
                    }
                    else if (keyFrameTimes.Count > 2 && !animateBackwards && currentKeyframeIndex >= keyFrameTimes.Count - 1) // if you reach the last keyframe when going forwards go backwards.
                    {
                        animateBackwards = true;
                        currentAnimationTime = 0;
                    }
                    else if (keyFrameTimes.Count > 2 && animateBackwards && currentAnimationTime <= 0 && currentKeyframeIndex <= keyFrameTimes.Count - 1 && currentKeyframeIndex > 0) // reach next keyframe when going backwards
                    {
                        currentAnimationTime = currentKeyframeAnimationTime;
                        currentKeyframeIndex--;
                        curveFloat = substanceCurve.Evaluate(animationTimeRestartEnd);
                        lerp = (Mathf.Lerp(1, 0, Mathf.InverseLerp(substanceCurve.keys[currentKeyframeIndex + 1].time, substanceCurve.keys[currentKeyframeIndex].time, curveFloat)));
                    }
                    else if (keyFrameTimes.Count == 2 && currentAnimationTime >= currentKeyframeAnimationTime)
                    {
                        animateBackwards = true;
                        currentAnimationTime = currentKeyframeAnimationTime;
                    }
                    if (animateBackwards && currentKeyframeIndex == 0 && currentAnimationTime < 0) // if you reach the last keyframe when going backwards go forwards.
                        animateBackwards = false;
                    if (!animateBackwards && currentKeyframeIndex < keyFrameTimes.Count - 1)
                        lerp = Mathf.InverseLerp(substanceCurve.keys[currentKeyframeIndex].time, substanceCurve.keys[currentKeyframeIndex + 1].time, curveFloat);// curveFloat is lerp% between time on current key and time on next key
                    else if (keyFrameTimes.Count > 2 && animateBackwards && currentAnimationTime != currentKeyframeAnimationTime && currentKeyframeIndex != substanceCurve.keys.Length - 1)
                    {
                        lerp = Mathf.InverseLerp(substanceCurve.keys[currentKeyframeIndex + 1].time, substanceCurve.keys[currentKeyframeIndex].time, curveFloat);
                        lerp = (Mathf.Lerp(1, 0, lerp));
                    }
                    else if (keyFrameTimes.Count == 2 && animateBackwards && currentAnimationTime != currentKeyframeAnimationTime)
                        lerp = curveFloat / currentKeyframeAnimationTime;
                }
                if (materialVariables != null)
                {
                    for (int i = 0; i < animatedMaterialVariables.Count; i++)// search through dictionary for variable names and if they match animate them
                    {
                        ProceduralPropertyDescription animatedMaterialVariable = animatedMaterialVariables[i];
                        ProceduralPropertyType propType = animatedMaterialVariables[i].type;
                        if (propType == ProceduralPropertyType.Float)
                        {
                            if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (animatedMaterialVariable.name[0] != '$' || (animatedMaterialVariable.name[0] == '$' && animateOutputParameters)))
                                substance.SetProceduralFloat(animatedMaterialVariable.name, Mathf.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyFloatDictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyFloatDictionary[animatedMaterialVariable.name], lerp * flickerFloatCalc));
                        }
                        else if (propType == ProceduralPropertyType.Color3)
                        {
                            if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                            {
                                substance.SetProceduralColor(animatedMaterialVariable.name, Color.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyColorDictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyColorDictionary[animatedMaterialVariable.name], lerp * flickerColor3Calc));
                            }
                        }
                        else if (propType == ProceduralPropertyType.Color4)
                        {
                            if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                substance.SetProceduralColor(animatedMaterialVariable.name, Color.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyColorDictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyColorDictionary[animatedMaterialVariable.name], lerp * flickerColor4Calc));
                        }
                        else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
                        {
                            if (propType == ProceduralPropertyType.Vector4)
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                    substance.SetProceduralVector(animatedMaterialVariable.name, Vector4.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector4Dictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyVector4Dictionary[animatedMaterialVariable.name], lerp * flickerVector4Calc));
                            }
                            else if (propType == ProceduralPropertyType.Vector3)
                            {
                                if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                    substance.SetProceduralVector(animatedMaterialVariable.name, Vector3.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector3Dictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyVector3Dictionary[animatedMaterialVariable.name], lerp * flickerVector3Calc));
                            }
                            else if (propType == ProceduralPropertyType.Vector2 && (animatedMaterialVariable.name[0] != '$' || (animatedMaterialVariable.name[0] == '$' && animateOutputParameters)))
                            {
                                if (animatedMaterialVariable.name[0] == '$' && currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                    substance.SetProceduralVector(animatedMaterialVariable.name, Vector2.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector2Dictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyVector2Dictionary[animatedMaterialVariable.name], lerp * flickerVector2Calc));
                            }
                        }
                        else if (propType == ProceduralPropertyType.Enum)
                        {
                            if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (currentAnimationTime == 0 || currentAnimationTime == currentKeyframeAnimationTime))
                                substance.SetProceduralEnum(animatedMaterialVariable.name, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyEnumDictionary[animatedMaterialVariable.name]);
                        }
                        else if (propType == ProceduralPropertyType.Boolean)
                        {
                            if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (currentAnimationTime == 0 || currentAnimationTime == currentKeyframeAnimationTime))
                                substance.SetProceduralBoolean(animatedMaterialVariable.name, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyBoolDictionary[animatedMaterialVariable.name]);
                        }
                    }
                }
                if (rend.sharedMaterial.HasProperty("_EmissionColor") && currentKeyframeIndex + 1 <= MaterialVariableKeyframeList.Count - 1)
                    rend.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].emissionColor, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].emissionColor, lerp * flickerEmissionCalc));
                if (rend.sharedMaterial.HasProperty("_MainTex") && currentKeyframeIndex + 1 <= MaterialVariableKeyframeList.Count - 1)
                    rend.sharedMaterial.SetTextureOffset("_MainTex", Vector2.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].MainTex, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].MainTex, lerp * flickerCalc));
                if (rebuildSubstanceImmediately)
                    substance.RebuildTexturesImmediately();
                else
                    substance.RebuildTextures();
            }
        }
        /* if (EditorApplication.isPaused) // commented for testing - remove comment later
             gameIsPaused = true;
         else
             gameIsPaused = false;
         if (gameIsPaused)
             substanceLerp = false;*/
    }

    void OnValidate() // runs when anything in the inspector/graph changes or the script gets reloaded.
    {
        Debug.Log("Validate");
        if (prefabScript != null)
            prefabScript.useSharedMaterial = true;
        if (MaterialVariableKeyframeList.Count >= 2)
        {
            for (int i = 0; i <= MaterialVariableKeyframeList.Count() - 1; i++)
            {
                if (MaterialVariableKeyframeDictionaryList[i].PropertyName.Count() <= 0)
                    AddProceduralVariablesToDictionaryFromList(MaterialVariableKeyframeDictionaryList[i], MaterialVariableKeyframeList[i]);
            }
        }
        ResetProceduralValuesOnUndo();
        currentAnimationTime = 0;
        currentKeyframeIndex = 0;
        animationTimeRestartEnd = 0;
        CacheProceduralVariables();
    }

    void DisplayControlForParameter(ProceduralPropertyDescription parameter) // sorts parameters by type and displays controls for that type in the GUI
    {
        ProceduralPropertyType propType = parameter.type;
        if (parameter.name[0] == '$')
        {
            if (parameter.name == "$outputsize") //Texture Size
            {
                GUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth));
                GUILayout.Label(parameter.name + '(' + parameter.label + ')', GUILayout.Width(150));
                substanceWidth = (int)substance.GetProceduralVector(parameter.name).x;
                substanceHeight = (int)substance.GetProceduralVector(parameter.name).y;
                Vector2 substanceSize = new Vector2(substanceWidth, substanceHeight);
                Vector2 oldSubstanceSize = new Vector2(substanceWidth, substanceHeight);
                GUILayout.Label("X:", GUILayout.MaxWidth(30));
                substanceSize.x = EditorGUILayout.IntPopup(substanceWidth, textureSizeStrings, textureSizeValues, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(55));
                GUILayout.Label("Y:", GUILayout.MaxWidth(30));
                substanceSize.y = EditorGUILayout.IntPopup(substanceHeight, textureSizeStrings, textureSizeValues, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(55));
                GUILayout.EndHorizontal();
                if (substanceSize != oldSubstanceSize)
                {
                    Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                    substance.SetProceduralVector(parameter.name, substanceSize);
                    if (VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        variablesToOverwrite.Add(parameter);
                        VariableValuesToOverwrite.Add(parameter.name, substanceSize.ToString());
                    }
                    else // when the value is changed more than once 
                        VariableValuesToOverwrite[parameter.name] = substanceSize.ToString(); // change the value of the dictionary element that already exists 
                }
            }
            else if (parameter.name == "$randomseed") // Current Seed value. 
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.90f));
                GUILayout.Label(parameter.name + '(' + parameter.label + ')');
                int randomSeed = (int)substance.GetProceduralFloat(parameter.name);
                int oldRandomSeed = randomSeed;
                randomSeed = EditorGUILayout.IntSlider(randomSeed, 1, 9999);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Randomize Seed"))
                    randomSeed = UnityEngine.Random.Range(0, 9999 + 1);
                if (EditorGUI.EndChangeCheck()) // anytime you change a slider it will save the old/new value to a debug text file that you can create.
                {
                    Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                    substance.SetProceduralFloat(parameter.name, randomSeed);
                    DebugStrings.Add(parameter.name + " Was " + oldRandomSeed + " is now: " + randomSeed);
                    if (VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        variablesToOverwrite.Add(parameter);
                        VariableValuesToOverwrite.Add(parameter.name, randomSeed.ToString());
                    }
                    else // when the value is changed more than once 
                        VariableValuesToOverwrite[parameter.name] = randomSeed.ToString(); // change the value of the dictionary element that already exists 
                }
            }
        }
        else if (propType == ProceduralPropertyType.Float) // Ints are counted as floats.
        {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.90f));
                GUILayout.Label(parameter.name + '(' + parameter.label + ')');
                float propFloat = substance.GetProceduralFloat(parameter.name);
                float oldPropFloat = propFloat;
                propFloat = EditorGUILayout.Slider(substance.GetProceduralFloat(parameter.name), parameter.minimum, parameter.maximum);
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck()) // anytime you change a slider it will save the old/new value to a debug text file that you can create.
                {
                    Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                    substance.SetProceduralFloat(parameter.name, propFloat);
                    DebugStrings.Add(parameter.name + " Was " + oldPropFloat + " is now: " + propFloat);
                    if (chooseOverwriteVariables) // if the user is overwriting values and changed this value
                    {
                        if (VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                        {
                            variablesToOverwrite.Add(parameter);
                            VariableValuesToOverwrite.Add(parameter.name, propFloat.ToString());
                        }
                        else // when the value is changed more than once 
                            VariableValuesToOverwrite[parameter.name] = propFloat.ToString(); // change the value of the dictionary element that already exists 
                    }
                }
        }
        else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label(parameter.name);
            Color colorInput = substance.GetProceduralColor(parameter.name);
            Color oldColorInput = colorInput;
            colorInput = EditorGUILayout.ColorField(colorInput);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                substance.SetProceduralColor(parameter.name, colorInput);
                DebugStrings.Add(parameter.name + " Was " + oldColorInput + " is now: " + colorInput);
                if (chooseOverwriteVariables) // if the user is overwriting values and changed this value
                {
                    if (VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        variablesToOverwrite.Add(parameter);
                        VariableValuesToOverwrite.Add(parameter.name, "#" + ColorUtility.ToHtmlStringRGBA(colorInput));
                    }
                    else // when the value is changed more than once 
                        VariableValuesToOverwrite[parameter.name] = "#" + ColorUtility.ToHtmlStringRGBA(colorInput); // change the value of the dictionary element that already exists 
                }
            }
        }
        else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4))
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.90f));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(parameter.name);
            int vectorComponentAmount = 4;
            if (propType == ProceduralPropertyType.Vector2)
                vectorComponentAmount = 2;
            if (propType == ProceduralPropertyType.Vector3)
                vectorComponentAmount = 3;
            Vector4 inputVector = substance.GetProceduralVector(parameter.name);
            Vector4 oldInputVector = inputVector;
            EditorGUILayout.EndHorizontal();
            int c = 0;
            EditorGUI.indentLevel++;
            while (c < vectorComponentAmount)
            {
                inputVector[c] = EditorGUILayout.Slider(parameter.componentLabels[c], inputVector[c], parameter.minimum, parameter.maximum); c++;
            }
            EditorGUI.indentLevel--;
            if (inputVector != oldInputVector)
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                substance.SetProceduralVector(parameter.name, inputVector);
                DebugStrings.Add(parameter.name + " Was " + oldInputVector + " is now: " + inputVector);
                if (chooseOverwriteVariables) // if the user is overwriting values and changed this value
                {
                    if (VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        variablesToOverwrite.Add(parameter);
                        VariableValuesToOverwrite.Add(parameter.name, inputVector.ToString());
                    }
                    else
                        VariableValuesToOverwrite[parameter.name] = inputVector.ToString();
                }
            }
            EditorGUILayout.EndVertical();
        }
        else if (propType == ProceduralPropertyType.Enum)
        {
            GUILayout.Label(parameter.name);
            int enumInput = substance.GetProceduralEnum(parameter.name);
            int oldEnumInput = enumInput;
            string[] enumOptions = parameter.enumOptions;
            enumInput = EditorGUILayout.Popup(enumInput, enumOptions);
            if (enumInput != oldEnumInput)
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                substance.SetProceduralEnum(parameter.name, enumInput);
                if (chooseOverwriteVariables) // if the user is overwriting values and changed this value
                {
                    if (VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        variablesToOverwrite.Add(parameter);
                        VariableValuesToOverwrite.Add(parameter.name, enumInput.ToString());
                    }
                    else
                        VariableValuesToOverwrite[parameter.name] = enumInput.ToString();
                }
            }
        }
        else if (propType == ProceduralPropertyType.Boolean)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label(parameter.name);
            bool boolInput = substance.GetProceduralBoolean(parameter.name);
            boolInput = EditorGUILayout.Toggle(boolInput);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                substance.SetProceduralBoolean(parameter.name, boolInput);
                if (chooseOverwriteVariables) // if the user is overwriting values and changed this value
                {
                    if (VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed only once
                    {
                        variablesToOverwrite.Add(parameter);
                        VariableValuesToOverwrite.Add(parameter.name, boolInput.ToString());
                    }
                    else
                    {
                        VariableValuesToOverwrite[parameter.name] = boolInput.ToString();
                    }
                }
            }
        }
    }
    void DisplayKeyframeControls(List<MaterialVariableDictionaryHolder> keyframeDictList, List<MaterialVariableListHolder> keyframeList, List<float> keyframeTimeList, AnimationCurve animationCurve)
    { // Show keyframe controls(animation curve, select/remove/overwrite keyframes) in the GUI
        if (keyframeTimeList.Count >= 2)
            animationCurve = EditorGUILayout.CurveField("Animation Time Curve", animationCurve); // field for the animation curve
        if (EditorWindow.focusedWindow && EditorWindow.focusedWindow.ToString() == " (UnityEditor.CurveEditorWindow)") // if you delete or add a keyframe in the curve editor, Reset keys.
        {
            if (animationCurve.keys[0].time != 0 || animationCurve.keys[0].value != 0)
            {
                animationCurve.keys = substanceCurveBackup.keys;
            }
            if (Event.current.button == 1 && !mousePressedInCurveEditor) // If User presses the right click button once (used mouse button because event.current could not catch mouseUp or mouseDown  )
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Edited Curve");
                mousePressedInCurveEditor = true;
            }
            else if (Event.current.button == 0 && Event.current.button != 1 && mousePressedInCurveEditor == true) // if user releases mouse 
            {
                if (animationCurve.keys.Count() == substanceCurveBackup.keys.Count() && Event.current.commandName == String.Empty) // Did not Add/removeKeys 
                {
                    Undo.SetCurrentGroupName("No Change");
                    Debug.Log("Event: " + Event.current.commandName);
                }
                mousePressedInCurveEditor = false;
            }
            if (Event.current.commandName.ToString() == "CurveChanged")
            {
                for (int i = 0; i <= animationCurve.keys.Count() - 1; i++)
                {
                    if (animationCurve.keys[i].value != animationCurve.keys[i].time)// if User moves a key in the curve Editor
                    {
                        animationCurve.MoveKey(i, new Keyframe(animationCurve.keys[i].time, animationCurve.keys[i].time, animationCurve.keys[i].inTangent, animationCurve.keys[i].outTangent));
                        SetProceduralMaterialBasedOnAnimationTime(substanceCurve.keys[i].time);
                        for (int j = 0; j <= animationCurve.keys.Count() - 2; j++) // Rebuild Animation Curve
                        {
                            keyFrameTimes[j] = animationCurve.keys[j + 1].time - animationCurve.keys[j].time;
                            keyframeList[j].animationTime = animationCurve.keys[j + 1].time - animationCurve.keys[j].time;
                        }
                    }
                }
            }
        }
        if (EditorWindow.focusedWindow && EditorWindow.focusedWindow.ToString() == " (UnityEditor.CurveEditorWindow)" && (animationCurve.keys.Count() != substanceCurveBackup.keys.Count() || animationCurve.keys[animationCurve.keys.Count() - 1].time != substanceCurveBackup.keys[substanceCurveBackup.keys.Count() - 1].time) || keyframeTimeList.Count() - animationCurve.keys.Count() > 1) // if you delete or add a keyframe in the curve editor, Reset keys.
        {
            if (animationCurve.keys.Count() <= 1)
                EditorWindow.focusedWindow.Close();
            if (animationCurve.keys.Count() < keyframeTimeList.Count())
            {
                List<float> BackupKeyframePointTimes = new List<float>();
                List<float> CurrentKeyframePointTimes = new List<float>();
                if (animationCurve.keys[0].time != 0)
                    animationCurve.keys[0].time = 0;
                for (int i = 0; i <= substanceCurveBackup.keys.Count() - 1; i++)
                {
                    BackupKeyframePointTimes.Add(substanceCurveBackup.keys[i].time);
                }
                for (int i = 0; i <= animationCurve.keys.Count() - 1; i++)
                {
                    CurrentKeyframePointTimes.Add(animationCurve.keys[i].time);
                }
                for (int i = BackupKeyframePointTimes.Count() - 1; i > 0; i--) // Go through every key in the backup list(Before the keys got deleted) 
                {
                    if (!CurrentKeyframePointTimes.Contains(BackupKeyframePointTimes[i])) // if the current list of curve points does not contain this value(it was deleted in the curve editor) 
                    {
                        //Find the index of the value and delete any information that has not already been deleted in the curve editor
                        keyframeTimeList.RemoveAt(BackupKeyframePointTimes.IndexOf(BackupKeyframePointTimes[i]));
                        keyframeList.RemoveAt(BackupKeyframePointTimes.IndexOf(BackupKeyframePointTimes[i]));
                        keyframeDictList.RemoveAt(BackupKeyframePointTimes.IndexOf(BackupKeyframePointTimes[i]));
                        if (keyFrames > 0)
                            keyFrames--;
                    }
                }
                for (int i = 0; i <= animationCurve.keys.Count() - 2; i++)
                {
                    keyFrameTimes[i] = animationCurve.keys[i + 1].time - animationCurve.keys[i].time;
                    keyframeList[i].animationTime = animationCurve.keys[i + 1].time - animationCurve.keys[i].time;
                }
                substanceCurveBackup.keys = substanceCurve.keys;
            }
            else if (animationCurve.keys.Count() > substanceCurveBackup.keys.Count()) // if keyframe has just been created in  the curve editor
            {
                for (int i = 0; i <= substanceCurveBackup.keys.Count() - 1; i++)
                {
                    if (i <= substanceCurve.keys.Count() - 1 && substanceCurve.keys[i].time != substanceCurveBackup.keys[i].time) // find keyframe that has just been created or deleted in the curve editor
                    {
                        if (animationCurve.keys.Count() > substanceCurveBackup.keys.Count())
                        {
                            substanceCurve.MoveKey(i, new Keyframe(substanceCurve.keys[i].time, substanceCurve.keys[i].time)); // lerp does not work correctly if the keyframe value is different than the time. i cant edit the key so i delete then add a new one
                            SetProceduralMaterialBasedOnAnimationTime(substanceCurve.keys[i].time);
                            InsertKeyframe(i, MaterialVariableKeyframeDictionaryList, MaterialVariableKeyframeList, keyFrameTimes);
                            substanceCurveBackup.keys = substanceCurve.keys;
                            SelectKeyframe(i);
                            currentAnimationTime = 0;
                            return;
                        }
                    }
                }
                substanceCurveBackup.keys = substanceCurve.keys;
            }
        }
        if (keyframeTimeList.Count == 0 && animationCurve.keys.Count() == 0)// make sure there are always two keys on the curve
        {
            animationCurve.AddKey(0, 0);
            animationCurve.AddKey(5, 5);
        }
        showKeyframes = EditorGUILayout.Foldout(showKeyframes, "Keyframes"); // drop down list for keyframes 
        if (showKeyframes && keyframeDictList.Count() >= 1)
        {
            EditorGUILayout.LabelField("Transition Time:",rightAnchorTextStyle);
            for (int i = 0; i <= keyframeTimeList.Count - 1; i++)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth));
                if (!substanceLerp)
                {
                    if (GUILayout.Button("Remove", GUILayout.Width(EditorGUIUtility.currentViewWidth / 10)) && !substanceLerp)
                    {
                        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Remove Keyframe " + i);
                        DeleteKeyframe(i, keyframeDictList, keyframeList, keyframeTimeList, animationCurve);
                        return; // Without returning i get a error when deleting the last keyframe
                    }
                }
                if (GUILayout.Button("Select", GUILayout.Width(EditorGUIUtility.currentViewWidth / 10)))
                {
                    SelectKeyframe(i);
                    keyframeSum = 0;
                    for (int j = 0; j < i; j++)
                    {
                        keyframeSum += keyframeTimeList[j];
                    };
                    currentKeyframeIndex = i;
                    if (i < keyframeTimeList.Count - 1)
                    {
                        currentAnimationTime = 0;
                        animationTimeRestartEnd = keyframeSum;
                    }
                    else
                    {
                        currentAnimationTime = keyframeTimeList[i];
                        animationTimeRestartEnd = keyframeSum;
                    }
                    CalculateAnimationLength();
                    Repaint();
                }
                if (GUILayout.Button("Overwrite", GUILayout.Width(EditorGUIUtility.currentViewWidth / 10)))
                    OverWriteKeyframe(i);
                GUILayout.Label("Keyframe: " + (i + 1), GUILayout.Width(EditorGUIUtility.currentViewWidth / 10));
                if (i != keyframeTimeList.Count - 1 || keyframeTimeList.Count == 1)
                {
                    if (keyframeTimeList[i] > 0) // display float value for the keyframe index. if it changes rebuild the animation curve 
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUIUtility.fieldWidth = 0.01f;
                        EditorGUIUtility.labelWidth = 0.2f;
                        keyframeTimeList[i] = EditorGUILayout.DelayedFloatField(keyframeTimeList[i]);
                        EditorGUIUtility.labelWidth = 175;
                        if (EditorGUI.EndChangeCheck() && keyframeTimeList[i] > 0) // check if a Animation time has changed and if this after first keyframe(0,0), 'i' is index of changed keyframe
                        {
                            List<Keyframe> tmpKeyframeList = animationCurve.keys.ToList();
                            keyframeSum = 0;
                            for (int j = keyframeList.Count() - 1; j > 0; j--)// remove all keys
                            {
                                animationCurve.RemoveKey(j);
                            }
                            for (int j = 0; j < keyframeList.Count(); j++)//rewrite keys with changed times
                            {
                                if (j == 0)
                                    animationCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, 0.25f, tmpKeyframeList[j].outTangent));
                                else
                                    animationCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, tmpKeyframeList[j].inTangent, tmpKeyframeList[j].outTangent));
                                if (reWriteAllKeyframeTimes)
                                {
                                    for (int k = 0; k < keyframeTimeList.Count() - 1; k++)
                                    {
                                        keyframeTimeList[k] = keyframeTimeList[i];
                                        keyframeList[k].animationTime = keyframeTimeList[i];
                                    }
                                    keyframeSum += keyframeTimeList[i];
                                }
                                else
                                    keyframeSum += keyframeTimeList[j];
                            }
                            keyframeList[i].animationTime = keyframeTimeList[i];
                            currentAnimationTime = 0;//Reset animation variables
                            currentKeyframeIndex = 0;
                            animationTimeRestartEnd = 0;
                            keyframeSum -= keyframeTimeList[keyframeTimeList.Count() - 1]; //gets rid of last keyframe time.
                            substanceCurveBackup.keys = animationCurve.keys;
                        }
                    }
                    else // display the first keyframe time
                    {
                        List<Keyframe> tmpKeyframeList = animationCurve.keys.ToList();
                        keyframeTimeList[i] = EditorGUILayout.DelayedFloatField(keyframeList[i].animationTime);
                        keyframeSum = 0;
                        for (int j = keyframeList.Count() - 1; j > 0; j--)// remove all keys
                        {
                            animationCurve.RemoveKey(j);
                        }
                        for (int j = 0; j < keyframeList.Count(); j++)//rewrite keys with changed times
                        {
                            if (j == 0)
                                animationCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, 0.25f, tmpKeyframeList[j].outTangent));
                            else
                                animationCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, tmpKeyframeList[j].inTangent, tmpKeyframeList[j].outTangent));
                            keyframeSum += keyframeTimeList[j];
                        }
                        keyframeList[i].animationTime = keyframeTimeList[i];
                        currentAnimationTime = 0;//Reset animation variables
                        currentKeyframeIndex = 0;
                        animationTimeRestartEnd = 0;
                        keyframeSum -= keyframeTimeList[keyframeTimeList.Count() - 1]; //gets rid of last keyframe time.
                    }
                    CalculateAnimationLength();
                }
                GUILayout.EndHorizontal();
            }
            reWriteAllKeyframeTimes = EditorGUILayout.Toggle("Rewrite All Keyframe Times", reWriteAllKeyframeTimes);
        }
        EditorGUILayout.BeginHorizontal("Button");
        EditorGUI.BeginDisabledGroup(chooseOverwriteVariables);
        if (GUILayout.Button("Choose variables to rewrite all keyframes")) // when this is toggled any variables that changed are added to a list to be overwritten on all keyframes..
        {
            chooseOverwriteVariables = true;
            saveOverwriteVariables = true;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(!saveOverwriteVariables);
        if (GUILayout.Button("Overwrite modified variables to all Keyframes"))// overwrites variables that have changed for all keyframes.
        {
            chooseOverwriteVariables = true;
            for (int i = 0; i <= keyframeList.Count() - 1; i++)
            {
                SelectKeyframe(i);
                foreach (ProceduralPropertyDescription variable in variablesToOverwrite)
                {
                    if (variable.type == ProceduralPropertyType.Float)
                    {
                        string currentVariable;
                        VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        substance.SetProceduralFloat(variable.name, float.Parse(currentVariable));
                    }

                    else if (variable.type == ProceduralPropertyType.Color3 || variable.type == ProceduralPropertyType.Color4)
                    {
                        string currentVariable;
                        Color currentVariableColor;
                        VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        ColorUtility.TryParseHtmlString(currentVariable, out currentVariableColor);
                        substance.SetProceduralColor(variable.name, currentVariableColor);
                    }

                    else if (variable.type == ProceduralPropertyType.Vector2 || variable.type == ProceduralPropertyType.Vector3 || variable.type == ProceduralPropertyType.Vector4)
                    {
                        Vector2 inputVector2;
                        Vector3 inputVector3;
                        Vector4 inputVector4;
                        string currentVariable;
                        VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        if (variable.type == ProceduralPropertyType.Vector2)
                        {
                            inputVector2 = StringToVector(currentVariable, 2);
                            substance.SetProceduralVector(variable.name, inputVector2);
                        }
                        else if (variable.type == ProceduralPropertyType.Vector3)
                        {
                            inputVector3 = StringToVector(currentVariable, 3);
                            substance.SetProceduralVector(variable.name, inputVector3);
                        }
                        else if (variable.type == ProceduralPropertyType.Vector4)
                        {
                            inputVector4 = StringToVector(currentVariable, 4);
                            substance.SetProceduralVector(variable.name, inputVector4);
                        }
                    }
                    else if (variable.type == ProceduralPropertyType.Enum)
                    {
                        string currentVariable;
                        VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        substance.SetProceduralEnum(variable.name, int.Parse(currentVariable));
                    }
                    else if (variable.type == ProceduralPropertyType.Boolean)
                    {
                        string currentVariable;
                        VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        substance.SetProceduralBoolean(variable.name, bool.Parse(currentVariable));
                    }
                    if (variable.ToString() == "$outputsize") //Texture Size
                    {
                        Vector2 substanceSize = StringToVector(VariableValuesToOverwrite[variable.ToString()], 2);
                        substanceWidth = (int)substanceSize.x;
                        substanceHeight = (int)substanceSize.y;
                    }
                }
                if (VariableValuesToOverwrite.ContainsKey("_EmissionColor") && rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    Color OverwrittenEmissionColor;
                    ColorUtility.TryParseHtmlString(VariableValuesToOverwrite["_EmissionColor"], out OverwrittenEmissionColor);
                    emissionInput = OverwrittenEmissionColor; // changes the gui color picker
                    rend.sharedMaterial.SetColor("_EmissionColor", OverwrittenEmissionColor);
                }

                if (VariableValuesToOverwrite.ContainsKey("_MainTex") && rend.sharedMaterial.HasProperty("_MainTex"))
                {
                    Vector2 OverwrittenMainTex = StringToVector(VariableValuesToOverwrite["_MainTex"], 2);
                    MainTexOffset = OverwrittenMainTex;
                    rend.sharedMaterial.SetTextureOffset("_MainTex", MainTexOffset);
                }
                OverWriteKeyframe(i);
            }
            chooseOverwriteVariables = false;
            saveOverwriteVariables = false;
            variablesToOverwrite.Clear();
            VariableValuesToOverwrite.Clear();
            CacheProceduralVariables();
            CalculateAnimationLength();
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Cancel")) // if you decide not to overwrite anything, delete list of variables that has changed.
        {
            chooseOverwriteVariables = false;
            saveOverwriteVariables = false;
            variablesToOverwrite.Clear();
            VariableValuesToOverwrite.Clear();
        }
        EditorGUILayout.EndHorizontal();
    }

    public void SelectKeyframe(int index) // select a keyframe by its number.
    {
        SetProceduralVariablesFromList(MaterialVariableKeyframeList[index]);
        emissionInput = MaterialVariableKeyframeList[index].emissionColor;
        if (rend.sharedMaterial.HasProperty("_EmissionColor"))
        {
            rend.sharedMaterial.SetColor("_EmissionColor", MaterialVariableKeyframeList[index].emissionColor);
            prefabScript.emissionInput = MaterialVariableKeyframeList[index].emissionColor;
        }
        if (rend.sharedMaterial.HasProperty("_MainTex"))
        {
            MainTexOffset = MaterialVariableKeyframeList[index].MainTex;
            rend.sharedMaterial.SetTextureOffset("_MainTex", MaterialVariableKeyframeList[index].MainTex);
        }
    }

    public void OverWriteKeyframe(int index)// Overwrites a keyframe with the current procedural values
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Overwrite keyframe " + index);
        if (keyFrameTimes.Count >= 1)
        {
            if (index >= 0)
            {
                MaterialVariableKeyframeList.Remove(MaterialVariableKeyframeList[index]);
                MaterialVariableKeyframeList.Insert(index, new MaterialVariableListHolder());
                AddProceduralVariablesToList(MaterialVariableKeyframeList[index]);
                MaterialVariableKeyframeDictionaryList.Remove(MaterialVariableKeyframeDictionaryList[index]);
                MaterialVariableKeyframeDictionaryList.Insert(index, new MaterialVariableDictionaryHolder());
                AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[index]);
                DebugStrings.Add("OverWrote Keyframe: " + (index + 1));
                Repaint();
            }
        }
        substanceCurveBackup.keys = substanceCurve.keys;
        CacheProceduralVariables();
    }

    public void CreateKeyframe(List<MaterialVariableDictionaryHolder> keyframeDictList, List<MaterialVariableListHolder> keyframeList, List<float> keyframeTimeList)
    {
        if (keyframeTimeList.Count == 0)
        {
            DebugStrings.Add("Created Keyframe 1:");
            keyframeDictList.Add(new MaterialVariableDictionaryHolder());
            keyframeList.Add(new MaterialVariableListHolder());
            AddProceduralVariablesToList(keyframeList[0]);
            AddProceduralVariablesToDictionary(keyframeDictList[0]);
            keyFrames++;
            keyframeTimeList.Add(keyframeList[0].animationTime);
            substanceCurve.AddKey(new Keyframe(keyframeList[0].animationTime, keyframeList[0].animationTime));
            AnimationUtility.SetKeyLeftTangentMode(substanceCurve, 0, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(substanceCurve, 0, AnimationUtility.TangentMode.Linear);
        }
        else if (keyframeTimeList.Count > 0)
        {
            for (int i = 0; i <= keyframeTimeList.Count - 1; i++)
            {// Goes through each key frame and checks if the keyframe that you are trying to create has the same number of parameters as the rest and if they all save Output parameters or not.
                if (saveOutputParameters && keyframeList[i].hasParametersWithoutRange == false)
                {//Subsance designer can export special properties like '$randomSeed' that can be saved. this checks if you selected to save those objects and turned it off later
                    EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are saving Parameters without a range however keyframe " + (i + 1) + " does " +
                    "not save these variables. To fix this uncheck \"Save Output Parameters\" on this frame and try again or check \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                    return;
                }
                if (!saveOutputParameters && keyframeList[i].hasParametersWithoutRange == true)
                {
                    EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are not saving Parameters without a range however keyframe " + i + " does " +
                    "save these variables. To fix this check \"Save Output Parameters\" on this frame and try again or uncheck \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                    return;
                }
            }
            keyframeList.Add(new MaterialVariableListHolder());
            DebugStrings.Add("Created KeyFrame: " + keyframeList.Count);
            keyframeDictList.Add(new MaterialVariableDictionaryHolder());
            AddProceduralVariablesToList(keyframeList[keyFrames]);
            AddProceduralVariablesToDictionary(keyframeDictList[keyFrames]);
            keyframeTimeList.Add(keyframeList[keyFrames].animationTime);
            keyframeSum = 0;
            for (int i = 0; i < keyframeList.Count() - 1; i++)
            {
                keyframeSum += keyframeList[i].animationTime;
            }
            substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum));
            AnimationUtility.SetKeyLeftTangentMode(substanceCurve, keyFrames, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(substanceCurve, keyFrames, AnimationUtility.TangentMode.Linear);
            keyFrames++;
        }
        substanceCurveBackup.keys = substanceCurve.keys;
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
        CacheProceduralVariables();
        CalculateAnimationLength();
    }

    public void InsertKeyframe(int index, List<MaterialVariableDictionaryHolder> keyframeDictList, List<MaterialVariableListHolder> keyframeList, List<float> keyframeTimeList)
    {
        if (keyframeTimeList.Count > 0)
        {
            for (int i = 0; i <= keyframeTimeList.Count - 1; i++)
            {// Goes through each key frame and checks if the keyframe that you are trying to create has the same number of parameters as the rest and if they all save Output parameters or not.
                if (saveOutputParameters && keyframeList[i].hasParametersWithoutRange == false)
                {//Substance designer can export special properties like '$randomSeed' that can be saved. this checks if you selected to save those objects and turned it off later
                    EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are saving Parameters without a range however keyframe " + (i + 1) + " does " +
                    "not save these variables. To fix this uncheck \"Save Output Parameters\" on this frame and try again or check \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                    return;
                }
                if (!saveOutputParameters && keyframeList[i].hasParametersWithoutRange == true)
                {
                    EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are not saving Parameters without a range however keyframe " + i + " does " +
                    "save these variables. To fix this check \"Save Output Parameters\" on this frame and try again or uncheck \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                    return;
                }
            }
            keyframeList.Insert(index, new MaterialVariableListHolder());
            DebugStrings.Add("Created KeyFrame: " + keyframeList.Count);
            keyframeDictList.Insert(index, new MaterialVariableDictionaryHolder());
            AddProceduralVariablesToList(keyframeList[index]);
            AddProceduralVariablesToDictionary(keyframeDictList[index]);
            keyframeSum = 0;
            for (int i = 0; i < index; i++)
            {
                keyframeSum += keyframeList[i].animationTime;
            }
            if (index >= 1)
            {
                keyframeList[index - 1].animationTime = currentAnimationTime;
                keyframeTimeList[index - 1] = currentAnimationTime;
            }
            else
            {
                keyframeList[index - 1].animationTime = currentAnimationTime;
                keyframeTimeList[index - 1] = currentAnimationTime;
            }
            keyframeList[index].animationTime = keyframeSum - animationTimeRestartEnd;
            keyframeTimeList.Insert(index, keyframeSum - animationTimeRestartEnd); // note: change animation time if inserting keyframe
            keyFrames++;
        }
        substanceCurveBackup.keys = substanceCurve.keys;
        CacheProceduralVariables();
        CalculateAnimationLength();
    }

    public void DeleteKeyframe(int index, List<MaterialVariableDictionaryHolder> keyframeDictList, List<MaterialVariableListHolder> keyframeList, List<float> keyframeTimeList, AnimationCurve animationCurve)
    {
        List<Keyframe> tmpKeyframeList = animationCurve.keys.ToList();
        keyframeTimeList.RemoveAt(index);
        keyframeList.RemoveAt(index);
        keyframeDictList.RemoveAt(index);
        keyframeSum = 0;
        for (int i = keyframeList.Count(); i >= 0; i--)
            animationCurve.RemoveKey(i);
        for (int i = 0; i <= keyframeList.Count() - 1; i++)
        {
            animationCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, tmpKeyframeList[i].inTangent, tmpKeyframeList[i].outTangent));
            keyframeSum += keyframeTimeList[i];
        };
        currentAnimationTime = 0;
        currentKeyframeIndex = 0;
        animationTimeRestartEnd = 0;
        keyframeSum = 0;
        if (keyFrames > 0)
            keyFrames--;
        substanceCurveBackup.keys = animationCurve.keys;
        CacheProceduralVariables();
        CalculateAnimationLength();
        Repaint();
    }

    public void DeleteAllKeyframes()
    {
        if (EditorUtility.DisplayDialog("Place Selection On Surface?",
                "Are you sure you want to DELETE ALL keyframes?", "YES", "NO"))
        {
            substanceLerp = false;
            MaterialVariableKeyframeList.Clear();
            MaterialVariableKeyframeDictionaryList.Clear();
            keyFrameTimes.Clear();
            keyFrames = 0;
            substanceCurve.keys = null;
            substanceCurveBackup.keys = null;
            currentAnimationTime = 0;
            currentKeyframeIndex = 0;
            animationTimeRestartEnd = 0;
            keyframeSum = 0;
            Repaint();
        }
    }
    public void ToggleAnimation(List<MaterialVariableDictionaryHolder> keyframeDictList) //Pause-Play animation
    {
        if (keyframeDictList.Count >= 2 && keyframeDictList[0].PropertyMaterialName == substance.name) // if you have 2 transitions and the name of the selected substance matches the name on keyframe 1
        {
            if (substanceLerp) // pause the animation, Set all values not to cache and clear the list of animated variables, it will be rebuilt when playing the animation 
            {
                MainTexOffset = MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].MainTex;
                substanceLerp = false;
            }
            else if (!substanceLerp)//Play animation, find any variables that change(animated) and set them to cache then add them to a list. 
            {
                CacheProceduralVariables();
                substanceLerp = true;
            }
        }
        else if (keyframeDictList.Count >= 2)
        { // If material names are different 
            DebugStrings.Add("Tried to animate object: " + currentSelection + " but the Transition Material name " + keyframeDictList[0].PropertyMaterialName + " did not match the current Procedural Material name: " + substance.name);
            var renameMaterialOption = EditorUtility.DisplayDialog(
                "error",
                "Transition Material name " + keyframeDictList[0].PropertyMaterialName + " does not match current Procedural Material name: " + substance.name + ". Would you like to rename " + keyframeDictList[0].PropertyMaterialName + " to " + substance.name + "?"
                + " (Only do this if you are sure the materials are the same and only have different names)", "Yes", "No");
            if (renameMaterialOption)
            {
                DebugStrings.Add("Renamed Material: " + keyframeDictList[0].PropertyMaterialName + " To: " + substance.name);
                keyframeDictList[0].PropertyMaterialName = substance.name; // Saves Substance name in keyframe as current substance name
                for (int i = 0; i <= keyframeDictList.Count - 1; i++)
                    keyframeDictList[i].PropertyMaterialName = substance.name;
                substanceLerp = true;
            }
            else
                DebugStrings.Add("Did not rename or take any other action.");
        }
        else
            EditorUtility.DisplayDialog("error", "You do not have two keyframes", "OK");
    }

    public void CalculateAnimationLength()
    {
        if (MaterialVariableKeyframeList.Count() >= 2)
        {
            totalAnimationLength = 0;
            for (int i = 0; i < MaterialVariableKeyframeList.Count() - 1; i++)
            {
                totalAnimationLength += keyFrameTimes[i];
            }
        }
    }

    public void CacheProceduralVariables() //Checks which variables change throughout keyframes and add them to a list that contains animated variables.If cacheSubstance is true it will also set those variables to cache
    {
        if (cacheSubstance && materialVariables != null)
        {
            animatedMaterialVariables.Clear(); // Makes sure if a variable stops being animatable it the list will clear and  refresh later 
            for (int i = 0; i < materialVariables.Length; i++)
            {
                substance.CacheProceduralProperty(materialVariables[i].name, false);// Makes sure if a variable stops being animatable it the list will  refresh 
            }
            if (MaterialVariableKeyframeDictionaryList.Count >= 2)
            {
                for (int i = 0; i < materialVariables.Length; i++)
                {
                    ProceduralPropertyDescription MaterialVariable = materialVariables[i];
                    ProceduralPropertyType propType = materialVariables[i].type;
                    bool varChanged = false;
                    if (propType == ProceduralPropertyType.Float)
                    {
                        float variableAnimationCheck = 0;
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, float> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyFloatDictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
                    {
                        Color variableAnimationCheck = Color.white;
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Color> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyColorDictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Vector2)
                    {
                        Vector2 variableAnimationCheck = new Vector2(0, 0);
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector2> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyVector2Dictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Vector3)
                    {
                        Vector3 variableAnimationCheck = new Vector3(0, 0, 0);
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector3> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyVector3Dictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Vector4)
                    {
                        Vector4 variableAnimationCheck = new Vector4(0, 0, 0, 0);
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector4> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyVector4Dictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Boolean)
                    {
                        bool variableAnimationCheck = false;
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Boolean> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyBoolDictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Enum)
                    {
                        int variableAnimationCheck = 0;
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, int> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyEnumDictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (!cacheSubstance && materialVariables != null) // add variables if the animate but do not cache them.
        {
            animatedMaterialVariables.Clear();
            for (int i = 0; i < materialVariables.Length; i++)
            {
                substance.CacheProceduralProperty(materialVariables[i].name, false);
            }
            if (MaterialVariableKeyframeDictionaryList.Count >= 2)
            {
                for (int i = 0; i < materialVariables.Length; i++)
                {
                    ProceduralPropertyDescription MaterialVariable = materialVariables[i];
                    ProceduralPropertyType propType = materialVariables[i].type;
                    bool varChanged = false;
                    if (propType == ProceduralPropertyType.Float)
                    {
                        float variableAnimationCheck = 0;
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, float> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyFloatDictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
                    {
                        Color variableAnimationCheck = Color.white;
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Color> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyColorDictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Vector2)
                    {
                        Vector2 variableAnimationCheck = new Vector2(0, 0);
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector2> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyVector2Dictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Vector3)
                    {
                        Vector3 variableAnimationCheck = new Vector3(0, 0, 0);
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector3> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyVector3Dictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Vector4)
                    {
                        Vector4 variableAnimationCheck = new Vector4(0, 0, 0, 0);
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector4> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyVector4Dictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Boolean)
                    {
                        bool variableAnimationCheck = false;
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Boolean> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyBoolDictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Enum)
                    {
                        int variableAnimationCheck = 0;
                        for (int j = 0; j <= keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, int> keyValue in MaterialVariableKeyframeDictionaryList[j].PropertyEnumDictionary)
                            {
                                if (!varChanged && keyValue.Key == materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    void WriteXML() // Write current material variables to a XML file.
    {
        StreamWriter writer;
        var path = EditorUtility.SaveFilePanel("Save XML file", "", "", "xml");
        if (path.Length != 0)
        {
            FileInfo fInfo = new FileInfo(path);
            AssetDatabase.Refresh();
            if (!fInfo.Exists)
                writer = fInfo.CreateText();
            else
            {
                writer = fInfo.CreateText(); Debug.Log("Overwriting File");
            }
            xmlDescription = new MaterialVariableListHolder();//Creates empty list to be saved to a XMl file      
            XmlSerializer serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
            AddProceduralVariablesToList(xmlDescription);// Writes current variables to a list to be saved to a XML file
            xmlDescription.PropertyMaterialName = substance.name;
            xmlDescription.emissionColor = emissionInput;
            xmlDescription.MainTex = MainTexOffset;
            serializer.Serialize(writer, xmlDescription); //Writes the xml file using the fileInfo and the list we want to write.
            writer.Close();//Closes the XML writer
            DebugStrings.Add("-----------------------------------");
            DebugStrings.Add("Wrote XML file to: " + fInfo + ", File has: ");
            DebugStrings.Add(xmlDescription.PropertyName.Count + " Total Properties ");
            DebugStrings.Add(xmlDescription.PropertyFloat.Count + " Float Properties");
            DebugStrings.Add(xmlDescription.PropertyColor.Count + " Color Properties");
            DebugStrings.Add(xmlDescription.PropertyVector4.Count + " Vector4 Properties");
            DebugStrings.Add(xmlDescription.PropertyVector3.Count + " Vector3 Properties");
            DebugStrings.Add(xmlDescription.PropertyVector2.Count + " Vector2 Properties");
            DebugStrings.Add(xmlDescription.PropertyEnum.Count + " Enum Properties");
            DebugStrings.Add(xmlDescription.PropertyBool.Count + " Boolean Properties");
            DebugStrings.Add(xmlDescription.myKeys.Count + " Keys");
            DebugStrings.Add(xmlDescription.myValues.Count + " Values");
            DebugStrings.Add("Material Name: " + xmlDescription.PropertyMaterialName);
            DebugStrings.Add("Substance Texture Size: " + substanceWidth + " " + substanceHeight);
            DebugStrings.Add("-----------------------------------");
        }
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    void ReadXML() // Sets current material variables from a XML file without creating a keyframe.
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Read XML File");
        var serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
        string path = EditorUtility.OpenFilePanel("", "", "xml"); // 'Open' Dialog that only accepts XML files
        if (path.Length != 0)
        {
            var stream = new FileStream(path, FileMode.Open);
            if (stream.Length != 0)
            {
                var container = serializer.Deserialize(stream) as MaterialVariableListHolder; //Convert XML to a list
                SetProceduralVariablesFromList(container);//Set current substance variables based on list
                MainTexOffset = container.MainTex;
                Color xmlEmissionColor = new Color(0, 0, 0, 0);
                xmlEmissionColor = container.emissionColor;
                if (rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    emissionInput = xmlEmissionColor;
                    rend.sharedMaterial.SetColor("_EmissionColor", xmlEmissionColor);
                    prefabScript.emissionInput = emissionInput;
                }
                stream.Close();
                DebugStrings.Add("-----------------------------------");
                DebugStrings.Add("Read XML file " + " from: " + stream.Name + ", File has: ");
                if (container.PropertyMaterialName != null)
                    DebugStrings.Add("Material Name: " + container.PropertyMaterialName);
                DebugStrings.Add(container.PropertyName.Count + " Total Properties");
                DebugStrings.Add(container.PropertyFloat.Count + " Float Properties");
                DebugStrings.Add(container.PropertyColor.Count + " Color Properties ");
                DebugStrings.Add(container.PropertyVector4.Count + " Vector4 Properties");
                DebugStrings.Add(container.PropertyVector3.Count + " Vector3 Properties");
                DebugStrings.Add(container.PropertyVector2.Count + " Vector2 Properties");
                DebugStrings.Add(xmlDescription.PropertyEnum.Count + " Enum Properties");
                DebugStrings.Add(xmlDescription.PropertyBool.Count + " Boolean Properties");
                DebugStrings.Add(container.myKeys.Count + " Keys");
                DebugStrings.Add(container.myValues.Count + " Values");
                DebugStrings.Add("_EmissionColor = " + container.emissionColor);
                DebugStrings.Add("_MainTex = " + MainTexOffset);
                DebugStrings.Add("-----------------------------------");
                substance.RebuildTexturesImmediately();
            }
        }
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
        CacheProceduralVariables();
    }

    void WriteAllXML() // Writes each keyframe to a XML file
    {
        int fileNumber = 1;
        StreamWriter writer;
        string path = EditorUtility.SaveFilePanel("Save XML Files", "", "", "xml");
        FileInfo fInfo;
        if (path.Length != 0)
        {
            for (int i = 0; i <= MaterialVariableKeyframeList.Count - 1; i++) // Go through each keyframe
            {
                string[] splitPath = path.Split(new string[] { ".xml" }, System.StringSplitOptions.None); // Splits xml file path before '.xml'
                if (i < 9) // helps for Lexicographical order
                    fInfo = new FileInfo(splitPath[0] + '-' + 0 + fileNumber + ".xml"); //Insert filenumber and add .XML to the extention
                else
                    fInfo = new FileInfo(splitPath[0] + '-' + +fileNumber + ".xml"); //Insert filenumber and add .XML to the extention
                AssetDatabase.Refresh();
                if (!fInfo.Exists) // if file name does not exist
                    writer = fInfo.CreateText();
                else
                {
                    writer = fInfo.CreateText(); Debug.Log("Overwriting File:" + fInfo + " with keyframe " + i);
                }
                xmlDescription = MaterialVariableKeyframeList[i];
                XmlSerializer serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
                xmlDescription.PropertyMaterialName = substance.name;
                if (keyFrameTimes.Count() >= 1)
                {
                    if (xmlDescription.AnimationCurveKeyframeList.Count <= 0)
                    {
                        if (fileNumber == 1) // if fileNumber is one make sure that there are no NAN tangents saved
                            xmlDescription.AnimationCurveKeyframeList.Add(new Keyframe(substanceCurve.keys[i].time, substanceCurve.keys[i].value, 0, substanceCurve.keys[i].outTangent));
                        else
                            xmlDescription.AnimationCurveKeyframeList.Add(substanceCurve.keys[i]);
                    }
                    else
                    {
                        if (fileNumber == 1) // if fileNumber is one make sure that there are no NAN tangents saved
                            xmlDescription.AnimationCurveKeyframeList[0] = (new Keyframe(substanceCurve.keys[i].time, substanceCurve.keys[i].value, 0, substanceCurve.keys[i].outTangent));
                        else
                            xmlDescription.AnimationCurveKeyframeList[0] = (substanceCurve.keys[i]);
                    }
                }
                serializer.Serialize(writer, xmlDescription); // write keyframe to xml
                writer.Close();
                DebugStrings.Add("Wrote  XML file" + fileNumber + " to: " + fInfo);
                fileNumber++;
            }
        }
        DebugStrings.Add(fileNumber + " keyframes saved as XML files"); DebugStrings.Add("-----------------------------------");
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    void ReadAllXML() // Read XML files and create keyframes from them.
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Create Keyframes From XML");
        var serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
        string xmlReadFolderPath = EditorUtility.OpenFolderPanel("Load xml files from folder", "", ""); //Creates 'Open Folder' Dialog
        if (xmlReadFolderPath.Length != 0)
        {
            string[] xmlReadFiles = Directory.GetFiles(xmlReadFolderPath);//array of selected xml file paths
            if (xmlReadFiles.Count() > 0)
            {
                keyFrames = keyFrameTimes.Count();
                foreach (string xmlReadFile in xmlReadFiles) //for each xml file path in the list.
                {
                    if (xmlReadFile.EndsWith(".xml"))
                    {
                        var stream = new FileStream(xmlReadFile, FileMode.Open);// defines how to use the file at the selected path
                        var container = serializer.Deserialize(stream) as MaterialVariableListHolder; //Puts current xml file into a list.
                        SetProceduralVariablesFromList(container);//Sets current substance values from list
                        MainTexOffset = container.MainTex;
                        Color xmlEmissionColor = new Color(0, 0, 0, 0);
                        xmlEmissionColor = container.emissionColor;
                        if (rend.sharedMaterial.HasProperty("_EmissionColor"))
                        {
                            emissionInput = xmlEmissionColor;
                            rend.sharedMaterial.SetColor("_EmissionColor", xmlEmissionColor);
                        }
                        stream.Close();//Close Xml reader
                        substance.RebuildTexturesImmediately();
                        if (keyFrameTimes.Count == 0) // Create keyframe from list containing XML variables
                        {
                            if (substanceCurve.keys.Count() > 0)
                            {
                                substanceCurve.RemoveKey(0);
                                substanceCurve.AddKey(0, 0);
                            }
                            MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                            MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                            MaterialVariableKeyframeList[0] = container;
                            AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[0]);
                            keyFrames++;
                            keyFrameTimes.Add(MaterialVariableKeyframeList[0].animationTime);

                            AnimationUtility.SetKeyBroken(substanceCurve, 0, true);
                            AnimationUtility.SetKeyLeftTangentMode(substanceCurve, 0, AnimationUtility.TangentMode.Linear);
                            AnimationUtility.SetKeyRightTangentMode(substanceCurve, 0, AnimationUtility.TangentMode.Linear);
                        }
                        else if (keyFrameTimes.Count > 0)
                        {
                            MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                            MaterialVariableKeyframeList[keyFrames] = container;
                            MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                            MaterialVariableKeyframeDictionaryList[keyFrames] = new MaterialVariableDictionaryHolder();
                            AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[keyFrames]);
                            keyFrameTimes.Add(container.animationTime);
                            keyframeSum = 0;
                            for (int i = 0; i < MaterialVariableKeyframeList.Count() - 1; i++)  //  -1 to count here 6/10/17
                                keyframeSum += MaterialVariableKeyframeList[i].animationTime;
                            if (container.AnimationCurveKeyframeList.Count() >= 1)
                                substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, container.AnimationCurveKeyframeList[0].inTangent, container.AnimationCurveKeyframeList[0].outTangent));
                            else
                                substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum));
                            if (keyFrames >= 1)
                                AnimationUtility.SetKeyBroken(substanceCurve, keyFrames, true);
                            keyFrames++;
                        }
                    }
                    DebugStrings.Add("Read keyframe from: " + xmlReadFile);
                }
                DebugStrings.Add(keyFrames - 1 + " Keyframes created from XML files ");
                substanceCurveBackup.keys = substanceCurve.keys;
                lastAction = MethodBase.GetCurrentMethod().Name.ToString();
            }
            else
                EditorUtility.DisplayDialog("Empty Folder", "No Files were found in the selected folder", "Ok");
            CacheProceduralVariables();
            CalculateAnimationLength();
        }
    }

    void WriteJSON()
    {
        StreamWriter writer;
        var path = EditorUtility.SaveFilePanel("Save JSON file", "", "", "json");
        if (path.Length != 0)
        {
            FileInfo fInfo = new FileInfo(path);
            AssetDatabase.Refresh();
            if (!fInfo.Exists)
                writer = fInfo.CreateText();
            else
            {
                writer = fInfo.CreateText(); Debug.Log("Overwriting File");
            }
            
            jsonDescription = new MaterialVariableListHolder();//Creates empty list to be saved to a JSON file  
            //XmlSerializer serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
            AddProceduralVariablesToList(jsonDescription);// Writes current variables to a list to be saved to a XML file
            jsonDescription.PropertyMaterialName = substance.name;
            jsonDescription.emissionColor = emissionInput;
            jsonDescription.MainTex = MainTexOffset;
            string json = JsonUtility.ToJson(jsonDescription);

            writer.Write(json);
            writer.Close();//Closes the Json writer
            DebugStrings.Add("-----------------------------------");
            DebugStrings.Add("Wrote JSON file to: " + fInfo + ", File has: ");
            DebugStrings.Add(jsonDescription.PropertyName.Count + " Total Properties ");
            DebugStrings.Add(jsonDescription.PropertyFloat.Count + " Float Properties");
            DebugStrings.Add(jsonDescription.PropertyColor.Count + " Color Properties");
            DebugStrings.Add(jsonDescription.PropertyVector4.Count + " Vector4 Properties");
            DebugStrings.Add(jsonDescription.PropertyVector3.Count + " Vector3 Properties");
            DebugStrings.Add(jsonDescription.PropertyVector2.Count + " Vector2 Properties");
            DebugStrings.Add(jsonDescription.PropertyEnum.Count + " Enum Properties");
            DebugStrings.Add(jsonDescription.PropertyBool.Count + " Boolean Properties");
            DebugStrings.Add(jsonDescription.myKeys.Count + " Keys");
            DebugStrings.Add(jsonDescription.myValues.Count + " Values");
            DebugStrings.Add("Material Name: " + jsonDescription.PropertyMaterialName);
            DebugStrings.Add("Substance Texture Size: " + substanceWidth + " " + substanceHeight);
            DebugStrings.Add("-----------------------------------");
        }
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    /*void ReadJSON() //Note: In development, I get sharing violation errors with File.ReadAllText(path);
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Read XML File");
       
        //var serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
        string path = EditorUtility.OpenFilePanel("", "", "json"); // 'Open' Dialog that only accepts JSON files
        if (path.Length != 0)
        {
            var stream = new FileStream(path, FileMode.Open);
            if (stream.Length != 0)
            {
                //var stream = new FileStream(path, FileMode.Open);
                string dataAsJson = File.ReadAllText(path);
                MaterialVariableListHolder jsonContainer =  JsonUtility.FromJson<MaterialVariableListHolder>(dataAsJson);
                SetProceduralVariablesFromList(jsonContainer);//Set current substance variables based on list
                MainTexOffset = jsonContainer.MainTex;
                Color jsonEmissionColor = new Color(0, 0, 0, 0);
                jsonEmissionColor = jsonContainer.emissionColor;
                if (rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    emissionInput = jsonEmissionColor;
                    rend.sharedMaterial.SetColor("_EmissionColor", jsonEmissionColor);
                    prefabScript.emissionInput = emissionInput;
                }
                stream.Close();
                DebugStrings.Add("-----------------------------------");
                DebugStrings.Add("Read XML file " + " from: " + stream.Name + ", File has: ");
                if (jsonContainer.PropertyMaterialName != null)
                    DebugStrings.Add("Material Name: " + jsonContainer.PropertyMaterialName);
                DebugStrings.Add(jsonContainer.PropertyName.Count + " Total Properties");
                DebugStrings.Add(jsonContainer.PropertyFloat.Count + " Float Properties");
                DebugStrings.Add(jsonContainer.PropertyColor.Count + " Color Properties ");
                DebugStrings.Add(jsonContainer.PropertyVector4.Count + " Vector4 Properties");
                DebugStrings.Add(jsonContainer.PropertyVector3.Count + " Vector3 Properties");
                DebugStrings.Add(jsonContainer.PropertyVector2.Count + " Vector2 Properties");
                DebugStrings.Add(jsonContainer.PropertyEnum.Count + " Enum Properties");
                DebugStrings.Add(jsonContainer.PropertyBool.Count + " Boolean Properties");
                DebugStrings.Add(jsonContainer.myKeys.Count + " Keys");
                DebugStrings.Add(jsonContainer.myValues.Count + " Values");
                DebugStrings.Add("_EmissionColor = " + jsonContainer.emissionColor);
                DebugStrings.Add("_MainTex = " + MainTexOffset);
                DebugStrings.Add("-----------------------------------");
                substance.RebuildTexturesImmediately();
            }
        }
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
        CacheProceduralVariables();
        
    }*/


    /*void WriteAllJSON()
    {
        int fileNumber = 1;
        StreamWriter writer;
        string path = EditorUtility.SaveFilePanel("Save JSON Files", "", "", "json");
        FileInfo fInfo;
        if (path.Length != 0)
        {
            for (int i = 0; i <= MaterialVariableKeyframeList.Count - 1; i++) // Go through each keyframe
            {
                string[] splitPath = path.Split(new string[] { ".json" }, System.StringSplitOptions.None); // Splits json file path before '.json'
                if (i < 9) // helps for Lexicographical order
                    fInfo = new FileInfo(splitPath[0] + '-' + 0 + fileNumber + ".json"); //Insert filenumber and add .json to the extention
                else
                    fInfo = new FileInfo(splitPath[0] + '-' + +fileNumber + ".json"); //Insert filenumber and add .json to the extention
                AssetDatabase.Refresh();
                if (!fInfo.Exists) // if file name does not exist
                    writer = fInfo.CreateText();
                else
                {
                    writer = fInfo.CreateText(); Debug.Log("Overwriting File:" + fInfo + " with keyframe " + i);
                }
                jsonDescription = MaterialVariableKeyframeList[i];

                jsonDescription.PropertyMaterialName = substance.name;
                string json = JsonUtility.ToJson(jsonDescription);
                if (keyFrameTimes.Count() >= 1)
                {
                    if (jsonDescription.AnimationCurveKeyframeList.Count <= 0)
                    {
                        if (fileNumber == 1) // if fileNumber is one make sure that there are no NAN tangents saved
                            jsonDescription.AnimationCurveKeyframeList.Add(new Keyframe(substanceCurve.keys[i].time, substanceCurve.keys[i].value, 0, substanceCurve.keys[i].outTangent));
                        else
                            jsonDescription.AnimationCurveKeyframeList.Add(substanceCurve.keys[i]);
                    }
                    else
                    {
                        if (fileNumber == 1) // if fileNumber is one make sure that there are no NAN tangents saved
                            jsonDescription.AnimationCurveKeyframeList[0] = (new Keyframe(substanceCurve.keys[i].time, substanceCurve.keys[i].value, 0, substanceCurve.keys[i].outTangent));
                        else
                            jsonDescription.AnimationCurveKeyframeList[0] = (substanceCurve.keys[i]);
                    }
                }
                writer.Write(json);
                //serializer.Serialize(writer, xmlDescription); // write keyframe to xml
                writer.Close();
                DebugStrings.Add("Wrote  JSON file" + fileNumber + " to: " + fInfo);
                fileNumber++;
            }
        }
        DebugStrings.Add(fileNumber + " keyframes saved as XML files"); DebugStrings.Add("-----------------------------------");
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }*/

    void SavePrefab() // Saves a Prefab to a specified folder
    {
        string prefabPath = EditorUtility.SaveFilePanelInProject("", "", "prefab", "");
        if (prefabPath.Length != 0)
        {
            GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, currentSelection.gameObject, ReplacePrefabOptions.Default);
            if (prefab.GetComponent<PrefabProperties>() != null)
                DestroyImmediate(prefab.GetComponent<PrefabProperties>(), true);
            prefab.AddComponent<PrefabProperties>(); // adds prefab script to prefab 
            PrefabProperties prefabProperties = prefab.GetComponent<PrefabProperties>();
            prefabProperties.substance = substance;
            Renderer prefabRend = prefab.GetComponent<Renderer>();
            prefabRend.material = substance;
            if (animationType == AnimationType.Loop)
                prefabProperties.animationType = PrefabProperties.AnimationType.Loop;
            if (animationType == AnimationType.BackAndForth)
                prefabProperties.animationType = PrefabProperties.AnimationType.BackAndForth;
            DebugStrings.Add("Created prefab: " + prefab.name);
            if (prefabRend.sharedMaterial)
                DebugStrings.Add("Prefab material: " + prefabRend.sharedMaterial.name);
            if (MaterialVariableKeyframeList.Count >= 1)
            {//writes keyfame lists to prefab(Could not serialize dictionaries)
                for (int i = 0; i <= MaterialVariableKeyframeList.Count - 1; i++)
                {
                    if (prefabProperties.keyFrameTimes.Count == 0)
                    {
                        prefabProperties.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                        prefabProperties.MaterialVariableKeyframeList[0] = MaterialVariableKeyframeList[0];
                        prefabProperties.MaterialVariableKeyframeList[0].PropertyMaterialName = substance.name;
                        prefabProperties.MaterialVariableKeyframeList[0].emissionColor = MaterialVariableKeyframeList[0].emissionColor;
                        prefabProperties.MaterialVariableKeyframeList[0].MainTex = MaterialVariableKeyframeList[prefabProperties.keyFrames].MainTex;
                        prefabProperties.keyFrameTimes.Add(MaterialVariableKeyframeList[0].animationTime);
                        prefabProperties.prefabAnimationCurve = new AnimationCurve();
                        prefabProperties.prefabAnimationCurve.AddKey(new Keyframe(substanceCurve.keys[0].time, substanceCurve.keys[0].value, substanceCurve.keys[0].inTangent, substanceCurve.keys[0].outTangent));
                        AnimationUtility.SetKeyBroken(prefabProperties.prefabAnimationCurve, 0, true);
                        if (MaterialVariableKeyframeList[0].hasParametersWithoutRange)
                            prefabProperties.MaterialVariableKeyframeList[0].hasParametersWithoutRange = true;
                        else
                            prefabProperties.MaterialVariableKeyframeList[0].hasParametersWithoutRange = false;
                        prefabProperties.keyFrames++;
                    }
                    else // After one keyframe is created create the rest
                    {
                        prefabProperties.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                        prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames] = MaterialVariableKeyframeList[prefabProperties.keyFrames];
                        prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].PropertyMaterialName = substance.name;
                        prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].emissionColor = MaterialVariableKeyframeList[prefabProperties.keyFrames].emissionColor;
                        prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].MainTex = MaterialVariableKeyframeList[prefabProperties.keyFrames].MainTex;
                        prefabProperties.keyFrameTimes.Add(MaterialVariableKeyframeList[prefabProperties.keyFrames].animationTime);
                        prefabProperties.prefabAnimationCurve.AddKey(new Keyframe(substanceCurve.keys[prefabProperties.keyFrames].time, substanceCurve.keys[prefabProperties.keyFrames].value, substanceCurve.keys[prefabProperties.keyFrames].inTangent, substanceCurve.keys[prefabProperties.keyFrames].outTangent));
                        if (saveOutputParameters)
                            prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].hasParametersWithoutRange = true;
                        else
                            prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].hasParametersWithoutRange = false;
                        prefabProperties.keyFrames++;
                    }
                }
                prefabProperties.keyFrameTimesOriginal = keyFrameTimes;
                prefabProperties.prefabAnimationCurveBackup = prefabProperties.prefabAnimationCurve;
                if (MaterialVariableKeyframeList.Count >= 2)
                {
                    prefabProperties.animateOnStart = true;
                    prefabProperties.animateBasedOnTime = true;
                }
            }
            if (cacheSubstance)
                prefabProperties.cacheAtStartup = true;
            else
                prefabProperties.cacheAtStartup = false;
            if (animateOutputParameters)
                prefabProperties.animateOutputParameters = true;
            else
                prefabProperties.animateOutputParameters = false;

            if (myProceduralCacheSize.ToString() == ProceduralCacheSize.Heavy.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.Heavy;
            else if (myProceduralCacheSize.ToString() == ProceduralCacheSize.Medium.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.Medium;
            else if (myProceduralCacheSize.ToString() == ProceduralCacheSize.NoLimit.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.NoLimit;
            else if (myProceduralCacheSize.ToString() == ProceduralCacheSize.None.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.None;
            else if (myProceduralCacheSize.ToString() == ProceduralCacheSize.Tiny.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.Tiny;
            if (mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.All.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.All;
            else if (mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.Half.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.Half;
            else if (mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.One.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.One;
            else if (mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.Unsupported.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.Unsupported;
            if (flickerToggle)
            {
                prefabProperties.flickerToggle = true;
                if (flickerFloatToggle)
                    prefabProperties.flickerFloatToggle = true;
                if (flickerColor3Toggle)
                    prefabProperties.flickerColor3Toggle = true;
                if (flickerColor4Toggle)
                    prefabProperties.flickerColor4Toggle = true;
                if (flickerVector2Toggle)
                    prefabProperties.flickerVector2Toggle = true;
                if (flickerVector3Toggle)
                    prefabProperties.flickerVector3Toggle = true;
                if (flickerVector4Toggle)
                    prefabProperties.flickerVector4Toggle = true;
                if (flickerEmissionToggle)
                    prefabProperties.flickerEmissionToggle = true;
                prefabProperties.flickerMin = flickerMin;
                prefabProperties.flickerMax = flickerMax;
            }
#if UNITY_2017_1_OR_NEWER
            prefabRend.sharedMaterial.enableInstancing = true;// enable GPU instancing 
#endif 
            DebugStrings.Add("Prefab Path: " + prefabPath);
            DebugStrings.Add(prefab.name + " has " + prefabProperties.MaterialVariableKeyframeList.Count + " keyframes");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            lastAction = MethodBase.GetCurrentMethod().Name.ToString();
        }
        else
            Debug.LogWarning("No Path/FileName specified for Prefab");
    }
    void WriteDebugText()
    {
        StreamWriter debugWrite;
        var path = EditorUtility.SaveFilePanel("Save Debug Text file", "", "", "txt");
        if (path.Length != 0)
        {
            FileInfo fInfo = new FileInfo(path);
            if (!fInfo.Exists)
                debugWrite = fInfo.CreateText();
            else
            {
                debugWrite = fInfo.CreateText(); Debug.Log("Overwriting File");
            }
            debugWrite.WriteLine(System.DateTime.Now + " Debug:");
            for (int i = 0; i < DebugStrings.Count; i++)
            {
                if (DebugStrings.Count > 1 && i > 1 && DebugStrings[i] != DebugStrings[i - 1])
                    debugWrite.WriteLine(DebugStrings[i]);
            }
            debugWrite.WriteLine("---Variables at current frame:---");
            debugWrite.WriteLine("Lerp: " + lerp.ToString());
            debugWrite.WriteLine("Current Animation Time: " + currentAnimationTime.ToString());
            debugWrite.WriteLine("Curve key count: " + substanceCurve.keys.Length.ToString());
            debugWrite.WriteLine("Curve Float value: " + curveFloat.ToString());
            debugWrite.WriteLine("Current Keyframe Animation Time: " + currentKeyframeAnimationTime.ToString());
            debugWrite.WriteLine("Animate Backwards: " + animateBackwards.ToString());
            debugWrite.WriteLine("Created debug log: " + path);
            debugWrite.Close();
            AssetDatabase.Refresh();
        }
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    public void SetProceduralMaterialBasedOnAnimationTime(float desiredTime)
    {
        for (int i = 0; i <= substanceCurveBackup.keys.Count() - 1; i++)
        {
            if (substanceCurveBackup.keys[i].time > desiredTime) // find first key time that is greater than the desiredAnimationTime 
            {
                float newLerp = (desiredTime - substanceCurveBackup.keys[i - 1].time) / (substanceCurveBackup.keys[i].time - substanceCurveBackup.keys[i - 1].time);// Finds point between two keyrames  - finds percentage of desiredtime between substanceCurveBackup.keys[i - 1].time and substanceCurveBackup.keys[i].time 
                currentAnimationTime = Mathf.Lerp(0, keyFrameTimes[i - 1], newLerp);
                animationTimeRestartEnd = desiredTime;
                currentKeyframeIndex = i-1;
                if (EditorWindow.focusedWindow && EditorWindow.focusedWindow.ToString() != " (UnityEditor.CurveEditorWindow)")
                    lerp = newLerp;
                for (int j = 0; j < animatedMaterialVariables.Count; j++)// search through dictionary for variable names and if they match animate them
                {
                    ProceduralPropertyDescription animatedMaterialVariable = animatedMaterialVariables[j];
                    ProceduralPropertyType propType = animatedMaterialVariables[j].type;
                    if (propType == ProceduralPropertyType.Float)
                        substance.SetProceduralFloat(animatedMaterialVariable.name, Mathf.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].PropertyFloatDictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[i].PropertyFloatDictionary[animatedMaterialVariable.name], newLerp));
                    else if (propType == ProceduralPropertyType.Color3)
                        substance.SetProceduralColor(animatedMaterialVariable.name, Color.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].PropertyColorDictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[i].PropertyColorDictionary[animatedMaterialVariable.name], newLerp * flickerColor3Calc));
                    else if (propType == ProceduralPropertyType.Color4)
                        substance.SetProceduralColor(animatedMaterialVariable.name, Color.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].PropertyColorDictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[i].PropertyColorDictionary[animatedMaterialVariable.name], newLerp * flickerColor4Calc));
                    else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
                    {
                        if (propType == ProceduralPropertyType.Vector4)
                            substance.SetProceduralVector(animatedMaterialVariable.name, Vector4.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].PropertyVector4Dictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[i].PropertyVector4Dictionary[animatedMaterialVariable.name], newLerp * flickerVector4Calc));
                        else if (propType == ProceduralPropertyType.Vector3)
                            substance.SetProceduralVector(animatedMaterialVariable.name, Vector3.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].PropertyVector3Dictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[i].PropertyVector3Dictionary[animatedMaterialVariable.name], newLerp * flickerVector3Calc));
                        else if (propType == ProceduralPropertyType.Vector2)
                            substance.SetProceduralVector(animatedMaterialVariable.name, Vector2.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].PropertyVector2Dictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[i].PropertyVector2Dictionary[animatedMaterialVariable.name], newLerp * flickerVector2Calc));
                    }
                    else if (propType == ProceduralPropertyType.Enum)
                        substance.SetProceduralEnum(animatedMaterialVariable.name, MaterialVariableKeyframeDictionaryList[i - 1].PropertyEnumDictionary[animatedMaterialVariable.name]);
                    else if (propType == ProceduralPropertyType.Boolean)
                        substance.SetProceduralBoolean(animatedMaterialVariable.name, MaterialVariableKeyframeDictionaryList[i - 1].PropertyBoolDictionary[animatedMaterialVariable.name]);
                }
                if (rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    emissionInput = Color.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].emissionColor, MaterialVariableKeyframeDictionaryList[i].emissionColor, newLerp * flickerCalc);
                    rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
                    prefabScript.emissionInput = emissionInput;
                }
                if (rend.sharedMaterial.HasProperty("_MainTex"))
                    rend.sharedMaterial.SetTextureOffset("_MainTex", Vector2.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].MainTex, MaterialVariableKeyframeDictionaryList[i].MainTex, newLerp * flickerCalc));

                substance.RebuildTextures();
                return;
            }
        }
    }

    public void SetAllProceduralValuesToMin() // Sets all procedural values to the minimum value
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Set all values to Minimum");
        for (int i = 0; i < materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = materialVariables[i];
            if (substance.IsProceduralPropertyVisible(materialVariable.name))
            {
                ProceduralPropertyType propType = materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float)
                    substance.SetProceduralFloat(materialVariable.name, materialVariables[i].minimum);
                if (propType == ProceduralPropertyType.Vector2 && materialVariable.hasRange)
                    substance.SetProceduralVector(materialVariable.name, new Vector2(materialVariables[i].minimum, materialVariables[i].minimum));
                if (propType == ProceduralPropertyType.Vector3 && materialVariable.hasRange)
                    substance.SetProceduralVector(materialVariable.name, new Vector3(materialVariables[i].minimum, materialVariables[i].minimum, materialVariables[i].minimum));
                if (propType == ProceduralPropertyType.Vector4 && materialVariable.hasRange)
                    substance.SetProceduralVector(materialVariable.name, new Vector4(materialVariables[i].minimum, materialVariables[i].minimum, materialVariables[i].minimum, materialVariables[i].minimum));
                if (propType == ProceduralPropertyType.Enum)
                    substance.SetProceduralEnum(materialVariable.name, 0);
            }
        }
        DebugStrings.Add("Set all properties to the minimum");
        substance.RebuildTexturesImmediately();
    }

    public void SetAllProceduralValuesToMax() // Sets all procedural values to the maximum value
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Set all values to Maximum");
        for (int i = 0; i < materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = materialVariables[i];
            if (substance.IsProceduralPropertyVisible(materialVariable.name))
            {
                ProceduralPropertyType propType = materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float)
                    substance.SetProceduralFloat(materialVariable.name, materialVariables[i].maximum);
                if (propType == ProceduralPropertyType.Vector2 && materialVariable.hasRange)
                    substance.SetProceduralVector(materialVariable.name, new Vector2(materialVariables[i].maximum, materialVariables[i].maximum));
                if (propType == ProceduralPropertyType.Vector3 && materialVariable.hasRange)
                    substance.SetProceduralVector(materialVariable.name, new Vector3(materialVariables[i].maximum, materialVariables[i].maximum, materialVariables[i].maximum));
                if (propType == ProceduralPropertyType.Vector4 && materialVariable.hasRange)
                    substance.SetProceduralVector(materialVariable.name, new Vector4(materialVariables[i].maximum, materialVariables[i].maximum, materialVariables[i].maximum, materialVariables[i].maximum));
                if (propType == ProceduralPropertyType.Enum)
                    substance.SetProceduralEnum(materialVariable.name, materialVariables[i].enumOptions.Count() - 1);
            }
        }
        DebugStrings.Add("Set all properties to the maximum");
        substance.RebuildTexturesImmediately();
    }

    public void RandomizeProceduralValues() // Sets all procedural values to a random value
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Randomize values");
        for (int i = 0; i < materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = materialVariables[i];
            if (substance.IsProceduralPropertyVisible(materialVariable.name))
            {
                ProceduralPropertyType propType = materialVariables[i].type;
                Debug.Log(materialVariable.name + "  " + propType);
                if (propType == ProceduralPropertyType.Float && materialVariable.name[0] != '$' && randomizeProceduralFloat)
                    substance.SetProceduralFloat(materialVariable.name, UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum));
                if (propType == ProceduralPropertyType.Color3 && randomizeProceduralColorRGB)
                    substance.SetProceduralColor(materialVariable.name, new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)));
                if (propType == ProceduralPropertyType.Color4 && randomizeProceduralColorRGBA)
                    substance.SetProceduralColor(materialVariable.name, new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)));
                if (propType == ProceduralPropertyType.Vector2 && materialVariable.name[0] != '$' && randomizeProceduralVector2)
                    substance.SetProceduralVector(materialVariable.name, new Vector2(UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum), UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum)));
                if (propType == ProceduralPropertyType.Vector3 && randomizeProceduralVector3)
                    substance.SetProceduralVector(materialVariable.name, new Vector3(UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum), UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum), UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum)));
                if (propType == ProceduralPropertyType.Vector4 && randomizeProceduralVector4)
                    substance.SetProceduralVector(materialVariable.name, new Vector4(UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum), UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum), UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum), UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum)));
                if (propType == ProceduralPropertyType.Enum && randomizeProceduralEnum)
                    substance.SetProceduralEnum(materialVariable.name, UnityEngine.Random.Range(0, materialVariables[i].enumOptions.Count()));
                if (propType == ProceduralPropertyType.Boolean && randomizeProceduralBoolean)
                    substance.SetProceduralBoolean(materialVariable.name, (UnityEngine.Random.value > 0.5f));
            }
        }
        DebugStrings.Add("Randomize all properties");
        substance.RebuildTexturesImmediately();
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    public void ResetAllProceduralValues() // Resets all procedural values to default(When the material was first selected)
    {
        for (int i = 0; i < defaultSubstanceObjProperties.Count; i++)
        {
            if ((substance.name == defaultSubstanceObjProperties[i].PropertyMaterialName) || (rend.sharedMaterial.name == defaultSubstanceObjProperties[i].PropertyMaterialName))
            {
                resettingValuesToDefault = true;
                SetProceduralVariablesFromList(defaultSubstanceObjProperties[i]);
                MainTexOffset = defaultSubstanceObjProperties[i].MainTex;
                if (rend.sharedMaterial.HasProperty("_EmissionColor"))
                    emissionInput = defaultSubstanceObjProperties[i].emissionColor;
                DebugStrings.Add("Reset all values to default");
                substance.RebuildTexturesImmediately();
                resettingValuesToDefault = false;
                return;
            }
        }
    }

    void AddProceduralVariablesToList(MaterialVariableListHolder materialVariableList) // Adds current procedural values to a list 
    {
        DebugStrings.Add("----------------------------------");
        for (int i = 0; i < this.materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = this.materialVariables[i];
            ProceduralPropertyType propType = this.materialVariables[i].type;
            if (propType != ProceduralPropertyType.Texture)
                materialVariableList.PropertyName.Add(materialVariable.name);
            if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
            {
                float propFloat = substance.GetProceduralFloat(materialVariable.name);
                materialVariableList.PropertyFloat.Add(propFloat);
                materialVariableList.myFloatKeys.Add(materialVariable.name);
                materialVariableList.myFloatValues.Add(propFloat);
                DebugStrings.Add(i + " " + materialVariable.name + ": " + propFloat.ToString());
            }
            if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                Color propColor = substance.GetProceduralColor(materialVariable.name);
                materialVariableList.PropertyColor.Add(propColor);
                materialVariableList.myColorKeys.Add(materialVariable.name);
                materialVariableList.myColorValues.Add(propColor);
                DebugStrings.Add(i + " " + materialVariable.name + ": #" + ColorUtility.ToHtmlStringRGBA(propColor));
            }
            if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    Vector4 propVector4 = substance.GetProceduralVector(materialVariable.name);
                    materialVariableList.PropertyVector4.Add(propVector4);
                    materialVariableList.myVector2Keys.Add(materialVariable.name);
                    materialVariableList.myVector2Values.Add(propVector4);
                    DebugStrings.Add(i + " " + materialVariable.name + ": " + propVector4.ToString());
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    Vector3 propVector3 = substance.GetProceduralVector(materialVariable.name);
                    materialVariableList.PropertyVector3.Add(propVector3);
                    materialVariableList.myVector3Keys.Add(materialVariable.name);
                    materialVariableList.myVector3Values.Add(propVector3);
                    DebugStrings.Add(i + " " + materialVariable.name + ": " + propVector3.ToString());
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    Vector2 propVector2 = substance.GetProceduralVector(materialVariable.name);
                    materialVariableList.PropertyVector2.Add(propVector2);
                    materialVariableList.myVector4Keys.Add(materialVariable.name);
                    materialVariableList.myVector4Values.Add(propVector2);
                    
                    DebugStrings.Add(i + " " + materialVariable.name + ": " + propVector2.ToString());
                }
            }
            if (propType == ProceduralPropertyType.Enum)
            {
                int propEnum = substance.GetProceduralEnum(materialVariable.name);
                materialVariableList.PropertyEnum.Add(propEnum);
                materialVariableList.myEnumKeys.Add(materialVariable.name);
                materialVariableList.myEnumValues.Add(propEnum);
            }
            if (propType == ProceduralPropertyType.Boolean)
            {
                bool propBool = substance.GetProceduralBoolean(materialVariable.name);
                materialVariableList.PropertyBool.Add(propBool);
                materialVariableList.myBooleanKeys.Add(materialVariable.name);
                materialVariableList.myBooleanValues.Add(propBool);
            }
        }
        materialVariableList.PropertyMaterialName = substance.name;
        materialVariableList.emissionColor = emissionInput;
        materialVariableList.MainTex = MainTexOffset;
        if (saveOutputParameters)
            materialVariableList.hasParametersWithoutRange = true;
        else
            materialVariableList.hasParametersWithoutRange = false;
        materialVariableList.animationTime = defaultAnimationTime;
    }

    void AddProceduralVariablesToDictionary(MaterialVariableDictionaryHolder materialVariableDictionary) // Adds current procedural values to a dictionary 
    {
        DebugStrings.Add("----------------------------------");
        for (int i = 0; i < this.materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = this.materialVariables[i];
            ProceduralPropertyType propType = this.materialVariables[i].type;
            if (propType != ProceduralPropertyType.Texture)
                materialVariableDictionary.PropertyName.Add(materialVariable.name);
            if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
            {
                float propFloat = substance.GetProceduralFloat(materialVariable.name);
                materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propFloat);
                materialVariableDictionary.PropertyFloatDictionary.Add(materialVariable.name, propFloat);
            }
            if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                Color propColor = substance.GetProceduralColor(materialVariable.name);
                materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propColor);
                materialVariableDictionary.PropertyColorDictionary.Add(materialVariable.name, propColor);
            }
            if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    Vector4 propVector4 = substance.GetProceduralVector(materialVariable.name);
                    materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propVector4);
                    materialVariableDictionary.PropertyVector4Dictionary.Add(materialVariable.name, propVector4);
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    Vector3 propVector3 = substance.GetProceduralVector(materialVariable.name);
                    materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propVector3);
                    materialVariableDictionary.PropertyVector3Dictionary.Add(materialVariable.name, propVector3);
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    Vector2 propVector2 = substance.GetProceduralVector(materialVariable.name);
                    materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propVector2);
                    materialVariableDictionary.PropertyVector2Dictionary.Add(materialVariable.name, propVector2);
                }
            }
            if (propType == ProceduralPropertyType.Enum)
            {
                int propEnum = substance.GetProceduralEnum(materialVariable.name);
                materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propEnum);
                materialVariableDictionary.PropertyEnumDictionary.Add(materialVariable.name, propEnum);
            }
            if (propType == ProceduralPropertyType.Boolean)
            {
                bool propBool = substance.GetProceduralBoolean(materialVariable.name);
                materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propBool);
                materialVariableDictionary.PropertyBoolDictionary.Add(materialVariable.name, propBool);
            }
        }
        materialVariableDictionary.PropertyMaterialName = substance.name;
        materialVariableDictionary.emissionColor = emissionInput;
        materialVariableDictionary.MainTex = MainTexOffset;
        if (saveOutputParameters)
            materialVariableDictionary.hasParametersWithoutRange = true;
        else
            materialVariableDictionary.hasParametersWithoutRange = false;
        materialVariableDictionary.animationTime = defaultAnimationTime;
    }

    public void AddProceduralVariablesToDictionaryFromList(MaterialVariableDictionaryHolder dictionary, MaterialVariableListHolder list) // sorts items from a list into a dictionary
    {
        if (substance && materialVariables != null)
        {
            for (int i = 0; i < this.materialVariables.Length; i++)
            {

                ProceduralPropertyDescription materialVariable = this.materialVariables[i];
                ProceduralPropertyType propType = this.materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
                {
                    if (!dictionary.PropertyFloatDictionary.ContainsKey(materialVariable.name))
                    {
                        dictionary.PropertyDictionary.Add(materialVariable.name, list.myFloatValues[list.myFloatKeys.IndexOf(materialVariable.name)]);
                        dictionary.PropertyFloatDictionary.Add(materialVariable.name, list.myFloatValues[list.myFloatKeys.IndexOf(materialVariable.name)]);
                    }
                    else // if it already contains the key overwrite it
                        dictionary.PropertyFloatDictionary[materialVariable.name] = list.myFloatValues[list.myFloatKeys.IndexOf(materialVariable.name)];
                }
                if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
                {
                    Color propColor = list.myColorValues[list.myColorKeys.IndexOf(materialVariable.name)];
                    if (!dictionary.PropertyColorDictionary.ContainsKey(materialVariable.name))
                    {
                        dictionary.PropertyDictionary.Add(materialVariable.name, propColor);
                        dictionary.PropertyColorDictionary.Add(materialVariable.name, propColor);
                    }
                    else
                    {
                        dictionary.PropertyColorDictionary[materialVariable.name] = propColor;
                    }
                }
                if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
                {
                    if (propType == ProceduralPropertyType.Vector4)
                    {
                        Vector4 propVector4 = list.myVector4Values[list.myVector4Keys.IndexOf(materialVariable.name)];
                        if (!dictionary.PropertyVector4Dictionary.ContainsKey(materialVariable.name))
                        {
                            dictionary.PropertyDictionary.Add(materialVariable.name, propVector4);
                            dictionary.PropertyVector4Dictionary.Add(materialVariable.name, propVector4);
                        }
                        else
                            dictionary.PropertyVector4Dictionary[materialVariable.name] = propVector4;
                    }
                    else if (propType == ProceduralPropertyType.Vector3)
                    {
                        Vector3 propVector3 = list.myVector3Values[list.myVector3Keys.IndexOf(materialVariable.name)];
                        if (!dictionary.PropertyVector3Dictionary.ContainsKey(materialVariable.name))
                        {
                            
                            dictionary.PropertyDictionary.Add(materialVariable.name, propVector3);
                            dictionary.PropertyVector3Dictionary.Add(materialVariable.name, propVector3);
                        }
                        else
                            dictionary.PropertyVector3Dictionary[materialVariable.name] = propVector3;
                    }
                    else if (propType == ProceduralPropertyType.Vector2)
                    {
                        Vector2 propVector2 = list.myVector2Values[list.myVector2Keys.IndexOf(materialVariable.name)];
                        if (!dictionary.PropertyVector2Dictionary.ContainsKey(materialVariable.name))
                        {
                            dictionary.PropertyDictionary.Add(materialVariable.name, propVector2);
                            dictionary.PropertyVector2Dictionary.Add(materialVariable.name, propVector2);
                        }
                        else
                            dictionary.PropertyVector2Dictionary[materialVariable.name] = propVector2;
                    }
                }
                if (propType == ProceduralPropertyType.Enum)
                {
                    int propEnum = list.myEnumValues[list.myEnumKeys.IndexOf(materialVariable.name)];
                    if (!dictionary.PropertyEnumDictionary.ContainsKey(materialVariable.name))
                    {
                        dictionary.PropertyDictionary.Add(materialVariable.name, propEnum);
                        dictionary.PropertyEnumDictionary.Add(materialVariable.name, propEnum);
                    }
                    else
                        dictionary.PropertyEnumDictionary[materialVariable.name] = propEnum;
                }

                if (propType == ProceduralPropertyType.Boolean)
                {
                    bool propBool = list.myBooleanValues[list.myBooleanKeys.IndexOf(materialVariable.name)];
                    if (!dictionary.PropertyBoolDictionary.ContainsKey(materialVariable.name))
                    {
                        dictionary.PropertyDictionary.Add(materialVariable.name, propBool);
                        dictionary.PropertyBoolDictionary.Add(materialVariable.name, propBool);
                    }
                    else
                        dictionary.PropertyBoolDictionary[materialVariable.name] = propBool;
                }
            }
            dictionary.MainTex = list.MainTex;
            dictionary.emissionColor = list.emissionColor;
        }
    }

    void SetProceduralVariablesFromList(MaterialVariableListHolder propertyList) // Sets current substance parameters from a List
    {
        for (int i = 0; i < materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = materialVariables[i];
            ProceduralPropertyType propType = materialVariables[i].type;
            if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange) || resettingValuesToDefault))
            {
                if (propertyList.myFloatKeys.Count() > 0)
                {
                    for (int j = 0; j < propertyList.myFloatKeys.Count(); j++)
                    {
                        if (propertyList.myFloatKeys[j] == materialVariable.name)
                        {
                            if (propertyList.myFloatKeys[j] == materialVariable.name)
                                substance.SetProceduralFloat(materialVariable.name, propertyList.myFloatValues[j]);
                        }
                    }
                }
                else //note: for old versions of tool, remove later?
                {
                    for (int j = 0; j < propertyList.myKeys.Count(); j++)
                    {
                        if (propertyList.myKeys[j] == materialVariable.name)
                        {
                            if (propertyList.myKeys[j] == materialVariable.name)
                                substance.SetProceduralFloat(materialVariable.name, float.Parse(propertyList.myValues[j]));
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                if (propertyList.myColorKeys.Count() > 0)
                {
                    for (int j = 0; j < propertyList.myColorKeys.Count(); j++)
                    {
                        if (propertyList.myColorKeys[j] == materialVariable.name)
                        {
                            Color curColor = propertyList.myColorValues[j];
                            substance.SetProceduralColor(materialVariable.name, curColor);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < propertyList.myKeys.Count(); j++)
                    {
                        if (propertyList.myKeys[j] == materialVariable.name)
                        {
                            Color curColor;
                            ColorUtility.TryParseHtmlString(propertyList.myValues[j], out curColor);
                            substance.SetProceduralColor(materialVariable.name, curColor);
                        }
                    }
                }
            }
            else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange) || resettingValuesToDefault))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    if (propertyList.myVector4Keys.Count() > 0)
                    {
                        for (int j = 0; j < propertyList.myVector4Keys.Count(); j++)
                        {
                            if (propertyList.myVector4Keys[j] == materialVariable.name)
                            {
                                Vector4 curVector4 = propertyList.myVector4Values[j];
                                substance.SetProceduralVector(materialVariable.name, curVector4);
                            }
                        }
                    }

                    else
                    {
                        for (int j = 0; j < propertyList.myKeys.Count(); j++)
                        {
                            if (propertyList.myKeys[j] == materialVariable.name)
                            {
                                Vector4 curVector4 = StringToVector(propertyList.myValues[j], 4);
                                substance.SetProceduralVector(materialVariable.name, curVector4);
                            }
                        }
                    }

                }
                else if (propType == ProceduralPropertyType.Vector3)
                {

                    if (propertyList.myVector3Keys.Count() > 0)
                    {
                        for (int j = 0; j < propertyList.myVector3Keys.Count(); j++)
                        {
                            if (propertyList.myVector3Keys[j] == materialVariable.name)
                            {
                                Vector3 curVector3 = propertyList.myVector3Values[j];
                                substance.SetProceduralVector(materialVariable.name, curVector3);
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < propertyList.myKeys.Count(); j++)
                        {
                            if (propertyList.myKeys[j] == materialVariable.name)
                            {
                                Vector3 curVector3 = StringToVector(propertyList.myValues[j], 3);
                                substance.SetProceduralVector(materialVariable.name, curVector3);
                            }
                        }
                    }
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    if (propertyList.myVector3Keys.Count() > 0)
                    {
                        for (int j = 0; j < propertyList.myVector2Keys.Count(); j++)
                        {
                            if (propertyList.myVector2Keys[j] == materialVariable.name)
                            {
                                Vector2 curVector2 = propertyList.myVector2Values[j];
                                substance.SetProceduralVector(materialVariable.name, curVector2);
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < propertyList.myKeys.Count(); j++)
                        {
                            if (propertyList.myKeys[j] == materialVariable.name)
                            {
                                Vector2 curVector2 = StringToVector(propertyList.myValues[j], 2);
                                substance.SetProceduralVector(materialVariable.name, curVector2);
                            }
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Enum)
            {
                if (propertyList.myEnumKeys.Count() > 0)
                {
                    for (int j = 0; j < propertyList.myEnumKeys.Count(); j++)
                    {
                        if (propertyList.myEnumKeys[j] == materialVariable.name)
                        {
                            int curEnum = propertyList.myEnumValues[j];
                            substance.SetProceduralEnum(materialVariable.name, curEnum);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < propertyList.myKeys.Count(); j++)
                    {
                        if (propertyList.myKeys[j] == materialVariable.name)
                        {
                            int curEnum = int.Parse(propertyList.myValues[j]);
                            substance.SetProceduralEnum(materialVariable.name, curEnum);
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Boolean)
            {
                if (propertyList.myBooleanKeys.Count() > 0)
                {
                    for (int j = 0; j < propertyList.myBooleanKeys.Count(); j++)
                    {
                        if (propertyList.myBooleanKeys[j] == materialVariable.name)
                        {
                            bool curBool = propertyList.myBooleanValues[j];
                            substance.SetProceduralBoolean(materialVariable.name, curBool);
                        }
                    }
                }

                else
                {
                    for (int j = 0; j < propertyList.myKeys.Count(); j++)
                    {
                        if (propertyList.myKeys[j] == materialVariable.name)
                        {
                            bool curBool = bool.Parse(propertyList.myValues[j]);
                            substance.SetProceduralBoolean(materialVariable.name, curBool);
                        }
                    }
                }
            }
        }
    }

    public void ResetProceduralValuesOnUndo() // when undoing any slider/field/enum the list/dictionary/parameter gets reset but the material will still look the same so i have to change it tosomething else then back.
    {
        if (materialVariables == null && substance)
            materialVariables = substance.GetProceduralPropertyDescriptions();
        if (materialVariables != null)
        {
            for (int i = 0; i <= this.materialVariables.Count() - 1; i++)
            {
                ProceduralPropertyDescription materialVariable = this.materialVariables[i];
                ProceduralPropertyType propType = this.materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
                {
                    float propFloat = substance.GetProceduralFloat(materialVariable.name);
                    if (propFloat != materialVariable.maximum)
                    {
                        substance.SetProceduralFloat(materialVariable.name, materialVariable.maximum);
                        substance.SetProceduralFloat(materialVariable.name, propFloat);
                    }
                    else
                    {
                        substance.SetProceduralFloat(materialVariable.name, materialVariable.minimum);
                        substance.SetProceduralFloat(materialVariable.name, propFloat);
                    }
                }
                if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
                {
                    Color propColor = substance.GetProceduralColor(materialVariable.name);
                    if (propColor != Color.white)
                    {
                        substance.SetProceduralColor(materialVariable.name, Color.white);
                        substance.SetProceduralColor(materialVariable.name, propColor);
                    }
                    else
                    {
                        substance.SetProceduralColor(materialVariable.name, Color.black);
                        substance.SetProceduralColor(materialVariable.name, propColor);
                    }
                }
                if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
                {
                    if (propType == ProceduralPropertyType.Vector4)
                    {
                        Vector4 propVector = substance.GetProceduralVector(materialVariable.name);
                        if (propVector != Vector4.zero)
                        {
                            substance.SetProceduralVector(materialVariable.name, Vector4.zero);
                            substance.SetProceduralVector(materialVariable.name, propVector);
                        }
                        else
                        {
                            substance.SetProceduralVector(materialVariable.name, Vector4.one);
                            substance.SetProceduralVector(materialVariable.name, propVector);
                        }
                    }
                    else if (propType == ProceduralPropertyType.Vector3)
                    {
                        Vector3 propVector = substance.GetProceduralVector(materialVariable.name);
                        if (propVector != Vector3.zero)
                        {
                            substance.SetProceduralVector(materialVariable.name, Vector3.zero);
                            substance.SetProceduralVector(materialVariable.name, propVector);
                        }
                        else
                        {
                            substance.SetProceduralVector(materialVariable.name, Vector3.one);
                            substance.SetProceduralVector(materialVariable.name, propVector);
                        }
                    }
                    else if (propType == ProceduralPropertyType.Vector2)
                    {
                        Vector2 propVector = substance.GetProceduralVector(materialVariable.name);
                        if (propVector != Vector2.zero)
                        {
                            substance.SetProceduralVector(materialVariable.name, Vector2.zero);
                            substance.SetProceduralVector(materialVariable.name, propVector);
                        }
                        else
                        {
                            substance.SetProceduralVector(materialVariable.name, Vector2.one);
                            substance.SetProceduralVector(materialVariable.name, propVector);
                        }
                    }
                }
                if (propType == ProceduralPropertyType.Enum)
                {
                    int propEnum = substance.GetProceduralEnum(materialVariable.name);
                    if (propEnum != 0)
                    {
                        substance.SetProceduralEnum(materialVariable.name, 0);
                        substance.SetProceduralEnum(materialVariable.name, propEnum);
                    }
                    else
                    {
                        substance.SetProceduralEnum(materialVariable.name, 1);
                        substance.SetProceduralEnum(materialVariable.name, propEnum);
                    }
                }
                if (propType == ProceduralPropertyType.Boolean)
                {
                    bool propBool = substance.GetProceduralBoolean(materialVariable.name);
                    if (propBool == true)
                    {
                        substance.SetProceduralBoolean(materialVariable.name, false);
                        substance.SetProceduralBoolean(materialVariable.name, propBool);
                    }
                    else
                    {
                        substance.SetProceduralBoolean(materialVariable.name, true);
                        substance.SetProceduralBoolean(materialVariable.name, propBool);
                    }
                }
            }
        }
        if (rend && rend.sharedMaterial.HasProperty("_EmissionColor"))
        {
            Color propColor = emissionInput;
            if (propColor != Color.white)
            {
                rend.sharedMaterial.SetColor("_EmissionColor", Color.white);
                rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
            }
            else
            {
                rend.sharedMaterial.SetColor("_EmissionColor", Color.gray);
                rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
            }
        }
        if (rend && rend.sharedMaterial.HasProperty("_MainTex"))
        {
            Vector2 propMainTex = MainTexOffset;
            if (propMainTex != Vector2.zero)
            {
                rend.sharedMaterial.SetTextureOffset("_MainTex", Vector2.zero);
                rend.sharedMaterial.SetTextureOffset("_MainTex", propMainTex);
            }
            else
            {
                rend.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(0.01f, 0.01f));
                rend.sharedMaterial.SetTextureOffset("_MainTex", propMainTex);
            }
        }
    }

    public static Vector4 StringToVector(string startVector, int VectorAmount)//Converts Strings to Vectors.
    {
        if (startVector.StartsWith("(") && startVector.EndsWith(")")) // Remove "()" from the string 
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
        flickerEnabled = true;
        flickerCalc = UnityEngine.Random.Range(flickerMin, flickerMax);
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
        if (flickerEmissionToggle)
            flickerEmissionCalc = flickerCalc;
        if (flickerBoolToggle)
            flickerBoolCalc = (int)UnityEngine.Random.Range(0, 1);
        return flickerCalc;
    }
}