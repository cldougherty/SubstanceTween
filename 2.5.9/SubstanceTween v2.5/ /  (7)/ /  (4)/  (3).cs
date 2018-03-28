using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;



public static class SubstanceTweenSetParameterUtility
{
    //public SubstanceToolEditor
    //public static void SetProceduralMaterialBasedOnAnimationTime(SubstanceToolEditor)
    public static void SetProceduralMaterialBasedOnAnimationTime(float desiredTime, List<MaterialVariableDictionaryHolder> MaterialVariableKeyframeDictionaryList, List<ProceduralPropertyDescription> animatedMaterialVariables, ProceduralMaterial substance,  Renderer rend, PrefabProperties prefabScript,   AnimationCurve substanceCurve,AnimationCurve substanceCurveBackup,   
      float currentAnimationTime,List<float> keyFrameTimes, float animationTimeRestartEnd, int currentKeyframeIndex, float lerp, Color emissionInput, float flickerFloatCalc, float flickerColor3Calc, float flickerColor4Calc, float flickerVector4Calc, float flickerVector3Calc, float flickerVector2Calc  )
    {
        for (int i = 0; i <= substanceCurveBackup.keys.Count() - 1; i++)
        {
            if (substanceCurveBackup.keys[i].time > desiredTime) // find first key time that is greater than the desiredAnimationTime 
            {
                float newLerp = (desiredTime - substanceCurveBackup.keys[i - 1].time) / (substanceCurveBackup.keys[i].time - substanceCurveBackup.keys[i - 1].time);// Finds point between two keyrames  - finds percentage of desiredtime between substanceCurveBackup.keys[i - 1].time and substanceCurveBackup.keys[i].time 
                currentAnimationTime = Mathf.Lerp(0, keyFrameTimes[i - 1], newLerp);
                animationTimeRestartEnd = desiredTime;
                currentKeyframeIndex = i - 1;
                if (UnityEditor.EditorWindow.focusedWindow && UnityEditor.EditorWindow.focusedWindow.ToString() != " (UnityEditor.CurveEditorWindow)")
                    lerp = newLerp;
                for (int j = 0; j < animatedMaterialVariables.Count(); j++)// search through dictionary for variable names and if they match animate them
                {
                    ProceduralPropertyDescription animatedMaterialVariable = animatedMaterialVariables[j];
                    ProceduralPropertyType propType = animatedMaterialVariables[j].type;
                    if (propType == ProceduralPropertyType.Float)
                        substance.SetProceduralFloat(animatedMaterialVariable.name, Mathf.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].PropertyFloatDictionary[animatedMaterialVariable.name], MaterialVariableKeyframeDictionaryList[i].PropertyFloatDictionary[animatedMaterialVariable.name], newLerp * flickerFloatCalc));
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
                    emissionInput = Color.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].emissionColor, MaterialVariableKeyframeDictionaryList[i].emissionColor, newLerp);
                    rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
                    prefabScript.emissionInput = emissionInput;
                }
                if (rend.sharedMaterial.HasProperty("_MainTex"))
                    rend.sharedMaterial.SetTextureOffset("_MainTex", Vector2.Lerp(MaterialVariableKeyframeDictionaryList[i - 1].MainTex, MaterialVariableKeyframeDictionaryList[i].MainTex, newLerp));
                substance.RebuildTextures();
                return;
            }
        }
    }



    public static void SetProceduralVariablesFromList(MaterialVariableListHolder propertyList, ProceduralMaterial substance, ProceduralPropertyDescription[] materialVariables, bool saveOutputParameters, bool resettingValuesToDefault) // Sets current substance parameters from a List
    {
        for (int i = 0; i < materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = materialVariables[i];
            ProceduralPropertyType propType = materialVariables[i].type;
            if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (saveOutputParameters && !materialVariable.hasRange) || resettingValuesToDefault))
            {
                if (propertyList.myFloatKeys.Count > 0)
                {
                    for (int j = 0; j < propertyList.myFloatKeys.Count; j++)
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
                    for (int j = 0; j < propertyList.myKeys.Count; j++)
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
                if (propertyList.myColorKeys.Count > 0)
                {
                    for (int j = 0; j < propertyList.myColorKeys.Count; j++)
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
                    for (int j = 0; j < propertyList.myKeys.Count; j++)
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
                    if (propertyList.myVector4Keys.Count > 0)
                    {
                        for (int j = 0; j < propertyList.myVector4Keys.Count; j++)
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
                        for (int j = 0; j < propertyList.myKeys.Count; j++)
                        {
                            if (propertyList.myKeys[j] == materialVariable.name)
                            {
                                Vector4 curVector4 = SubstanceTweenMiscUtility.StringToVector(propertyList.myValues[j], 4);
                                substance.SetProceduralVector(materialVariable.name, curVector4);
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
                                substance.SetProceduralVector(materialVariable.name, curVector3);
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
                                substance.SetProceduralVector(materialVariable.name, curVector3);
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
                                substance.SetProceduralVector(materialVariable.name, curVector2);
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
                                substance.SetProceduralVector(materialVariable.name, curVector2);
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
                            substance.SetProceduralEnum(materialVariable.name, curEnum);
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
                            substance.SetProceduralEnum(materialVariable.name, curEnum);
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
                            substance.SetProceduralBoolean(materialVariable.name, curBool);
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
                            substance.SetProceduralBoolean(materialVariable.name, curBool);
                        }
                    }
                }
            }
        }
    }
    public static void SetAllProceduralValuesToMin(ProceduralMaterial substance, ProceduralPropertyDescription[] materialVariables) // Sets all procedural values to the minimum value
    {
        UnityEditor.Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { substance }, "Set all values to Minimum");
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
       // DebugStrings.Add("Set all properties to the minimum");
        substance.RebuildTexturesImmediately();
    }

    public static void SetAllProceduralValuesToMax(ProceduralMaterial substance, ProceduralPropertyDescription[] materialVariables) // Sets all procedural values to the maximum value
    {
       UnityEditor.Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { substance }, "Set all values to Maximum");
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
        //DebugStrings.Add("Set all properties to the maximum");
        substance.RebuildTexturesImmediately();
    }
    
}
