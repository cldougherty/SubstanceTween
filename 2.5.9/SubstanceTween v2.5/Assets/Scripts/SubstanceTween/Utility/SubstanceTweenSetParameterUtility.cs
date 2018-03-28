using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Functions for setting Procedural Materials/Variables
/// </summary>
public static class SubstanceTweenSetParameterUtility
{
    public static void SetProceduralMaterialBasedOnAnimationTime(ref float desiredTime,  SubstanceMaterialParams substanceMaterialVariables,  SubstanceAnimationParams substanceAnimationParams ,  SubstanceToolParams substanceToolParams  ,  SubstanceFlickerParams flickerValues  )
    {
        for (int i = 0; i <= substanceAnimationParams.substanceCurveBackup.keys.Count() - 1; i++)
        {
            if (substanceAnimationParams.substanceCurveBackup.keys[i].time > desiredTime) // find first key time that is greater than the desiredAnimationTime 
            {
                float newLerp = (desiredTime - substanceAnimationParams.substanceCurveBackup.keys[i - 1].time) / (substanceAnimationParams.substanceCurveBackup.keys[i].time - substanceAnimationParams.substanceCurveBackup.keys[i - 1].time);// Finds point between two keyrames  - finds percentage of desiredtime between substanceCurveBackup.keys[i - 1].time and substanceCurveBackup.keys[i].time 
                substanceAnimationParams.currentAnimationTime = Mathf.Lerp(0, substanceAnimationParams.keyFrameTimes[i - 1], newLerp);
                substanceAnimationParams.animationTimeRestartEnd = desiredTime;
                substanceAnimationParams.currentKeyframeIndex = i - 1;
                if (EditorWindow.focusedWindow && EditorWindow.focusedWindow.ToString() != " (UnityEditor.CurveEditorWindow)")
                    substanceAnimationParams.lerp = newLerp;
                
                for (int j = 0; j < substanceMaterialVariables.animatedMaterialVariables.Count; j++)// search through dictionary for variable names and if they match animate them
                {
                    ProceduralPropertyDescription animatedMaterialVariable = substanceMaterialVariables.animatedMaterialVariables[j];
                    ProceduralPropertyType propType = substanceMaterialVariables.animatedMaterialVariables[j].type;
                    if (substanceMaterialVariables.materialVariableNames.Contains(animatedMaterialVariable.name))
                    {
                        if (propType == ProceduralPropertyType.Float)
                        {
                            substanceMaterialVariables.substance.SetProceduralFloat(animatedMaterialVariable.name, Mathf.Lerp(substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].PropertyFloatDictionary[animatedMaterialVariable.name], substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i].PropertyFloatDictionary[animatedMaterialVariable.name], newLerp * flickerValues.flickerFloatCalc));
                        }
                        else if (propType == ProceduralPropertyType.Color3)
                            substanceMaterialVariables.substance.SetProceduralColor(animatedMaterialVariable.name, Color.Lerp(substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].PropertyColorDictionary[animatedMaterialVariable.name], substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i].PropertyColorDictionary[animatedMaterialVariable.name], newLerp * flickerValues.flickerColor3Calc));
                        else if (propType == ProceduralPropertyType.Color4)
                            substanceMaterialVariables.substance.SetProceduralColor(animatedMaterialVariable.name, Color.Lerp(substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].PropertyColorDictionary[animatedMaterialVariable.name], substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i].PropertyColorDictionary[animatedMaterialVariable.name], newLerp * flickerValues.flickerColor4Calc));
                        else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
                        {
                            if (propType == ProceduralPropertyType.Vector4)
                                substanceMaterialVariables.substance.SetProceduralVector(animatedMaterialVariable.name, Vector4.Lerp(substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].PropertyVector4Dictionary[animatedMaterialVariable.name], substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i].PropertyVector4Dictionary[animatedMaterialVariable.name], newLerp * flickerValues.flickerVector4Calc));
                            else if (propType == ProceduralPropertyType.Vector3)
                                substanceMaterialVariables.substance.SetProceduralVector(animatedMaterialVariable.name, Vector3.Lerp(substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].PropertyVector3Dictionary[animatedMaterialVariable.name], substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i].PropertyVector3Dictionary[animatedMaterialVariable.name], newLerp * flickerValues.flickerVector3Calc));
                            else if (propType == ProceduralPropertyType.Vector2)
                                substanceMaterialVariables.substance.SetProceduralVector(animatedMaterialVariable.name, Vector2.Lerp(substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].PropertyVector2Dictionary[animatedMaterialVariable.name], substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i].PropertyVector2Dictionary[animatedMaterialVariable.name], newLerp * flickerValues.flickerVector2Calc));
                        }
                        else if (propType == ProceduralPropertyType.Enum)
                            substanceMaterialVariables.substance.SetProceduralEnum(animatedMaterialVariable.name, substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].PropertyEnumDictionary[animatedMaterialVariable.name]);
                        else if (propType == ProceduralPropertyType.Boolean)
                            substanceMaterialVariables.substance.SetProceduralBoolean(animatedMaterialVariable.name, substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].PropertyBoolDictionary[animatedMaterialVariable.name]);
                    }
                }
                if (substanceMaterialVariables.rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    substanceMaterialVariables.emissionInput = Color.Lerp(substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].emissionColor, substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i].emissionColor, newLerp * flickerValues.flickerCalc);
                    substanceMaterialVariables.rend.sharedMaterial.SetColor("_EmissionColor", substanceMaterialVariables.emissionInput);
                    substanceToolParams.selectedPrefabScript.emissionInput = substanceMaterialVariables.emissionInput;
                }
                if (substanceMaterialVariables.rend.sharedMaterial.HasProperty("_MainTex"))
                    substanceMaterialVariables.rend.sharedMaterial.SetTextureOffset("_MainTex", Vector2.Lerp(substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i - 1].MainTex, substanceMaterialVariables.MaterialVariableKeyframeDictionaryList[i].MainTex, newLerp * flickerValues.flickerCalc));
                substanceMaterialVariables.substance.RebuildTextures();
                return;
            }
           
        }
    }

    public static void SetProceduralVariablesFromList(MaterialVariableListHolder propertyList, SubstanceMaterialParams substanceMaterialVariables, SubstanceAnimationParams substanceAnimationParams, SubstanceToolParams substanceToolParams) // Sets current substance parameters from a List
    {
        for (int i = 0; i < substanceMaterialVariables.materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = substanceMaterialVariables.materialVariables[i];
            ProceduralPropertyType propType = substanceMaterialVariables.materialVariables[i].type;
            if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (substanceMaterialVariables.saveOutputParameters && !materialVariable.hasRange) || substanceMaterialVariables.resettingValuesToDefault))
            {
                if (propertyList.myFloatKeys.Count > 0)
                {
                    for (int j = 0; j < propertyList.myFloatKeys.Count; j++)
                    {
                        if (propertyList.myFloatKeys[j] == materialVariable.name)
                        {
                            if (propertyList.myFloatKeys[j] == materialVariable.name)
                                substanceMaterialVariables.substance.SetProceduralFloat(materialVariable.name, propertyList.myFloatValues[j]);
                        }
                    }
                }
                else //note: for old versions of tool, remove later?
                {
                    for (int j = 0; j < propertyList.myKeys.Count; j++)
                    {
                        if (propertyList.myKeys[j] == materialVariable.name)
                        {
                            if (propertyList.myKeys[j] == materialVariable.name)
                                substanceMaterialVariables.substance.SetProceduralFloat(materialVariable.name, float.Parse(propertyList.myValues[j]));
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                if (propertyList.myColorKeys.Count > 0)
                {
                    for (int j = 0; j < propertyList.myColorKeys.Count; j++)
                    {
                        if (propertyList.myColorKeys[j] == materialVariable.name)
                        {
                            Color curColor = propertyList.myColorValues[j];
                            substanceMaterialVariables.substance.SetProceduralColor(materialVariable.name, curColor);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < propertyList.myKeys.Count; j++)
                    {
                        if (propertyList.myKeys[j] == materialVariable.name)
                        {
                            Color curColor;
                            ColorUtility.TryParseHtmlString(propertyList.myValues[j], out curColor);
                            substanceMaterialVariables.substance.SetProceduralColor(materialVariable.name, curColor);
                        }
                    }
                }
            }
            else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (substanceMaterialVariables.saveOutputParameters && !materialVariable.hasRange) || substanceMaterialVariables.resettingValuesToDefault))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    if (propertyList.myVector4Keys.Count > 0)
                    {
                        for (int j = 0; j < propertyList.myVector4Keys.Count; j++)
                        {
                            if (propertyList.myVector4Keys[j] == materialVariable.name)
                            {
                                Vector4 curVector4 = propertyList.myVector4Values[j];
                                substanceMaterialVariables.substance.SetProceduralVector(materialVariable.name, curVector4);
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < propertyList.myKeys.Count; j++)
                        {
                            if (propertyList.myKeys[j] == materialVariable.name)
                            {
                                Vector4 curVector4 = SubstanceTweenMiscUtility.StringToVector(propertyList.myValues[j], 4);
                                substanceMaterialVariables.substance.SetProceduralVector(materialVariable.name, curVector4);
                            }
                        }
                    }
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    if (propertyList.myVector3Keys.Count > 0)
                    {
                        for (int j = 0; j < propertyList.myVector3Keys.Count; j++)
                        {
                            if (propertyList.myVector3Keys[j] == materialVariable.name)
                            {
                                Vector3 curVector3 = propertyList.myVector3Values[j];
                                substanceMaterialVariables.substance.SetProceduralVector(materialVariable.name, curVector3);
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < propertyList.myKeys.Count; j++)
                        {
                            if (propertyList.myKeys[j] == materialVariable.name)
                            {
                                Vector3 curVector3 = SubstanceTweenMiscUtility.StringToVector(propertyList.myValues[j], 3);
                                substanceMaterialVariables.substance.SetProceduralVector(materialVariable.name, curVector3);
                            }
                        }
                    }
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    if (propertyList.myVector2Keys.Count > 0)
                    {
                        for (int j = 0; j < propertyList.myVector2Keys.Count; j++)
                        {
                            if (propertyList.myVector2Keys[j] == materialVariable.name)
                            {
                                Vector2 curVector2 = propertyList.myVector2Values[j];
                                substanceMaterialVariables.substance.SetProceduralVector(materialVariable.name, curVector2);
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < propertyList.myKeys.Count; j++)
                        {
                            if (propertyList.myKeys[j] == materialVariable.name)
                            {
                                Vector2 curVector2 = SubstanceTweenMiscUtility.StringToVector(propertyList.myValues[j], 2);
                                substanceMaterialVariables.substance.SetProceduralVector(materialVariable.name, curVector2);
                            }
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Enum)
            {
                if (propertyList.myEnumKeys.Count > 0)
                {
                    for (int j = 0; j < propertyList.myEnumKeys.Count; j++)
                    {
                        if (propertyList.myEnumKeys[j] == materialVariable.name)
                        {
                            int curEnum = propertyList.myEnumValues[j];
                            substanceMaterialVariables.substance.SetProceduralEnum(materialVariable.name, curEnum);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < propertyList.myKeys.Count; j++)
                    {
                        if (propertyList.myKeys[j] == materialVariable.name)
                        {
                            int curEnum = int.Parse(propertyList.myValues[j]);
                            substanceMaterialVariables.substance.SetProceduralEnum(materialVariable.name, curEnum);
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Boolean)
            {
                if (propertyList.myBooleanKeys.Count > 0)
                {
                    for (int j = 0; j < propertyList.myBooleanKeys.Count; j++)
                    {
                        if (propertyList.myBooleanKeys[j] == materialVariable.name)
                        {
                            bool curBool = propertyList.myBooleanValues[j];
                            substanceMaterialVariables.substance.SetProceduralBoolean(materialVariable.name, curBool);
                        }
                    }
                }

                else
                {
                    for (int j = 0; j < propertyList.myKeys.Count; j++)
                    {
                        if (propertyList.myKeys[j] == materialVariable.name)
                        {
                            bool curBool = bool.Parse(propertyList.myValues[j]);
                            substanceMaterialVariables.substance.SetProceduralBoolean(materialVariable.name, curBool);
                        }
                    }
                }
            }
        }
    }
    public static void SetAllProceduralValuesToMin(SubstanceMaterialParams substanceMaterialParams) // Sets all procedural values to the minimum value
    {
        UnityEditor.Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { substanceMaterialParams.substance }, "Set all values to Minimum");
        for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = substanceMaterialParams.materialVariables[i];
            if (substanceMaterialParams.substance.IsProceduralPropertyVisible(materialVariable.name))
            {
                ProceduralPropertyType propType =substanceMaterialParams.materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float)
                    substanceMaterialParams.substance.SetProceduralFloat(materialVariable.name, substanceMaterialParams.materialVariables[i].minimum);
                if (propType == ProceduralPropertyType.Vector2 && materialVariable.hasRange)
                    substanceMaterialParams.substance.SetProceduralVector(materialVariable.name, new Vector2(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].minimum));
                if (propType == ProceduralPropertyType.Vector3 && materialVariable.hasRange)
                    substanceMaterialParams.substance.SetProceduralVector(materialVariable.name, new Vector3(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].minimum));
                if (propType == ProceduralPropertyType.Vector4 && materialVariable.hasRange)
                    substanceMaterialParams.substance.SetProceduralVector(materialVariable.name, new Vector4(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].minimum));
                if (propType == ProceduralPropertyType.Enum)
                    substanceMaterialParams.substance.SetProceduralEnum(materialVariable.name, 0);
            }
        }
        substanceMaterialParams.substance.RebuildTexturesImmediately();
    }

    public static void SetAllProceduralValuesToMax(SubstanceMaterialParams substanceMaterialParams) // Sets all procedural values to the maximum value
    {
       UnityEditor.Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { substanceMaterialParams.substance }, "Set all values to Maximum");
        for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = substanceMaterialParams.materialVariables[i];
            if (substanceMaterialParams.substance.IsProceduralPropertyVisible(materialVariable.name))
            {
                ProceduralPropertyType propType = substanceMaterialParams.materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float)
                    substanceMaterialParams.substance.SetProceduralFloat(materialVariable.name, substanceMaterialParams.materialVariables[i].maximum);
                if (propType == ProceduralPropertyType.Vector2 && materialVariable.hasRange)
                    substanceMaterialParams.substance.SetProceduralVector(materialVariable.name, new Vector2(substanceMaterialParams.materialVariables[i].maximum, substanceMaterialParams.materialVariables[i].maximum));
                if (propType == ProceduralPropertyType.Vector3 && materialVariable.hasRange)
                    substanceMaterialParams.substance.SetProceduralVector(materialVariable.name, new Vector3(substanceMaterialParams.materialVariables[i].maximum, substanceMaterialParams.materialVariables[i].maximum, substanceMaterialParams.materialVariables[i].maximum));
                if (propType == ProceduralPropertyType.Vector4 && materialVariable.hasRange)
                    substanceMaterialParams.substance.SetProceduralVector(materialVariable.name, new Vector4(substanceMaterialParams.materialVariables[i].maximum, substanceMaterialParams.materialVariables[i].maximum, substanceMaterialParams.materialVariables[i].maximum, substanceMaterialParams.materialVariables[i].maximum));
                if (propType == ProceduralPropertyType.Enum)
                    substanceMaterialParams.substance.SetProceduralEnum(materialVariable.name, substanceMaterialParams.materialVariables[i].enumOptions.Count() - 1);
            }
        }
        substanceMaterialParams.substance.RebuildTexturesImmediately();
    }

    public static void RandomizeProceduralValues(SubstanceMaterialParams substanceMaterialParams, SubstanceRandomizeParams randomizeSettings) // Sets all procedural values to a random value
    {
        for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = substanceMaterialParams.materialVariables[i];
            if (substanceMaterialParams.substance.IsProceduralPropertyVisible(materialVariable.name))
            {
                ProceduralPropertyType propType = substanceMaterialParams.materialVariables[i].type;
                if (propType == ProceduralPropertyType.Float && materialVariable.name[0] != '$' && randomizeSettings.randomizeProceduralFloatToggle)
                    substanceMaterialParams.substance.SetProceduralFloat(materialVariable.name, UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum));
                if (propType == ProceduralPropertyType.Color3 && randomizeSettings.randomizeProceduralColorRGBToggle)
                    substanceMaterialParams.substance.SetProceduralColor(materialVariable.name, new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)));
                if (propType == ProceduralPropertyType.Color4 && randomizeSettings.randomizeProceduralColorRGBAToggle)
                    substanceMaterialParams.substance.SetProceduralColor(materialVariable.name, new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)));
                if (propType == ProceduralPropertyType.Vector2 && materialVariable.name[0] != '$' && randomizeSettings.randomizeProceduralVector2Toggle)
                    substanceMaterialParams.substance.SetProceduralVector(materialVariable.name, new Vector2(UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum), UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum)));
                if (propType == ProceduralPropertyType.Vector3 && randomizeSettings.randomizeProceduralVector3Toggle)
                    substanceMaterialParams.substance.SetProceduralVector(materialVariable.name, new Vector3(UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum), UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum), UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum)));
                if (propType == ProceduralPropertyType.Vector4 && randomizeSettings.randomizeProceduralVector4Toggle)
                    substanceMaterialParams.substance.SetProceduralVector(materialVariable.name, new Vector4(UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum), UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum), UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum), UnityEngine.Random.Range(substanceMaterialParams.materialVariables[i].minimum, substanceMaterialParams.materialVariables[i].maximum)));
                if (propType == ProceduralPropertyType.Enum && randomizeSettings.randomizeProceduralEnumToggle)
                    substanceMaterialParams.substance.SetProceduralEnum(materialVariable.name, UnityEngine.Random.Range(0, substanceMaterialParams.materialVariables[i].enumOptions.Count()));
                if (propType == ProceduralPropertyType.Boolean && randomizeSettings.randomizeProceduralBooleanToggle)
                    substanceMaterialParams.substance.SetProceduralBoolean(materialVariable.name, (UnityEngine.Random.value > 0.5f));
            }
        }
        substanceMaterialParams.substance.RebuildTexturesImmediately();
    }

    public static void ResetAllProceduralValues( SubstanceDefaultMaterialParams substanceDefaultMaterialParams, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams   ) // Resets all procedural values to default(When the material was first selected)
    {
        for (int i = 0; i <= substanceDefaultMaterialParams.defaultSubstanceObjProperties.Count -1; i++)
        {
            if ((substanceMaterialParams.substance.name == substanceDefaultMaterialParams.defaultSubstanceObjProperties[i].PropertyMaterialName) || (substanceMaterialParams.rend.sharedMaterial.name == substanceDefaultMaterialParams.defaultSubstanceObjProperties[i].PropertyMaterialName))
            {
                substanceMaterialParams.resettingValuesToDefault = true;
                SubstanceTweenSetParameterUtility.SetProceduralVariablesFromList(substanceDefaultMaterialParams.defaultSubstanceObjProperties[i], substanceMaterialParams,animationParams,substanceToolParams);
                substanceMaterialParams.MainTexOffset = substanceDefaultMaterialParams.defaultSubstanceObjProperties[i].MainTex;
                if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    substanceMaterialParams.rend.sharedMaterial.EnableKeyword("_EMISSION");
                    substanceMaterialParams.emissionInput = substanceDefaultMaterialParams.defaultSubstanceObjProperties[i].emissionColor;
                    substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", substanceMaterialParams.emissionInput);;
                    substanceToolParams.selectedPrefabScript.emissionInput = substanceMaterialParams.emissionInput;
                }
                substanceMaterialParams.substance.RebuildTexturesImmediately();
                substanceMaterialParams.resettingValuesToDefault = false;
                return;
            }
        }
    }
}
