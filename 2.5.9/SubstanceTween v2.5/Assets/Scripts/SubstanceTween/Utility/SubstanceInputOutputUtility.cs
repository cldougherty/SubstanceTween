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


public static class SubstanceInputOutputUtility
{
    public static void WriteXML(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams) // Write current material variables to a XML file.
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
            MaterialVariableListHolder xmlDescription = new MaterialVariableListHolder();//Creates empty list to be saved to a XMl file      
            XmlSerializer serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
            SubstanceTweenStorageUtility.AddProceduralVariablesToList(xmlDescription, substanceMaterialParams, animationParams, substanceToolParams);// Writes current variables to a list to be saved to a XML file
            xmlDescription.PropertyMaterialName = substanceMaterialParams.substance.name;
            xmlDescription.emissionColor = substanceMaterialParams.emissionInput;
            xmlDescription.MainTex = substanceMaterialParams.MainTexOffset;
            serializer.Serialize(writer, xmlDescription); //Writes the xml file using the fileInfo and the list we want to write.
            writer.Close();//Closes the XML writer
            substanceToolParams.DebugStrings.Add("-----------------------------------");
            substanceToolParams.DebugStrings.Add("Wrote XML file to: " + fInfo + ", File has: ");
            substanceToolParams.DebugStrings.Add(xmlDescription.PropertyName.Count + " Total Properties ");
            substanceToolParams.DebugStrings.Add(xmlDescription.PropertyFloat.Count + " Float Properties");
            substanceToolParams.DebugStrings.Add(xmlDescription.PropertyColor.Count + " Color Properties");
            substanceToolParams.DebugStrings.Add(xmlDescription.PropertyVector4.Count + " Vector4 Properties");
            substanceToolParams.DebugStrings.Add(xmlDescription.PropertyVector3.Count + " Vector3 Properties");
            substanceToolParams.DebugStrings.Add(xmlDescription.PropertyVector2.Count + " Vector2 Properties");
            substanceToolParams.DebugStrings.Add(xmlDescription.PropertyEnum.Count + " Enum Properties");
            substanceToolParams.DebugStrings.Add(xmlDescription.PropertyBool.Count + " Boolean Properties");
            substanceToolParams.DebugStrings.Add(xmlDescription.myKeys.Count + " Keys");
            substanceToolParams.DebugStrings.Add(xmlDescription.myValues.Count + " Values");
            substanceToolParams.DebugStrings.Add("Material Name: " + xmlDescription.PropertyMaterialName);
            //DebugStrings.Add("Substance Texture Size: " + substanceWidth + " " + substanceHeight);
            substanceToolParams.DebugStrings.Add("-----------------------------------");
        }
        substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    public static void ReadXML(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams) // Sets current material variables from a XML file without creating a keyframe.
    {
        var serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
        string path = EditorUtility.OpenFilePanel("", "", "xml"); // 'Open' Dialog that only accepts XML files
        if (path.Length != 0)
        {
            var stream = new FileStream(path, FileMode.Open);
            if (stream.Length != 0)
            {
                var container = serializer.Deserialize(stream) as MaterialVariableListHolder; //Convert XML to a list
                SubstanceTweenSetParameterUtility.SetProceduralVariablesFromList(container, substanceMaterialParams, animationParams, substanceToolParams);
                substanceMaterialParams.MainTexOffset = container.MainTex;
                Color xmlEmissionColor = new Color(0, 0, 0, 0);
                xmlEmissionColor = container.emissionColor;
                if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    substanceMaterialParams.emissionInput = xmlEmissionColor;
                    substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", xmlEmissionColor);
                }
                stream.Close();
                substanceToolParams.DebugStrings.Add("-----------------------------------");
                substanceToolParams.DebugStrings.Add("Read XML file " + " from: " + stream.Name + ", File has: ");
                if (container.PropertyMaterialName != null)
                    substanceToolParams.DebugStrings.Add("Material Name: " + container.PropertyMaterialName);
                substanceToolParams.DebugStrings.Add(container.PropertyName.Count + " Total Properties");
                substanceToolParams.DebugStrings.Add(container.PropertyFloat.Count + " Float Properties");
                substanceToolParams.DebugStrings.Add(container.PropertyColor.Count + " Color Properties ");
                substanceToolParams.DebugStrings.Add(container.PropertyVector4.Count + " Vector4 Properties");
                substanceToolParams.DebugStrings.Add(container.PropertyVector3.Count + " Vector3 Properties");
                substanceToolParams.DebugStrings.Add(container.PropertyVector2.Count + " Vector2 Properties");
                substanceToolParams.DebugStrings.Add(container.PropertyEnum.Count + " Enum Properties");
                substanceToolParams.DebugStrings.Add(container.PropertyBool.Count + " Boolean Properties");
                substanceToolParams.DebugStrings.Add(container.myKeys.Count + " Keys");
                substanceToolParams.DebugStrings.Add(container.myValues.Count + " Values");
                substanceToolParams.DebugStrings.Add("_EmissionColor = " + container.emissionColor);
                substanceToolParams.DebugStrings.Add("_MainTex = " + substanceMaterialParams.MainTexOffset);
                substanceToolParams.DebugStrings.Add("-----------------------------------");
                substanceMaterialParams.substance.RebuildTexturesImmediately();
            }
        }
        substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
        SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
    }

    public static void WriteAllXML(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams) // Writes each keyframe to a XML file
    {
        int fileNumber = 1;
        StreamWriter writer;
        string path = EditorUtility.SaveFilePanel("Save XML Files", "", "", "xml");
        FileInfo fInfo;
        if (path.Length != 0)
        {
            for (int i = 0; i <= substanceMaterialParams.MaterialVariableKeyframeList.Count - 1; i++) // Go through each keyframe
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
                MaterialVariableListHolder xmlDescription = substanceMaterialParams.MaterialVariableKeyframeList[i];
                XmlSerializer serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
                xmlDescription.PropertyMaterialName = substanceMaterialParams.substance.name;
                if (animationParams.keyFrameTimes.Count() >= 1)
                {
                    if (xmlDescription.AnimationCurveKeyframeList.Count <= 0)
                    {
                        if (fileNumber == 1) // if fileNumber is one make sure that there are no NAN tangents saved
                            xmlDescription.AnimationCurveKeyframeList.Add(new Keyframe(animationParams.substanceCurve.keys[i].time, animationParams.substanceCurve.keys[i].value, 0, animationParams.substanceCurve.keys[i].outTangent));
                        else
                            xmlDescription.AnimationCurveKeyframeList.Add(animationParams.substanceCurve.keys[i]);
                    }
                    else
                    {
                        if (fileNumber == 1) // if fileNumber is one make sure that there are no NAN tangents saved
                            xmlDescription.AnimationCurveKeyframeList[0] = (new Keyframe(animationParams.substanceCurve.keys[i].time, animationParams.substanceCurve.keys[i].value, 0, animationParams.substanceCurve.keys[i].outTangent));
                        else
                            xmlDescription.AnimationCurveKeyframeList[0] = (animationParams.substanceCurve.keys[i]);
                    }
                }
                serializer.Serialize(writer, xmlDescription); // write keyframe to xml
                writer.Close();
                substanceToolParams.DebugStrings.Add("Wrote  XML file" + fileNumber + " to: " + fInfo);
                fileNumber++;
            }
        }
        substanceToolParams.DebugStrings.Add(fileNumber + " keyframes saved as XML files"); substanceToolParams.DebugStrings.Add("-----------------------------------");
        substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    public static void ReadAllXML(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams) // Read XML files and create keyframes from them.
    {
        var serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
        string xmlReadFolderPath = EditorUtility.OpenFolderPanel("Load xml files from folder", "", ""); //Creates 'Open Folder' Dialog
        if (xmlReadFolderPath.Length != 0)
        {
            string[] xmlReadFiles = Directory.GetFiles(xmlReadFolderPath);//array of selected xml file paths
            if (xmlReadFiles.Count() > 0)
            {
                animationParams.keyFrames = animationParams.keyFrameTimes.Count();
                foreach (string xmlReadFile in xmlReadFiles) //for each xml file path in the list.
                {
                    if (xmlReadFile.EndsWith(".xml"))
                    {
                        var stream = new FileStream(xmlReadFile, FileMode.Open);// defines how to use the file at the selected path
                        var container = serializer.Deserialize(stream) as MaterialVariableListHolder; //Puts current xml file into a list.
                                                                                                      //SubstanceTweenSetParameterUtility.SetProceduralVariablesFromList(container);//Sets current substance values from list
                        SubstanceTweenSetParameterUtility.SetProceduralVariablesFromList(container, substanceMaterialParams, animationParams, substanceToolParams);
                        substanceMaterialParams.MainTexOffset = container.MainTex;
                        Color xmlEmissionColor = new Color(0, 0, 0, 0);
                        xmlEmissionColor = container.emissionColor;
                        if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor"))
                        {
                            substanceMaterialParams.emissionInput = xmlEmissionColor;
                            if (substanceToolParams.selectedPrefabScript)
                                substanceToolParams.selectedPrefabScript.emissionInput = substanceMaterialParams.emissionInput;
                            substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", xmlEmissionColor);
                        }
                        stream.Close();//Close Xml reader
                        substanceMaterialParams.substance.RebuildTexturesImmediately();
                        if (animationParams.keyFrameTimes.Count == 0) // Create keyframe from list containing XML variables
                        {
                            if (animationParams.substanceCurve.keys.Count() > 0)
                            {
                                animationParams.substanceCurve.RemoveKey(0);
                                animationParams.substanceCurve.AddKey(0, 0);
                            }
                            substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                            substanceMaterialParams.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                            substanceMaterialParams.MaterialVariableKeyframeList[0] = container;
                            SubstanceTweenStorageUtility.AddProceduralVariablesToDictionary(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[0], substanceMaterialParams, animationParams, substanceToolParams);
                            animationParams.keyFrames++;
                            animationParams.keyFrameTimes.Add(substanceMaterialParams.MaterialVariableKeyframeList[0].animationTime);
                            AnimationUtility.SetKeyBroken(animationParams.substanceCurve, 0, true);
                            AnimationUtility.SetKeyLeftTangentMode(animationParams.substanceCurve, 0, AnimationUtility.TangentMode.Linear);
                            AnimationUtility.SetKeyRightTangentMode(animationParams.substanceCurve, 0, AnimationUtility.TangentMode.Linear);
                        }
                        else if (animationParams.keyFrameTimes.Count > 0)
                        {
                            substanceMaterialParams.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                            substanceMaterialParams.MaterialVariableKeyframeList[animationParams.keyFrames] = container;
                            substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                            substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.keyFrames] = new MaterialVariableDictionaryHolder();
                            SubstanceTweenStorageUtility.AddProceduralVariablesToDictionary(substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.keyFrames], substanceMaterialParams, animationParams, substanceToolParams);
                            animationParams.keyFrameTimes.Add(container.animationTime);
                            animationParams.keyframeSum = 0;
                            for (int i = 0; i < substanceMaterialParams.MaterialVariableKeyframeList.Count() - 1; i++)
                                animationParams.keyframeSum += substanceMaterialParams.MaterialVariableKeyframeList[i].animationTime;
                            if (container.AnimationCurveKeyframeList.Count() >= 1)
                                animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum, container.AnimationCurveKeyframeList[0].inTangent, container.AnimationCurveKeyframeList[0].outTangent));
                            else
                                animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum));
                            if (animationParams.keyFrames >= 1)
                                AnimationUtility.SetKeyBroken(animationParams.substanceCurve, animationParams.keyFrames, true);
                            animationParams.keyFrames++;
                        }
                    }
                    substanceToolParams.DebugStrings.Add("Read keyframe from: " + xmlReadFile);
                }
                substanceToolParams.DebugStrings.Add(animationParams.keyFrames - 1 + " Keyframes created from XML files ");
                animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
                substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
            }
            else
                EditorUtility.DisplayDialog("Empty Folder", "No Files were found in the selected folder", "Ok");
            substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
            SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
            SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
        }
    }

    public static void WriteJSON(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams) //  Write current material variables to a JSON file
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
            MaterialVariableListHolder jsonDescription = new MaterialVariableListHolder();//Creates empty list to be saved to a JSON file  
            SubstanceTweenStorageUtility.AddProceduralVariablesToList(jsonDescription, substanceMaterialParams, animationParams, substanceToolParams);
            jsonDescription.PropertyMaterialName = substanceMaterialParams.substance.name;
            jsonDescription.emissionColor = substanceMaterialParams.emissionInput;
            jsonDescription.MainTex = substanceMaterialParams.MainTexOffset;
            string json = JsonUtility.ToJson(jsonDescription);
            writer.Write(json);
            writer.Close();//Closes the Json writer
            substanceToolParams.DebugStrings.Add("-----------------------------------");
            substanceToolParams.DebugStrings.Add("Wrote JSON file to: " + fInfo + ", File has: ");
            substanceToolParams.DebugStrings.Add(jsonDescription.PropertyName.Count + " Total Properties ");
            substanceToolParams.DebugStrings.Add(jsonDescription.PropertyFloat.Count + " Float Properties");
            substanceToolParams.DebugStrings.Add(jsonDescription.PropertyColor.Count + " Color Properties");
            substanceToolParams.DebugStrings.Add(jsonDescription.PropertyVector4.Count + " Vector4 Properties");
            substanceToolParams.DebugStrings.Add(jsonDescription.PropertyVector3.Count + " Vector3 Properties");
            substanceToolParams.DebugStrings.Add(jsonDescription.PropertyVector2.Count + " Vector2 Properties");
            substanceToolParams.DebugStrings.Add(jsonDescription.PropertyEnum.Count + " Enum Properties");
            substanceToolParams.DebugStrings.Add(jsonDescription.PropertyBool.Count + " Boolean Properties");
            substanceToolParams.DebugStrings.Add(jsonDescription.myKeys.Count + " Keys");
            substanceToolParams.DebugStrings.Add(jsonDescription.myValues.Count + " Values");
            substanceToolParams.DebugStrings.Add("Material Name: " + jsonDescription.PropertyMaterialName);
            substanceToolParams.DebugStrings.Add("Substance Texture Size: " + substanceMaterialParams.substanceWidth + " " + substanceMaterialParams.substanceHeight);
            substanceToolParams.DebugStrings.Add("-----------------------------------");
        }
        substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }

    public static void ReadJSON(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams)
    {
        string path = EditorUtility.OpenFilePanel("", "", "json"); // 'Open' Dialog that only accepts JSON files
        if (path.Length != 0)
        {
            string dataAsJson = File.ReadAllText(path);
            var stream = new FileStream(path, FileMode.Open);
            if (stream.Length != 0)
            {
                MaterialVariableListHolder jsonContainer = JsonUtility.FromJson<MaterialVariableListHolder>(dataAsJson);
                SubstanceTweenSetParameterUtility.SetProceduralVariablesFromList(jsonContainer, substanceMaterialParams, animationParams, substanceToolParams);
                substanceMaterialParams.MainTexOffset = jsonContainer.MainTex;
                Color jsonEmissionColor = new Color(0, 0, 0, 0);
                jsonEmissionColor = jsonContainer.emissionColor;
                if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor"))
                {
                    substanceMaterialParams.emissionInput = jsonEmissionColor;
                    substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", jsonEmissionColor);
                    substanceToolParams.selectedPrefabScript.emissionInput = substanceMaterialParams.emissionInput;
                }
                stream.Close();
                substanceToolParams.DebugStrings.Add("-----------------------------------");
                substanceToolParams.DebugStrings.Add("Read XML file " + " from: " + stream.Name + ", File has: ");
                if (jsonContainer.PropertyMaterialName != null)
                    substanceToolParams.DebugStrings.Add("Material Name: " + jsonContainer.PropertyMaterialName);
                substanceToolParams.DebugStrings.Add(jsonContainer.PropertyName.Count + " Total Properties");
                substanceToolParams.DebugStrings.Add(jsonContainer.PropertyFloat.Count + " Float Properties");
                substanceToolParams.DebugStrings.Add(jsonContainer.PropertyColor.Count + " Color Properties ");
                substanceToolParams.DebugStrings.Add(jsonContainer.PropertyVector4.Count + " Vector4 Properties");
                substanceToolParams.DebugStrings.Add(jsonContainer.PropertyVector3.Count + " Vector3 Properties");
                substanceToolParams.DebugStrings.Add(jsonContainer.PropertyVector2.Count + " Vector2 Properties");
                substanceToolParams.DebugStrings.Add(jsonContainer.PropertyEnum.Count + " Enum Properties");
                substanceToolParams.DebugStrings.Add(jsonContainer.PropertyBool.Count + " Boolean Properties");
                substanceToolParams.DebugStrings.Add(jsonContainer.myKeys.Count + " Keys");
                substanceToolParams.DebugStrings.Add(jsonContainer.myValues.Count + " Values");
                substanceToolParams.DebugStrings.Add("_EmissionColor = " + jsonContainer.emissionColor);
                substanceToolParams.DebugStrings.Add("_MainTex = " + substanceMaterialParams.MainTexOffset);
                substanceToolParams.DebugStrings.Add("-----------------------------------");
                substanceMaterialParams.substance.RebuildTexturesImmediately();
            }
        }
        substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
        SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
    } //  Sets current material variables from a JSON file without creating a keyframe

    public static void WriteAllJSON(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams)
    {
        int fileNumber = 1;
        StreamWriter writer;
        string path = EditorUtility.SaveFilePanel("Save JSON Files", "", "", "json");
        FileInfo fInfo;
        if (path.Length != 0)
        {
            for (int i = 0; i <= substanceMaterialParams.MaterialVariableKeyframeList.Count - 1; i++) // Go through each keyframe
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
                MaterialVariableListHolder jsonDescription = substanceMaterialParams.MaterialVariableKeyframeList[i];
                jsonDescription.PropertyMaterialName = substanceMaterialParams.substance.name;
                string json = JsonUtility.ToJson(jsonDescription);
                if (animationParams.keyFrameTimes.Count() >= 1)
                {
                    if (jsonDescription.AnimationCurveKeyframeList.Count <= 0)
                    {
                        if (fileNumber == 1) // if fileNumber is one make sure that there are no NAN tangents saved
                            jsonDescription.AnimationCurveKeyframeList.Add(new Keyframe(animationParams.substanceCurve.keys[i].time, animationParams.substanceCurve.keys[i].value, 0, animationParams.substanceCurve.keys[i].outTangent));
                        else
                            jsonDescription.AnimationCurveKeyframeList.Add(animationParams.substanceCurve.keys[i]);
                    }
                    else
                    {
                        if (fileNumber == 1) // if fileNumber is one make sure that there are no NAN tangents saved
                            jsonDescription.AnimationCurveKeyframeList[0] = (new Keyframe(animationParams.substanceCurve.keys[i].time, animationParams.substanceCurve.keys[i].value, 0, animationParams.substanceCurve.keys[i].outTangent));
                        else
                            jsonDescription.AnimationCurveKeyframeList[0] = (animationParams.substanceCurve.keys[i]);
                    }
                }
                writer.Write(json);
                writer.Close();
                substanceToolParams.DebugStrings.Add("Wrote  JSON file" + fileNumber + " to: " + fInfo);
                fileNumber++;
            }
        }
        substanceToolParams.DebugStrings.Add(fileNumber + " keyframes saved as XML files"); substanceToolParams.DebugStrings.Add("-----------------------------------");
        substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    } //Writes all keyframes to JSON files

    public static void ReadAllJSON(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams) // Read JSON files and create keyframes from them.
    {
        string JsonReadFolderPath = EditorUtility.OpenFolderPanel("Load JSON files from folder", "", ""); //Creates 'Open Folder' Dialog
        if (JsonReadFolderPath.Length != 0)
        {
            string[] JsonReadFiles = Directory.GetFiles(JsonReadFolderPath);//array of selected xml file paths
            if (JsonReadFiles.Count() > 0)
            {
                animationParams.keyFrames = animationParams.keyFrameTimes.Count();
                foreach (string jsonReadFile in JsonReadFiles) //for each xml file path in the list.
                {
                    if (jsonReadFile.EndsWith(".json"))
                    {
                        string dataAsJson = File.ReadAllText(jsonReadFile);
                        var stream = new FileStream(jsonReadFile, FileMode.Open);// defines how to use the file at the selected path
                        MaterialVariableListHolder jsonContainer = JsonUtility.FromJson<MaterialVariableListHolder>(dataAsJson);
                        SubstanceTweenSetParameterUtility.SetProceduralVariablesFromList(jsonContainer, substanceMaterialParams, animationParams, substanceToolParams);//Sets current substance values from list
                        substanceMaterialParams.MainTexOffset = jsonContainer.MainTex;
                        Color jsonEmissionColor = new Color(0, 0, 0, 0);
                        jsonEmissionColor = jsonContainer.emissionColor;
                        if (substanceMaterialParams.rend.sharedMaterial.HasProperty("_EmissionColor"))
                        {
                            substanceMaterialParams.emissionInput = jsonEmissionColor;
                            if (substanceToolParams.selectedPrefabScript)
                                substanceToolParams.selectedPrefabScript.emissionInput = substanceMaterialParams.emissionInput;
                            substanceMaterialParams.rend.sharedMaterial.SetColor("_EmissionColor", jsonEmissionColor);
                        }
                        stream.Close();//Close Xml reader
                        substanceMaterialParams.substance.RebuildTexturesImmediately();
                        if (animationParams.keyFrameTimes.Count == 0) // Create keyframe from list containing XML variables
                        {
                            if (animationParams.substanceCurve.keys.Count() > 0)
                            {
                                animationParams.substanceCurve.RemoveKey(0);
                                animationParams.substanceCurve.AddKey(0, 0);
                            }
                            substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                            substanceMaterialParams.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                            substanceMaterialParams.MaterialVariableKeyframeList[0] = jsonContainer;
                            animationParams.keyFrames++;
                            animationParams.keyFrameTimes.Add(substanceMaterialParams.MaterialVariableKeyframeList[0].animationTime);
                            AnimationUtility.SetKeyBroken(animationParams.substanceCurve, 0, true);
                            AnimationUtility.SetKeyLeftTangentMode(animationParams.substanceCurve, 0, AnimationUtility.TangentMode.Linear);
                            AnimationUtility.SetKeyRightTangentMode(animationParams.substanceCurve, 0, AnimationUtility.TangentMode.Linear);
                        }
                        else if (animationParams.keyFrameTimes.Count > 0)
                        {
                            substanceMaterialParams.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                            substanceMaterialParams.MaterialVariableKeyframeList[animationParams.keyFrames] = jsonContainer;
                            substanceMaterialParams.MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                            substanceMaterialParams.MaterialVariableKeyframeDictionaryList[animationParams.keyFrames] = new MaterialVariableDictionaryHolder();
                            animationParams.keyFrameTimes.Add(jsonContainer.animationTime);
                            animationParams.keyframeSum = 0;
                            for (int i = 0; i < substanceMaterialParams.MaterialVariableKeyframeList.Count() - 1; i++)  //  -1 to count here 6/10/17
                                animationParams.keyframeSum += substanceMaterialParams.MaterialVariableKeyframeList[i].animationTime;
                            if (jsonContainer.AnimationCurveKeyframeList.Count() >= 1)
                                animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum, jsonContainer.AnimationCurveKeyframeList[0].inTangent, jsonContainer.AnimationCurveKeyframeList[0].outTangent));
                            else
                                animationParams.substanceCurve.AddKey(new Keyframe(animationParams.keyframeSum, animationParams.keyframeSum));
                            if (animationParams.keyFrames >= 1)
                                AnimationUtility.SetKeyBroken(animationParams.substanceCurve, animationParams.keyFrames, true);
                            animationParams.keyFrames++;
                        }
                    }
                    substanceToolParams.DebugStrings.Add("Read keyframe from: " + jsonReadFile);
                }
                substanceToolParams.DebugStrings.Add(animationParams.keyFrames - 1 + " Keyframes created from XML files ");
                animationParams.substanceCurveBackup.keys = animationParams.substanceCurve.keys;
                substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
            }
            else
                EditorUtility.DisplayDialog("Empty Folder", "No Files were found in the selected folder", "Ok");
            SubstanceTweenAnimationUtility.CacheAnimatedProceduralVariables(substanceMaterialParams, animationParams);
            SubstanceTweenAnimationUtility.CalculateAnimationLength(substanceMaterialParams, animationParams);
        }
    }


    public static void SavePrefab(SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams, SubstancePerformanceParams substancePerformanceParams, SubstanceFlickerParams flickerValues) // Saves a Prefab to a specified folder
    {
        string prefabPath = EditorUtility.SaveFilePanelInProject("", "", "prefab", "");
        if (prefabPath.Length != 0)
        {
            GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, substanceToolParams.currentSelection.gameObject, ReplacePrefabOptions.Default);
            if (prefab.GetComponent<PrefabProperties>() != null)
                UnityEngine.Object.DestroyImmediate(prefab.GetComponent<PrefabProperties>(), true);
            prefab.AddComponent<PrefabProperties>(); // adds prefab script to prefab 
            PrefabProperties prefabProperties = prefab.GetComponent<PrefabProperties>();
            prefabProperties.substance = substanceMaterialParams.substance;
            Renderer prefabRend = prefab.GetComponent<Renderer>();
            prefabRend.material = substanceMaterialParams.substance;
            if (animationParams.animationType == SubstanceAnimationParams.AnimationType.Loop)
                prefabProperties.animationType = PrefabProperties.AnimationType.Loop;
            if (animationParams.animationType == SubstanceAnimationParams.AnimationType.BackAndForth)
                prefabProperties.animationType = PrefabProperties.AnimationType.BackAndForth;
            substanceToolParams.DebugStrings.Add("Created prefab: " + prefab.name);
            if (prefabRend.sharedMaterial)
                substanceToolParams.DebugStrings.Add("Prefab material: " + prefabRend.sharedMaterial.name);
            if (substanceMaterialParams.MaterialVariableKeyframeList.Count >= 1)
            {//writes keyfame lists to prefab(Could not serialize dictionaries)
                for (int i = 0; i <= substanceMaterialParams.MaterialVariableKeyframeList.Count - 1; i++)
                {
                    if (prefabProperties.keyFrameTimes.Count == 0)
                    {
                        prefabProperties.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                        prefabProperties.MaterialVariableKeyframeList[0] = substanceMaterialParams.MaterialVariableKeyframeList[0];
                        prefabProperties.MaterialVariableKeyframeList[0].PropertyMaterialName = substanceMaterialParams.substance.name;
                        prefabProperties.MaterialVariableKeyframeList[0].emissionColor = substanceMaterialParams.MaterialVariableKeyframeList[0].emissionColor;
                        prefabProperties.MaterialVariableKeyframeList[0].MainTex = substanceMaterialParams.MaterialVariableKeyframeList[prefabProperties.keyFrames].MainTex;
                        prefabProperties.keyFrameTimes.Add(substanceMaterialParams.MaterialVariableKeyframeList[0].animationTime);
                        prefabProperties.prefabAnimationCurve = new AnimationCurve();
                        prefabProperties.prefabAnimationCurve.AddKey(new Keyframe(animationParams.substanceCurve.keys[0].time, animationParams.substanceCurve.keys[0].value, animationParams.substanceCurve.keys[0].inTangent, animationParams.substanceCurve.keys[0].outTangent));
                        AnimationUtility.SetKeyBroken(prefabProperties.prefabAnimationCurve, 0, true);
                        if (substanceMaterialParams.MaterialVariableKeyframeList[0].hasParametersWithoutRange)
                            prefabProperties.MaterialVariableKeyframeList[0].hasParametersWithoutRange = true;
                        else
                            prefabProperties.MaterialVariableKeyframeList[0].hasParametersWithoutRange = false;
                        prefabProperties.keyFrames++;
                    }
                    else // After one keyframe is created create the rest
                    {
                        prefabProperties.MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                        prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames] = substanceMaterialParams.MaterialVariableKeyframeList[prefabProperties.keyFrames];
                        prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].PropertyMaterialName = substanceMaterialParams.substance.name;
                        prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].emissionColor = substanceMaterialParams.MaterialVariableKeyframeList[prefabProperties.keyFrames].emissionColor;
                        prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].MainTex = substanceMaterialParams.MaterialVariableKeyframeList[prefabProperties.keyFrames].MainTex;
                        prefabProperties.keyFrameTimes.Add(substanceMaterialParams.MaterialVariableKeyframeList[prefabProperties.keyFrames].animationTime);
                        prefabProperties.prefabAnimationCurve.AddKey(new Keyframe(animationParams.substanceCurve.keys[prefabProperties.keyFrames].time, animationParams.substanceCurve.keys[prefabProperties.keyFrames].value, animationParams.substanceCurve.keys[prefabProperties.keyFrames].inTangent, animationParams.substanceCurve.keys[prefabProperties.keyFrames].outTangent));
                        if (substanceMaterialParams.saveOutputParameters)
                            prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].hasParametersWithoutRange = true;
                        else
                            prefabProperties.MaterialVariableKeyframeList[prefabProperties.keyFrames].hasParametersWithoutRange = false;
                        prefabProperties.keyFrames++;
                    }
                }
                prefabProperties.keyFrameTimesOriginal = animationParams.keyFrameTimes;
                prefabProperties.prefabAnimationCurveBackup = prefabProperties.prefabAnimationCurve;

                prefabProperties.MaterialVariableKeyframeDictionaryList = substanceMaterialParams.MaterialVariableKeyframeDictionaryList;

                SubstanceTweenAnimationUtility.DeleteNonAnimatingParametersOnPrefab(prefabProperties, substanceMaterialParams, animationParams);
                if (substanceMaterialParams.MaterialVariableKeyframeList.Count >= 2)
                {
                    prefabProperties.animateOnStart = true;
                    prefabProperties.animateBasedOnTime = true;
                }
            }
            if (animationParams.cacheSubstance)
                prefabProperties.cacheAtStartup = true;
            else
                prefabProperties.cacheAtStartup = false;
            if (animationParams.animateOutputParameters)
                prefabProperties.animateOutputParameters = true;
            else
                prefabProperties.animateOutputParameters = false;

            if (substancePerformanceParams.myProceduralCacheSize.ToString() == ProceduralCacheSize.Heavy.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.Heavy;
            else if (substancePerformanceParams.myProceduralCacheSize.ToString() == ProceduralCacheSize.Medium.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.Medium;
            else if (substancePerformanceParams.myProceduralCacheSize.ToString() == ProceduralCacheSize.NoLimit.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.NoLimit;
            else if (substancePerformanceParams.myProceduralCacheSize.ToString() == ProceduralCacheSize.None.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.None;
            else if (substancePerformanceParams.myProceduralCacheSize.ToString() == ProceduralCacheSize.Tiny.ToString())
                prefabProperties.myProceduralCacheSize = PrefabProperties.MyProceduralCacheSize.Tiny;
            if (substancePerformanceParams.mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.All.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.All;
            else if (substancePerformanceParams.mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.Half.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.Half;
            else if (substancePerformanceParams.mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.One.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.One;
            else if (substancePerformanceParams.mySubstanceProcessorUsage.ToString() == ProceduralProcessorUsage.Unsupported.ToString())
                prefabProperties.mySubstanceProcessorUsage = PrefabProperties.MySubstanceProcessorUsage.Unsupported;
            if (flickerValues.flickerEnabled)
            {
                prefabProperties.flickerEnabled = true;
                if (flickerValues.flickerFloatToggle)
                    prefabProperties.flickerFloatToggle = true;
                if (flickerValues.flickerColor3Toggle)
                    prefabProperties.flickerColor3Toggle = true;
                if (flickerValues.flickerColor4Toggle)
                    prefabProperties.flickerColor4Toggle = true;
                if (flickerValues.flickerVector2Toggle)
                    prefabProperties.flickerVector2Toggle = true;
                if (flickerValues.flickerVector3Toggle)
                    prefabProperties.flickerVector3Toggle = true;
                if (flickerValues.flickerVector4Toggle)
                    prefabProperties.flickerVector4Toggle = true;
                if (flickerValues.flickerEmissionToggle)
                    prefabProperties.flickerEmissionToggle = true;
                prefabProperties.flickerMin = flickerValues.flickerMin;
                prefabProperties.flickerMax = flickerValues.flickerMax;
            }
#if UNITY_2017_1_OR_NEWER
            prefabRend.sharedMaterial.enableInstancing = true;// enables GPU instancing 
#endif 
            prefabProperties.useSharedMaterial = true;

            prefabProperties.rend = prefabRend;

            substanceToolParams.DebugStrings.Add("Prefab Path: " + prefabPath);
            substanceToolParams.DebugStrings.Add(prefab.name + " has " + prefabProperties.MaterialVariableKeyframeList.Count + " keyframes");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
        }
        else
            Debug.LogWarning("No Path/FileName specified for Prefab");
    }


    public static void WriteDebugText(SubstanceAnimationParams animationParams, SubstanceToolParams substanceToolParams)
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
            for (int i = 0; i < substanceToolParams.DebugStrings.Count; i++)
            {
                if (substanceToolParams.DebugStrings.Count > 1 && i > 1 && substanceToolParams.DebugStrings[i] != substanceToolParams.DebugStrings[i - 1])
                    debugWrite.WriteLine(substanceToolParams.DebugStrings[i]);
            }
            debugWrite.WriteLine("---Variables at current frame:---");
            debugWrite.WriteLine("Lerp: " + animationParams.lerp.ToString());
            debugWrite.WriteLine("Current Animation Time: " + animationParams.currentAnimationTime.ToString());
            debugWrite.WriteLine("Curve key count: " + animationParams.substanceCurve.keys.Length.ToString());
            debugWrite.WriteLine("Curve Float value: " + animationParams.curveFloat.ToString());
            debugWrite.WriteLine("Current Keyframe Animation Time: " + animationParams.currentKeyframeAnimationTime.ToString());
            debugWrite.WriteLine("Animate Backwards: " + animationParams.animateBackwards.ToString());
            debugWrite.WriteLine("Created debug log: " + path);
            debugWrite.Close();
            AssetDatabase.Refresh();
        }
        substanceToolParams.lastAction = MethodBase.GetCurrentMethod().Name.ToString();
    }
}
