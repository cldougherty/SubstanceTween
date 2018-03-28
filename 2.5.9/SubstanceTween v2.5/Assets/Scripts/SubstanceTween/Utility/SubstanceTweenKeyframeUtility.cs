using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
/// <summary>
/// Functions for using keyframes
/// </summary>
public static class SubstanceTweenKeyframeUtility
{
    public static void SelectKeyframe(int index, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams) // select a keyframe by its number.
    {
        SubstanceTweenSetParameterUtility.SetProceduralVariablesFromList(substanceMaterialParams.MaterialVariableKeyframeList[index], substanceMaterialParams, animationParams, substanceToolParams);
        substanceMaterialParams.emissionInput = substanceMaterialParams.MaterialVariableKeyframeList[index].emissionColor;
        if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor"))
        {
            substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", substanceMaterialParams.MaterialVariableKeyframeList[index].emissionColor);
            substanceToolParams.selectedPrefabScript.emissionInput = substanceMaterialParams.MaterialVariableKeyframeList[index].emissionColor;
        }
        if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_MainTex"))
        {
            substanceMaterialParams.MainTexOffset = substanceMaterialParams.MaterialVariableKeyframeList[index].MainTex;
            substanceMaterialParams.rend.sharedMaterial.SetTextureOffset("_MainTex", substanceMaterialParams.MaterialVariableKeyframeList[index].MainTex);
        }
    }

    public static void OverWriteKeyframe(int index, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams)// Overwrites a keyframe with the current procedural values
    {
        if (animationParams.keyFrameTimes.Count >= 1)
        {
            if (index >= 0)
            {
                substanceMaterialParams.MaterialVariableKeyframeList.Remove(substanceMaterialParams.MaterialVariableKeyframeList[index]);
                substanceMaterialParams.MaterialVariableKeyframeList.Insert(index, new MaterialVariableListHolder());
                SubstanceTweenStorageUtility.AddProceduralVariablesToList(substanceMaterialParams.MaterialVariableKeyframeList[index], substanceMaterialParams, animationParams, substanceToolParams);
                    substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Remove(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[index]);
                substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Insert(index, new MaterialVariableDictionaryHolder());
                SubstanceTweenStorageUtility.AddProceduralVariablesToDictionary(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[index], substanceMaterialParams, animationParams, substanceToolParams);
                substanceToolParams.DebugStrings.Add("OverWrote Keyframe: " + (index + 1));
            }
        }
        animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
        SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
    }

    public static void SelectAndOverWriteAllKeyframes(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams)
    {
        animationParams.keyframeSum = 0;
        for (int i = 0; i <= substanceMaterialParams.MaterialVariableKeyframeList.Count() - 1; i++)
        {
            SubstanceTweenKeyframeUtility.SelectKeyframe(i, substanceMaterialParams, animationParams, substanceToolParams);
            SubstanceTweenKeyframeUtility.OverWriteKeyframe(i, substanceMaterialParams, animationParams, substanceToolParams);
            animationParams.keyframeSum += animationParams.keyFrameTimes[i];
        }
        SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
    }

    public static void CreateKeyframe( SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams)
    {
        //UnityEditor.Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { substanceMaterialParams.substance, substanceToolParams.currentSelection }, "Create Keyframe " + substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count().ToString());
        if (animationParams.keyFrameTimes.Count == 0)
        {
            substanceToolParams.DebugStrings.Add("Created Keyframe 1:");
            substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
            substanceMaterialParams.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
            SubstanceTweenStorageUtility.AddProceduralVariablesToList(substanceMaterialParams.MaterialVariableKeyframeList[0], substanceMaterialParams, animationParams, substanceToolParams);
            SubstanceTweenStorageUtility.AddProceduralVariablesToDictionary(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[0], substanceMaterialParams, animationParams, substanceToolParams);
            animationParams.keyFrames++;
            animationParams.keyFrameTimes.Add(substanceMaterialParams.MaterialVariableKeyframeList[0].animationTime);
            animationParams.substanceCurve.AddKey(new Keyframe(substanceMaterialParams.MaterialVariableKeyframeList[0].animationTime, substanceMaterialParams.MaterialVariableKeyframeList[0].animationTime));
            UnityEditor.AnimationUtility.SetKeyLeftTangentMode(animationParams.substanceCurve, 0, UnityEditor.AnimationUtility.TangentMode.Linear);
            UnityEditor.AnimationUtility.SetKeyRightTangentMode(animationParams.substanceCurve, 0, UnityEditor.AnimationUtility.TangentMode.Linear);
        }
        else if (animationParams.keyFrameTimes.Count > 0)
        {
            for (int i = 0; i <= animationParams.keyFrameTimes.Count - 1; i++)
            {// Goes through each key frame and checks if the keyframe that you are trying to create has the same number of parameters as the rest and if they all save Output parameters or not.
                if (substanceMaterialParams.saveOutputParameters && substanceMaterialParams.MaterialVariableKeyframeList[i].hasParametersWithoutRange == false)
                {//Subsance designer can export special properties like '$randomSeed' that can be saved. this checks if you selected to save those objects and turned it off later
                   UnityEditor.EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are saving Parameters without a range however keyframe " + (i + 1) + " does " +
                    "not save these variables. To fix this uncheck \"Save Output Parameters\" on this frame and try again or check \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                    return;
                }
                if (!substanceMaterialParams.saveOutputParameters && substanceMaterialParams.MaterialVariableKeyframeList[i].hasParametersWithoutRange == true)
                {
                   UnityEditor.EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are not saving Parameters without a range however keyframe " + i + " does " +
                    "save these variables. To fix this check \"Save Output Parameters\" on this frame and try again or uncheck \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                    return;
                }
            }
            substanceMaterialParams.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
            substanceToolParams.DebugStrings.Add("Created KeyFrame: " + substanceMaterialParams.MaterialVariableKeyframeList.Count);
            substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
            SubstanceTweenStorageUtility.AddProceduralVariablesToList(substanceMaterialParams.MaterialVariableKeyframeList[animationParams.keyFrames], substanceMaterialParams, animationParams, substanceToolParams);
            SubstanceTweenStorageUtility.AddProceduralVariablesToDictionary(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.keyFrames], substanceMaterialParams, animationParams, substanceToolParams);
            animationParams.keyFrameTimes.Add(substanceMaterialParams.MaterialVariableKeyframeList[animationParams.keyFrames].animationTime);
            animationParams.keyframeSum = 0;
            for (int i = 0; i < substanceMaterialParams.MaterialVariableKeyframeList.Count() - 1; i++)
            {
                animationParams.keyframeSum += substanceMaterialParams.MaterialVariableKeyframeList[i].animationTime;
            }
            animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum));
            if (animationParams.keyFrames >0)
            UnityEditor.AnimationUtility.SetKeyLeftTangentMode(animationParams.substanceCurve, animationParams.keyFrames, UnityEditor.AnimationUtility.TangentMode.Linear);
            UnityEditor.AnimationUtility.SetKeyRightTangentMode(animationParams.substanceCurve, animationParams.keyFrames, UnityEditor.AnimationUtility.TangentMode.Linear);
            animationParams.keyFrames++;
        }
        animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
        substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
        SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
        SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
    }

    public static void InsertKeyframe(int index, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams)
    {
        if (animationParams.keyFrameTimes.Count > 0)
        {
            for (int i = 0; i <= animationParams.keyFrameTimes.Count - 1; i++)
            {// Goes through each key frame and checks if the keyframe that you are trying to create has the same number of parameters as the rest and if they all save Output parameters or not.
                if (substanceMaterialParams.saveOutputParameters && substanceMaterialParams.MaterialVariableKeyframeList[i].hasParametersWithoutRange == false)
                {//Substance designer can export special properties like '$randomSeed' that can be saved. this checks if you selected to save those objects and turned it off later
                   UnityEditor.EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are saving Parameters without a range however keyframe " + (i + 1) + " does " +
                    "not save these variables. To fix this uncheck \"Save Output Parameters\" on this frame and try again or check \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                    return;
                }
                if (!substanceMaterialParams.saveOutputParameters && substanceMaterialParams.MaterialVariableKeyframeList[i].hasParametersWithoutRange == true)
                {
                  UnityEditor.EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are not saving Parameters without a range however keyframe " + i + " does " +
                    "save these variables. To fix this check \"Save Output Parameters\" on this frame and try again or uncheck \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
                    return;
                }
            }
            substanceMaterialParams.MaterialVariableKeyframeList.Insert(index, new MaterialVariableListHolder());
            substanceToolParams.DebugStrings.Add("Created KeyFrame: " + substanceMaterialParams.MaterialVariableKeyframeList.Count);
            substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Insert(index, new MaterialVariableDictionaryHolder());
            SubstanceTweenStorageUtility.AddProceduralVariablesToList(substanceMaterialParams.MaterialVariableKeyframeList[index], substanceMaterialParams, animationParams, substanceToolParams);
            SubstanceTweenStorageUtility.AddProceduralVariablesToDictionary(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[index], substanceMaterialParams, animationParams, substanceToolParams);
            animationParams.keyframeSum = 0;
            for (int i = 0; i < index; i++)
            {
                animationParams.keyframeSum += substanceMaterialParams.MaterialVariableKeyframeList[i].animationTime;
            }
            if (index >= 1)
            {
                substanceMaterialParams.MaterialVariableKeyframeList[index - 1].animationTime = animationParams.currentAnimationTime;
                animationParams.keyFrameTimes[index - 1] = animationParams.currentAnimationTime;
            }
            else
            {
                substanceMaterialParams.MaterialVariableKeyframeList[index - 1].animationTime = animationParams.currentAnimationTime;
                animationParams.keyFrameTimes[index - 1] = animationParams.currentAnimationTime;
            }
            substanceMaterialParams.MaterialVariableKeyframeList[index].animationTime = animationParams.keyframeSum - animationParams.animationTimeRestartEnd;
            animationParams.keyFrameTimes.Insert(index, animationParams.substanceCurve.keys[index+1].time - animationParams.substanceCurve.keys[index].time); // note: change animation time if inserting keyframe

            animationParams.keyFrames++;
        }
        animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
        SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
        SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
    }

    public static void DeleteKeyframe(int index, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams  )
    {
        List<Keyframe> tmpKeyframeList = animationParams.substanceCurve.keys.ToList();
        animationParams.keyFrameTimes.RemoveAt(index);
       substanceMaterialParams.MaterialVariableKeyframeList.RemoveAt(index);
        substanceMaterialParams.MaterialVariableKeyframeDictionaryList.RemoveAt(index);
        animationParams.keyframeSum = 0;
        for (int i = substanceMaterialParams.MaterialVariableKeyframeList.Count(); i >= 0; i--)
            animationParams.substanceCurve.RemoveKey(i);
        for (int i = 0; i <= substanceMaterialParams.MaterialVariableKeyframeList.Count() - 1; i++)
        {
            animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum, tmpKeyframeList[i].inTangent, tmpKeyframeList[i].outTangent));
            animationParams.keyframeSum += animationParams.keyFrameTimes[i];
        };
        animationParams.currentAnimationTime = 0;
        animationParams.currentKeyframeIndex = 0;
        animationParams.animationTimeRestartEnd = 0;
        animationParams.keyframeSum = 0;
        if (animationParams.keyFrames > 0)
            animationParams.keyFrames--;
        animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
        SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
        SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
    }

    public static void DeleteAllKeyframes(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams )
    {
        if (UnityEditor.EditorUtility.DisplayDialog("DELETE ALL keyframes?",
                "Are you sure you want to DELETE ALL keyframes?", "YES", "NO"))
        {
            animationParams.substanceLerp = false;
            substanceMaterialParams.MaterialVariableKeyframeList.Clear();
            substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Clear();
            animationParams.keyFrameTimes.Clear();
            animationParams.keyFrames = 0;
            animationParams.substanceCurve.keys = null;
            animationParams.substanceCurveBackup.keys = null;
            animationParams.currentAnimationTime = 0;
            animationParams.currentKeyframeIndex = 0;
            animationParams.animationTimeRestartEnd = 0;
            animationParams.keyframeSum = 0;
        }
    }

   public static void  CheckForAddKeyFromCurveEditor(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams, SubstanceFlickerParams flickerValues)
    {
        for (int i = 0; i <= animationParams.substanceCurveBackup.keys.Count() - 1; i++)
        {
            if (i <= animationParams.substanceCurve.keys.Count() - 1 && animationParams.substanceCurve.keys[i].time != animationParams.substanceCurveBackup.keys[i].time) // find keyframe that has just been created or deleted in the curve editor
            {
                if (animationParams.substanceCurve.keys.Count() > animationParams.substanceCurveBackup.keys.Count())
                {
                    animationParams.substanceCurve.MoveKey(i, new Keyframe(animationParams.substanceCurve.keys[i].time, animationParams.substanceCurve.keys[i].time)); // lerp does not work correctly if the keyframe value is different than the time. i cant edit the key so i delete then add a new one
                    float tempKeyframeTime = animationParams.substanceCurve.keys[i].time;
                    SubstanceTweenSetParameterUtility.SetProceduralMaterialBasedOnAnimationTime(ref tempKeyframeTime, substanceMaterialParams, animationParams, substanceToolParams, flickerValues);
                    SubstanceTweenKeyframeUtility.InsertKeyframe(i, substanceMaterialParams, animationParams, substanceToolParams);
                    SubstanceTweenKeyframeUtility.SelectKeyframe(i, substanceMaterialParams, animationParams, substanceToolParams);
                    animationParams.currentAnimationTime = 0;
                    animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
                    SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
                    return;
                }
            }
        }
    }


    public static void CheckForRemoveOrEditFromCurveEditor(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams  substanceToolParams, SubstanceFlickerParams flickerValues)
    {
        bool inEditKeysMode = false;
        int keyframesModifiedSoFar = 0; //number of keyframes deleted edited so far
        int curvePointsCountDifference = animationParams.substanceCurveBackup.keys.Count() - animationParams.substanceCurve.keys.Count(); // difference between number of points before the change compared to the number of points after the change
        List<float> BackupKeyframePointTimes = new List<float>();// points on curve before change
        List<float> CurrentKeyframePointTimes = new List<float>();// points on curve after change
        Keyframe firstEditedKeyframeBeforeEdit = new Keyframe(-1, -1); //the first keyframe in the list that has been edited ore deleted 
        int firstEditedKeyframeIndexBeforeEdit = -1;  // index of the first edited keyframe
        if (animationParams.substanceCurve.keys[0].time != 0)
            animationParams.substanceCurve.keys[0].time = 0;
        for (int i = 0; i <= animationParams.substanceCurveBackup.keys.Count() - 1; i++)
        {
            BackupKeyframePointTimes.Add(animationParams.substanceCurveBackup.keys[i].time);
        }
        for (int i = 0; i <= animationParams.substanceCurve.keys.Count() - 1; i++)
        {
            CurrentKeyframePointTimes.Add(animationParams.substanceCurve.keys[i].time);
        }

        for (int i = 0; i <= BackupKeyframePointTimes.Count() - 2; i++)
        {
            if (!CurrentKeyframePointTimes.Contains(BackupKeyframePointTimes[i]))
            { // find index of the first key that has ben edited using context menu 
                firstEditedKeyframeBeforeEdit = animationParams.substanceCurveBackup.keys[i];
                float tempKeyframeTime = firstEditedKeyframeBeforeEdit.time;
                firstEditedKeyframeIndexBeforeEdit = i;
                SubstanceTweenSetParameterUtility.SetProceduralMaterialBasedOnAnimationTime(ref tempKeyframeTime, substanceMaterialParams, animationParams, substanceToolParams, flickerValues);
                break;
            }
        }

        for (int i = 0; i <= CurrentKeyframePointTimes.Count() - 2; i++)
        {
            if (!BackupKeyframePointTimes.Contains(CurrentKeyframePointTimes[i]))
            {// if  i use the context menu to edit multiple keyframes to a value that is not already on the graph i can find that new keyframe here.
             // I can tell when i have EDITED a keyframe to a new value because when deleting keyframes, every new point's time should also exist in the backup
                inEditKeysMode = true;
                float tempKeyframeTime = firstEditedKeyframeBeforeEdit.time;
                SubstanceTweenSetParameterUtility.SetProceduralMaterialBasedOnAnimationTime(ref tempKeyframeTime, substanceMaterialParams, animationParams, substanceToolParams, flickerValues); // find/set material based on first edited keyframe
                if (firstEditedKeyframeIndexBeforeEdit != -1) // if multiple keyframes has been edited using context menu 
                    SubstanceTweenKeyframeUtility.OverWriteKeyframe(firstEditedKeyframeIndexBeforeEdit, substanceMaterialParams, animationParams, substanceToolParams); // overwrite new keyframe with material of the first edited keyframe
            }
        }

        for (int i = BackupKeyframePointTimes.Count() - 1; i > 0; i--) // Go through every key in the backup list(Before the keys got deleted) 
        {
            if (!CurrentKeyframePointTimes.Contains(BackupKeyframePointTimes[i])) // if the current list of curve points does not contain this value(it was deleted in the curve editor) 
            {  //Find the index of the value and delete any information that has not already been deleted in the curve editor
               // get the index of the deleted point and delete the Dictionary/List associated with that index 
                keyframesModifiedSoFar++;
                animationParams.keyFrameTimes.RemoveAt(BackupKeyframePointTimes.IndexOf(BackupKeyframePointTimes[i]));
                substanceMaterialParams.MaterialVariableKeyframeList.RemoveAt(BackupKeyframePointTimes.IndexOf(BackupKeyframePointTimes[i]));
                substanceMaterialParams.MaterialVariableKeyframeDictionaryList.RemoveAt(BackupKeyframePointTimes.IndexOf(BackupKeyframePointTimes[i]));
                if (animationParams.keyFrames > 0)
                    animationParams.keyFrames--;
            }
            if (inEditKeysMode && i == 1 && keyframesModifiedSoFar > 0)
            { // if i edited multiple keyframes at the same time using the context menu I insert a keyframe when i reach the end of the for loop
                SubstanceTweenKeyframeUtility.InsertKeyframe(i, substanceMaterialParams, animationParams, substanceToolParams);
            }
        }
        for (int i = 0; i <= animationParams.substanceCurve.keys.Count() - 2; i++)
        { // rebuild/refresh animation times
            animationParams.keyFrameTimes[i] = animationParams.substanceCurve.keys[i + 1].time - animationParams.substanceCurve.keys[i].time;  // this keyframe time = (next keyframe time - This keyframe time)
            substanceMaterialParams.MaterialVariableKeyframeList[i].animationTime = animationParams.substanceCurve.keys[i + 1].time - animationParams.substanceCurve.keys[i].time;
        }

        if (curvePointsCountDifference != keyframesModifiedSoFar)
        { // Because curvePoint EditDifference != keyframes Edited/changed, I know that keyframes have been changed with 'Edit Keys..' and not 'Delete' 
          // when editing multipule keyframes to a new singular time that is not already on the list,
          // there should always be more edited keyframes then the difference between current curve points and backup curve points. 
            if (animationParams.substanceCurve.keys[animationParams.substanceCurve.keys.Count() - 1].time > animationParams.substanceCurveBackup.keys[animationParams.substanceCurveBackup.keys.Count() - 1].time)
            {// Edited multiple keyframes to a new time that is more than the length of the backup curve
                SubstanceTweenKeyframeUtility.CreateKeyframe(substanceMaterialParams, animationParams, substanceToolParams);
            }
        }
        animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
        SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
        inEditKeysMode = false;
    }
}
