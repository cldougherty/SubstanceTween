using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
/// <summary>
/// Custom Inspector for prefabProperties.cs
/// </summary>
[CustomEditor(typeof(PrefabProperties)), CanEditMultipleObjects] // A more user friendly version of the original script, to use the original Delete or comment out this line. Some properties on the original are not meant to be changed by the user.
public class PrefabPropertiesEditor : Editor
{
    bool showFlickerFoldout;

    public bool repaintInspectorConstantly;
    public override bool RequiresConstantRepaint() { return repaintInspectorConstantly; }

    public override void OnInspectorGUI()
    {
        PrefabProperties prefabProperties = (PrefabProperties)target;
        prefabProperties.animateOnStart = EditorGUILayout.Toggle("Animate On Start", prefabProperties.animateOnStart);
        prefabProperties.animationType = (PrefabProperties.AnimationType)EditorGUILayout.EnumPopup("Animation Type:", prefabProperties.animationType);
        prefabProperties.animateBasedOnTime = EditorGUILayout.Toggle("Animate Based On Time:", prefabProperties.animateBasedOnTime);
        prefabProperties.playOnce = EditorGUILayout.Toggle("Play Once", prefabProperties.playOnce);
        if (prefabProperties.animationDelay >= 0)
            prefabProperties.animationDelay = EditorGUILayout.FloatField("Animation Delay On Start", prefabProperties.animationDelay);
        else
            prefabProperties.animationDelay = EditorGUILayout.FloatField("Animation Delay On Start", 0);

        prefabProperties.animationStartKeyframe = EditorGUILayout.IntField("Start Keyframe", prefabProperties.animationStartKeyframe);
        prefabProperties.animationEndKeyframe = EditorGUILayout.IntField("End Keyframe", prefabProperties.animationEndKeyframe);
        prefabProperties.prefabAnimationCurve = EditorGUILayout.CurveField("Animation Time Curve", prefabProperties.prefabAnimationCurve);
        prefabProperties.animateObjectBasedOnPosition = (PrefabProperties.AnimateObjectBasedOnPosition)EditorGUILayout.EnumPopup("Animate Object Based On Position:", prefabProperties.animateObjectBasedOnPosition);
        prefabProperties.objectMinDistance = EditorGUILayout.FloatField("Object Min Distance", prefabProperties.objectMinDistance);
        prefabProperties.objectMaxDistance = EditorGUILayout.FloatField("Object Max Distance", prefabProperties.objectMaxDistance);
        prefabProperties.useSharedMaterial = EditorGUILayout.Toggle("Used Shared Material", prefabProperties.useSharedMaterial);
        prefabProperties.rebuildSubstanceImmediately = EditorGUILayout.Toggle("Rebuild Substance Immediately(SLOW):", prefabProperties.rebuildSubstanceImmediately);
        prefabProperties.cacheAtStartup = EditorGUILayout.Toggle("Cache Animated Values", prefabProperties.cacheAtStartup);
        prefabProperties.animateOutputParameters = EditorGUILayout.Toggle("Animate Output Parameters($randomSeed,$outputsize,etc)", prefabProperties.animateOutputParameters);
        prefabProperties.mySubstanceProcessorUsage = (PrefabProperties.MySubstanceProcessorUsage)EditorGUILayout.EnumPopup("Substance Processor Usage", prefabProperties.mySubstanceProcessorUsage);
        prefabProperties.myProceduralCacheSize = (PrefabProperties.MyProceduralCacheSize)EditorGUILayout.EnumPopup("Substance Cache Size", prefabProperties.myProceduralCacheSize);
        prefabProperties.LodNearDistance = EditorGUILayout.FloatField("LOD 0 Distance(Near)", prefabProperties.LodNearDistance);
        prefabProperties.LodMidDistance = EditorGUILayout.FloatField("LOD 1 Distance(Mid)", prefabProperties.LodMidDistance);
        prefabProperties.LodFarDistance = EditorGUILayout.FloatField("LOD 2 Distance(Far)", prefabProperties.LodFarDistance);

        showFlickerFoldout = EditorGUILayout.Foldout(showFlickerFoldout, "Enable Flicker");
        if (showFlickerFoldout)
        {
            prefabProperties.flickerEnabled = EditorGUILayout.Toggle("Enable Flicker", prefabProperties.flickerEnabled);
            EditorGUI.BeginDisabledGroup(!prefabProperties.flickerEnabled);
            prefabProperties.flickerMin = EditorGUILayout.Slider("Flicker Minimum", prefabProperties.flickerMin, 0, 0.99f);
            prefabProperties.flickerMax = EditorGUILayout.Slider("Flicker Maximum", prefabProperties.flickerMax, 0.01f, 1f);
            prefabProperties.flickerFloatToggle = EditorGUILayout.Toggle("Flicker Float Values", prefabProperties.flickerFloatToggle);
            prefabProperties.flickerColor3Toggle = EditorGUILayout.Toggle("Flicker Color3(RGB) Values", prefabProperties.flickerColor3Toggle);
            prefabProperties.flickerColor4Toggle = EditorGUILayout.Toggle("Flicker Color4(RGBA) Values", prefabProperties.flickerColor4Toggle);
            prefabProperties.flickerVector2Toggle = EditorGUILayout.Toggle("Flicker Vector2 Values", prefabProperties.flickerVector2Toggle);
            prefabProperties.flickerVector3Toggle = EditorGUILayout.Toggle("Flicker Vector3 Values", prefabProperties.flickerVector3Toggle);
            prefabProperties.flickerVector4Toggle = EditorGUILayout.Toggle("Flicker Vector4 Values", prefabProperties.flickerVector4Toggle);
            prefabProperties.flickerEmissionToggle = EditorGUILayout.Toggle("Flicker Emission Values", prefabProperties.flickerEmissionToggle);
            EditorGUI.EndDisabledGroup();
        }

        if (EditorWindow.focusedWindow && EditorWindow.focusedWindow.ToString() == " (UnityEditor.CurveEditorWindow)")
        {
            repaintInspectorConstantly = true;
            if (prefabProperties.prefabAnimationCurve.keys[0].time != 0 || prefabProperties.prefabAnimationCurve.keys[0].value != 0)
            {
                prefabProperties.prefabAnimationCurve.keys = prefabProperties.prefabAnimationCurveBackup.keys;
            }
            if (Event.current.commandName.ToString() == "CurveChanged")
            {
                for (int i = 0; i <= prefabProperties.prefabAnimationCurve.keys.Count() - 1; i++)
                {
                    if (prefabProperties.prefabAnimationCurve.keys[i].value != prefabProperties.prefabAnimationCurve.keys[i].time)// if User moves a key in the curve Editor
                    {
                        prefabProperties.prefabAnimationCurve.MoveKey(i, new Keyframe(prefabProperties.prefabAnimationCurve.keys[i].time, prefabProperties.prefabAnimationCurve.keys[i].time, prefabProperties.prefabAnimationCurve.keys[i].inTangent, prefabProperties.prefabAnimationCurve.keys[i].outTangent));
                        for (int j = 0; j <= prefabProperties.prefabAnimationCurve.keys.Count() - 2; j++) // Rebuild Animation Curve
                        {
                            prefabProperties.keyFrameTimes[j] = prefabProperties.prefabAnimationCurve.keys[j + 1].time - prefabProperties.prefabAnimationCurve.keys[j].time;
                            prefabProperties.MaterialVariableKeyframeList[j].animationTime = prefabProperties.prefabAnimationCurve.keys[j + 1].time - prefabProperties.prefabAnimationCurve.keys[j].time;
                        }
                    }
                }
                Repaint();
            }
        }
        else
            repaintInspectorConstantly = false;

        if (EditorWindow.focusedWindow && EditorWindow.focusedWindow.ToString() == " (UnityEditor.CurveEditorWindow)" && (prefabProperties.prefabAnimationCurve.keys.Count() != prefabProperties.prefabAnimationCurveBackup.keys.Count() || prefabProperties.prefabAnimationCurve.keys[prefabProperties.prefabAnimationCurve.keys.Count() - 1].time != prefabProperties.prefabAnimationCurveBackup.keys[prefabProperties.prefabAnimationCurveBackup.keys.Count() - 1].time) || prefabProperties.keyFrameTimes.Count() - prefabProperties.prefabAnimationCurve.keys.Count() > 1) // if you delete or add a keyframe in the curve editor, Reset keys.
        {
           
            if (prefabProperties.prefabAnimationCurve.keys.Count() <= 1)
                EditorWindow.focusedWindow.Close();
            if (prefabProperties.prefabAnimationCurve.keys.Count() < prefabProperties.keyFrameTimes.Count())
            {
                List<float> BackupKeyframePointTimes = new List<float>();
                List<float> CurrentKeyframePointTimes = new List<float>();
                if (prefabProperties.prefabAnimationCurve.keys[0].time != 0)
                    prefabProperties.prefabAnimationCurve.keys[0].time = 0;
                for (int i = 0; i <= prefabProperties.prefabAnimationCurveBackup.keys.Count() - 1; i++)
                {
                    BackupKeyframePointTimes.Add(prefabProperties.prefabAnimationCurveBackup.keys[i].time);
                }
                for (int i = 0; i <= prefabProperties.prefabAnimationCurve.keys.Count() - 1; i++)
                {
                    CurrentKeyframePointTimes.Add(prefabProperties.prefabAnimationCurve.keys[i].time);
                }
                for (int i = BackupKeyframePointTimes.Count() - 1; i > 0; i--) // Go through every key in the backup list(Before the keys got deleted) 
                {
                    if (!CurrentKeyframePointTimes.Contains(BackupKeyframePointTimes[i])) // if the current list of curve points does not contain this value(it was deleted in the curve editor) 
                    {
                        //Find the index of the value and delete any information that has not already been deleted in the curve editor
                        prefabProperties.keyFrameTimes.RemoveAt(BackupKeyframePointTimes.IndexOf(BackupKeyframePointTimes[i]));
                        prefabProperties.MaterialVariableKeyframeList.RemoveAt(BackupKeyframePointTimes.IndexOf(BackupKeyframePointTimes[i]));
                        prefabProperties.MaterialVariableKeyframeDictionaryList.RemoveAt(BackupKeyframePointTimes.IndexOf(BackupKeyframePointTimes[i]));
                        if (prefabProperties.keyFrames > 0)
                            prefabProperties.keyFrames--;
                    }
                }
                for (int i = 0; i <= prefabProperties.prefabAnimationCurve.keys.Count() - 2; i++)
                {
                    prefabProperties.keyFrameTimes[i] = prefabProperties.prefabAnimationCurve.keys[i + 1].time - prefabProperties.prefabAnimationCurve.keys[i].time;
                    prefabProperties.MaterialVariableKeyframeList[i].animationTime = prefabProperties.prefabAnimationCurve.keys[i + 1].time - prefabProperties.prefabAnimationCurve.keys[i].time;
                }
                prefabProperties.prefabAnimationCurveBackup.keys = prefabProperties.prefabAnimationCurve.keys;
            }
            else if (prefabProperties.prefabAnimationCurve.keys.Count() > prefabProperties.prefabAnimationCurveBackup.keys.Count()) // if keyframe has just been created in  the curve editor
            {
                for (int i = 0; i <= prefabProperties.prefabAnimationCurveBackup.keys.Count() - 1; i++)
                {
                    if (i <= prefabProperties.prefabAnimationCurve.keys.Count() - 1 && prefabProperties.prefabAnimationCurve.keys[i].time != prefabProperties.prefabAnimationCurveBackup.keys[i].time) // find keyframe that has just been created or deleted in the curve editor
                    {
                        if (prefabProperties.prefabAnimationCurve.keys.Count() > prefabProperties.prefabAnimationCurveBackup.keys.Count())
                        {
                            prefabProperties.prefabAnimationCurve.MoveKey(i, new Keyframe(prefabProperties.prefabAnimationCurve.keys[i].time, prefabProperties.prefabAnimationCurve.keys[i].time)); // lerp does not work correctly if the keyframe value is different than the time. i cant edit the key so i delete then add a new one
                            //SetProceduralMaterialBasedOnAnimationTime(substanceCurve.keys[i].time);
                            //InsertKeyframe(i, MaterialVariableKeyframeDictionaryList, MaterialVariableKeyframeList, prefabProperties.keyFrameTimes);
                            prefabProperties.prefabAnimationCurveBackup.keys = prefabProperties.prefabAnimationCurve.keys;
                            //SelectKeyframe(i);
                            prefabProperties.currentAnimationTime = 0;
                            return;
                        }
                    }
                }
                prefabProperties.prefabAnimationCurveBackup.keys = prefabProperties.prefabAnimationCurve.keys;
            }
        }
    }

    public void OnEnable()
    {
        PrefabProperties prefabProperties = (PrefabProperties)target;
        if (!EditorApplication.isPlaying && prefabProperties.MaterialVariableKeyframeDictionaryList.Count <= 0)
            prefabProperties.ConvertAnimatedListToDictionaryandSet();
    }

    void OnValidate() // runs when anything in the inspector or graph changes.
    {
#if UNITY_EDITOR
        PrefabProperties prefabProperties = (PrefabProperties)target;
        if (!UnityEditor.EditorWindow.focusedWindow || UnityEditor.EditorWindow.focusedWindow.ToString() != " (UnityEditor.CurveEditorWindow)") // if you delete or add a keyframe in the curve editor, Reset keys.
        {
            if (UnityEditor.EditorWindow.focusedWindow)
            {
                if (prefabProperties.keyFrameTimes.Count > prefabProperties.keyFrameTimesOriginal.Count) // if the keyframe size is larger than the original number of keyframes delete the extra keyframes.
                {
                    int numOfAddedKeyframes = (prefabProperties.keyFrameTimes.Count - prefabProperties.keyFrameTimesOriginal.Count);
                    for (int i = 0; i < numOfAddedKeyframes; i++)
                    {
                        prefabProperties.keyFrameTimes.RemoveAt(prefabProperties.keyFrameTimes.Count - 1);
                    }
                }
                List<Keyframe> tmpKeyframes = new List<Keyframe>();
                if (prefabProperties.prefabAnimationCurve != null && prefabProperties.prefabAnimationCurve.length > 0)
                {
                    tmpKeyframes = prefabProperties.prefabAnimationCurve.keys.ToList();
                    prefabProperties.keyframeSum = 0;
                    if (prefabProperties.prefabAnimationCurve.keys.Count() >= 1 && prefabProperties.keyFrameTimes.Count > 1)
                    {
                        for (int j = prefabProperties.keyFrameTimes.Count() - 1; j > 0; j--)// remove all keys
                            prefabProperties.prefabAnimationCurve.RemoveKey(j);
                    }
                    for (int j = 0; j < prefabProperties.keyFrameTimes.Count(); j++)//rewrite keys with changed times
                    {
                        prefabProperties.prefabAnimationCurve.AddKey(new Keyframe(prefabProperties.keyframeSum, prefabProperties.keyframeSum, tmpKeyframes[j].inTangent, tmpKeyframes[j].outTangent));
                        if (j == 0)
                            UnityEditor.AnimationUtility.SetKeyBroken(prefabProperties.prefabAnimationCurve, 0, true);
                        prefabProperties.keyframeSum += prefabProperties.keyFrameTimes[j];
                    }
                    if (prefabProperties.keyFrameTimes.Count > 1)
                        prefabProperties.keyframeSum -= prefabProperties.keyFrameTimes[prefabProperties.keyFrameTimes.Count - 1];

                    if (prefabProperties.animationTimeRestartEnd > prefabProperties.keyframeSum) //Reset animation variables
                    {
                        prefabProperties.currentAnimationTime = 0;
                        prefabProperties.currentKeyframeIndex = 0;
                        prefabProperties.animationTimeRestartEnd = 0;
                    }
                    // prefabProperties.prefabAnimationCurveBackup.keys = prefabProperties.prefabAnimationCurve.keys;
                }
            }
        }
        if (UnityEditor.EditorWindow.focusedWindow && UnityEditor.EditorWindow.focusedWindow.ToString() == " (UnityEditor.CurveEditorWindow)" && prefabProperties.prefabAnimationCurve.keys.Count() != prefabProperties.prefabAnimationCurveBackup.keys.Count()) // if you delete or add a keyframe in the curve editor, Reset keys.
        {
            prefabProperties.prefabAnimationCurve.keys = prefabProperties.prefabAnimationCurveBackup.keys;
        }
        prefabProperties.CalculateAnimationStartTime(prefabProperties.animationStartKeyframe);
#endif
    }
}
