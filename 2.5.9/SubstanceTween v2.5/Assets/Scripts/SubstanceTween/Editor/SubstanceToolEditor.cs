using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
/// <summary>
/// Main inspector tool
/// </summary>
[CustomEditor(typeof(SubstanceTool))]
[CanEditMultipleObjects]
[System.Serializable]
public class SubstanceToolEditor : Editor
{
    public SubstanceAnimationParams animationParams = new SubstanceAnimationParams();
    public SubstanceFlickerParams flickerValues = new SubstanceFlickerParams();
    public SubstanceRandomizeParams randomizeSettings = new SubstanceRandomizeParams();
    public SubstanceToolParams substanceToolParams = new SubstanceToolParams();
    public SubstancePerformanceParams substancePerformanceParams = new SubstancePerformanceParams();
    public SubstanceDefaultMaterialParams substanceDefaultMaterialParams = new SubstanceDefaultMaterialParams();
    public SubstanceMaterialParams substanceMaterialParams = new SubstanceMaterialParams();
    public SubstanceGUIStyleParams substanceGUIStyleParams = new SubstanceGUIStyleParams();
    protected SubstanceImporter substanceImporter;
    public bool repaintInspectorConstantly;
    public override bool RequiresConstantRepaint() { return repaintInspectorConstantly; } // makes the inspector constantly update. This allows the curve editor recieves input from the right/middle button
    public SubstanceToolEditor substanceEditorTesting;
    public static FieldInfo m_CurveField;

    public void OnEnable()
    {
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        StartupTool();
    }
    public void OnDisable()
    {
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        if (substanceToolParams.selectedPrefabScript && animationParams.substanceLerp)
        { // user selects another object/nothing at all with animation running
            substanceToolParams.selectedPrefabScript.animationToggle = true;
            animationParams.substanceLerp = false;
        }
        else if (substanceToolParams.selectedPrefabScript && !animationParams.substanceLerp)
        {// user selects another object/nothing at all with animation paused
            substanceToolParams.selectedPrefabScript.animationToggle = false;
            animationParams.substanceLerp = false;
        }
        animationParams.defaultSubstanceIndex = 0;
    }

    public void OnBeforeAssemblyReload()
    {// when compiling some variables become null if animated object is selected and that animated object has a prefab type of 'none' or 'disconnected'
        if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<SubstanceTool>() != null && (PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.None || PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.DisconnectedPrefabInstance || PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.MissingPrefabInstance))
        {// also check if prefab type is none || disconnected
            Selection.activeGameObject = null;
        }
    }

    public void StartupTool()
    {
        if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<SubstanceTool>() && Selection.activeGameObject.GetComponent<SubstanceTool>().isActiveAndEnabled && Selection.activeGameObject.GetComponent<Renderer>() && Selection.activeGameObject.GetComponent<Renderer>().sharedMaterial && Selection.activeGameObject.activeSelf)
        {
            substanceToolParams.currentSelection = Selection.activeGameObject;

            if (substanceToolParams.currentSelection.GetComponent<SubstanceTweenTesterParams>() != null)
            {// these are for unit testing
                substanceToolParams.currentSelection.GetComponent<SubstanceTweenTesterParams>().animationParams = animationParams;
                substanceToolParams.currentSelection.GetComponent<SubstanceTweenTesterParams>().substanceDefaultMaterialParams = substanceDefaultMaterialParams;
                substanceToolParams.currentSelection.GetComponent<SubstanceTweenTesterParams>().substanceGUIStyleParams = substanceGUIStyleParams;
                substanceToolParams.currentSelection.GetComponent<SubstanceTweenTesterParams>().substanceMaterialParams = substanceMaterialParams;
                substanceToolParams.currentSelection.GetComponent<SubstanceTweenTesterParams>().substancePerformanceParams = substancePerformanceParams;
                substanceToolParams.currentSelection.GetComponent<SubstanceTweenTesterParams>().substanceToolParams = substanceToolParams;
                substanceToolParams.currentSelection.GetComponent<SubstanceTweenTesterParams>().referenceToEditorScript = this;
            }
            if (substanceToolParams.currentSelection)
                substanceMaterialParams.rend = substanceToolParams.currentSelection.GetComponent<Renderer>();

            if (substanceToolParams.currentSelection.GetComponent<PrefabProperties>() != null && substanceToolParams.currentSelection.GetComponent<PrefabProperties>().isActiveAndEnabled) // if object has the predab script get all of the keyframe/animation information from that script
            {
                substanceToolParams.selectedPrefabScript = substanceToolParams.currentSelection.GetComponent<PrefabProperties>();
                substanceMaterialParams.rend = substanceToolParams.selectedPrefabScript.rend;
                if (substanceMaterialParams.rend)
                    substanceMaterialParams.substance = substanceMaterialParams.rend.sharedMaterial as ProceduralMaterial;
                if (substanceMaterialParams.substance)
                {
                    if (substanceDefaultMaterialParams.selectedStartupMaterials.Count <= 0)
                    {
                        substanceDefaultMaterialParams.selectedStartupMaterials.Add(substanceMaterialParams.substance);
                        SubstanceTweenStorageUtility.AddDefaultMaterial(substanceMaterialParams, animationParams, substanceToolParams, substanceDefaultMaterialParams);
                    }
                    substanceMaterialParams.materialVariables = substanceMaterialParams.substance.GetProceduralPropertyDescriptions();
                    for (int i = 0; i <= substanceMaterialParams.materialVariables.Count() - 1; i++)
                    {
                        substanceMaterialParams.materialVariableNames.Add(substanceMaterialParams.materialVariables[i].name);
                    }
                    substanceToolParams.selectedPrefabScript.useSharedMaterial = true;
                    substanceToolParams.selectedPrefabScript.animationToggle = false;
                    animationParams.keyFrameTimes = substanceToolParams.selectedPrefabScript.keyFrameTimes;
                    animationParams.keyFrames = substanceToolParams.selectedPrefabScript.keyFrameTimes.Count;
                    animationParams.substanceCurve = substanceToolParams.selectedPrefabScript.prefabAnimationCurve;
                    animationParams.substanceCurve.keys = substanceToolParams.selectedPrefabScript.prefabAnimationCurve.keys;
                    animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;

                    substanceMaterialParams.MaterialVariableKeyframeDictionaryList = substanceToolParams.selectedPrefabScript.MaterialVariableKeyframeDictionaryList;
                    substanceMaterialParams.MaterialVariableKeyframeList = substanceToolParams.selectedPrefabScript.MaterialVariableKeyframeList;
                    substanceMaterialParams.reorderList = new ReorderableList(substanceMaterialParams.MaterialVariableKeyframeList, typeof(MaterialVariableListHolder), true, true, false, false);
                    substanceMaterialParams.reorderList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "Keyframe: " + index);
                        if (!animationParams.substanceLerp)
                        {
                            if (GUI.Button(new Rect(rect.x + 80, rect.y, 60, EditorGUIUtility.singleLineHeight), "Remove"))
                            {
                                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, }, "Remove Keyframe " + index);
                                SubstanceTweenKeyframeUtility.DeleteKeyframe(index, substanceMaterialParams, animationParams);
                                Repaint();
                                return; // Without returning i get a error when deleting the last keyframe
                            }
                        }
                        if (GUI.Button(new Rect(rect.x + 145, rect.y, 50, EditorGUIUtility.singleLineHeight), "Select"))
                        {
                            SubstanceTweenKeyframeUtility.SelectKeyframe(index, substanceMaterialParams, animationParams, substanceToolParams);
                            animationParams.keyframeSum = 0;
                            for (int j = 0; j < index; j++)
                            {
                                animationParams.keyframeSum += animationParams.keyFrameTimes[j];
                            };
                            animationParams.currentKeyframeIndex = index;
                            if (index < animationParams.keyFrameTimes.Count - 1)
                            {
                                animationParams.currentAnimationTime = 0;
                                animationParams.animationTimeRestartEnd = animationParams.keyframeSum;
                            }
                            else
                            {
                                animationParams.currentAnimationTime = animationParams.keyFrameTimes[index];
                                animationParams.animationTimeRestartEnd = animationParams.keyframeSum;
                            }
                            SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
                            Repaint();
                        }
                        if (GUI.Button(new Rect(rect.x + 200, rect.y, 70, EditorGUIUtility.singleLineHeight), "Overwrite"))
                        {
                            Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "Overwrite keyframe " + index);
                            SubstanceTweenKeyframeUtility.OverWriteKeyframe(index, substanceMaterialParams, animationParams, substanceToolParams);
                            Repaint();
                        }

                        if (index < animationParams.keyFrameTimes.Count - 1 || animationParams.keyFrameTimes.Count == 1)
                        {
                            if (animationParams.keyFrameTimes[index] > 0) // display float value for the keyframe index. if it changes rebuild the animation curve 
                            {
                                EditorGUI.BeginChangeCheck();
                                animationParams.keyFrameTimes[index] = EditorGUI.DelayedFloatField(new Rect(rect.x + 280, rect.y, 50, EditorGUIUtility.singleLineHeight), animationParams.keyFrameTimes[index]);
                                if (EditorGUI.EndChangeCheck() && animationParams.keyFrameTimes[index] > 0) // check if a Animation time has changed and if this after first keyframe(0,0), 'i' is index of changed keyframe
                                {
                                    Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this }, "Overwrite keyframe " + index);
                                    List<Keyframe> tmpKeyframeList = animationParams.substanceCurve.keys.ToList();
                                    animationParams.keyframeSum = 0;
                                    for (int j = substanceMaterialParams.MaterialVariableKeyframeList.Count() - 1; j > 0; j--)// remove all keys
                                    {
                                        animationParams.substanceCurve.RemoveKey(j);
                                    }
                                    for (int j = 0; j < substanceMaterialParams.MaterialVariableKeyframeList.Count(); j++)//rewrite keys with changed times
                                    {
                                        if (j == 0)
                                            animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum, 0.25f, tmpKeyframeList[j].outTangent));
                                        else
                                            animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum, tmpKeyframeList[j].inTangent, tmpKeyframeList[j].outTangent));
                                        if (substanceToolParams.reWriteAllKeyframeTimes)
                                        {
                                            for (int k = 0; k < animationParams.keyFrameTimes.Count() - 1; k++)
                                            {
                                                animationParams.keyFrameTimes[k] = animationParams.keyFrameTimes[index];
                                                substanceMaterialParams.MaterialVariableKeyframeList[k].animationTime = animationParams.keyFrameTimes[index];
                                            }
                                            animationParams.keyframeSum += animationParams.keyFrameTimes[index];
                                        }
                                        else
                                            animationParams.keyframeSum += animationParams.keyFrameTimes[j];
                                    }
                                    substanceMaterialParams.MaterialVariableKeyframeList[index].animationTime = animationParams.keyFrameTimes[index];
                                    animationParams.currentAnimationTime = 0;//Reset animation variables
                                    animationParams.currentKeyframeIndex = 0;
                                    animationParams.animationTimeRestartEnd = 0;
                                    animationParams.keyframeSum -= animationParams.keyFrameTimes[animationParams.keyFrameTimes.Count() - 1]; //gets rid of last keyframe time.
                                    animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
                                }
                            }
                            else
                            {
                                animationParams.keyFrameTimes[index] = EditorGUILayout.DelayedFloatField(substanceMaterialParams.MaterialVariableKeyframeList[index].animationTime);
                                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this }, "Overwrite keyframe " + index);
                                List<Keyframe> tmpKeyframeList = animationParams.substanceCurve.keys.ToList();
                                animationParams.keyframeSum = 0;
                                for (int j = substanceMaterialParams.MaterialVariableKeyframeList.Count() - 1; j > 0; j--)// remove all keys
                                {
                                    animationParams.substanceCurve.RemoveKey(j);
                                }
                                for (int j = 0; j < substanceMaterialParams.MaterialVariableKeyframeList.Count(); j++)//rewrite keys with changed times
                                {
                                    if (j == 0)
                                        animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum, 0.25f, tmpKeyframeList[j].outTangent));
                                    else
                                        animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum, tmpKeyframeList[j].inTangent, tmpKeyframeList[j].outTangent));
                                    animationParams.keyframeSum += animationParams.keyFrameTimes[j];
                                }
                                substanceMaterialParams.MaterialVariableKeyframeList[index].animationTime = animationParams.keyFrameTimes[index];
                                animationParams.currentAnimationTime = 0;//Reset animation variables
                                animationParams.currentKeyframeIndex = 0;
                                animationParams.animationTimeRestartEnd = 0;
                                animationParams.keyframeSum -= animationParams.keyFrameTimes[animationParams.keyFrameTimes.Count() - 1]; //gets rid of last keyframe time.
                            }
                            SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
                        }
                    };

                    substanceMaterialParams.reorderList.onReorderCallback = (list) =>
                    {
                        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance }, "Reorder Keyframe: " + substanceMaterialParams.reorderIndexInt + " To: " + list.index);
                        if (substanceMaterialParams.reorderIndexInt != list.index)
                        {
                            Swap(substanceMaterialParams.MaterialVariableKeyframeDictionaryList, substanceMaterialParams.reorderIndexInt, list.index);
                            Swap(animationParams.keyFrameTimes, substanceMaterialParams.reorderIndexInt, list.index);
                            List<Keyframe> tmpKeyframeList = animationParams.substanceCurve.keys.ToList();
                            animationParams.keyframeSum = 0;
                            for (int j = animationParams.keyFrameTimes.Count() - 1; j > 0; j--)// remove all keys
                            {
                                animationParams.substanceCurve.RemoveKey(j);
                            }
                            for (int j = 0; j < animationParams.keyFrameTimes.Count(); j++)//rewrite keys with changed times
                            {
                                if (j == 0)
                                    animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum, 0.25f, tmpKeyframeList[j].outTangent));
                                else
                                    animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum, tmpKeyframeList[j].inTangent, tmpKeyframeList[j].outTangent));
                                if (substanceToolParams.reWriteAllKeyframeTimes)
                                {
                                    for (int k = 0; k < animationParams.keyFrameTimes.Count() - 1; k++)
                                    {
                                        animationParams.keyFrameTimes[k] = animationParams.keyFrameTimes[substanceMaterialParams.reorderIndexInt];
                                        substanceMaterialParams.MaterialVariableKeyframeList[k].animationTime = animationParams.keyFrameTimes[substanceMaterialParams.reorderIndexInt];
                                    }
                                    animationParams.keyframeSum += animationParams.keyFrameTimes[substanceMaterialParams.reorderIndexInt];
                                }
                                else
                                    animationParams.keyframeSum += animationParams.keyFrameTimes[j];
                            }
                            substanceMaterialParams.MaterialVariableKeyframeList[substanceMaterialParams.reorderIndexInt].animationTime = animationParams.keyFrameTimes[substanceMaterialParams.reorderIndexInt];
                            animationParams.currentAnimationTime = 0;//Reset animation variables
                            animationParams.currentKeyframeIndex = 0;
                            animationParams.animationTimeRestartEnd = 0;
                            animationParams.keyframeSum -= animationParams.keyFrameTimes[animationParams.keyFrameTimes.Count() - 1]; //gets rid of last keyframe time.
                            animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
                        }
                    };

                    substanceMaterialParams.reorderList.onSelectCallback = (list) =>
                    {
                        substanceMaterialParams.reorderIndexInt = list.index;
                    };

                    if (substanceMaterialParams.substance.HasProceduralProperty("$outputsize") && substanceMaterialParams.MaterialVariableKeyframeList.Count > 0 && substanceMaterialParams.MaterialVariableKeyframeList[0].myVector2Keys.Count > 0) // makes sure output size is based off the one in the first list
                    {
                        substanceMaterialParams.substance.SetProceduralVector("$outputsize", substanceMaterialParams.MaterialVariableKeyframeList[0].myVector2Values[substanceMaterialParams.MaterialVariableKeyframeList[0].myVector2Keys.IndexOf("$outputsize")]);
                    }
                    if (substanceMaterialParams.MaterialVariableKeyframeList.Count >= 2)
                    {
                        SubstanceTweenKeyframeUtility.SelectAndOverWriteAllKeyframes(substanceMaterialParams, animationParams, substanceToolParams);
                        Repaint();
                    }
                    if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor"))
                    {
                        substanceMaterialParams.emissionInput = substanceMaterialParams.rend.sharedMaterial.GetColor("_EmissionColor");
                        substanceToolParams.selectedPrefabScript.emissionInput = substanceMaterialParams.emissionInput;
                        substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", substanceMaterialParams.emissionInput);
                    }
                }

                if (substanceMaterialParams.substance)
                {
                    substanceMaterialParams.substanceAssetName = AssetDatabase.GetAssetPath(substanceMaterialParams.substance);
                    if (substanceMaterialParams.substanceAssetName != String.Empty)
                    {
                        substanceImporter = AssetImporter.GetAtPath(substanceMaterialParams.substanceAssetName) as SubstanceImporter;
                        substanceImporter.GetPlatformTextureSettings(substanceMaterialParams.substanceAssetName, "", out substanceMaterialParams.substanceWidth, out substanceMaterialParams.substanceHeight, out substanceMaterialParams.substanceTextureFormat, out substanceMaterialParams.substanceLoadBehavior);
                    }
                    substanceMaterialParams.substanceHeight = (int)substanceMaterialParams.substance.GetProceduralVector("$outputSize").y; // 8 is 512x512 by default if the right output settings are chosen in substance designer.
                    substanceMaterialParams.substanceWidth = (int)substanceMaterialParams.substance.GetProceduralVector("$outputSize").x;
                    substancePerformanceParams.myProceduralCacheSize = SubstancePerformanceParams.MyProceduralCacheSize.NoLimit;
                    substanceMaterialParams.substance.cacheSize = ProceduralCacheSize.NoLimit;
                    if (animationParams.keyFrames > 0)
                        SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
                    substanceMaterialParams.substance.RebuildTextures();
                }
                substanceDefaultMaterialParams.selectedStartupMaterials.Add(substanceMaterialParams.substance); // First selected object material 
            }
            if (!Selection.activeGameObject.GetComponent<PrefabProperties>())
            {
                substanceToolParams.DebugStrings.Add("Opened Tool");
                Selection.activeGameObject.AddComponent<PrefabProperties>();
                substanceToolParams.selectedPrefabScript = Selection.activeGameObject.GetComponent<PrefabProperties>();
                substanceToolParams.selectedPrefabScript.useSharedMaterial = true;
                substanceToolParams.selectedPrefabScript.animationToggle = false;
                substanceToolParams.selectedPrefabScript.rend = substanceMaterialParams.rend;
                substanceMaterialParams.substance = substanceMaterialParams.rend.sharedMaterial as ProceduralMaterial;
                substanceToolParams.selectedPrefabScript.substance = substanceMaterialParams.substance; // Makes it so anytime i edit substance it will affect prefabScript.substance.
                substanceToolParams.selectedPrefabScript.keyFrameTimes = animationParams.keyFrameTimes;
                substanceToolParams.selectedPrefabScript.keyFrames = animationParams.keyFrames;
                substanceToolParams.selectedPrefabScript.prefabAnimationCurve = new AnimationCurve();
                animationParams.substanceCurve = substanceToolParams.selectedPrefabScript.prefabAnimationCurve;
                animationParams.substanceCurve.keys = substanceToolParams.selectedPrefabScript.prefabAnimationCurve.keys;
                animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
                substanceMaterialParams.MaterialVariableKeyframeDictionaryList = substanceToolParams.selectedPrefabScript.MaterialVariableKeyframeDictionaryList;
                substanceMaterialParams.MaterialVariableKeyframeList = substanceToolParams.selectedPrefabScript.MaterialVariableKeyframeList;
                substancePerformanceParams.myProceduralCacheSize = (SubstancePerformanceParams.MyProceduralCacheSize)ProceduralCacheSize.NoLimit;
                if (substanceMaterialParams.substance)
                    substanceMaterialParams.substance.cacheSize = ProceduralCacheSize.NoLimit;
                substancePerformanceParams.mySubstanceProcessorUsage = (SubstancePerformanceParams.MySubstanceProcessorUsage)ProceduralProcessorUsage.All;
                ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;
                if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    substanceMaterialParams.emissionInput = substanceMaterialParams.rend.sharedMaterial.GetColor("_EmissionColor");
                    substanceToolParams.selectedPrefabScript.emissionInput = substanceMaterialParams.emissionInput;

                    substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", substanceMaterialParams.emissionInput);
                }
                substanceToolParams.DebugStrings.Add("First object selected: " + substanceToolParams.currentSelection + " Selected objects material name: " + substanceMaterialParams.rend.sharedMaterial.name);
            }

            if (substanceDefaultMaterialParams.selectedStartupMaterials.Count > 0)
            {
                for (int i = 0; i <= substanceDefaultMaterialParams.selectedStartupMaterials.Count() - 1; i++)
                {
                    if (substanceMaterialParams.substance.name != substanceDefaultMaterialParams.selectedStartupMaterials[i].name)
                    {
                        SubstanceTweenStorageUtility.AddDefaultMaterial(substanceMaterialParams, animationParams, substanceToolParams, substanceDefaultMaterialParams);
                    }
                }
            }
            Repaint();
            EditorApplication.update += Update; // allows me to use Update() as it appears by default in a MonoBehavior script.
        }
    }

    public static void Swap<T>(IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }

    // All toggles and settings for the menu bar.
    public void HideNonAnimatingParametersFunction()
    {
        if (animationParams.hideNonAnimatingVariables)
            animationParams.hideNonAnimatingVariables = false;
        else
            animationParams.hideNonAnimatingVariables = true;
    }
    public void SaveOutputMenuFunction()
    {
        if (substanceMaterialParams.saveOutputParameters)
            substanceMaterialParams.saveOutputParameters = false;
        else
            substanceMaterialParams.saveOutputParameters = true;
    }
    public void AnimateOutputParamMenuFunction()
    {
        if (animationParams.animateOutputParameters)
            animationParams.animateOutputParameters = false;
        else
            animationParams.animateOutputParameters = true;
    }
    public void RebuildImmediatelyMenuFunction()
    {
        if (substanceMaterialParams.rebuildSubstanceImmediately)
            substanceMaterialParams.rebuildSubstanceImmediately = false;
        else
            substanceMaterialParams.rebuildSubstanceImmediately = true;
    }
    public void CacheMaterialMenuFunction()
    {
        if (animationParams.cacheSubstance)
            animationParams.cacheSubstance = false;
        else
            animationParams.cacheSubstance = true;
    }

    public void ProcessorUsageAllMenuFunction()
    {
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;
        substancePerformanceParams.mySubstanceProcessorUsage = (SubstancePerformanceParams.MySubstanceProcessorUsage)ProceduralProcessorUsage.All;
        substancePerformanceParams.ProcessorUsageAllMenuToggle = true;
        substancePerformanceParams.ProcessorUsageHalfMenuToggle = false;
        substancePerformanceParams.ProcessorUsageOneMenuToggle = false;
        substancePerformanceParams.ProcessorUsageUnsupportedMenuToggle = false;
    }
    public void ProcessorUsageOneMenuFunction()
    {
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.One;
        substancePerformanceParams.mySubstanceProcessorUsage = (SubstancePerformanceParams.MySubstanceProcessorUsage)ProceduralProcessorUsage.One;
        substancePerformanceParams.ProcessorUsageOneMenuToggle = true;
        substancePerformanceParams.ProcessorUsageAllMenuToggle = false;
        substancePerformanceParams.ProcessorUsageHalfMenuToggle = false;
        substancePerformanceParams.ProcessorUsageUnsupportedMenuToggle = false;
    }
    public void ProcessorUsageHalfMenuFunction()
    {
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.Half;
        substancePerformanceParams.mySubstanceProcessorUsage = (SubstancePerformanceParams.MySubstanceProcessorUsage)ProceduralProcessorUsage.Half;
        substancePerformanceParams.ProcessorUsageHalfMenuToggle = true;
        substancePerformanceParams.ProcessorUsageOneMenuToggle = false;
        substancePerformanceParams.ProcessorUsageAllMenuToggle = false;
        substancePerformanceParams.ProcessorUsageUnsupportedMenuToggle = false;
    }
    public void ProcessorUsageUnsupportedMenuFunction()
    {
        ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.Unsupported;
        substancePerformanceParams.mySubstanceProcessorUsage = (SubstancePerformanceParams.MySubstanceProcessorUsage)ProceduralProcessorUsage.Unsupported;
        substancePerformanceParams.ProcessorUsageUnsupportedMenuToggle = true;
        substancePerformanceParams.ProcessorUsageHalfMenuToggle = false;
        substancePerformanceParams.ProcessorUsageOneMenuToggle = false;
        substancePerformanceParams.ProcessorUsageAllMenuToggle = false;
    }
    public void ProceduralCacheSizeNoLimitMenuFunction()
    {
        substancePerformanceParams.myProceduralCacheSize = SubstancePerformanceParams.MyProceduralCacheSize.NoLimit;
        substanceMaterialParams.substance.cacheSize = ProceduralCacheSize.NoLimit;
        substancePerformanceParams.ProceduralCacheSizeNoLimitMenuToggle = true;
        substancePerformanceParams.ProceduralCacheSizeHeavyMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeMediumMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeTinyMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeNoneMenuToggle = false;
    }
    public void ProceduralCacheSizeHeavyMenuFunction()
    {
        substancePerformanceParams.myProceduralCacheSize = SubstancePerformanceParams.MyProceduralCacheSize.Heavy;
        substanceMaterialParams.substance.cacheSize = ProceduralCacheSize.Heavy;
        substancePerformanceParams.ProceduralCacheSizeNoLimitMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeHeavyMenuToggle = true;
        substancePerformanceParams.ProceduralCacheSizeMediumMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeTinyMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeNoneMenuToggle = false;
    }
    public void ProceduralCacheSizeMediumMenuFunction()
    {
        substancePerformanceParams.myProceduralCacheSize = SubstancePerformanceParams.MyProceduralCacheSize.Medium;
        substanceMaterialParams.substance.cacheSize = ProceduralCacheSize.Medium;
        substancePerformanceParams.ProceduralCacheSizeNoLimitMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeHeavyMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeMediumMenuToggle = true;
        substancePerformanceParams.ProceduralCacheSizeTinyMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeNoneMenuToggle = false;
    }
    public void ProceduralCacheSizeTinyMenuFunction()
    {
        substancePerformanceParams.myProceduralCacheSize = SubstancePerformanceParams.MyProceduralCacheSize.Tiny;
        substanceMaterialParams.substance.cacheSize = ProceduralCacheSize.Tiny;
        substancePerformanceParams.ProceduralCacheSizeNoLimitMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeHeavyMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeMediumMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeTinyMenuToggle = true;
        substancePerformanceParams.ProceduralCacheSizeNoneMenuToggle = false;
    }
    public void ProceduralCacheSizeNoneMenuFunction()
    {
        substancePerformanceParams.myProceduralCacheSize = SubstancePerformanceParams.MyProceduralCacheSize.None;
        substanceMaterialParams.substance.cacheSize = ProceduralCacheSize.None;
        substancePerformanceParams.ProceduralCacheSizeNoLimitMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeHeavyMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeMediumMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeTinyMenuToggle = false;
        substancePerformanceParams.ProceduralCacheSizeNoneMenuToggle = true;
    }
    public void RandomizeFloatToggleFunction()
    {
        if (randomizeSettings.randomizeProceduralFloatToggle)
            randomizeSettings.randomizeProceduralFloatToggle = false;
        else
            randomizeSettings.randomizeProceduralFloatToggle = true;
    }
    public void RandomizeColorRGBToggleFunction()
    {
        if (randomizeSettings.randomizeProceduralColorRGBToggle)
            randomizeSettings.randomizeProceduralColorRGBToggle = false;
        else
            randomizeSettings.randomizeProceduralColorRGBToggle = true;
    }
    public void RandomizeColorRGBAToggleFunction()
    {
        if (randomizeSettings.randomizeProceduralColorRGBAToggle)
            randomizeSettings.randomizeProceduralColorRGBAToggle = false;
        else
            randomizeSettings.randomizeProceduralColorRGBAToggle = true;
    }
    public void RandomizeVector2ToggleFunction()
    {
        if (randomizeSettings.randomizeProceduralVector2Toggle)
            randomizeSettings.randomizeProceduralVector2Toggle = false;
        else
            randomizeSettings.randomizeProceduralVector2Toggle = true;
    }
    public void RandomizeVector3ToggleFunction()
    {
        if (randomizeSettings.randomizeProceduralVector3Toggle)
            randomizeSettings.randomizeProceduralVector3Toggle = false;
        else
            randomizeSettings.randomizeProceduralVector3Toggle = true;
    }
    public void RandomizeVector4ToggleFunction()
    {
        if (randomizeSettings.randomizeProceduralVector4Toggle)
            randomizeSettings.randomizeProceduralVector4Toggle = false;
        else
            randomizeSettings.randomizeProceduralVector4Toggle = true;
    }
    public void RandomizeEnumToggleFunction()
    {
        if (randomizeSettings.randomizeProceduralEnumToggle)
            randomizeSettings.randomizeProceduralEnumToggle = false;
        else
            randomizeSettings.randomizeProceduralEnumToggle = true;
    }
    public void RandomizeBooleanToggleFunction()
    {
        if (randomizeSettings.randomizeProceduralBooleanToggle)
            randomizeSettings.randomizeProceduralBooleanToggle = false;
        else
            randomizeSettings.randomizeProceduralBooleanToggle = true;
    }
    public void ShowVariableInformationFunction()
    {
        if (substanceToolParams.showVariableInformationToggle)
            substanceToolParams.showVariableInformationToggle = false;
        else
            substanceToolParams.showVariableInformationToggle = true;
    }
    public void DisplayAboutDialog()
    {
        EditorUtility.DisplayDialog("About", "SubstanceTween Ver 2.5.9 - 2/21/2018 \n Written by: Chris Dougherty \n  https://www.linkedin.com/in/archarchaic \n chris.ll.dougherty@gmail.com \n https://www.artstation.com/artist/archarchaic \n \n What does this tool do? \n This tool takes exposed parameters from substance designer files(SBAR) and allows you to create multiple key frames by manipulating the exposed Variables, creating transitions and animating them.\n " +
            "You can Write variables to XML files and read from them as well.When you are done creating your animated object you can save the object as a Prefab for future use. \n" +
     "Hotkeys: \n G - Repeat last action \n R = Randomize Variables \n CTRL + Z = Undo \n CTRL + Y = Redo ", "OK");
    }
    // - End of menu toggles and settings

    public override void OnInspectorGUI() // where content gets displayed.
    {
        EditorGUI.BeginChangeCheck();
        if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<SubstanceTool>() && Selection.activeGameObject.GetComponent<SubstanceTool>().isActiveAndEnabled && Selection.activeGameObject.GetComponent<Renderer>() && Selection.activeGameObject.GetComponent<Renderer>().sharedMaterial && Selection.activeGameObject.activeSelf)
        {
            substanceGUIStyleParams.toolbarStyle = new GUIStyle(EditorStyles.toolbar);
            substanceGUIStyleParams.toolbarDropdownStyle = new GUIStyle(EditorStyles.toolbarDropDown);
            substanceGUIStyleParams.toolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            substanceGUIStyleParams.toolbarPopupStyle = new GUIStyle(EditorStyles.toolbarPopup);
            substanceGUIStyleParams.styleAnimationButton = new GUIStyle(EditorStyles.toolbarButton);
            substanceGUIStyleParams.rightAnchorTextStyle = new GUIStyle();
            substanceGUIStyleParams.rightAnchorTextStyle.alignment = TextAnchor.MiddleRight;
            substanceGUIStyleParams.styleMainHeader = new GUIStyle();
            substanceGUIStyleParams.styleMainHeader.fontSize = 25;
            substanceGUIStyleParams.styleMainHeader.alignment = TextAnchor.UpperCenter;
#if UNITY_PRO_LICENSE
            styleMainHeader.normal.textColor = Color.white;
#endif
            substanceGUIStyleParams.errorTextStyle = new GUIStyle();
            substanceGUIStyleParams.errorTextStyle.fontSize = 15;
            substanceGUIStyleParams.errorTextStyle.wordWrap = true;

            if ((Event.current.type == EventType.ExecuteCommand || Event.current.type == EventType.ValidateCommand) && Event.current.commandName == "UndoRedoPerformed")
            {
                if (Undo.GetCurrentGroupName().Contains("Reorder"))
                {
                    SubstanceTweenKeyframeUtility.SelectAndOverWriteAllKeyframes(substanceMaterialParams, animationParams, substanceToolParams);
                    Repaint();
                }
                substanceMaterialParams.substance.RebuildTextures();
            }

            if (Event.current.type != EventType.ExecuteCommand || Event.current.commandName == "PopupMenuChanged" || Event.current.commandName == "ColorPickerChanged" || Event.current.commandName == "DelayedControlShouldCommit") //Drawing GUI while adding keys in the curve editor will give an error
            {
                if (substanceToolParams.selectedPrefabScript && substanceToolParams.selectedPrefabScript.animationToggle)
                {
                    substanceToolParams.selectedPrefabScript.animationToggle = false;
                    animationParams.substanceLerp = true;
                }

                if (substanceMaterialParams.MaterialVariableKeyframeList.Count >= 2 && animationParams.substanceLerp)
                    substanceGUIStyleParams.styleAnimationButton.normal.textColor = Color.green; // green for animating 
                else if (substanceMaterialParams.MaterialVariableKeyframeList.Count >= 2 && !animationParams.substanceLerp)
                    substanceGUIStyleParams.styleAnimationButton.normal.textColor = new Color(255, 204, 0); // yellow for pause/standby
                else
                    substanceGUIStyleParams.styleAnimationButton.normal.textColor = Color.red; // red for not ready to animate (needs 2 keyframes)

                if (!substanceToolParams.gameIsPaused && substanceToolParams.currentSelection != null && substanceMaterialParams.rend && substanceMaterialParams.substance && AssetDatabase.GetAssetPath(Selection.activeObject) == String.Empty)//Check if you are in play mode, game is not paused, a object is selected and active , it has a renderer and the object is not in the project view
                {
                    GUILayout.BeginHorizontal(substanceGUIStyleParams.toolbarStyle, GUILayout.Width(EditorGUIUtility.currentViewWidth));
                    EditorStyles.toolbar.hover.textColor = Color.red;
                    if (GUILayout.Button("Save-Load", substanceGUIStyleParams.toolbarDropdownStyle))
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
                        toolsMenu.AddItem(new GUIContent("Read JSON File"), false, ReadJSON);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Write All Keyframes To JSON Files"), false, WriteAllJSON);
                        toolsMenu.AddItem(new GUIContent("Create Keyframes From JSON Files"), false, ReadAllJSON);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Save Prefab"), false, SavePrefab);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Save Debug Text File"), false, WriteDebugText);
                        toolsMenu.DropDown(new Rect(Event.current.mousePosition.x, 0, 0, 16));
                        EditorGUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button("Set-Modify", substanceGUIStyleParams.toolbarDropdownStyle))
                    {
                        GenericMenu toolsMenu = new GenericMenu();
                        GUILayout.BeginHorizontal(EditorStyles.toolbar);
                        EditorStyles.toolbar.hover.textColor = Color.red;
                        toolsMenu.AddItem(new GUIContent("Set All Procedural Values To Minimum"), false, SetAllProceduralValuesToMin);
                        toolsMenu.AddItem(new GUIContent("Set All Procedural Values To Maximum"), false, SetAllProceduralValuesToMax);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Randomize All Procedural Values(R)"), false, RandomizeProceduralValues);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Floats"), randomizeSettings.randomizeProceduralFloatToggle, RandomizeFloatToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize RGB Colors"), randomizeSettings.randomizeProceduralColorRGBToggle, RandomizeColorRGBToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Vector2"), randomizeSettings.randomizeProceduralVector2Toggle, RandomizeVector2ToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Vector3"), randomizeSettings.randomizeProceduralVector3Toggle, RandomizeVector3ToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Vector4"), randomizeSettings.randomizeProceduralVector4Toggle, RandomizeVector4ToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Enums"), randomizeSettings.randomizeProceduralEnumToggle, RandomizeEnumToggleFunction);
                        toolsMenu.AddItem(new GUIContent("Randomize Settings/Randomize Booleans"), randomizeSettings.randomizeProceduralBooleanToggle, RandomizeBooleanToggleFunction);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Reset All Procedural Values To Default"), false, ResetAllProceduralValues);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("DELETE ALL KEYFRAMES"), false, DeleteAllKeyframes);
                        toolsMenu.DropDown(new Rect(Event.current.mousePosition.x, 0, 0, 16));
                    }
                    if (GUILayout.Button("Performance-Misc", substanceGUIStyleParams.toolbarDropdownStyle))
                    {
                        GenericMenu toolsMenu = new GenericMenu();
                        toolsMenu.AddItem(new GUIContent("About"), false, DisplayAboutDialog);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Hide Non Animating Parameters While Animating"), animationParams.hideNonAnimatingVariables, HideNonAnimatingParametersFunction);
                        toolsMenu.AddItem(new GUIContent("Save Output Paramaters($randomSeed-$outputSize)"), substanceMaterialParams.saveOutputParameters, SaveOutputMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Animate Output Paramaters($randomSeed-$outputSize)"), animationParams.animateOutputParameters, AnimateOutputParamMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Rebuild Substance Immediately(SLOW)"), substanceMaterialParams.rebuildSubstanceImmediately, RebuildImmediatelyMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Cache Substance"), animationParams.cacheSubstance, CacheMaterialMenuFunction);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Procedural Processor Usage/All"), substancePerformanceParams.ProcessorUsageAllMenuToggle, ProcessorUsageAllMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Processor Usage/Half"), substancePerformanceParams.ProcessorUsageHalfMenuToggle, ProcessorUsageHalfMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Processor Usage/One"), substancePerformanceParams.ProcessorUsageOneMenuToggle, ProcessorUsageOneMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Processor Usage/Unsupported"), substancePerformanceParams.ProcessorUsageUnsupportedMenuToggle, ProcessorUsageUnsupportedMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/No Limit"), substancePerformanceParams.ProceduralCacheSizeNoLimitMenuToggle, ProceduralCacheSizeNoLimitMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/Heavy"), substancePerformanceParams.ProceduralCacheSizeHeavyMenuToggle, ProceduralCacheSizeHeavyMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/Medium"), substancePerformanceParams.ProceduralCacheSizeMediumMenuToggle, ProceduralCacheSizeMediumMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/Tiny"), substancePerformanceParams.ProceduralCacheSizeTinyMenuToggle, ProceduralCacheSizeTinyMenuFunction);
                        toolsMenu.AddItem(new GUIContent("Procedural Cache Size/None"), substancePerformanceParams.ProceduralCacheSizeNoneMenuToggle, ProceduralCacheSizeNoneMenuFunction);
                        toolsMenu.AddSeparator("");
                        toolsMenu.AddItem(new GUIContent("Show Variable Information"), substanceToolParams.showVariableInformationToggle, ShowVariableInformationFunction);
                        toolsMenu.DropDown(new Rect(Event.current.mousePosition.x, 0, 0, 16));
                        EditorGUIUtility.ExitGUI();
                    }
                    if (!substanceToolParams.gameIsPaused)
                    {
                        if (GUILayout.Button("Toggle Animation On/Off", substanceGUIStyleParams.styleAnimationButton)) //Pauses-Unpauses animation
                        {
                            if (substanceToolParams.currentSelection.GetComponent<PrefabProperties>() != null)
                                SubstanceTweenAnimationUtility.ToggleAnimation(substanceToolParams.selectedPrefabScript.MaterialVariableKeyframeDictionaryList, substanceMaterialParams, animationParams, substanceToolParams);
                            else
                                SubstanceTweenAnimationUtility.ToggleAnimation(substanceMaterialParams.MaterialVariableKeyframeDictionaryList, substanceMaterialParams, animationParams, substanceToolParams);
                        }
                    }
                    else if (substanceToolParams.gameIsPaused)
                        EditorGUILayout.LabelField("GAME IS PAUSED", substanceGUIStyleParams.styleAnimationButton);
                    if (GUILayout.Button("Create keyframe: " + (animationParams.keyFrameTimes.Count + 1), substanceGUIStyleParams.toolbarButtonStyle))
                    {
                        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "Create Keyframe " + substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count().ToString());
                        SubstanceTweenKeyframeUtility.CreateKeyframe(substanceMaterialParams, animationParams, substanceToolParams);
                        EditorGUIUtility.ExitGUI();
                    }
                    EditorGUIUtility.labelWidth = 100f;
                    animationParams.animationType = (SubstanceAnimationParams.AnimationType)EditorGUILayout.EnumPopup("Animation Type:", animationParams.animationType, substanceGUIStyleParams.toolbarPopupStyle);
                    EditorGUIUtility.labelWidth = 0;
                    GUILayout.EndHorizontal();
                    EditorGUILayout.BeginVertical();
                    substanceToolParams.scrollVal = GUILayout.BeginScrollView(substanceToolParams.scrollVal);
                    EditorGUILayout.LabelField("SubstanceTween", substanceGUIStyleParams.styleMainHeader); EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Currently selected Material:", substanceGUIStyleParams.styleMainHeader); EditorGUILayout.Space();
                    if (substanceMaterialParams.substance)
                        EditorGUILayout.LabelField(substanceMaterialParams.substance.name, substanceGUIStyleParams.styleMainHeader); EditorGUILayout.Space();

                    if (substanceMaterialParams.substance)//if object has a procedural material, loop through properties and create sliders/fields based on type
                    {
                        substanceMaterialParams.materialVariables = substanceMaterialParams.substance.GetProceduralPropertyDescriptions();
                        List<string> variableGroups = new List<string>(); // List of different group names
                        for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)//Finds group names and adds them to a list. if current paramter has no group then display the control
                        {
                            ProceduralPropertyDescription materialVariable = substanceMaterialParams.materialVariables[i];
                            if (substanceMaterialParams.substance.IsProceduralPropertyVisible(materialVariable.name)) // checks the current variable for any $visibleIf paramters(Set in Substance Designer)
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
                            string name = substanceMaterialParams.substance.name;
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
                                if (!animationParams.hideNonAnimatingVariables || (!animationParams.substanceLerp || (animationParams.substanceLerp && substanceMaterialParams.animatedMaterialVariables.Count == 0)))
                                {
                                    for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
                                    {
                                        ProceduralPropertyDescription materialVariable = substanceMaterialParams.materialVariables[i];
                                        if (showGroup && materialVariable.group == curGroupName) // if current group can be displayed and the current variable group matches the one in the current  list 
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.Space((float)(EditorGUI.indentLevel * 40));
                                            DisplayControlForParameter(materialVariable);
                                            EditorGUILayout.EndHorizontal();
                                        }
                                    }
                                }
                                else if (animationParams.hideNonAnimatingVariables && animationParams.substanceLerp)
                                {
                                    for (int i = 0; i < substanceMaterialParams.animatedMaterialVariables.Count; i++)
                                    {
                                        ProceduralPropertyDescription animatedMaterialVariable = substanceMaterialParams.animatedMaterialVariables[i];
                                        if (showGroup && animatedMaterialVariable.group == curGroupName) // if current group can be displayed and the current variable group matches the one in the current  list 
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
                        if (substanceMaterialParams.rend && substanceMaterialParams.rend.sharedMaterial.HasProperty("_MainTex")) // If object has a Main Texture Offset field
                        {
                            GUILayout.Label("_MainTex");
                            Vector2 oldOffset = substanceMaterialParams.MainTexOffset;
                            substanceMaterialParams.MainTexOffset.x = EditorGUILayout.Slider(substanceMaterialParams.MainTexOffset.x, -10f, 10.0f);
                            substanceMaterialParams.MainTexOffset.y = EditorGUILayout.Slider(substanceMaterialParams.MainTexOffset.y, -10f, 10.0f);
                            if (substanceMaterialParams.MainTexOffset != oldOffset)
                            {
                                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "_MainTex Changed");
                                if (!substanceToolParams.gameIsPaused && animationParams.substanceLerp)
                                    substanceMaterialParams.rend.sharedMaterial.SetTextureOffset("_MainTex", substanceMaterialParams.MainTexOffset);
                                else
                                    substanceMaterialParams.rend.sharedMaterial.SetTextureOffset("_MainTex", substanceMaterialParams.MainTexOffset);
                                substanceToolParams.DebugStrings.Add("_MainTex" + " Was " + oldOffset + " is now: " + substanceMaterialParams.MainTexOffset);
                                if (substanceMaterialParams.chooseOverwriteVariables)
                                {
                                    if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey("_MainTex") == false) //if the value is changed once
                                        substanceMaterialParams.VariableValuesToOverwrite.Add("_MainTex", substanceMaterialParams.MainTexOffset.ToString());
                                    else // when the value is changed more than once 
                                        substanceMaterialParams.VariableValuesToOverwrite["_MainTex"] = substanceMaterialParams.MainTexOffset.ToString();
                                }
                            }
                        }
                        if (substanceMaterialParams.rend && substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor")) // if object has a field for the EmissionColor
                        {
                            EditorGUI.BeginChangeCheck();
                            GUILayout.Label("Emission:");
                            if (substanceMaterialParams.emissionInput != substanceToolParams.selectedPrefabScript.emissionInput) // emission gets reset do default when i add the prefabProperties Script.
                            {
                                substanceMaterialParams.emissionInput = substanceToolParams.selectedPrefabScript.emissionInput;
                                substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", substanceMaterialParams.emissionInput);
                            }
                            substanceMaterialParams.emissionInput = EditorGUILayout.ColorField(GUIContent.none, value: substanceMaterialParams.emissionInput, showEyedropper: true, showAlpha: true, hdr: true, hdrConfig: null);
                            Color oldEmissionInput = substanceMaterialParams.emissionInput;
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "Emission Changed");
                                substanceMaterialParams.rend.sharedMaterial.EnableKeyword("_EMISSION");
                                substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", substanceMaterialParams.emissionInput);
                                substanceToolParams.selectedPrefabScript.emissionInput = substanceMaterialParams.emissionInput;
                                substanceToolParams.DebugStrings.Add("_EmissionColor" + " Was " + oldEmissionInput + " is now: " + substanceMaterialParams.emissionInput);
                                if (substanceMaterialParams.chooseOverwriteVariables)
                                {
                                    if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey("_EmissionColor") == false) //if the value is changed once
                                        substanceMaterialParams.VariableValuesToOverwrite.Add("_EmissionColor", "#" + ColorUtility.ToHtmlStringRGBA(substanceMaterialParams.emissionInput));
                                    else // when the value is changed more than once 
                                        substanceMaterialParams.VariableValuesToOverwrite["_EmissionColor"] = "#" + ColorUtility.ToHtmlStringRGBA(substanceMaterialParams.emissionInput);
                                }
                            }

                            flickerValues.flickerEnabled = EditorGUILayout.Toggle("Enable Flicker", flickerValues.flickerEnabled);
                            if (flickerValues.flickerEnabled)
                            {
                                substanceToolParams.emissionFlickerFoldout = EditorGUILayout.Foldout(substanceToolParams.emissionFlickerFoldout, "Emission Flicker");
                                EditorGUILayout.LabelField("FlickerCalc: " + flickerValues.flickerCalc);
                                if (flickerValues.flickerMin >= 0 && flickerValues.flickerMin <= 1)
                                    flickerValues.flickerMin = EditorGUILayout.DelayedFloatField(flickerValues.flickerMin);
                                else
                                    flickerValues.flickerMin = EditorGUILayout.DelayedFloatField(1);
                                if (flickerValues.flickerMax >= 0 && flickerValues.flickerMax <= 1)
                                    flickerValues.flickerMax = EditorGUILayout.DelayedFloatField(flickerValues.flickerMax);
                                else
                                    flickerValues.flickerMax = EditorGUILayout.DelayedFloatField(1);
                                flickerValues.flickerFloatToggle = EditorGUILayout.Toggle(" Float Flicker: ", flickerValues.flickerFloatToggle);
                                flickerValues.flickerColor3Toggle = EditorGUILayout.Toggle(" Color3(RGB) Flicker: ", flickerValues.flickerColor3Toggle);
                                flickerValues.flickerColor4Toggle = EditorGUILayout.Toggle(" Color4(RGBA) Flicker: ", flickerValues.flickerColor4Toggle);
                                flickerValues.flickerVector2Toggle = EditorGUILayout.Toggle(" Vector2 Flicker: ", flickerValues.flickerVector2Toggle);
                                flickerValues.flickerVector3Toggle = EditorGUILayout.Toggle(" Vector3 Flicker: ", flickerValues.flickerVector3Toggle);
                                flickerValues.flickerVector4Toggle = EditorGUILayout.Toggle(" Vector4 Flicker: ", flickerValues.flickerVector4Toggle);
                                flickerValues.flickerEmissionToggle = EditorGUILayout.Toggle(" Emission Flicker: ", flickerValues.flickerEmissionToggle);
                                if (flickerValues.flickerEnabled)
                                    ChangeFlicker();
                            }
                        }

                        if (substanceToolParams.currentSelection.GetComponent<PrefabProperties>() != null) // Selected object already has a script attached for animating variables
                            DisplayKeyframeControls(substanceToolParams.selectedPrefabScript.MaterialVariableKeyframeDictionaryList, substanceToolParams.selectedPrefabScript.MaterialVariableKeyframeList, substanceToolParams.selectedPrefabScript.keyFrameTimes, substanceToolParams.selectedPrefabScript.prefabAnimationCurve);
                        else
                            DisplayKeyframeControls(substanceMaterialParams.MaterialVariableKeyframeDictionaryList, substanceMaterialParams.MaterialVariableKeyframeList, animationParams.keyFrameTimes, animationParams.substanceCurve);
                        EditorGUI.BeginChangeCheck();
                        if (substanceMaterialParams.MaterialVariableKeyframeList.Count >= 2)
                        {
                            GUILayout.Label("Animation Scrub");
                            animationParams.desiredAnimationTime = GUILayout.HorizontalSlider(animationParams.animationTimeRestartEnd, 0, animationParams.totalAnimationLength);
                            if (EditorGUI.EndChangeCheck() && !animationParams.substanceLerp)
                            {
                                SubstanceTweenSetParameterUtility.SetProceduralMaterialBasedOnAnimationTime(ref animationParams.desiredAnimationTime, substanceMaterialParams, animationParams, substanceToolParams, flickerValues);
                            }
                            EditorGUILayout.LabelField("Animation length: " + animationParams.totalAnimationLength.ToString());
                        }
                        if (substanceToolParams.showVariableInformationToggle)
                        {
                            substanceToolParams.curveDebugFoldout = EditorGUILayout.Foldout(substanceToolParams.curveDebugFoldout, "Curve Keyframes"); //Drop down list for animation curve keys
                            if (substanceToolParams.curveDebugFoldout && animationParams.substanceCurve.keys.Count() >= 1)
                            {
                                for (int i = 0; i <= animationParams.substanceCurve.keys.Count() - 1; i++)// list of current keys on curve 
                                {
                                    EditorGUILayout.LabelField("Keyframe: " + i + " Time:" + animationParams.substanceCurve.keys[i].time + " Value:" + animationParams.substanceCurve.keys[i].value + " In Tangent: " + animationParams.substanceCurve.keys[i].inTangent + " Out Tangent: " + animationParams.substanceCurve.keys[i].outTangent + " Tangent Mode: " + animationParams.substanceCurve[i].tangentMode);
                                }
                                EditorGUILayout.LabelField("Animation time length: " + animationParams.keyframeSum);
                            }

                            GUILayout.Label("CacheSize:" + substancePerformanceParams.myProceduralCacheSize.ToString());
                            EditorGUILayout.LabelField("Percentage between keyframes: " + animationParams.lerp.ToString());
                            EditorGUILayout.LabelField("Current animation Time between keyframes: " + animationParams.currentAnimationTime.ToString());
                            EditorGUILayout.LabelField("Curve keyframe count: " + animationParams.substanceCurve.keys.Length.ToString());
                            EditorGUILayout.LabelField("Curve backup keyframe count: " + animationParams.substanceCurveBackup.keys.Length.ToString());
                            EditorGUILayout.LabelField("Curve value " + animationParams.curveFloat.ToString());
                            EditorGUILayout.LabelField("Current Keyframe Index: " + animationParams.currentKeyframeIndex.ToString());
                            EditorGUILayout.LabelField("Animation Time between current/next keyframe" + animationParams.currentKeyframeAnimationTime.ToString());
                            EditorGUILayout.LabelField("Current animation time total: " + animationParams.animationTimeRestartEnd.ToString());
                            EditorGUILayout.LabelField("Animation length: " + animationParams.keyframeSum.ToString());
                            EditorGUILayout.LabelField("Animating Backwards?: " + animationParams.animateBackwards.ToString());
                            EditorGUILayout.LabelField("Keyframe times Count: " + animationParams.keyFrameTimes.Count());
                            EditorGUILayout.LabelField("Keyframe Count: " + animationParams.keyFrames);
                            EditorGUILayout.LabelField("Overwriting Variables?: " + substanceMaterialParams.chooseOverwriteVariables);
                            EditorGUILayout.LabelField("Current Selection: " + substanceToolParams.currentSelection);
                            EditorGUILayout.LabelField("substance: " + substanceMaterialParams.substance);
                            EditorGUILayout.LabelField("substance Processing?: " + substanceMaterialParams.substance.isProcessing);
                            EditorGUILayout.LabelField("animatedMaterialVariables.Count: " + substanceMaterialParams.animatedMaterialVariables.Count);
                            EditorGUILayout.LabelField("Compiling:", EditorApplication.isCompiling ? "Yes" : "No");
                        }
                        EditorGUILayout.EndScrollView();
                        EditorGUILayout.EndVertical();
                        if (Event.current.keyCode == KeyCode.G && Event.current.type == EventType.KeyUp && substanceToolParams.lastAction != null) //repeat last action
                        {
                            MethodInfo lastUsedMethod = this.GetType().GetMethod(substanceToolParams.lastAction, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            Action myAction = (Action)Delegate.CreateDelegate(typeof(Action), this, lastUsedMethod);
                            myAction();
                        }
                        if (Event.current.keyCode == KeyCode.R && Event.current.type == EventType.KeyUp) // randomize values
                            RandomizeProceduralValues();
                    }
                }
                else if (substanceDefaultMaterialParams.selectedStartupMaterials.Count < 1) // when opening the tool for the first time and not having something thats required.
                {
                    if (!Selection.activeGameObject)
                    {
                        EditorGUILayout.LabelField("No active object");
                    }

                    if (!Selection.activeGameObject.GetComponent<SubstanceTool>())
                    {
                        EditorGUILayout.LabelField("No SubstanceTool component");
                    }

                    if (Selection.activeGameObject.GetComponent<SubstanceTool>().isActiveAndEnabled)
                    {
                        EditorGUILayout.LabelField(" SubstanceTool not active/enabled");
                    }

                    if (!Selection.activeGameObject.activeSelf)
                    {
                        EditorGUILayout.LabelField("Selected object is not visible/enabled");
                        return;
                    }
                    if (!substanceMaterialParams.rend)
                    {
                        StartupTool();
                        EditorGUILayout.LabelField("No renderer!");
                        return;
                    }
                    EditorGUILayout.LabelField("Select a game object in the Hierarchy that has a Substance material to use this tool", substanceGUIStyleParams.errorTextStyle);
                }
                if (EditorGUI.EndChangeCheck() || animationParams.substanceLerp)
                {
                    Repaint();
                }
#if UNITY_5_3
			if (substance && !animationParams.substanceLerp)
				substance.RebuildTexturesImmediately();
#endif
#if UNITY_5_4_OR_NEWER // In Unity 5.4 and above I am able to use RebuildTextures() which is faster but it is not compatible with 5.3 
                if (substanceMaterialParams.substance && !animationParams.substanceLerp)
                    substanceMaterialParams.substance.RebuildTextures();
#endif
            }
        }
    }

    void Update()
    {
        if (EditorApplication.isPlaying || !EditorApplication.isPlaying)
        {
            if (substanceMaterialParams.rend && substanceMaterialParams.substance && (animationParams.substanceLerp) && substanceToolParams.currentSelection == Selection.activeGameObject)
            {
                animationParams.currentKeyframeAnimationTime = animationParams.keyFrameTimes[animationParams.currentKeyframeIndex];
                if (animationParams.animationType == SubstanceAnimationParams.AnimationType.BackAndForth && animationParams.animateBackwards && animationParams.currentKeyframeIndex <= animationParams.keyFrameTimes.Count - 1 && animationParams.currentKeyframeIndex > 0)
                    animationParams.currentKeyframeAnimationTime = animationParams.keyFrameTimes[animationParams.currentKeyframeIndex - 1];
                if (animationParams.animationType == SubstanceAnimationParams.AnimationType.Loop)// animate through every keyframe and repeat at the beginning
                {
                    animationParams.currentKeyframeAnimationTime = animationParams.keyFrameTimes[animationParams.currentKeyframeIndex];
                    if (animationParams.substanceLerp)
                    {
                        animationParams.currentAnimationTime += Time.deltaTime;
                        animationParams.animationTimeRestartEnd += Time.deltaTime;
                    }
                    animationParams.curveFloat = animationParams.substanceCurve.Evaluate(animationParams.animationTimeRestartEnd);
                    if (animationParams.keyFrameTimes.Count > 2 && animationParams.currentAnimationTime > animationParams.currentKeyframeAnimationTime && animationParams.currentKeyframeIndex <= animationParams.keyFrameTimes.Count - 2)//goto next keyframe
                    {
                        animationParams.currentAnimationTime = 0;
                        animationParams.currentKeyframeIndex++;
                    }
                    else if (animationParams.keyFrameTimes.Count > 2 && animationParams.currentKeyframeIndex >= animationParams.keyFrameTimes.Count - 1)// reached last keyframe. Reset animation
                    {
                        animationParams.currentAnimationTime = 0;
                        animationParams.animationTimeRestartEnd = 0;
                        animationParams.currentKeyframeIndex = 0;
                        animationParams.curveFloat = animationParams.substanceCurve.Evaluate(animationParams.animationTimeRestartEnd);
                        animationParams.lerp = Mathf.InverseLerp(animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex].time, animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex + 1].time, animationParams.curveFloat);
                    }
                    else if (animationParams.keyFrameTimes.Count == 2 && animationParams.currentAnimationTime > animationParams.currentKeyframeAnimationTime)// If you have 2 keyframes and it reaches the end, reset animation
                    {
                        animationParams.currentAnimationTime = 0;
                        animationParams.animationTimeRestartEnd = 0;
                        animationParams.currentKeyframeIndex = 0;
                    }
                    if (animationParams.currentKeyframeIndex <= animationParams.keyFrameTimes.Count - 2)
                    {
                        animationParams.lerp = Mathf.InverseLerp(animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex].time, animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex + 1].time, animationParams.curveFloat);
                    }
                }
                else if (animationParams.animationType == SubstanceAnimationParams.AnimationType.BackAndForth) //animate through every keyframe and then repeat backwards.
                {
                    if (!animationParams.animateBackwards)
                    {
                        animationParams.currentAnimationTime += Time.deltaTime;
                        animationParams.animationTimeRestartEnd += Time.deltaTime;
                    }
                    else if (animationParams.animateBackwards)
                    {
                        animationParams.currentAnimationTime -= Time.deltaTime;
                        animationParams.animationTimeRestartEnd -= Time.deltaTime;
                    }
                    animationParams.curveFloat = animationParams.substanceCurve.Evaluate(animationParams.animationTimeRestartEnd);
                    if (animationParams.keyFrameTimes.Count > 2 && !animationParams.animateBackwards && animationParams.currentAnimationTime > animationParams.currentKeyframeAnimationTime && animationParams.currentKeyframeIndex < animationParams.keyFrameTimes.Count()) // reach next keyframe when going forwards
                    {
                        if (animationParams.currentKeyframeIndex == animationParams.keyFrameTimes.Count() - 2)
                            animationParams.animateBackwards = true;
                        else
                        {
                            animationParams.currentKeyframeIndex++;
                            animationParams.currentAnimationTime = 0;
                        }
                    }
                    else if (animationParams.keyFrameTimes.Count > 2 && animationParams.animateBackwards && animationParams.currentAnimationTime <= 0 && animationParams.currentKeyframeIndex <= animationParams.keyFrameTimes.Count() - 1 && animationParams.currentKeyframeIndex > 0) // reach next keyframe when going backwards
                    {
                        animationParams.currentAnimationTime = animationParams.currentKeyframeAnimationTime;
                        animationParams.currentKeyframeIndex--;
                        animationParams.curveFloat = animationParams.substanceCurve.Evaluate(animationParams.animationTimeRestartEnd);
                        animationParams.lerp = (Mathf.Lerp(1, 0, Mathf.InverseLerp(animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex + 1].time, animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex].time, animationParams.curveFloat)));
                    }
                    else if ((animationParams.keyFrameTimes.Count == 2) && animationParams.currentAnimationTime >= animationParams.currentKeyframeAnimationTime)
                    {
                        animationParams.animateBackwards = true;
                        animationParams.currentAnimationTime = animationParams.currentKeyframeAnimationTime;
                    }
                    if (animationParams.animateBackwards && animationParams.currentKeyframeIndex == 0 && animationParams.currentAnimationTime <= 0) // if you reach the last keyframe when going backwards go forwards.
                    {
                        animationParams.animateBackwards = false;
                        animationParams.currentAnimationTime = 0;
                        animationParams.animationTimeRestartEnd = 0;
                    }
                    if (!animationParams.animateBackwards && animationParams.currentKeyframeIndex < animationParams.keyFrameTimes.Count() - 1)
                    {
                        animationParams.lerp = Mathf.InverseLerp(animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex].time, animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex + 1].time, animationParams.curveFloat);
                    }
                    else if (animationParams.keyFrameTimes.Count > 2 && animationParams.animateBackwards && animationParams.currentAnimationTime != animationParams.currentKeyframeAnimationTime && animationParams.currentKeyframeIndex != animationParams.keyFrameTimes.Count())
                    {
                        animationParams.lerp = (Mathf.Lerp(1, 0, Mathf.InverseLerp(animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex + 1].time, animationParams.substanceCurve.keys[animationParams.currentKeyframeIndex].time, animationParams.curveFloat)));
                    }
                    else if (animationParams.keyFrameTimes.Count == 2 && animationParams.animateBackwards && animationParams.currentAnimationTime != animationParams.currentKeyframeAnimationTime)
                        animationParams.lerp = animationParams.curveFloat / animationParams.currentKeyframeAnimationTime;
                }
                if (substanceMaterialParams.materialVariables != null)
                {
                    for (int i = 0; i < substanceMaterialParams.animatedMaterialVariables.Count; i++)// search through dictionary for variable names and if they match animate them
                    {
                        ProceduralPropertyDescription animatedMaterialVariable = substanceMaterialParams.animatedMaterialVariables[i];
                        if (substanceMaterialParams.materialVariableNames.Contains(animatedMaterialVariable.name))
                        {
                            ProceduralPropertyType propType = substanceMaterialParams.animatedMaterialVariables[i].type;
                            if (propType == ProceduralPropertyType.Float)
                            {
                                if (animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count - 1 && (animatedMaterialVariable.name[0] != '$' || (animatedMaterialVariable.name[0] == '$' && animationParams.animateOutputParameters)))
                                    substanceMaterialParams.substance.SetProceduralFloat(animatedMaterialVariable.name, Mathf.Lerp(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].PropertyFloatDictionary[animatedMaterialVariable.name], substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex + 1].PropertyFloatDictionary[animatedMaterialVariable.name], animationParams.lerp * flickerValues.flickerFloatCalc));
                            }
                            else if (propType == ProceduralPropertyType.Color3)
                            {
                                if (animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count - 1)
                                {
                                    substanceMaterialParams.substance.SetProceduralColor(animatedMaterialVariable.name, Color.Lerp(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].PropertyColorDictionary[animatedMaterialVariable.name], substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex + 1].PropertyColorDictionary[animatedMaterialVariable.name], animationParams.lerp * flickerValues.flickerColor3Calc));
                                }
                            }
                            else if (propType == ProceduralPropertyType.Color4)
                            {
                                if (animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count - 1)
                                    substanceMaterialParams.substance.SetProceduralColor(animatedMaterialVariable.name, Color.Lerp(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].PropertyColorDictionary[animatedMaterialVariable.name], substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex + 1].PropertyColorDictionary[animatedMaterialVariable.name], animationParams.lerp * flickerValues.flickerColor4Calc));
                            }
                            else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
                            {
                                if (propType == ProceduralPropertyType.Vector4)
                                {
                                    if (animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count - 1)
                                        substanceMaterialParams.substance.SetProceduralVector(animatedMaterialVariable.name, Vector4.Lerp(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].PropertyVector4Dictionary[animatedMaterialVariable.name], substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex + 1].PropertyVector4Dictionary[animatedMaterialVariable.name], animationParams.lerp * flickerValues.flickerVector4Calc));
                                }
                                else if (propType == ProceduralPropertyType.Vector3)
                                {
                                    if (animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count - 1)
                                        substanceMaterialParams.substance.SetProceduralVector(animatedMaterialVariable.name, Vector3.Lerp(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].PropertyVector3Dictionary[animatedMaterialVariable.name], substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex + 1].PropertyVector3Dictionary[animatedMaterialVariable.name], animationParams.lerp * flickerValues.flickerVector3Calc));
                                }
                                else if (propType == ProceduralPropertyType.Vector2)
                                {
                                    if (animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count - 1 && (animatedMaterialVariable.name[0] != '$' || (animatedMaterialVariable.name[0] == '$' && animationParams.animateOutputParameters)))
                                        substanceMaterialParams.substance.SetProceduralVector(animatedMaterialVariable.name, Vector2.Lerp(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].PropertyVector2Dictionary[animatedMaterialVariable.name], substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex + 1].PropertyVector2Dictionary[animatedMaterialVariable.name], animationParams.lerp * flickerValues.flickerVector2Calc));
                                }
                            }
                            else if (propType == ProceduralPropertyType.Enum)
                            {
                                if (animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count - 1 && (animationParams.currentAnimationTime == 0 || animationParams.currentAnimationTime == animationParams.currentKeyframeAnimationTime))
                                    substanceMaterialParams.substance.SetProceduralEnum(animatedMaterialVariable.name, substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].PropertyEnumDictionary[animatedMaterialVariable.name]);
                            }
                            else if (propType == ProceduralPropertyType.Boolean)
                            {
                                if (animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count - 1 && (animationParams.currentAnimationTime == 0 || animationParams.currentAnimationTime == animationParams.currentKeyframeAnimationTime))
                                    substanceMaterialParams.substance.SetProceduralBoolean(animatedMaterialVariable.name, substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].PropertyBoolDictionary[animatedMaterialVariable.name]);
                            }
                        }
                    }
                }
                if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor") && animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeList.Count - 1)
                    substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].emissionColor, substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex + 1].emissionColor, animationParams.lerp * flickerValues.flickerEmissionCalc));
                if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_MainTex") && animationParams.currentKeyframeIndex + 1 <= substanceMaterialParams.MaterialVariableKeyframeList.Count - 1)
                    substanceMaterialParams.rend.sharedMaterial.SetTextureOffset("_MainTex", Vector2.Lerp(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].MainTex, substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex + 1].MainTex, animationParams.lerp * flickerValues.flickerCalc));
                if (substanceMaterialParams.rebuildSubstanceImmediately)
                    substanceMaterialParams.substance.RebuildTexturesImmediately();
                else
                    substanceMaterialParams.substance.RebuildTextures();
            }
        }
    }

    void OnValidate() // runs when anything in the inspector/graph changes or the script gets reloaded.
    {
        if (Selection.activeObject && substanceMaterialParams.rend && substanceMaterialParams.substance)
        {
            if (substanceToolParams.selectedPrefabScript != null)
                substanceToolParams.selectedPrefabScript.useSharedMaterial = true;
            if (substanceMaterialParams.MaterialVariableKeyframeList.Count >= 2)
            {
                for (int i = 0; i <= substanceMaterialParams.MaterialVariableKeyframeList.Count() - 1; i++)
                {
                    SubstanceTweenStorageUtility.AddProceduralVariablesToDictionaryFromList(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[i], substanceMaterialParams.MaterialVariableKeyframeList[i], substanceMaterialParams.materialVariables, substanceMaterialParams.saveOutputParameters);
                }
            }
            substanceMaterialParams.substance.RebuildTextures();
        }
        animationParams.currentAnimationTime = 0;
        animationParams.currentKeyframeIndex = 0;
        animationParams.animationTimeRestartEnd = 0;
        SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
    }

    void DisplayControlForParameter(ProceduralPropertyDescription parameter) // sorts parameters by type and displays controls for that type in the GUI
    {
        ProceduralPropertyType propType = parameter.type;
        if (parameter.name[0] == '$')
        {
            if (parameter.name == "$outputsize") //Texture Size
            {
                GUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.9f));
                GUILayout.Label(parameter.name + '(' + parameter.label + ')', GUILayout.Width(150));
                substanceMaterialParams.substanceWidth = (int)substanceMaterialParams.substance.GetProceduralVector(parameter.name).x;
                substanceMaterialParams.substanceHeight = (int)substanceMaterialParams.substance.GetProceduralVector(parameter.name).y;
                Vector2 substanceSize = new Vector2(substanceMaterialParams.substanceWidth, substanceMaterialParams.substanceHeight);
                Vector2 oldSubstanceSize = new Vector2(substanceMaterialParams.substanceWidth, substanceMaterialParams.substanceHeight);
                GUILayout.Label("X:", GUILayout.MaxWidth(30));
                substanceSize.x = EditorGUILayout.IntPopup(substanceMaterialParams.substanceWidth, substanceMaterialParams.textureSizeStrings, substanceMaterialParams.textureSizeValues, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(55));
                GUILayout.Label("Y:", GUILayout.MaxWidth(30));
                substanceSize.y = EditorGUILayout.IntPopup(substanceMaterialParams.substanceHeight, substanceMaterialParams.textureSizeStrings, substanceMaterialParams.textureSizeValues, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(55));
                GUILayout.EndHorizontal();
                if (substanceSize != oldSubstanceSize)
                {
                    Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, parameter.name + " Changed");
                    substanceMaterialParams.substance.SetProceduralVector(parameter.name, substanceSize);
                    if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        substanceMaterialParams.variablesToOverwrite.Add(parameter);
                        substanceMaterialParams.VariableValuesToOverwrite.Add(parameter.name, substanceSize.ToString());
                    }
                    else // when the value is changed more than once 
                        substanceMaterialParams.VariableValuesToOverwrite[parameter.name] = substanceSize.ToString(); // change the value of the dictionary element that already exists 
                }
            }
            else if (parameter.name == "$randomseed") // Current Seed value. 
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.90f));
                GUILayout.Label(parameter.name + '(' + parameter.label + ')');
                int randomSeed = (int)substanceMaterialParams.substance.GetProceduralFloat(parameter.name);
                int oldRandomSeed = randomSeed;
                randomSeed = EditorGUILayout.IntSlider(randomSeed, 1, 9999);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Randomize Seed"))
                    randomSeed = UnityEngine.Random.Range(0, 9999 + 1);
                if (EditorGUI.EndChangeCheck()) // anytime you change a slider it will save the old/new value to a debug text file that you can create.
                {
                    Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, parameter.name + " Changed");
                    substanceMaterialParams.substance.SetProceduralFloat(parameter.name, randomSeed);
                    substanceToolParams.DebugStrings.Add(parameter.name + " Was " + oldRandomSeed + " is now: " + randomSeed);
                    if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        substanceMaterialParams.variablesToOverwrite.Add(parameter);
                        substanceMaterialParams.VariableValuesToOverwrite.Add(parameter.name, randomSeed.ToString());
                    }
                    else // when the value is changed more than once 
                        substanceMaterialParams.VariableValuesToOverwrite[parameter.name] = randomSeed.ToString(); // change the value of the dictionary element that already exists 
                }
            }
        }
        else if (propType == ProceduralPropertyType.Float) // Ints are counted as floats.
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.90f));
            GUILayout.Label(parameter.name + '(' + parameter.label + ')');
            float propFloat = substanceMaterialParams.substance.GetProceduralFloat(parameter.name);
            float oldPropFloat = propFloat;
            propFloat = EditorGUILayout.Slider(substanceMaterialParams.substance.GetProceduralFloat(parameter.name), parameter.minimum, parameter.maximum);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) // anytime you change a slider it will save the old/new value to a debug text file that you can create.
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, parameter.name + " Changed");
                substanceMaterialParams.substance.SetProceduralFloat(parameter.name, propFloat);
                substanceToolParams.DebugStrings.Add(parameter.name + " Was " + oldPropFloat + " is now: " + propFloat);
                if (substanceMaterialParams.chooseOverwriteVariables) // if the user is overwriting values and changed this value
                {
                    if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        substanceMaterialParams.variablesToOverwrite.Add(parameter);
                        substanceMaterialParams.VariableValuesToOverwrite.Add(parameter.name, propFloat.ToString());
                    }
                    else // when the value is changed more than once 
                        substanceMaterialParams.VariableValuesToOverwrite[parameter.name] = propFloat.ToString(); // change the value of the dictionary element that already exists 
                }
            }
        }
        else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label(parameter.name);
            Color colorInput = substanceMaterialParams.substance.GetProceduralColor(parameter.name);
            Color oldColorInput = colorInput;
            colorInput = EditorGUILayout.ColorField(colorInput);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, parameter.name + " Changed");
                substanceMaterialParams.substance.SetProceduralColor(parameter.name, colorInput);
                substanceToolParams.DebugStrings.Add(parameter.name + " Was " + oldColorInput + " is now: " + colorInput);
                if (substanceMaterialParams.chooseOverwriteVariables) // if the user is overwriting values and changed this value
                {
                    if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        substanceMaterialParams.variablesToOverwrite.Add(parameter);
                        substanceMaterialParams.VariableValuesToOverwrite.Add(parameter.name, "#" + ColorUtility.ToHtmlStringRGBA(colorInput));
                    }
                    else // when the value is changed more than once 
                        substanceMaterialParams.VariableValuesToOverwrite[parameter.name] = "#" + ColorUtility.ToHtmlStringRGBA(colorInput); // change the value of the dictionary element that already exists 
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
            Vector4 inputVector = substanceMaterialParams.substance.GetProceduralVector(parameter.name);
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
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, parameter.name + " Changed");
                substanceMaterialParams.substance.SetProceduralVector(parameter.name, inputVector);
                substanceToolParams.DebugStrings.Add(parameter.name + " Was " + oldInputVector + " is now: " + inputVector);
                if (substanceMaterialParams.chooseOverwriteVariables) // if the user is overwriting values and changed this value
                {
                    if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        substanceMaterialParams.variablesToOverwrite.Add(parameter);
                        substanceMaterialParams.VariableValuesToOverwrite.Add(parameter.name, inputVector.ToString());
                    }
                    else
                        substanceMaterialParams.VariableValuesToOverwrite[parameter.name] = inputVector.ToString();
                }
            }
            EditorGUILayout.EndVertical();
        }
        else if (propType == ProceduralPropertyType.Enum)
        {
            GUILayout.Label(parameter.name);
            int enumInput = substanceMaterialParams.substance.GetProceduralEnum(parameter.name);
            int oldEnumInput = enumInput;
            string[] enumOptions = parameter.enumOptions;
            enumInput = EditorGUILayout.Popup(enumInput, enumOptions);
            if (enumInput != oldEnumInput)
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, parameter.name + " Changed");
                substanceMaterialParams.substance.SetProceduralEnum(parameter.name, enumInput);
                if (substanceMaterialParams.chooseOverwriteVariables) // if the user is overwriting values and changed this value
                {
                    if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed once
                    {
                        substanceMaterialParams.variablesToOverwrite.Add(parameter);
                        substanceMaterialParams.VariableValuesToOverwrite.Add(parameter.name, enumInput.ToString());
                    }
                    else
                        substanceMaterialParams.VariableValuesToOverwrite[parameter.name] = enumInput.ToString();
                }
            }
        }
        else if (propType == ProceduralPropertyType.Boolean)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label(parameter.name);
            bool boolInput = substanceMaterialParams.substance.GetProceduralBoolean(parameter.name);
            boolInput = EditorGUILayout.Toggle(boolInput);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, parameter.name + " Changed");
                substanceMaterialParams.substance.SetProceduralBoolean(parameter.name, boolInput);
                if (substanceMaterialParams.chooseOverwriteVariables) // if the user is overwriting values and changed this value
                {
                    if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey(parameter.name) == false) //if the value is changed only once
                    {
                        substanceMaterialParams.variablesToOverwrite.Add(parameter);
                        substanceMaterialParams.VariableValuesToOverwrite.Add(parameter.name, boolInput.ToString());
                    }
                    else
                    {
                        substanceMaterialParams.VariableValuesToOverwrite[parameter.name] = boolInput.ToString();
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
            repaintInspectorConstantly = true;
            if (animationCurve.keys[0].time != 0 || animationCurve.keys[0].value != 0)
            {
                animationCurve.keys = animationParams.substanceCurveBackup.keys;
            }

            else if (Event.current.button == 1 && !substanceToolParams.mousePressedInCurveEditor) // if i right click in curve editor
            {
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this }, "Edited Curve");
                substanceToolParams.mousePressedInCurveEditor = true;
            }

            else if (Event.current.button == 0 && Event.current.button != 1 && substanceToolParams.mousePressedInCurveEditor == true) // if user releases mouse 
            {
                if (animationCurve.keys.Count() == animationParams.substanceCurveBackup.keys.Count() && Event.current.commandName == String.Empty) // Did not Add/removeKeys 
                {
                    Undo.SetCurrentGroupName("No Change");
                }
                substanceToolParams.mousePressedInCurveEditor = false;
            }
            if (Event.current.commandName.ToString() == "CurveChanged")
            {
                for (int i = 0; i <= animationCurve.keys.Count() - 1; i++)
                {
                    if (animationCurve.keys[i].value != animationCurve.keys[i].time)// if User moves a key in the curve editor
                    {
                        animationCurve.MoveKey(i, new Keyframe(animationCurve.keys[i].time, animationCurve.keys[i].time, animationCurve.keys[i].inTangent, animationCurve.keys[i].outTangent));
                        float tempKeyframeTime = animationParams.substanceCurve.keys[i].time;
                        SubstanceTweenSetParameterUtility.SetProceduralMaterialBasedOnAnimationTime(ref tempKeyframeTime, substanceMaterialParams, animationParams, substanceToolParams, flickerValues);
                        for (int j = 0; j <= animationCurve.keys.Count() - 2; j++) // Rebuild Animation Curve
                        {
                            animationParams.keyFrameTimes[j] = animationCurve.keys[j + 1].time - animationCurve.keys[j].time;
                            keyframeList[j].animationTime = animationCurve.keys[j + 1].time - animationCurve.keys[j].time;
                        }
                    }
                }
                if (animationParams.substanceCurve.keys.Count() == animationParams.substanceCurveBackup.keys.Count())
                    animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
                Repaint();
            }
        }
        else
        {
            repaintInspectorConstantly = false;
        }

        if (EditorWindow.focusedWindow && EditorWindow.focusedWindow.ToString() == " (UnityEditor.CurveEditorWindow)" && (animationCurve.keys.Count() != animationParams.substanceCurveBackup.keys.Count() || animationCurve.keys[animationCurve.keys.Count() - 1].time != animationParams.substanceCurveBackup.keys[animationParams.substanceCurveBackup.keys.Count() - 1].time) || keyframeTimeList.Count() - animationCurve.keys.Count() > 1) // if you delete or add a keyframe in the curve editor, Reset keys.
        {

            if (animationCurve.keys.Count() < keyframeTimeList.Count())
            {
                SubstanceTweenKeyframeUtility.CheckForRemoveOrEditFromCurveEditor(substanceMaterialParams, animationParams, substanceToolParams, flickerValues);
            }
            else if (animationCurve.keys.Count() > animationParams.substanceCurveBackup.keys.Count()) // if keyframe has just been created in the curve editor
            {
                SubstanceTweenKeyframeUtility.CheckForAddKeyFromCurveEditor(substanceMaterialParams, animationParams, substanceToolParams, flickerValues);
                for (int i = 0; i <= animationCurve.keys.Count() - 1; i++)
                {
                    if (animationCurve.keys[i].value != animationCurve.keys[i].time)
                    {// make sure that all of the times of the curve are the same as the value
                        animationCurve.MoveKey(i, new Keyframe(animationCurve.keys[i].time, animationCurve.keys[i].time, animationCurve.keys[i].inTangent, animationCurve.keys[i].outTangent));
                        float tempKeyframeTime = animationParams.substanceCurve.keys[i].time;
                        SubstanceTweenSetParameterUtility.SetProceduralMaterialBasedOnAnimationTime(ref tempKeyframeTime, substanceMaterialParams, animationParams, substanceToolParams, flickerValues);
                        for (int j = 0; j <= animationCurve.keys.Count() - 2; j++) // Rebuild Animation Curve
                        {
                            animationParams.keyFrameTimes[j] = animationCurve.keys[j + 1].time - animationCurve.keys[j].time;
                            keyframeList[j].animationTime = animationCurve.keys[j + 1].time - animationCurve.keys[j].time;
                        }
                    }
                }
                animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
            }

            if (animationCurve.keys.Count() <= 1 || (animationCurve.keys.Count() <= 2 && (substanceMaterialParams.MaterialVariableKeyframeList.Count <= 1 || animationParams.keyFrames <= 1)))
            {
                EditorApplication.ExecuteMenuItem("Window/Inspector"); //close the curve editor by loading the inspector
            }
        }
        if (keyframeTimeList.Count == 0 && animationCurve.keys.Count() == 0)// make sure there are always two keys on the curve
        {
            animationCurve.AddKey(0, 0);
            animationCurve.AddKey(5, 5);
        }
        substanceToolParams.showKeyframes = EditorGUILayout.Foldout(substanceToolParams.showKeyframes, "Keyframes"); // drop down list for keyframes 
        if (substanceToolParams.showKeyframes && keyframeDictList.Count() >= 1)
        {
            EditorGUILayout.LabelField("Transition Time:", substanceGUIStyleParams.rightAnchorTextStyle);
            substanceMaterialParams.reorderList.DoLayoutList();
            substanceToolParams.reWriteAllKeyframeTimes = EditorGUILayout.Toggle("Rewrite All Keyframe Times", substanceToolParams.reWriteAllKeyframeTimes);
        }
        EditorGUILayout.BeginHorizontal("Button");
        EditorGUI.BeginDisabledGroup(substanceMaterialParams.chooseOverwriteVariables);
        if (GUILayout.Button("Choose variables to rewrite all keyframes")) // when this is toggled any variables that changed are added to a list to be overwritten on all keyframes..
        {
            substanceMaterialParams.chooseOverwriteVariables = true;
            substanceMaterialParams.saveOverwriteVariables = true;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(!substanceMaterialParams.saveOverwriteVariables);
        if (GUILayout.Button("Overwrite modified variables to all Keyframes"))// overwrites variables that have changed for all keyframes.
        {
            substanceMaterialParams.chooseOverwriteVariables = true;
            for (int i = 0; i <= keyframeList.Count() - 1; i++)
            {
                SubstanceTweenKeyframeUtility.SelectKeyframe(i, substanceMaterialParams, animationParams, substanceToolParams);
                foreach (ProceduralPropertyDescription variable in substanceMaterialParams.variablesToOverwrite)
                {
                    if (variable.type == ProceduralPropertyType.Float)
                    {
                        string currentVariable;
                        substanceMaterialParams.VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        substanceMaterialParams.substance.SetProceduralFloat(variable.name, float.Parse(currentVariable));
                    }
                    else if (variable.type == ProceduralPropertyType.Color3 || variable.type == ProceduralPropertyType.Color4)
                    {
                        string currentVariable;
                        Color currentVariableColor;
                        substanceMaterialParams.VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        ColorUtility.TryParseHtmlString(currentVariable, out currentVariableColor);
                        substanceMaterialParams.substance.SetProceduralColor(variable.name, currentVariableColor);
                    }
                    else if (variable.type == ProceduralPropertyType.Vector2 || variable.type == ProceduralPropertyType.Vector3 || variable.type == ProceduralPropertyType.Vector4)
                    {
                        Vector2 inputVector2;
                        Vector3 inputVector3;
                        Vector4 inputVector4;
                        string currentVariable;
                        substanceMaterialParams.VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        if (variable.type == ProceduralPropertyType.Vector2)
                        {
                            inputVector2 = SubstanceTweenMiscUtility.StringToVector(currentVariable, 2);
                            substanceMaterialParams.substance.SetProceduralVector(variable.name, inputVector2);
                        }
                        else if (variable.type == ProceduralPropertyType.Vector3)
                        {
                            inputVector3 = SubstanceTweenMiscUtility.StringToVector(currentVariable, 3);
                            substanceMaterialParams.substance.SetProceduralVector(variable.name, inputVector3);
                        }
                        else if (variable.type == ProceduralPropertyType.Vector4)
                        {
                            inputVector4 = SubstanceTweenMiscUtility.StringToVector(currentVariable, 4);
                            substanceMaterialParams.substance.SetProceduralVector(variable.name, inputVector4);
                        }
                    }
                    else if (variable.type == ProceduralPropertyType.Enum)
                    {
                        string currentVariable;
                        substanceMaterialParams.VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        substanceMaterialParams.substance.SetProceduralEnum(variable.name, int.Parse(currentVariable));
                    }
                    else if (variable.type == ProceduralPropertyType.Boolean)
                    {
                        string currentVariable;
                        substanceMaterialParams.VariableValuesToOverwrite.TryGetValue(variable.name, out currentVariable);
                        substanceMaterialParams.substance.SetProceduralBoolean(variable.name, bool.Parse(currentVariable));
                    }
                    if (variable.ToString() == "$outputsize") //Texture Size
                    {
                        Vector2 substanceSize = SubstanceTweenMiscUtility.StringToVector(substanceMaterialParams.VariableValuesToOverwrite[variable.ToString()], 2);
                        substanceMaterialParams.substanceWidth = (int)substanceSize.x;
                        substanceMaterialParams.substanceHeight = (int)substanceSize.y;
                    }
                }
                if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey("_EmissionColor") && substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    Color OverwrittenEmissionColor;
                    ColorUtility.TryParseHtmlString(substanceMaterialParams.VariableValuesToOverwrite["_EmissionColor"], out OverwrittenEmissionColor);
                    substanceMaterialParams.emissionInput = OverwrittenEmissionColor; // changes the gui color picker
                    substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", OverwrittenEmissionColor);
                }
                if (substanceMaterialParams.VariableValuesToOverwrite.ContainsKey("_MainTex") && substanceMaterialParams.rend.sharedMaterial.HasProperty("_MainTex"))
                {
                    Vector2 OverwrittenMainTex = SubstanceTweenMiscUtility.StringToVector(substanceMaterialParams.VariableValuesToOverwrite["_MainTex"], 2);
                    substanceMaterialParams.MainTexOffset = OverwrittenMainTex;
                    substanceMaterialParams.rend.sharedMaterial.SetTextureOffset("_MainTex", substanceMaterialParams.MainTexOffset);
                }
                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "Overwrite keyframe " + i);
                SubstanceTweenKeyframeUtility.OverWriteKeyframe(i, substanceMaterialParams, animationParams, substanceToolParams);
                Repaint();
            }
            substanceMaterialParams.chooseOverwriteVariables = false;
            substanceMaterialParams.saveOverwriteVariables = false;
            substanceMaterialParams.variablesToOverwrite.Clear();
            substanceMaterialParams.VariableValuesToOverwrite.Clear();
            SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
            SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Cancel")) // if you decide not to overwrite anything, delete list of variables that has changed.
        {
            substanceMaterialParams.chooseOverwriteVariables = false;
            substanceMaterialParams.saveOverwriteVariables = false;
            substanceMaterialParams.variablesToOverwrite.Clear();
            substanceMaterialParams.VariableValuesToOverwrite.Clear();
        }
        EditorGUILayout.EndHorizontal();
    }

    public void DeleteAllKeyframes()
    {
        SubstanceTweenKeyframeUtility.DeleteAllKeyframes(substanceMaterialParams, animationParams);
        Repaint();
    }

    public void WriteXML() // Write current material variables to a XML file.
    {
        SubstanceInputOutputUtility.WriteXML(substanceMaterialParams, animationParams, substanceToolParams);
    }

    public void ReadXML() // Sets current material variables from a XML file without creating a keyframe.
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "Read XML File");
        SubstanceInputOutputUtility.ReadXML(substanceMaterialParams, animationParams, substanceToolParams);
    }

    public void WriteAllXML() // Writes each keyframe to a XML file
    {
        SubstanceInputOutputUtility.WriteAllXML(substanceMaterialParams, animationParams, substanceToolParams);
    }

    public void ReadAllXML() // Read XML files and create keyframes from them.
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "Create Keyframes From XML");
        SubstanceInputOutputUtility.ReadAllXML(substanceMaterialParams, animationParams, substanceToolParams);
    }

    public void WriteJSON() //  Write current material variables to a JSON file
    {
        SubstanceInputOutputUtility.WriteJSON(substanceMaterialParams, animationParams, substanceToolParams);
    }

    public void ReadJSON()
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "Read XML File");
        SubstanceInputOutputUtility.ReadJSON(substanceMaterialParams, animationParams, substanceToolParams);
    } //  Sets current material variables from a JSON file without creating a keyframe

    public void WriteAllJSON()
    {//Writes all keyframes to JSON files
        SubstanceInputOutputUtility.WriteAllJSON(substanceMaterialParams, animationParams, substanceToolParams);
    }

    public void ReadAllJSON() // Read JSON files and create keyframes from them.
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "Create Keyframes From JSON files");
        SubstanceInputOutputUtility.ReadAllJSON(substanceMaterialParams, animationParams, substanceToolParams);
    }

    void SavePrefab() // Saves a Prefab to a specified folder
    {
        SubstanceInputOutputUtility.SavePrefab(substanceMaterialParams, animationParams, substanceToolParams, substancePerformanceParams, flickerValues);
    }
    void WriteDebugText()
    {
        SubstanceInputOutputUtility.WriteDebugText(animationParams, substanceToolParams);
    }

    public void SetAllProceduralValuesToMin() // Sets all procedural values to the minimum value
    {
        SubstanceTweenSetParameterUtility.SetAllProceduralValuesToMin(substanceMaterialParams);
    }

    public void SetAllProceduralValuesToMax() // Sets all procedural values to the maximum value
    {
        SubstanceTweenSetParameterUtility.SetAllProceduralValuesToMax(substanceMaterialParams);
    }

    public void RandomizeProceduralValues() // Sets all procedural values to a random value
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substanceMaterialParams.substance, substanceImporter, substanceToolParams.currentSelection }, "Randomize values");
        SubstanceTweenSetParameterUtility.RandomizeProceduralValues(substanceMaterialParams, randomizeSettings);
    }

    public void ResetAllProceduralValues() // Resets all procedural values to default(When the material was first selected)
    {
        SubstanceTweenSetParameterUtility.ResetAllProceduralValues(substanceDefaultMaterialParams, substanceMaterialParams, animationParams, substanceToolParams);
    }

    public float ChangeFlicker()
    {
        flickerValues.flickerEnabled = true;
        flickerValues.flickerCalc = UnityEngine.Random.Range(flickerValues.flickerMin, flickerValues.flickerMax);
        if (flickerValues.flickerFloatToggle)
            flickerValues.flickerFloatCalc = flickerValues.flickerCalc;
        else
            flickerValues.flickerFloatCalc = 1;
        if (flickerValues.flickerColor3Toggle)
            flickerValues.flickerColor3Calc = flickerValues.flickerCalc;
        else
            flickerValues.flickerColor3Calc = 1;
        if (flickerValues.flickerColor4Toggle)
            flickerValues.flickerColor4Calc = flickerValues.flickerCalc;
        else
            flickerValues.flickerColor4Calc = 1;
        if (flickerValues.flickerVector2Toggle)
            flickerValues.flickerVector2Calc = flickerValues.flickerCalc;
        else
            flickerValues.flickerVector2Calc = 1;
        if (flickerValues.flickerVector3Toggle)
            flickerValues.flickerVector3Calc = flickerValues.flickerCalc;
        else
            flickerValues.flickerVector3Calc = 1;
        if (flickerValues.flickerVector4Toggle)
            flickerValues.flickerVector4Calc = flickerValues.flickerCalc;
        else
            flickerValues.flickerVector4Calc = 1;
        if (flickerValues.flickerEmissionToggle)
            flickerValues.flickerEmissionCalc = flickerValues.flickerCalc;
        return flickerValues.flickerCalc;
    }
}