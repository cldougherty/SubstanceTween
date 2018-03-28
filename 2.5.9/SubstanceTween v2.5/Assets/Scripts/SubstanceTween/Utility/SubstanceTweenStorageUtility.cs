using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SubstanceTweenStorageUtility {
    /// <summary>
    /// Functions for storing/converting Procedural variables
    /// </summary>
    public static void AddProceduralVariablesToList(MaterialVariableListHolder materialVariableList, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams ) // Adds current procedural values to a list 
    {
        for (int i = 0; i <substanceMaterialParams.materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = substanceMaterialParams.materialVariables[i];
            ProceduralPropertyType propType = substanceMaterialParams.materialVariables[i].type;
            if (propType != ProceduralPropertyType.Texture) // Texture Type not supported
                materialVariableList.PropertyName.Add(materialVariable.name);
            if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (substanceMaterialParams.saveOutputParameters && !materialVariable.hasRange)))
            {
                float propFloat = substanceMaterialParams.substance.GetProceduralFloat(materialVariable.name);
                materialVariableList.PropertyFloat.Add(propFloat);
                materialVariableList.myFloatKeys.Add(materialVariable.name);
                materialVariableList.myFloatValues.Add(propFloat);
            }
            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                Color propColor = substanceMaterialParams.substance.GetProceduralColor(materialVariable.name);
                materialVariableList.PropertyColor.Add(propColor);
                materialVariableList.myColorKeys.Add(materialVariable.name);
                materialVariableList.myColorValues.Add(propColor);
            }
            else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (substanceMaterialParams.saveOutputParameters && !materialVariable.hasRange)))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    Vector4 propVector4 = substanceMaterialParams.substance.GetProceduralVector(materialVariable.name);
                    materialVariableList.PropertyVector4.Add(propVector4);
                    materialVariableList.myVector4Keys.Add(materialVariable.name);
                    materialVariableList.myVector4Values.Add(propVector4);
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    Vector3 propVector3 = substanceMaterialParams.substance.GetProceduralVector(materialVariable.name);
                    materialVariableList.PropertyVector3.Add(propVector3);
                    materialVariableList.myVector3Keys.Add(materialVariable.name);
                    materialVariableList.myVector3Values.Add(propVector3);
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    Vector2 propVector2 = substanceMaterialParams.substance.GetProceduralVector(materialVariable.name);
                    materialVariableList.PropertyVector2.Add(propVector2);
                    materialVariableList.myVector2Keys.Add(materialVariable.name);
                    materialVariableList.myVector2Values.Add(propVector2);
                }
            }
            else if (propType == ProceduralPropertyType.Enum)
            {
                int propEnum = substanceMaterialParams.substance.GetProceduralEnum(materialVariable.name);
                materialVariableList.PropertyEnum.Add(propEnum);
                materialVariableList.myEnumKeys.Add(materialVariable.name);
                materialVariableList.myEnumValues.Add(propEnum);
            }
            else if (propType == ProceduralPropertyType.Boolean)
            {
                bool propBool = substanceMaterialParams.substance.GetProceduralBoolean(materialVariable.name);
                materialVariableList.PropertyBool.Add(propBool);
                materialVariableList.myBooleanKeys.Add(materialVariable.name);
                materialVariableList.myBooleanValues.Add(propBool);
            }
        }
        materialVariableList.PropertyMaterialName = substanceMaterialParams.substance.name;
        materialVariableList.emissionColor = substanceMaterialParams.emissionInput;
        materialVariableList.MainTex = substanceMaterialParams.MainTexOffset;
        if (substanceMaterialParams.saveOutputParameters)
            materialVariableList.hasParametersWithoutRange = true;
        else
            materialVariableList.hasParametersWithoutRange = false;
        materialVariableList.animationTime = animationParams.defaultAnimationTime;
    }

    public static void AddProceduralVariablesToDictionary(MaterialVariableDictionaryHolder materialVariableDictionary, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams) // Adds current procedural values to a dictionary 
    {
        for (int i = 0; i < substanceMaterialParams.materialVariables.Length; i++)
        {
            ProceduralPropertyDescription materialVariable = substanceMaterialParams.materialVariables[i];
            ProceduralPropertyType propType = substanceMaterialParams.materialVariables[i].type;
            if (propType != ProceduralPropertyType.Texture)
                materialVariableDictionary.PropertyName.Add(materialVariable.name);
            if (propType == ProceduralPropertyType.Float && (materialVariable.hasRange || (substanceMaterialParams.saveOutputParameters && !materialVariable.hasRange)))
            {
                float propFloat = substanceMaterialParams.substance.GetProceduralFloat(materialVariable.name);
                materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propFloat);
                materialVariableDictionary.PropertyFloatDictionary.Add(materialVariable.name, propFloat);
            }
            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                Color propColor = substanceMaterialParams.substance.GetProceduralColor(materialVariable.name);
                materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propColor);
                materialVariableDictionary.PropertyColorDictionary.Add(materialVariable.name, propColor);
            }
            else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariable.hasRange || (substanceMaterialParams.saveOutputParameters && !materialVariable.hasRange)))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    Vector4 propVector4 = substanceMaterialParams.substance.GetProceduralVector(materialVariable.name);
                    materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propVector4);
                    materialVariableDictionary.PropertyVector4Dictionary.Add(materialVariable.name, propVector4);
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    Vector3 propVector3 = substanceMaterialParams.substance.GetProceduralVector(materialVariable.name);
                    materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propVector3);
                    materialVariableDictionary.PropertyVector3Dictionary.Add(materialVariable.name, propVector3);
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    Vector2 propVector2 = substanceMaterialParams.substance.GetProceduralVector(materialVariable.name);
                    materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propVector2);
                    materialVariableDictionary.PropertyVector2Dictionary.Add(materialVariable.name, propVector2);
                }
            }
            else if (propType == ProceduralPropertyType.Enum)
            {
                int propEnum = substanceMaterialParams.substance.GetProceduralEnum(materialVariable.name);
                materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propEnum);
                materialVariableDictionary.PropertyEnumDictionary.Add(materialVariable.name, propEnum);
            }
            else if (propType == ProceduralPropertyType.Boolean)
            {
                bool propBool = substanceMaterialParams.substance.GetProceduralBoolean(materialVariable.name);
                materialVariableDictionary.PropertyDictionary.Add(materialVariable.name, propBool);
                materialVariableDictionary.PropertyBoolDictionary.Add(materialVariable.name, propBool);
            }
        }
        materialVariableDictionary.PropertyMaterialName = substanceMaterialParams.substance.name;
        materialVariableDictionary.emissionColor = substanceMaterialParams.emissionInput;
        materialVariableDictionary.MainTex = substanceMaterialParams.MainTexOffset;
        if (substanceMaterialParams.saveOutputParameters)
            materialVariableDictionary.hasParametersWithoutRange = true;
        else
            materialVariableDictionary.hasParametersWithoutRange = false;
        materialVariableDictionary.animationTime = animationParams.defaultAnimationTime;
    }

    public static void AddProceduralVariablesToDictionaryFromList(MaterialVariableDictionaryHolder dictionary, MaterialVariableListHolder list, ProceduralPropertyDescription[] materialVariables, bool saveOutputParameters) // sorts items from a list into a dictionary
    {
        if (materialVariables != null)
        {
            for (int i = 0; i < materialVariables.Length; i++)
            {
                ProceduralPropertyDescription materialVariable = materialVariables[i];
                ProceduralPropertyType propType = materialVariables[i].type;
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

    public static void AddDefaultMaterial(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams, SubstanceDefaultMaterialParams substanceDefaultMaterialParams)
    {
        substanceDefaultMaterialParams.defaultSubstanceObjProperties.Add(new MaterialVariableListHolder());
        substanceDefaultMaterialParams.defaultSubstance = substanceMaterialParams.rend.sharedMaterial as ProceduralMaterial;
        substanceMaterialParams.materialVariables = substanceMaterialParams.substance.GetProceduralPropertyDescriptions();
        substanceDefaultMaterialParams.defaultSubstanceObjProperties[animationParams.defaultSubstanceIndex].PropertyMaterialName = substanceDefaultMaterialParams.defaultSubstance.name;
        SubstanceTweenStorageUtility.AddProceduralVariablesToList(substanceDefaultMaterialParams.defaultSubstanceObjProperties[animationParams.defaultSubstanceIndex], substanceMaterialParams, animationParams, substanceToolParams);
        substanceDefaultMaterialParams.defaultSubstanceObjProperties[animationParams.defaultSubstanceIndex].MainTex = substanceMaterialParams.MainTexOffset;
        substanceDefaultMaterialParams.defaultSubstanceObjProperties[animationParams.defaultSubstanceIndex].emissionColor = substanceMaterialParams.emissionInput;
        animationParams.defaultSubstanceIndex++;
    }
}
