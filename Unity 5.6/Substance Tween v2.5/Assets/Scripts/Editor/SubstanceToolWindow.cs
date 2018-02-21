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

public class SubstanceToolWindow : EditorWindow
{
    //SubstanceTween Ver 2.5 - 10/21/2011
    //Written by: Chris Dougherty
    //https://www.linkedin.com/in/archarchaic
    //chris.ll.dougherty@gmail.com
    //https://www.artstation.com/artist/archarchaic

    private GameObject currentSelection;
    public Renderer rend;
    public bool UpdatingStartVariables = true, saveDefaultSubstanceVars = true, rebuildSubstanceImmediately, cacheSubstance = true, gameIsPaused, substanceLerp, saveOutputParameters = true, animateOutputParameters = true, resettingValuesToDefault = true, showEnumDropdown, showKeyframes, reWriteAllKeyframeTimes, curveDebugFoldout, emissionFlickerFoldout, animateBackwards;
    public ProceduralMaterial substance, defaultSubstance;
    private ProceduralPropertyDescription[] materialVariables, defaultObjProperties;
    public MaterialVariableListHolder xmlDescription;
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
    public float animationTime = 5, currentAnimationTime = 0, lerp, lerpCalc, keyframeSum = 0, curveFloat, animationTimeRestartEnd, currentKeyframeAnimationTime;
    public enum AnimationType { Loop, BackAndForth };
    public AnimationType animationType;
    public enum MySubstanceProcessorUsage { Unsupported, One, Half, All };
    public MySubstanceProcessorUsage mySubstanceProcessorUsage;
    public enum MyProceduralCacheSize { Medium = 0, Heavy = 1, None = 2, NoLimit = 3, Tiny = 4 };
    public MyProceduralCacheSize myProceduralCacheSize;

    public List<Keyframe> keyframeTimesCurve = new List<Keyframe>();
    public AnimationCurve substanceCurve = AnimationCurve.Linear(0, 0, 0, 0);
    public AnimationCurve substanceCurveBackup = AnimationCurve.Linear(0, 0, 0, 0);


    public bool flickerEnabled, flickerEmissionToggle, flickerFloatToggle, flickerColor3Toggle, flickerColor4Toggle, flickerVector2Toggle, flickerVector3Toggle, flickerVector4Toggle;
    public float flickerCalc = 1, flickerMin = 0.2f, flickerMax = 1.0f, flickerEmissionCalc = 1, flickerFloatCalc = 1, flickerColor3Calc = 1, flickerColor4Calc = 1, flickerVector2Calc = 1, flickerVector3Calc = 1, flickerVector4Calc = 1;
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
    public string lastAction;

    [MenuItem("Window/SubstanceTween")]
    static void Init()
    {

        //SubstanceToolWindow window = EditorWindow.CreateInstance<SubstanceToolWindow>();
        SubstanceToolWindow window = (SubstanceToolWindow)GetWindow(typeof(SubstanceToolWindow), utility: true);
        window.position = new Rect(Screen.width * 2, Screen.height / 4, Screen.width, Screen.height * 2);
        var content = new GUIContent();
        content.text = "SubstanceTween";
        var icon = new Texture2D(16, 16);
        content.image = icon;
        window.titleContent = content;
        window.Show();
    }

    private void OnFocus()
    {
        if (currentSelection == null && Selection.activeGameObject)
        {
            currentSelection = Selection.activeGameObject;
            if (currentSelection)
                rend = currentSelection.GetComponent<Renderer>();
            if (rend && UpdatingStartVariables)// should only be called after the first time you open the tool
            {
                DebugStrings.Add("Opened Tool");
                substance = rend.sharedMaterial as ProceduralMaterial;
                myProceduralCacheSize = (MyProceduralCacheSize)ProceduralCacheSize.NoLimit;
                substance.cacheSize = ProceduralCacheSize.NoLimit;
                mySubstanceProcessorUsage = (MySubstanceProcessorUsage)ProceduralProcessorUsage.All;
                ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;
                UpdatingStartVariables = false;
                selectedStartupMaterials.Add(substance); // First selected object material 
                selectedStartupGameObjects.Add(currentSelection); // First selected game object
                DebugStrings.Add("First object selected: " + currentSelection + " Selected objects material name: " + rend.sharedMaterial.name);
            }
            if (substance)
                substance.RebuildTextures();
            substanceAssetName = AssetDatabase.GetAssetPath(substance);
            if (substanceAssetName != String.Empty)
            {
                substanceImporter = AssetImporter.GetAtPath(substanceAssetName) as SubstanceImporter;
                substanceImporter.GetPlatformTextureSettings(substanceAssetName, "", out substanceWidth, out substanceHeight, out substanceTextureFormat, out substanceLoadBehavior);
            }
            substanceHeight = 8; // 8 is 512x512 by default
            substanceWidth = 8;
            Repaint();
            myProceduralCacheSize = MyProceduralCacheSize.NoLimit;
            substance.cacheSize = ProceduralCacheSize.NoLimit;
        }
    }

    void OnSelectionChange() // Gets called whenever you change objects 
    {
        if (currentSelection != Selection.activeGameObject)
            substanceLerp = false;
        currentSelection = Selection.activeGameObject;
        if (!gameIsPaused && currentSelection)
        {
            if (currentSelection)
                rend = currentSelection.GetComponent<Renderer>();
            if (rend && currentSelection)
            {
                DebugStrings.Add("Selected: " + currentSelection + " Selected objects material name: " + rend.sharedMaterial.name);
            }
            if (selectedStartupMaterials.Count > 0)
            {
                bool materialExists = false;
                if (rend)
                    substance = rend.sharedMaterial as ProceduralMaterial;
                for (int i = 0; i < selectedStartupMaterials.Count; i++) // goes through every material that you have selected in the past 
                {
                    if (currentSelection.name == selectedStartupGameObjects[i].name) // if currently selected object name = one of the gameobjects that you have already selected before
                    {
                        substance = selectedStartupMaterials[i];
                        materialExists = true; // object has already been selected before 
                    }
                    DebugStrings.Add("Material " + i + ": " + selectedStartupMaterials[i]);
                }
                if (!materialExists && rend) //Object has not been selected before so save all of the current object variables as this object's default variables 
                {
                    if (rend)
                        substance = rend.sharedMaterial as ProceduralMaterial;
                    saveOutputParameters = true;
                    selectedStartupMaterials.Add(substance);
                    selectedStartupGameObjects.Add(currentSelection);
                    defaultSubstanceObjProperties.Add(new MaterialVariableListHolder());
                    defaultSubstance = rend.sharedMaterial as ProceduralMaterial;
                    if (substance)
                    {
                        defaultObjProperties = substance.GetProceduralPropertyDescriptions();
                        materialVariables = substance.GetProceduralPropertyDescriptions();
                        defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName = defaultSubstance.name;
                        AddProceduralVariablesToList(defaultSubstanceObjProperties[defaultSubstanceIndex]);
                        defaultSubstanceObjProperties[defaultSubstanceIndex].MainTex = MainTexOffset;
                        defaultSubstanceObjProperties[defaultSubstanceIndex].emissionColor = emissionInput;
                        DebugStrings.Add("Default substance material " + defaultSubstanceIndex + ": " + defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName);
                        defaultSubstanceIndex++;
                        substanceAssetName = AssetDatabase.GetAssetPath(substance);
                        if (substanceAssetName != String.Empty)
                        {
                            substanceImporter = AssetImporter.GetAtPath(substanceAssetName) as SubstanceImporter;
                            substanceImporter.GetPlatformTextureSettings(substanceAssetName, "", out substanceWidth, out substanceHeight, out substanceTextureFormat, out substanceLoadBehavior);
                        }
                        substanceHeight = 8;
                        substanceWidth = 8;
                    }
                }
            }
            if (MaterialVariableKeyframeList.Count == 1 && (MaterialVariableKeyframeList[0].PropertyName.Count > 0))
            {
                MaterialVariableKeyframeList[0].PropertyName.Clear(); MaterialVariableKeyframeList[0].PropertyFloat.Clear(); MaterialVariableKeyframeList[0].PropertyColor.Clear();
                MaterialVariableKeyframeList[0].PropertyVector2.Clear(); MaterialVariableKeyframeList[0].PropertyVector3.Clear(); MaterialVariableKeyframeList[0].PropertyVector4.Clear();
                MaterialVariableKeyframeList[0].myKeys.Clear(); MaterialVariableKeyframeList[0].myValues.Clear();
            }
        }
        Repaint();
    }

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

    public void OnGUI()
    {
        var styleAnimationButton = new GUIStyle(EditorStyles.toolbarButton);
        if (MaterialVariableKeyframeList.Count >= 2 && substanceLerp)
            styleAnimationButton.normal.textColor = Color.green; // green for animating
        else if (MaterialVariableKeyframeList.Count >= 2 && !substanceLerp)
            styleAnimationButton.normal.textColor = new Color(255, 204, 0); // yellow for pause/standby
        else
            styleAnimationButton.normal.textColor = Color.red; // red for not ready to animate (needs 2 keyframes)
        var styleMainHeader = new GUIStyle();
        styleMainHeader.fontSize = 25;
        styleMainHeader.alignment = TextAnchor.UpperCenter;
        var styleH2 = new GUIStyle();
        styleH2.fontSize = 20;
        styleH2.alignment = TextAnchor.UpperCenter;
        GUIStyle errorTextStyle = new GUIStyle();
        errorTextStyle.fontSize = 15;
        errorTextStyle.wordWrap = true;
        if (EditorApplication.isPlaying && /*gameIsPaused == false &&*/ currentSelection != null && rend && substance && AssetDatabase.GetAssetPath(Selection.activeObject) == String.Empty)//Check if you are in play mode, game is not paused, a object is selected, it has a renderer and the object is not in the project view
        {
            GUIStyle toolbarStyle = new GUIStyle(EditorStyles.toolbar);
            GUIStyle toolbarDropdownStyle = new GUIStyle(EditorStyles.toolbarDropDown);
            GUIStyle toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            GUIStyle toolbarPopupStyle = new GUIStyle(EditorStyles.toolbarPopup);
            GUILayout.BeginHorizontal(toolbarStyle);
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
                if (GUILayout.Button("Toggle Animation On/Off(needs Two keyframes)", styleAnimationButton)) //Pauses-Unpauses animation
                {
                    if (MaterialVariableKeyframeList.Count >= 2 && MaterialVariableKeyframeList[0].PropertyMaterialName == substance.name) // if you have 2 transitions and the name of the selected substance matches the name on keyframe 1
                    {
                        if (substanceLerp)
                        {
                            for (int i = 0; i < materialVariables.Length; i++)
                                substance.CacheProceduralProperty(materialVariables[i].name, false);
                            substanceLerp = false;
                        }
                        else if (!substanceLerp)
                        {
                            if (cacheSubstance)
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
                                                        varChanged = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            substanceLerp = true;
                        }
                    }
                    else if (MaterialVariableKeyframeList.Count >= 2)
                    { // If material names are different 
                        DebugStrings.Add("Tried to animate object: " + currentSelection + " but the Transition Material name " + MaterialVariableKeyframeList[0].PropertyMaterialName + " did not match the current Procedural Material name: " + substance.name);
                        var renameMaterialOption = EditorUtility.DisplayDialog(
                            "error",
                            "Transition Material name " + MaterialVariableKeyframeList[0].PropertyMaterialName + " does not match current Procedural Material name: " + substance.name + ". Would you like to rename " + MaterialVariableKeyframeList[0].PropertyMaterialName + " to " + substance.name + "?"
                            + " (Only do this if you are sure the materials are the same and only have different names)", "Yes", "No");
                        if (renameMaterialOption)
                        {
                            DebugStrings.Add("Renamed Material: " + MaterialVariableKeyframeList[0].PropertyMaterialName + " To: " + substance.name);
                            MaterialVariableKeyframeList[0].PropertyMaterialName = substance.name; // Saves Substance name in keyframe as current substance name
                            for (int i = 0; i <= MaterialVariableKeyframeList.Count - 1; i++)
                                MaterialVariableKeyframeList[i].PropertyMaterialName = substance.name;
                            substanceLerp = true;
                        }
                        else
                            DebugStrings.Add("Did not rename or take any other action.");
                    }
                    else
                        EditorUtility.DisplayDialog("error", "You do not have two keyframes", "OK");
                }
            }
            else if (gameIsPaused) 
                EditorGUILayout.LabelField("GAME IS PAUSED", styleAnimationButton);

            if (GUILayout.Button("Create keyframe: " + (keyFrameTimes.Count + 1), toolbarButtonStyle))
            {
                CreateKeyframe();
                EditorGUIUtility.ExitGUI();
            }
            EditorGUIUtility.labelWidth = 100f;
            animationType = (AnimationType)EditorGUILayout.EnumPopup("Animation Type:", animationType, toolbarPopupStyle);
            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            scrollVal = GUILayout.BeginScrollView(scrollVal);
            EditorGUILayout.LabelField("SubstanceTween 2.5", styleMainHeader); EditorGUILayout.Space();
            EditorGUILayout.LabelField("Currently selected Material:", styleH2); EditorGUILayout.Space();
            if (substance)
                EditorGUILayout.LabelField(substance.name, styleH2); EditorGUILayout.Space();
            if (currentSelection.GetComponent<PrefabProperties>() != null) // Selected object already has a script attached for animating variables
            {
                EditorGUILayout.LabelField("This tool is not meant to run on animated prefabs/instances", styleH2);
                EditorGUILayout.Space();
            }

            if (substance && saveDefaultSubstanceVars) // saves the selected substance variables on start if you need to reset to default
            {
                defaultSubstanceObjProperties.Add(new MaterialVariableListHolder());
                defaultSubstance = rend.sharedMaterial as ProceduralMaterial;
                defaultObjProperties = substance.GetProceduralPropertyDescriptions();
                materialVariables = substance.GetProceduralPropertyDescriptions();
                defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName = defaultSubstance.name;
                DebugStrings.Add("Default substance material " + defaultSubstanceIndex + ": " + selectedStartupMaterials[0]);
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
                    ProceduralPropertyType propType = materialVariables[i].type;
                    if (substance.IsProceduralPropertyVisible(materialVariable.name)) // checks the current variable for any $visibleIf paramters(Set in Substance Designer)
                    {
                        string variableGroupName = materialVariable.group; // Gets the group that the current parameter is in 
                        if (!variableGroups.Contains(variableGroupName) && variableGroupName != String.Empty)
                        {
                            variableGroups.Add(variableGroupName);
                        }
                        else if (variableGroupName == String.Empty)
                        {
                            DisplayControlForParameter(materialVariable);
                        }
                    }
                }
                foreach (string curGroupName in variableGroups) // Go through each group and display controls 
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
                        for (int i = 0; i < materialVariables.Length; i++)
                        {
                            ProceduralPropertyDescription materialVariable = materialVariables[i];
                            if (showGroup && materialVariable.group == curGroupName)
                            {
                                EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
                                GUILayout.Space((float)(EditorGUI.indentLevel * 40));
                                DisplayControlForParameter(materialVariable);
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUI.indentLevel -= 1;
                    }
                }

                if (rend && rend.sharedMaterial.HasProperty("_MainTex"))
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Label("_MainTex");
                    if (MainTexOffset != null)
                    {
                        Vector2 oldOffset = MainTexOffset;
                        if (!gameIsPaused && substanceLerp)
                        {
                            MainTexOffset.x = EditorGUILayout.Slider(MainTexOffset.x, -10f, 10.0f);
                            MainTexOffset.y = EditorGUILayout.Slider(MainTexOffset.y, -10f, 10.0f);
                            if (EditorGUI.EndChangeCheck())
                            {

                                    Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Emission Changed");
                                    EditorUtility.SetDirty(this);
                                    rend.sharedMaterial.SetTextureOffset("_MainTex", MainTexOffset * Time.time);
                                    DebugStrings.Add("_MainTex" + " Was " + oldOffset + " is now: " + MainTexOffset);
                            }
                        }
                        else
                        {

                            MainTexOffset.x = EditorGUILayout.Slider(MainTexOffset.x, 0f, 1f);
                            MainTexOffset.y = EditorGUILayout.Slider(MainTexOffset.y, 0f, 1f);
                            rend.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(0,0));
                        }
                    }
                }
                if (rend && rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Label("Emission:");
                    emissionInput = EditorGUILayout.ColorField(GUIContent.none, value: emissionInput, showEyedropper: true, showAlpha: true, hdr: true, hdrConfig: null);
                    Color oldEmissionInput = emissionInput;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Emission Changed");
                        EditorUtility.SetDirty(this);
                        rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
                        DebugStrings.Add("_EmissionColor" + " Was " + oldEmissionInput + " is now: " + emissionInput);
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
                        if (flickerEnabled)
                            ChangeFlicker();
                    }
                }
                if (keyFrameTimes.Count >= 2)
                    substanceCurve = EditorGUILayout.CurveField("Animation Time Curve", substanceCurve); // field for the animation curve
                if (EditorWindow.focusedWindow && EditorWindow.focusedWindow.ToString() == " (UnityEditor.CurveEditorWindow)" && substanceCurve.keys.Count() != substanceCurveBackup.keys.Count()) // if you delete or add a keyframe in the curve editor, Reset keys.
                {
                    substanceCurve.keys = substanceCurveBackup.keys;
                }

                if (keyFrameTimes.Count == 0 && substanceCurve.keys.Count() == 0)//
                {
                    substanceCurve.AddKey(0, 0);
                    substanceCurve.AddKey(5, 5);
                }

                showKeyframes = EditorGUILayout.Foldout(showKeyframes, "Keyframes"); // drop down list for keyframes 
                if (showKeyframes && keyFrames >= 1)
                {
                    GUIStyle tst = new GUIStyle();
                    tst.alignment = TextAnchor.MiddleRight;
                    EditorGUILayout.LabelField("Transition Time:", tst);
                    for (int i = 0; i <= keyFrameTimes.Count - 1; i++)
                    {
                        GUILayout.BeginHorizontal();
                        if (!substanceLerp)
                        {
                            if (GUILayout.Button("Remove") && !substanceLerp)
                            {
                                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Remove Keyframe " + i);
                                keyFrameTimes.RemoveAt(i);
                                MaterialVariableKeyframeList.RemoveAt(i);
                                MaterialVariableKeyframeDictionaryList.RemoveAt(i);
                                keyframeSum = 0;
                                for (int j = MaterialVariableKeyframeList.Count(); j >= 0; j--)
                                    substanceCurve.RemoveKey(j);
                                for (int j = 0; j <= MaterialVariableKeyframeList.Count() - 1; j++)
                                {
                                    substanceCurve.AddKey(keyframeSum, keyframeSum);
                                    keyframeSum += keyFrameTimes[j];
                                };
                                currentAnimationTime = 0;
                                currentKeyframeIndex = 0;
                                animationTimeRestartEnd = 0;
                                keyframeSum = 0;
                                if (keyFrames > 0)
                                    keyFrames--;
                                substanceCurveBackup.keys = substanceCurve.keys;
                                Repaint();
                                return; // Without returning i get a error when deleting the last keyframe
                            }
                        }
                        if (GUILayout.Button("Select"))
                        {
                            SetProceduralVariablesFromList(MaterialVariableKeyframeList[i]);
                            emissionInput = MaterialVariableKeyframeList[i].emissionColor;
                            if (rend.sharedMaterial.HasProperty("_EmissionColor"))
                                rend.sharedMaterial.SetColor("_EmissionColor", MaterialVariableKeyframeList[i].emissionColor);
                            if (rend.sharedMaterial.HasProperty("_MainTex"))
                            {
                                MainTexOffset = MaterialVariableKeyframeList[i].MainTex;
                                rend.sharedMaterial.SetTextureOffset("_MainTex", MaterialVariableKeyframeList[i].MainTex);
                            }
                            Repaint();
                        }
                        if (GUILayout.Button("Overwrite"))
                        {
                            Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Overwrite keyframe " + i);
                            if (keyFrameTimes.Count >= 1)
                            {
                                int lastKeyframeIndex = i - 1;
                                if (i >= 0)
                                {
                                    MaterialVariableKeyframeList.Remove(MaterialVariableKeyframeList[i]);
                                    MaterialVariableKeyframeList.Insert(i, new MaterialVariableListHolder());
                                    keyFrameTimes.RemoveAt(i);
                                    AddProceduralVariablesToList(MaterialVariableKeyframeList[i]);
                                    MaterialVariableKeyframeDictionaryList.Remove(MaterialVariableKeyframeDictionaryList[i]);
                                    MaterialVariableKeyframeDictionaryList.Insert(i, new MaterialVariableDictionaryHolder());
                                    AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[i]);
                                    keyFrameTimes.Add(MaterialVariableKeyframeList[i].animationTime);
                                    DebugStrings.Add("OverWrote Keyframe: " + (i + 1));
                                    Repaint();
                                }
                            }
                            substanceCurveBackup.keys = substanceCurve.keys;
                        }
                        GUILayout.Label("Keyframe: " + (i + 1));
                        if ((i != keyFrameTimes.Count - 1 || keyFrameTimes.Count == 1))
                        {
                            if (keyFrameTimes[i] > 0)
                            {
                                EditorGUI.BeginChangeCheck();
                                EditorGUIUtility.fieldWidth = 0.01f;
                                EditorGUIUtility.labelWidth = 0.2f;
                                keyFrameTimes[i] = EditorGUILayout.DelayedFloatField(keyFrameTimes[i]);
                                EditorGUIUtility.labelWidth = 175;
                                if (EditorGUI.EndChangeCheck() && keyFrameTimes[i] > 0) // check if a Animation time has changed and if this after first keyframe(0,0), 'i' is index of changed keyframe
                                {
                                    List<Keyframe> tmpKeyframeList = substanceCurve.keys.ToList();
                                    keyframeSum = 0;
                                    for (int j = MaterialVariableKeyframeList.Count() - 1; j > 0; j--)// remove all keys
                                    {
                                        substanceCurve.RemoveKey(j);
                                    }
                                    for (int j = 0; j < MaterialVariableKeyframeList.Count(); j++)//rewrite keys with changed times
                                    {
                                        if (j == 0)
                                            substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, 0.25f, tmpKeyframeList[j].outTangent));
                                        else
                                            substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, tmpKeyframeList[j].inTangent, tmpKeyframeList[j].outTangent));
                                        if (reWriteAllKeyframeTimes)
                                        {
                                            for (int k = 0; k < keyFrameTimes.Count() - 1; k++)
                                            {
                                                keyFrameTimes[k] = keyFrameTimes[i];
                                                MaterialVariableKeyframeList[k].animationTime = keyFrameTimes[i];
                                            }
                                            keyframeSum += keyFrameTimes[i];
                                        }
                                        else
                                            keyframeSum += keyFrameTimes[j];
                                    }
                                    MaterialVariableKeyframeList[i].animationTime = keyFrameTimes[i];
                                    currentAnimationTime = 0;//Reset animation variables
                                    currentKeyframeIndex = 0;
                                    animationTimeRestartEnd = 0;
                                    keyframeSum -= keyFrameTimes[keyFrameTimes.Count() - 1]; //gets rid of last keyframe time.
                                    substanceCurveBackup.keys = substanceCurve.keys;
                                }
                            }
                            else
                            {
                                List<Keyframe> tmpKeyframeList = substanceCurve.keys.ToList();
                                keyFrameTimes[i] = EditorGUILayout.DelayedFloatField(MaterialVariableKeyframeList[i].animationTime);
                                keyframeSum = 0;
                                for (int j = MaterialVariableKeyframeList.Count() - 1; j > 0; j--)// remove all keys
                                {
                                    substanceCurve.RemoveKey(j);
                                }
                                for (int j = 0; j < MaterialVariableKeyframeList.Count(); j++)//rewrite keys with changed times
                                {
                                    if (j == 0)
                                        substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, 0.25f, tmpKeyframeList[j].outTangent));
                                    else
                                        substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, tmpKeyframeList[j].inTangent, tmpKeyframeList[j].outTangent));
                                    keyframeSum += keyFrameTimes[j];
                                }
                                MaterialVariableKeyframeList[i].animationTime = keyFrameTimes[i];
                                currentAnimationTime = 0;//Reset animation variables
                                currentKeyframeIndex = 0;
                                animationTimeRestartEnd = 0;
                                keyframeSum -= keyFrameTimes[keyFrameTimes.Count() - 1]; //gets rid of last keyframe time.
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    reWriteAllKeyframeTimes = EditorGUILayout.Toggle("Rewrite All Keyframe Times", reWriteAllKeyframeTimes);
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
                    EditorGUILayout.LabelField("Lerp: " + lerp.ToString());
                    EditorGUILayout.LabelField("Current Animation Time: " + currentAnimationTime.ToString());
                    EditorGUILayout.LabelField("Curve key count: " + substanceCurve.keys.Length.ToString());
                    EditorGUILayout.LabelField("Curve Float value: " + curveFloat.ToString());
                    EditorGUILayout.LabelField("Curve Animation Restart" + animationTimeRestartEnd.ToString());
                    EditorGUILayout.LabelField("Current Keyframe Animation Time: " + currentKeyframeAnimationTime.ToString());
                    EditorGUILayout.LabelField("Animate Backwards: " + animateBackwards.ToString());
                    EditorGUILayout.LabelField("Keyframe Count: " + keyFrameTimes.Count());
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                if (Event.current.keyCode == KeyCode.G && Event.current.type == EventType.KeyUp && lastAction != null)
                {
                    MethodInfo lastUsedMethod = this.GetType().GetMethod(lastAction, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    Action myAction = (Action)Delegate.CreateDelegate(typeof(Action), this, lastUsedMethod);
                    myAction();
                }
                if (Event.current.keyCode == KeyCode.R && Event.current.type == EventType.KeyUp)
                {
                    RandomizeProceduralValues();
                }
            }
        }

        else if (EditorApplication.isPlaying && selectedStartupMaterials.Count < 1)
        {
            if (!rend && !currentSelection)
            {
                EditorUtility.DisplayDialog("Error", " No object selected.", "OK"); this.Close() ; return;
            }
            else if (!rend)
            {
                EditorUtility.DisplayDialog("Error", " No renderer attached to object.wad", "OK");  this.Close();  return;
            }
            if (!substance)//Checks if a substance file is on selected object
            {
                this.Close();  EditorUtility.DisplayDialog("Error", " No ProceduralMaterial/Substance attached to object.", "OK");  return;
            }
            EditorGUILayout.LabelField("Select a game object in the Hierarchy that has a Substance material first, then select this window.(Make sure that you are in play mode)A", errorTextStyle);
        }
        else if (EditorApplication.isPlaying && !rend)
           EditorGUILayout.LabelField("This object has no renderer attached.", errorTextStyle);
        if (selectedStartupMaterials.Count >= 1 && !substance)
            EditorGUILayout.LabelField("No ProceduralMaterial attached to object, Select another object", errorTextStyle);
        Repaint();
#if UNITY_5_3
			if (substance && !substanceLerp)
				substance.RebuildTexturesImmediately();
#endif
#if UNITY_5_4_OR_NEWER // In Unity 5.4 and above I am able to use RebuildTextures() which is faster but it is not compatible with 5.3 
        if (substance && !substanceLerp)
            substance.RebuildTextures();
#endif
    }

    void Update()
    {
        if (EditorApplication.isPlaying)
        {
            if (rend)
                rend.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(MainTexOffset.x * Time.time, MainTexOffset.y * Time.time));
            if (substance && substanceLerp && currentSelection == Selection.activeGameObject)
            {
                currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
                if (animationType == AnimationType.BackAndForth && animateBackwards && currentKeyframeIndex <= keyFrameTimes.Count - 1 && currentKeyframeIndex > 0)
                    currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex - 1];
                if (animationType == AnimationType.Loop)// animate through every keyframe and repeat at the beginning
                {
                    currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
                    currentAnimationTime += Time.deltaTime;
                    animationTimeRestartEnd += Time.deltaTime;
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
                    if (animationTime > 0)
                    {
                        if (currentKeyframeIndex <= keyFrameTimes.Count - 2)
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
                    if (animationTime > 0)
                    {
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
                }

                if (materialVariables != null)
                    for (int i = 0; i < materialVariables.Length; i++)// search through dictionary for variable names and if they match animate them
                    {
                        ProceduralPropertyDescription materialVariable = materialVariables[i];
                        ProceduralPropertyType propType = materialVariables[i].type;
                        if (propType == ProceduralPropertyType.Float)
                        {
                            foreach (KeyValuePair<string, float> keyValue in MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyFloatDictionary)
                            {
                                if (keyValue.Key == materialVariable.name)
                                {
                                    if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (materialVariable.name[0] != '$' || (materialVariable.name[0] == '$' && animateOutputParameters)))// check if current keyframe number is less than the total
                                    {
                                        substance.SetProceduralFloat(materialVariable.name, Mathf.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyFloatDictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyFloatDictionary[materialVariable.name], lerp * flickerFloatCalc));
                                    }
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Color3)
                        {
                            foreach (KeyValuePair<string, Color> keyValue in MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyColorDictionary)
                            {
                                if (keyValue.Key == materialVariable.name)
                                {
                                    if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                    {
                                        substance.SetProceduralColor(materialVariable.name, Color.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyColorDictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyColorDictionary[materialVariable.name], lerp * flickerColor3Calc));
                                    }
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Color4)
                        {
                            foreach (KeyValuePair<string, Color> keyValue in MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyColorDictionary)
                            {
                                if (keyValue.Key == materialVariable.name)
                                {
                                    if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                    {
                                        substance.SetProceduralColor(materialVariable.name, Color.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyColorDictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyColorDictionary[materialVariable.name], lerp * flickerColor4Calc));
                                    }
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
                        {
                            Vector4 curLerp1Vector = Vector4.zero, curLerp2Vector = Vector4.zero;
                            if (propType == ProceduralPropertyType.Vector4)
                            {
                                foreach (KeyValuePair<string, Vector4> keyValue in MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector4Dictionary)
                                {
                                    if (keyValue.Key == materialVariable.name)
                                    {
                                        if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                        {
                                            substance.SetProceduralVector(materialVariable.name, Vector4.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector4Dictionary[materialVariable.name], curLerp2Vector = MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyVector4Dictionary[materialVariable.name], lerp * flickerVector4Calc));
                                        }
                                    }
                                }
                            }
                            else if (propType == ProceduralPropertyType.Vector3)
                            {
                                foreach (KeyValuePair<string, Vector3> keyValue in MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector3Dictionary)
                                {
                                    if (keyValue.Key == materialVariable.name)
                                    {
                                        if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                        {
                                            substance.SetProceduralVector(materialVariable.name, Vector3.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector3Dictionary[materialVariable.name], curLerp2Vector = MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyVector3Dictionary[materialVariable.name], lerp * flickerVector3Calc));
                                        }
                                    }
                                }
                            }
                            else if (propType == ProceduralPropertyType.Vector2 && (materialVariable.name[0] != '$' || (materialVariable.name[0] == '$' && animateOutputParameters)))
                            {
                                foreach (KeyValuePair<string, Vector2> keyValue in MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector2Dictionary)
                                {
                                    if (keyValue.Key == materialVariable.name)
                                    {
                                        if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1)
                                        {
                                            substance.SetProceduralVector(materialVariable.name, Vector2.Lerp(curLerp1Vector = MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyVector2Dictionary[materialVariable.name], MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].PropertyVector2Dictionary[materialVariable.name], lerp * flickerVector2Calc));
                                        }
                                    }
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Enum)
                        {
                            foreach (KeyValuePair<string, int> keyvalue in MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyEnumDictionary)
                            {
                                if (keyvalue.Key == materialVariable.name)
                                {
                                    if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (currentAnimationTime == 0 || currentAnimationTime == currentKeyframeAnimationTime))
                                    {
                                        substance.SetProceduralEnum(materialVariable.name, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyEnumDictionary[materialVariable.name]);
                                    }
                                }
                            }
                        }
                        else if (propType == ProceduralPropertyType.Boolean)
                        {
                            foreach (KeyValuePair<string, bool> keyvalue in MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyBoolDictionary)
                            {
                                if (keyvalue.Key == materialVariable.name)
                                {
                                    if (currentKeyframeIndex + 1 <= MaterialVariableKeyframeDictionaryList.Count - 1 && (currentAnimationTime == 0 || currentAnimationTime == currentKeyframeAnimationTime))
                                    {
                                        //bool curBool = MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyBoolDictionary[materialVariable.name];
                                        substance.SetProceduralBoolean(materialVariable.name, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].PropertyBoolDictionary[materialVariable.name]);
                                    }
                                }
                            }
                        }
                    }
                if (rend.sharedMaterial.HasProperty("_EmissionColor") && currentKeyframeIndex + 1 <= MaterialVariableKeyframeList.Count - 1)
                {
                    Color emissionInput = rend.sharedMaterial.GetColor("_EmissionColor");
                    rend.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].emissionColor, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].emissionColor, lerp * flickerCalc));
                }
                if (rend.sharedMaterial.HasProperty("_MainTex") && currentKeyframeIndex + 1 <= MaterialVariableKeyframeList.Count - 1)
                {
                    MainTexOffset = rend.sharedMaterial.GetTextureOffset("_MainTex");
                    rend.sharedMaterial.SetTextureOffset("_MainTex", Vector2.Lerp(MaterialVariableKeyframeDictionaryList[currentKeyframeIndex].MainTex, MaterialVariableKeyframeDictionaryList[currentKeyframeIndex + 1].MainTex, lerp * flickerCalc));
                }
                if (rebuildSubstanceImmediately)
                    substance.RebuildTexturesImmediately();
                else
                    substance.RebuildTextures();
            }
        }
        else
        {
            this.Close(); EditorUtility.DisplayDialog("error", " This tool only works in play mode", "OK");
        }
        if (EditorApplication.isPaused)
            gameIsPaused = true;
        else
            gameIsPaused = false;
        if (gameIsPaused)
            substanceLerp = false;
    }

    void CreateKeyframe()
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Create Keyframe " + keyFrameTimes.Count().ToString());
        EditorUtility.SetDirty(this);
        if (animationTime > 0)
        {
            if (keyFrameTimes.Count == 0)
            {
                DebugStrings.Add("Created Keyframe 1:");
                MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                AddProceduralVariablesToList(MaterialVariableKeyframeList[0]);
                AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[0]);
                keyFrames++;
                keyFrameTimes.Add(MaterialVariableKeyframeList[0].animationTime);
                substanceCurve.AddKey(new Keyframe(MaterialVariableKeyframeList[0].animationTime, MaterialVariableKeyframeList[0].animationTime));
                AnimationUtility.SetKeyLeftTangentMode(substanceCurve, 0, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(substanceCurve, 0, AnimationUtility.TangentMode.Linear);
            }
            else if (keyFrameTimes.Count > 0)
            {
                for (int i = 0; i <= keyFrameTimes.Count - 1; i++)
                {// Goes through each key frame and checks if the keyframe that you are trying to create has the same number of parameters as the rest and if they all save Output parameters or not.
                    if (saveOutputParameters && MaterialVariableKeyframeList[i].hasParametersWithoutRange == false)
                    {//Subsance designer can export special properties like '$randomSeed' that can be saved. this checks if you selected to save those objects and turned it off later
                        EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are saving Parameters without a range however keyframe " + (i + 1) + " does " +
                        "not save these variables. To fix this uncheck \"Save Output Parameters\" on this frame and try again or check \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                        return;
                    }
                    if (!saveOutputParameters && MaterialVariableKeyframeList[i].hasParametersWithoutRange == true)
                    {
                        EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are not saving Parameters without a range however keyframe " + i + "does " +
                        "save these variables. To fix this check \"Save Output Parameters\" on this frame and try again or uncheck \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                        return;
                    }
                }
                MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                DebugStrings.Add("Created KeyFrame: " + MaterialVariableKeyframeList.Count);
                MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                AddProceduralVariablesToList(MaterialVariableKeyframeList[keyFrames]);
                AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[keyFrames]);
                keyFrameTimes.Add(MaterialVariableKeyframeList[keyFrames].animationTime);
                keyframeSum = 0;
                for (int i = 0; i < MaterialVariableKeyframeList.Count() - 1; i++)  //  -1 to count here 6/10/17
                    keyframeSum += MaterialVariableKeyframeList[i].animationTime;
                substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum));
                AnimationUtility.SetKeyLeftTangentMode(substanceCurve, keyFrames, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(substanceCurve, keyFrames, AnimationUtility.TangentMode.Linear);
                keyFrames++;
            }
            substanceCurveBackup.keys = substanceCurve.keys;
        }
        else
            EditorUtility.DisplayDialog("Error", "Animation time cannot be 0 or less", "OK");
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    void WriteXML()
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

    void ReadXML()
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Read XML File");
        EditorUtility.SetDirty(this);
        var serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
        string path = EditorUtility.OpenFilePanel("", "", "xml"); // 'Open' Dialog that only accepts XML files
        if (path.Length != 0)
        {
            var stream = new FileStream(path, FileMode.Open); // 'Open' Dialog that only accepts XML files
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
                DebugStrings.Add(container.myKeys.Count + " Keys");
                DebugStrings.Add(container.myValues.Count + " Values");
                if (emissionInput != null)
                    DebugStrings.Add("_EmissionColor = " + container.emissionColor);
                if (MainTexOffset != null)
                    DebugStrings.Add("_MainTex = " + MainTexOffset);
                DebugStrings.Add("-----------------------------------");
                substance.RebuildTexturesImmediately();
            }
        }
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    void WriteAllXML()
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
                    if (fileNumber == 1) // if fileNumber is one make sure that there are no NAN tangents saved
                        xmlDescription.AnimationCurveKeyframeList.Add(new Keyframe(substanceCurve.keys[i].time, substanceCurve.keys[i].value, 0, substanceCurve.keys[i].outTangent));
                    else
                        xmlDescription.AnimationCurveKeyframeList.Add(substanceCurve.keys[i]);
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

    void ReadAllXML()
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Create Keyframes From XML");
        EditorUtility.SetDirty(this);
        var serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
        string xmlReadFolderPath = EditorUtility.OpenFolderPanel("Load xml files from folder", "", ""); //Creates 'Open Folder' Dialog
        if (xmlReadFolderPath.Length != 0)
        {
            string[] xmlReadFiles = Directory.GetFiles(xmlReadFolderPath);//array of selected xml file paths
            keyFrames = keyFrameTimes.Count();
            List<Keyframe> tmpKeyframeList = new List<Keyframe>();
            int totalKeyframesBeforeXmlRead = keyFrameTimes.Count();
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
    }
    void SavePrefab()
    {
        string prefabPath = EditorUtility.SaveFilePanelInProject("", "", "prefab", "");
        if (prefabPath.Length != 0)
        {
            GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, currentSelection.gameObject, ReplacePrefabOptions.Default);
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
                    if (animationTime > 0)
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
                }
                prefabProperties.keyFrameTimesOriginal = keyFrameTimes;
                prefabProperties.prefabAnimationCurveBackup = prefabProperties.prefabAnimationCurve;
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
            if (mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.Half.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.Half;
            if (mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.One.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.One;
            if (mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.Unsupported.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.Unsupported;
            if (flickerEmissionToggle)
            {
                prefabProperties.flickerEmissionToggle = true;
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
                prefabProperties.flickerMin = flickerMin;
                prefabProperties.flickerMax = flickerMax;
            }
            DebugStrings.Add("Prefab Path: " + prefabPath);
            DebugStrings.Add(prefab.name + " has " + prefabProperties.MaterialVariableKeyframeList.Count + " keyframes");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        lastAction = MethodBase.GetCurrentMethod().Name.ToString();
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

    void SetAllProceduralValuesToMin()
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Set all values to Minimum");
        EditorUtility.SetDirty(this);
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

    void SetAllProceduralValuesToMax()
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Set all values to Maximum");
        EditorUtility.SetDirty(this);
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

    void RandomizeProceduralValues()
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Randomize values");
        EditorUtility.SetDirty(this);
        for (int i = 0; i < materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = materialVariables[i];
            if (substance.IsProceduralPropertyVisible(materialVariable.name))
            {
                ProceduralPropertyType propType = materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float && materialVariable.hasRange && randomizeProceduralFloat)
                    substance.SetProceduralFloat(materialVariable.name, UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum));
                if (propType == ProceduralPropertyType.Color3 && randomizeProceduralColorRGB)
                    substance.SetProceduralColor(materialVariable.name, new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)));
                if (propType == ProceduralPropertyType.Color4 && randomizeProceduralColorRGBA)
                    substance.SetProceduralColor(materialVariable.name, new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)));
                if (propType == ProceduralPropertyType.Vector2 && materialVariable.hasRange && randomizeProceduralVector2)
                    substance.SetProceduralVector(materialVariable.name, new Vector2(UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum), UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum)));
                if (propType == ProceduralPropertyType.Vector3 && materialVariable.hasRange && randomizeProceduralVector3)
                    substance.SetProceduralVector(materialVariable.name, new Vector3(UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum), UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum), UnityEngine.Random.Range(materialVariables[i].minimum, materialVariables[i].maximum)));
                if (propType == ProceduralPropertyType.Vector4 && materialVariable.hasRange && randomizeProceduralVector4)
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

    void ResetAllProceduralValues()
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

    void OnValidate() // runs when anything in the inspector/graph changes or the script gets reloaded.
    {
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
    }

    void DisplayControlForParameter(ProceduralPropertyDescription parameter) // sorts parameters by type and displays controls for that type
    {
        ProceduralPropertyType propType = parameter.type;
        if (parameter.name[0] == '$')
        {
            if (parameter.name == "$outputsize") //Texture Size
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                GUILayout.Label(parameter.name + '(' + parameter.label + ')');
                substanceWidth = (int)substance.GetProceduralVector(parameter.name).x;
                substanceHeight = (int)substance.GetProceduralVector(parameter.name).y;
                Vector2 substanceSize = new Vector2(substanceWidth, substanceHeight);
                Vector2 oldSubstanceSize = new Vector2(substanceWidth, substanceHeight);
                substanceSize.x = EditorGUILayout.IntPopup("X:", substanceWidth, textureSizeStrings, textureSizeValues, new GUILayoutOption[0]);
                substanceSize.y = EditorGUILayout.IntPopup("Y:", substanceHeight, textureSizeStrings, textureSizeValues, new GUILayoutOption[0]);
                if (substanceSize != oldSubstanceSize)
                {
                    Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                    EditorUtility.SetDirty(this);
                    substance.SetProceduralVector(parameter.name, substanceSize);
                }
                GUILayout.EndHorizontal();
            }
            else if (parameter.name == "$randomseed") // Current Seed value. 
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                GUILayout.Label(parameter.name + '(' + parameter.label + ')');
                int randomSeed = (int)substance.GetProceduralFloat(parameter.name);
                int oldRandomSeed = randomSeed;
                randomSeed = EditorGUILayout.IntSlider(randomSeed, 1, 9999);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Randomize Seed", new GUILayoutOption[0]))
                    randomSeed = UnityEngine.Random.Range(0, 9999 + 1);
                if (EditorGUI.EndChangeCheck()) // anytime you change a slider it will save the old/new value to a debug text file that you can create.
                {
                    Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                    EditorUtility.SetDirty(this);
                    substance.SetProceduralFloat(parameter.name, randomSeed);
                    DebugStrings.Add(parameter.name + " Was " + oldRandomSeed + " is now: " + randomSeed);
                }
            }
        }
        else if (propType == ProceduralPropertyType.Float) // Ints are counted as floats.
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.Label(parameter.name + '(' + parameter.label + ')');
            float propFloat = substance.GetProceduralFloat(parameter.name);
            float oldPropFloat = propFloat;
            propFloat = EditorGUILayout.Slider(substance.GetProceduralFloat(parameter.name), parameter.minimum, parameter.maximum);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) // anytime you change a slider it will save the old/new value to a debug text file that you can create.
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                EditorUtility.SetDirty(this);
                substance.SetProceduralFloat(parameter.name, propFloat);
                DebugStrings.Add(parameter.name + " Was " + oldPropFloat + " is now: " + propFloat);
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
                EditorUtility.SetDirty(this);
                substance.SetProceduralColor(parameter.name, colorInput);
                DebugStrings.Add(parameter.name + " Was " + oldColorInput + " is now: " + colorInput);
            }
        }
        else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
            EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
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
                inputVector[c] = EditorGUILayout.Slider(parameter.componentLabels[c], inputVector[c], parameter.minimum, parameter.maximum, new GUILayoutOption[0]); c++;
            }
            EditorGUI.indentLevel--;
            if (inputVector != oldInputVector)
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                EditorUtility.SetDirty(this);
                substance.SetProceduralVector(parameter.name, inputVector);
                DebugStrings.Add(parameter.name + " Was " + oldInputVector + " is now: " + inputVector);
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
                EditorUtility.SetDirty(this);
                substance.SetProceduralEnum(parameter.name, enumInput);
            }
        }
        else if (propType == ProceduralPropertyType.Boolean)
        {
            GUILayout.Label(parameter.name);
            bool boolInput = substance.GetProceduralBoolean(parameter.name);
            bool oldBoolInput = boolInput;
            boolInput = EditorGUILayout.Toggle(boolInput);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, parameter.name + " Changed");
                EditorUtility.SetDirty(this);
                substance.SetProceduralBoolean(parameter.name, boolInput);
            }
        }
    }

    void AddProceduralVariablesToList(MaterialVariableListHolder materialVariableList) // Adds substance values to a list 
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
                materialVariableList.myKeys.Add(materialVariable.name);
                materialVariableList.myValues.Add(propFloat.ToString());
                DebugStrings.Add(i + " " + materialVariable.name + ": " + propFloat.ToString());
            }
            if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                Color propColor = substance.GetProceduralColor(materialVariable.name);
                materialVariableList.PropertyColor.Add(propColor);
                materialVariableList.myKeys.Add(materialVariable.name);
                materialVariableList.myValues.Add("#" + ColorUtility.ToHtmlStringRGBA(propColor));
                DebugStrings.Add(i + " " + materialVariable.name + ": #" + ColorUtility.ToHtmlStringRGBA(propColor));
            }
            if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    Vector4 propVector4 = substance.GetProceduralVector(materialVariable.name);
                    materialVariableList.PropertyVector4.Add(propVector4);
                    materialVariableList.myKeys.Add(materialVariable.name);
                    materialVariableList.myValues.Add(propVector4.ToString());
                    DebugStrings.Add(i + " " + materialVariable.name + ": " + propVector4.ToString());
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    Vector3 propVector3 = substance.GetProceduralVector(materialVariable.name);
                    materialVariableList.PropertyVector3.Add(propVector3);
                    materialVariableList.myKeys.Add(materialVariable.name);
                    materialVariableList.myValues.Add(propVector3.ToString());
                    DebugStrings.Add(i + " " + materialVariable.name + ": " + propVector3.ToString());
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    Vector2 propVector2 = substance.GetProceduralVector(materialVariable.name);
                    materialVariableList.PropertyVector2.Add(propVector2);
                    materialVariableList.myKeys.Add(materialVariable.name);
                    materialVariableList.myValues.Add(propVector2.ToString());
                    DebugStrings.Add(i + " " + materialVariable.name + ": " + propVector2.ToString());
                }
            }
            if (propType == ProceduralPropertyType.Enum)
            {
                int propEnum = substance.GetProceduralEnum(materialVariable.name);
                materialVariableList.PropertyEnum.Add(propEnum);
                materialVariableList.myKeys.Add(materialVariable.name);
                materialVariableList.myValues.Add(propEnum.ToString());
            }
            if (propType == ProceduralPropertyType.Boolean)
            {
                bool propBool = substance.GetProceduralBoolean(materialVariable.name);
                materialVariableList.PropertyBool.Add(propBool);
                materialVariableList.myKeys.Add(materialVariable.name);
                materialVariableList.myValues.Add(propBool.ToString());

            }
        }
        materialVariableList.PropertyMaterialName = substance.name;
        materialVariableList.emissionColor = emissionInput;
        materialVariableList.MainTex = MainTexOffset;
        if (saveOutputParameters)
            materialVariableList.hasParametersWithoutRange = true;
        else
            materialVariableList.hasParametersWithoutRange = false;
        materialVariableList.animationTime = animationTime;
    }

    void AddProceduralVariablesToDictionary(MaterialVariableDictionaryHolder materialVariableDictionary) // Adds substance values to a dictionary 
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
        materialVariableDictionary.animationTime = animationTime;
    }

    public void AddProceduralVariablesToDictionaryFromList(MaterialVariableDictionaryHolder dictionary, MaterialVariableListHolder list) // sorts items from a list into a dictionary
    {
        if (substance)
        {
            for (int i = 0; i < this.materialVariables.Length; i++)
            {
                ProceduralPropertyDescription materialVariable = this.materialVariables[i];
                ProceduralPropertyType propType = this.materialVariables[i].type;
                if (!dictionary.PropertyDictionary.ContainsKey(materialVariable.name))
                    dictionary.PropertyDictionary.Add(materialVariable.name, list.myValues[i]);
                if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
                {
                    if (!dictionary.PropertyDictionary.ContainsKey(materialVariable.name))
                    {
                        dictionary.PropertyDictionary.Add(materialVariable.name, float.Parse(list.myValues[i]));
                        dictionary.PropertyFloatDictionary.Add(materialVariable.name, float.Parse(list.myValues[i]));
                    }
                    else // if it already contains the key overwrite it
                        dictionary.PropertyFloatDictionary[materialVariable.name] = float.Parse(list.myValues[i]);
                }
                if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
                {
                    Color propColor = substance.GetProceduralColor(materialVariable.name);
                    if (!dictionary.PropertyDictionary.ContainsKey(materialVariable.name))
                    {
                        dictionary.PropertyDictionary.Add(materialVariable.name, propColor);
                        ColorUtility.TryParseHtmlString(list.myValues[i], out propColor);
                        dictionary.PropertyColorDictionary.Add(materialVariable.name, propColor);
                    }
                    else
                    {
                        ColorUtility.TryParseHtmlString(list.myValues[i], out propColor);
                        dictionary.PropertyColorDictionary[materialVariable.name] = propColor;
                    }
                }
                if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange)))
                {
                    if (propType == ProceduralPropertyType.Vector4)
                    {
                        if (!dictionary.PropertyDictionary.ContainsKey(materialVariable.name))
                        {
                            Vector4 propVector4 = substance.GetProceduralVector(materialVariable.name);
                            dictionary.PropertyDictionary.Add(materialVariable.name, propVector4);
                            dictionary.PropertyVector4Dictionary.Add(materialVariable.name, StringToVector(list.myValues[i], 4));
                        }
                        else
                            dictionary.PropertyVector4Dictionary[materialVariable.name] = StringToVector(list.myValues[i], 4);
                    }
                    else if (propType == ProceduralPropertyType.Vector3)
                    {
                        if (!dictionary.PropertyDictionary.ContainsKey(materialVariable.name))
                        {
                            Vector3 propVector3 = substance.GetProceduralVector(materialVariable.name);
                            dictionary.PropertyDictionary.Add(materialVariable.name, propVector3);
                            dictionary.PropertyVector3Dictionary.Add(materialVariable.name, StringToVector(list.myValues[i], 3));
                        }
                        else
                            dictionary.PropertyVector3Dictionary[materialVariable.name] = StringToVector(list.myValues[i], 3);
                    }
                    else if (propType == ProceduralPropertyType.Vector2)
                    {
                        Vector2 propVector2 = substance.GetProceduralVector(materialVariable.name);
                        if (!dictionary.PropertyDictionary.ContainsKey(materialVariable.name))
                        {
                            dictionary.PropertyDictionary.Add(materialVariable.name, propVector2);
                            dictionary.PropertyVector2Dictionary.Add(materialVariable.name, StringToVector(list.myValues[i], 2));
                        }
                        else
                            dictionary.PropertyVector2Dictionary[materialVariable.name] = StringToVector(list.myValues[i], 2);
                    }
                }
                if (propType == ProceduralPropertyType.Enum)
                {
                    if (!dictionary.PropertyDictionary.ContainsKey(materialVariable.name))
                    {
                        int propEnum = substance.GetProceduralEnum(materialVariable.name);
                        dictionary.PropertyDictionary.Add(materialVariable.name, propEnum);
                        dictionary.PropertyEnumDictionary.Add(materialVariable.name, int.Parse(list.myValues[i]));
                    }
                    else
                        dictionary.PropertyEnumDictionary[materialVariable.name] = int.Parse(list.myValues[i]);
                }

                if (propType == ProceduralPropertyType.Boolean)
                {
                    if (!dictionary.PropertyDictionary.ContainsKey(materialVariable.name))
                    {
                        bool propBool = substance.GetProceduralBoolean(materialVariable.name);
                        dictionary.PropertyDictionary.Add(materialVariable.name, propBool);
                        dictionary.PropertyBoolDictionary.Add(materialVariable.name, bool.Parse(list.myValues[i]));
                    }
                    else
                        dictionary.PropertyBoolDictionary[materialVariable.name] = bool.Parse(list.myValues[i]);
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
                for (int j = 0; j < propertyList.myKeys.Count(); j++)
                {
                    if (propertyList.myKeys[j] == materialVariable.name)
                    {
                        if (propertyList.myKeys[j] == materialVariable.name)
                            substance.SetProceduralFloat(materialVariable.name, float.Parse(propertyList.myValues[j]));
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
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
            else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange) || resettingValuesToDefault))
            {
                if (propType == ProceduralPropertyType.Vector4)
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
                else if (propType == ProceduralPropertyType.Vector3)
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
                else if (propType == ProceduralPropertyType.Vector2)
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
            else if (propType == ProceduralPropertyType.Enum)
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
            else if (propType == ProceduralPropertyType.Boolean)
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

    public void ResetProceduralValuesOnUndo() // when undoing any slider/field/enum the list/dictionary/parameter gets reset but the material will still look the same so i have to change it tosomething else then back.
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
        return flickerCalc;
    }
}