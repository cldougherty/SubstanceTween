using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SubstanceTweenAnimationUtility
{
    public static void CalculateAnimationLength(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams )
    {
        if (substanceMaterialParams.MaterialVariableKeyframeList.Count() >= 2)
        {
            animationParams.totalAnimationLength = 0;
            for (int i = 0; i < substanceMaterialParams.MaterialVariableKeyframeList.Count() - 1; i++)
            {
                animationParams.totalAnimationLength += animationParams.keyFrameTimes[i];
            }
        }
    }

    public static void CacheAnimatedProceduralVariables(SubstanceMaterialParams substanceMaterialParams,SubstanceAnimationParams animationParams, bool prefab = false) //Checks which variables change throughout keyframes and add them to a list that contains animated variables.If cacheSubstance is true it will also set those variables to cache
    {
        if (animationParams.cacheSubstance && substanceMaterialParams.materialVariables != null)
        {
            substanceMaterialParams.animatedMaterialVariables.Clear(); // Makes sure if a variable stops being animatable it the list will clear and  refresh later 
            for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
            {
                substanceMaterialParams.substance.CacheProceduralProperty(substanceMaterialParams.materialVariables[i].name, false);// Makes sure if a variable stops being animatable it the list will  refresh 
            }
            if (substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count >= 2)
            {
                for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
                {
                    ProceduralPropertyDescription MaterialVariable = substanceMaterialParams.materialVariables[i];
                    ProceduralPropertyType propType = substanceMaterialParams.materialVariables[i].type;
                    bool varChanged = false;
                    if (propType == ProceduralPropertyType.Float)
                    {
                        float variableAnimationCheck = 0;
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, float> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyFloatDictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)// find value of current parameter on first frame;
                                        variableAnimationCheck = keyValue.Value;
                                    else if (j > 0 && keyValue.Value != variableAnimationCheck) // if value of parameter on this keyframe is diffrent then the the value on the first keyframe.
                                    {
                                        substanceMaterialParams.substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
                    {
                        Color variableAnimationCheck = Color.white;
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Color> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyColorDictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    else if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substanceMaterialParams.substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (propType == ProceduralPropertyType.Vector2)
                    {
                        Vector2 variableAnimationCheck = new Vector2(0, 0);
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector2> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyVector2Dictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    else if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substanceMaterialParams.substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (propType == ProceduralPropertyType.Vector3)
                    {
                        Vector3 variableAnimationCheck = new Vector3(0, 0, 0);
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector3> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyVector3Dictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    else if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substanceMaterialParams.substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (propType == ProceduralPropertyType.Vector4)
                    {
                        Vector4 variableAnimationCheck = new Vector4(0, 0, 0, 0);
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector4> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyVector4Dictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    else if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substanceMaterialParams.substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (propType == ProceduralPropertyType.Boolean)
                    {
                        bool variableAnimationCheck = false;
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Boolean> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyBoolDictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    else if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substanceMaterialParams.substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (propType == ProceduralPropertyType.Enum)
                    {
                        int variableAnimationCheck = 0;
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, int> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyEnumDictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    else if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        substanceMaterialParams.substance.CacheProceduralProperty(keyValue.Key, true);
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (!animationParams.cacheSubstance && substanceMaterialParams.materialVariables != null) // add variables if the animate but do not cache them.
        {
            substanceMaterialParams.animatedMaterialVariables.Clear();
            for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
            {
                substanceMaterialParams.substance.CacheProceduralProperty(substanceMaterialParams.materialVariables[i].name, false);
            }
            if (substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Count >= 2)
            {
                for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
                {
                    ProceduralPropertyDescription MaterialVariable = substanceMaterialParams.materialVariables[i];
                    ProceduralPropertyType propType = substanceMaterialParams.materialVariables[i].type;
                    bool varChanged = false;
                    if (propType == ProceduralPropertyType.Float)
                    {
                        float variableAnimationCheck = 0;
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, float> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyFloatDictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
                    {
                        Color variableAnimationCheck = Color.white;
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Color> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyColorDictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Vector2)
                    {
                        Vector2 variableAnimationCheck = new Vector2(0, 0);
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector2> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyVector2Dictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Vector3)
                    {
                        Vector3 variableAnimationCheck = new Vector3(0, 0, 0);
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector3> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyVector3Dictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Vector4)
                    {
                        Vector4 variableAnimationCheck = new Vector4(0, 0, 0, 0);
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Vector4> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyVector4Dictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Boolean)
                    {
                        bool variableAnimationCheck = false;
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, Boolean> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyBoolDictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
                                        varChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (propType == ProceduralPropertyType.Enum)
                    {
                        int variableAnimationCheck = 0;
                        for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                        {
                            foreach (KeyValuePair<string, int> keyValue in substanceMaterialParams.MaterialVariableKeyframeDictionaryList[j].PropertyEnumDictionary)
                            {
                                if (!varChanged && keyValue.Key == substanceMaterialParams.materialVariables[i].name)
                                {
                                    if (j == 0)
                                        variableAnimationCheck = keyValue.Value;
                                    if (j > 0 && keyValue.Value != variableAnimationCheck)
                                    {
                                        if (substanceMaterialParams.animatedMaterialVariables.Contains(MaterialVariable) == false)
                                            substanceMaterialParams.animatedMaterialVariables.Add(MaterialVariable);
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

    public static void ToggleAnimation(List<MaterialVariableDictionaryHolder> keyframeDictList, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams) //Pause-Play animation
    {
        if (keyframeDictList.Count >= 2 && keyframeDictList[0].PropertyMaterialName == substanceMaterialParams.substance.name) // if you have 2 transitions and the name of the selected substance matches the name on keyframe 1
        {
            if (animationParams.substanceLerp) // pause the animation, Set all values not to cache and clear the list of animated variables, it will be rebuilt when playing the animation 
            {
                substanceMaterialParams.MainTexOffset = substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.currentKeyframeIndex].MainTex;
                animationParams.substanceLerp = false;
            }
            else if (!animationParams.substanceLerp)//Play animation, find any variables that change(animated) and set them to cache then add them to a list. 
            {
                SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
                animationParams.substanceLerp = true;
            }
        }
        else if (keyframeDictList.Count >= 2)
        { // If material names are different 
            substanceToolParams.DebugStrings.Add("Tried to animate object: " + substanceToolParams.currentSelection + " but the Transition Material name " + keyframeDictList[0].PropertyMaterialName + " did not match the current Procedural Material name: " + substanceMaterialParams.substance.name);
            var renameMaterialOption = UnityEditor.EditorUtility.DisplayDialog(
                "error",
                "Transition Material name " + keyframeDictList[0].PropertyMaterialName + " does not match current Procedural Material name: " + substanceMaterialParams.substance.name + ". Would you like to rename " + keyframeDictList[0].PropertyMaterialName + " to " + substanceMaterialParams.substance.name + "?"
                + " (Only do this if you are sure the materials are the same and only have different names)", "Yes", "No");
            if (renameMaterialOption)
            {
                substanceToolParams.DebugStrings.Add("Renamed Material: " + keyframeDictList[0].PropertyMaterialName + " To: " + substanceMaterialParams.substance.name);
                keyframeDictList[0].PropertyMaterialName = substanceMaterialParams.substance.name; // Saves Substance name in keyframe as current substance name
                for (int i = 0; i <= keyframeDictList.Count - 1; i++)
                    keyframeDictList[i].PropertyMaterialName = substanceMaterialParams.substance.name;
                animationParams.substanceLerp = true;
            }
            else
                substanceToolParams.DebugStrings.Add("Did not rename or take any other action.");
        }
        else
            UnityEditor.EditorUtility.DisplayDialog("error", "You do not have two keyframes", "OK");
    }

    public static void DeleteNonAnimatingParametersOnPrefab(PrefabProperties prefabProperties, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams)//deletes unused parameters past the first keyframe for optimizing animations for prefabs.
    {
        float floatValue;
        Color ColorValue;
        Vector2 Vector2Value;
        Vector3 Vector3Value;
        Vector4 Vector4Value;
        bool boolValue;
        int enumValue;

        for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
        {// search for variables that never change in animation and delete them from the dictionary.
            ProceduralPropertyDescription MaterialVariable = substanceMaterialParams.materialVariables[i];
            ProceduralPropertyType propType = substanceMaterialParams.materialVariables[i].type;
            bool varChanged = false;
            if (propType == ProceduralPropertyType.Float)
            {
                float propertyFloatAnimationCheck = 0;
                for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                {
                    if (!varChanged && prefabProperties.MaterialVariableKeyframeList[j].myFloatKeys.Contains(substanceMaterialParams.materialVariables[i].name))
                    {
                        //MaterialVariableKeyframeDictionaryList[j].PropertyFloatDictionary.TryGetValue(materialVariables[i].name, out floatValue);
                        floatValue = prefabProperties.MaterialVariableKeyframeList[j].myFloatValues[prefabProperties.MaterialVariableKeyframeList[j].myFloatKeys.IndexOf(substanceMaterialParams.materialVariables[i].name)];
                        if (j == 0)
                            propertyFloatAnimationCheck = floatValue;
                        else if (j > 0 && floatValue != propertyFloatAnimationCheck)
                        {
                            prefabProperties.animatedParameterNames.Add(MaterialVariable.name);
                            varChanged = true;
                        }
                        else if (j == animationParams.keyFrameTimes.Count - 1 && floatValue == propertyFloatAnimationCheck)
                        {
                            for (int k = 1; k <= animationParams.keyFrameTimes.Count - 1; k++) // skips first keyframe
                            {
                                prefabProperties.MaterialVariableKeyframeList[k].myFloatValues.RemoveAt(prefabProperties.MaterialVariableKeyframeList[j].myFloatKeys.IndexOf(substanceMaterialParams.materialVariables[i].name));
                                prefabProperties.MaterialVariableKeyframeList[k].myFloatKeys.Remove(substanceMaterialParams.materialVariables[i].name);
                            }
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                Color propertyColorAnimationCheck = Color.white;
                for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                {
                    if (!varChanged && prefabProperties.MaterialVariableKeyframeList[j].myColorKeys.Contains(substanceMaterialParams.materialVariables[i].name))
                    {
                        ColorValue = prefabProperties.MaterialVariableKeyframeList[j].myColorValues[prefabProperties.MaterialVariableKeyframeList[j].myColorKeys.IndexOf(substanceMaterialParams.materialVariables[i].name)];
                        if (j == 0)
                            propertyColorAnimationCheck = ColorValue;
                        else if (j > 0 && ColorValue != propertyColorAnimationCheck)
                        {
                            prefabProperties.animatedParameterNames.Add(MaterialVariable.name);
                            varChanged = true;
                        }
                        else if (j == animationParams.keyFrameTimes.Count - 1 && ColorValue == propertyColorAnimationCheck)
                        {
                            for (int k = 1; k <= animationParams.keyFrameTimes.Count - 1; k++) // skips first keyframe
                            {
                                prefabProperties.MaterialVariableKeyframeList[k].myColorValues.RemoveAt(prefabProperties.MaterialVariableKeyframeList[j].myColorKeys.IndexOf(substanceMaterialParams.materialVariables[i].name));
                                prefabProperties.MaterialVariableKeyframeList[k].myColorKeys.Remove(substanceMaterialParams.materialVariables[i].name);
                            }
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Vector4)
            {
                Vector4 propertyVector4AnimationCheck = Vector4.zero;
                for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                {
                    if (!varChanged && prefabProperties.MaterialVariableKeyframeList[j].myVector4Keys.Contains(substanceMaterialParams.materialVariables[i].name))
                    {
                        Vector4Value = prefabProperties.MaterialVariableKeyframeList[j].myVector4Values[prefabProperties.MaterialVariableKeyframeList[j].myVector4Keys.IndexOf(substanceMaterialParams.materialVariables[i].name)];
                        if (j == 0)
                            propertyVector4AnimationCheck = Vector4Value;
                        else if (j > 0 && Vector4Value != propertyVector4AnimationCheck)
                        {
                            prefabProperties.animatedParameterNames.Add(MaterialVariable.name);
                            varChanged = true;
                        }
                        else if (j == animationParams.keyFrameTimes.Count - 1 && Vector4Value == propertyVector4AnimationCheck)
                        {
                            for (int k = 1; k <= animationParams.keyFrameTimes.Count - 1; k++) // skips first keyframe
                            {
                                prefabProperties.MaterialVariableKeyframeList[k].myVector4Values.RemoveAt(prefabProperties.MaterialVariableKeyframeList[j].myVector4Keys.IndexOf(substanceMaterialParams.materialVariables[i].name));
                                prefabProperties.MaterialVariableKeyframeList[k].myVector4Keys.Remove(substanceMaterialParams.materialVariables[i].name);
                            }
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Vector3)
            {
                Vector3 propertyVector3AnimationCheck = Vector3.zero;
                for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                {
                    if (!varChanged && prefabProperties.MaterialVariableKeyframeList[j].myVector3Keys.Contains(substanceMaterialParams.materialVariables[i].name))
                    {
                        Vector3Value = prefabProperties.MaterialVariableKeyframeList[j].myVector3Values[prefabProperties.MaterialVariableKeyframeList[j].myVector3Keys.IndexOf(substanceMaterialParams.materialVariables[i].name)];
                        if (j == 0)
                            propertyVector3AnimationCheck = Vector3Value;
                        else if (j > 0 && Vector3Value != propertyVector3AnimationCheck)
                        {
                            prefabProperties.animatedParameterNames.Add(MaterialVariable.name);
                            varChanged = true;
                        }
                        else if (j == animationParams.keyFrameTimes.Count - 1 && Vector3Value == propertyVector3AnimationCheck)
                        {
                            for (int k = 1; k <= animationParams.keyFrameTimes.Count - 1; k++) // skips first keyframe
                            {
                                prefabProperties.MaterialVariableKeyframeList[k].myVector3Values.RemoveAt(prefabProperties.MaterialVariableKeyframeList[j].myVector3Keys.IndexOf(substanceMaterialParams.materialVariables[i].name));
                                prefabProperties.MaterialVariableKeyframeList[k].myVector3Keys.Remove(substanceMaterialParams.materialVariables[i].name);
                            }
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Vector2)
            {
                Vector2 propertyVector2AnimationCheck = Vector2.zero;
                for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                {
                    if (!varChanged && prefabProperties.MaterialVariableKeyframeList[j].myVector2Keys.Contains(substanceMaterialParams.materialVariables[i].name))
                    {
                        Vector2Value = prefabProperties.MaterialVariableKeyframeList[j].myVector2Values[prefabProperties.MaterialVariableKeyframeList[j].myVector2Keys.IndexOf(substanceMaterialParams.materialVariables[i].name)];
                        if (j == 0)
                            propertyVector2AnimationCheck = Vector2Value;
                        else if (j > 0 && Vector2Value != propertyVector2AnimationCheck)
                        {
                            prefabProperties.animatedParameterNames.Add(MaterialVariable.name);
                            varChanged = true;
                        }
                        else if (j == animationParams.keyFrameTimes.Count - 1 && Vector2Value == propertyVector2AnimationCheck)
                        {
                            for (int k = 1; k <= animationParams.keyFrameTimes.Count - 1; k++) // skips first keyframe
                            {
                                prefabProperties.MaterialVariableKeyframeList[k].myVector2Values.RemoveAt(prefabProperties.MaterialVariableKeyframeList[j].myVector2Keys.IndexOf(substanceMaterialParams.materialVariables[i].name));
                                prefabProperties.MaterialVariableKeyframeList[k].myVector2Keys.Remove(substanceMaterialParams.materialVariables[i].name);
                            }
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Enum)
            {
                int propertyEnumAnimationCheck = 9999;
                for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                {
                    if (!varChanged && prefabProperties.MaterialVariableKeyframeList[j].myEnumKeys.Contains(substanceMaterialParams.materialVariables[i].name))
                    {
                        enumValue = prefabProperties.MaterialVariableKeyframeList[j].myEnumValues[prefabProperties.MaterialVariableKeyframeList[j].myEnumKeys.IndexOf(substanceMaterialParams.materialVariables[i].name)];
                        if (j == 0)
                            propertyEnumAnimationCheck = enumValue;
                        else if (j > 0 && enumValue != propertyEnumAnimationCheck)
                        {
                            prefabProperties.animatedParameterNames.Add(MaterialVariable.name);
                            varChanged = true;
                        }
                        else if (j == animationParams.keyFrameTimes.Count - 1 && enumValue == propertyEnumAnimationCheck)
                        {
                            for (int k = 1; k <= animationParams.keyFrameTimes.Count - 1; k++) // skips first keyframe
                            {
                                prefabProperties.MaterialVariableKeyframeList[k].myEnumValues.RemoveAt(prefabProperties.MaterialVariableKeyframeList[j].myEnumKeys.IndexOf(substanceMaterialParams.materialVariables[i].name));
                                prefabProperties.MaterialVariableKeyframeList[k].myEnumKeys.Remove(substanceMaterialParams.materialVariables[i].name);
                            }
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Boolean)
            {
                bool propertyBoolAnimationCheck = false;
                for (int j = 0; j <= animationParams.keyFrameTimes.Count - 1; j++)
                {
                    if (!varChanged && prefabProperties.MaterialVariableKeyframeList[j].myBooleanKeys.Contains(substanceMaterialParams.materialVariables[i].name))
                    {
                        boolValue = prefabProperties.MaterialVariableKeyframeList[j].myBooleanValues[prefabProperties.MaterialVariableKeyframeList[j].myBooleanKeys.IndexOf(substanceMaterialParams.materialVariables[i].name)];
                        if (j == 0)
                            propertyBoolAnimationCheck = boolValue;
                        else if (j > 0 && boolValue != propertyBoolAnimationCheck)
                        {
                            prefabProperties.animatedParameterNames.Add(MaterialVariable.name);
                            varChanged = true;
                        }
                        else if (j == animationParams.keyFrameTimes.Count - 1 && boolValue == propertyBoolAnimationCheck)
                        {
                            for (int k = 1; k <= animationParams.keyFrameTimes.Count - 1; k++) // skips first keyframe
                            {
                                prefabProperties.MaterialVariableKeyframeList[k].myBooleanValues.RemoveAt(prefabProperties.MaterialVariableKeyframeList[j].myBooleanKeys.IndexOf(substanceMaterialParams.materialVariables[i].name));
                                prefabProperties.MaterialVariableKeyframeList[k].myBooleanKeys.Remove(substanceMaterialParams.materialVariables[i].name);
                            }
                        }
                    }
                }
            }
        }
    }
}
