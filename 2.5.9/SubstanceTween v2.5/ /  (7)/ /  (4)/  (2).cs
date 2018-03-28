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


public static class SubstanceInputOutputUtility  {

   /*

    public static void WriteXML() // Write current material variables to a XML file.
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
            //AddProceduralVariablesToList(xmlDescription);// Writes current variables to a list to be saved to a XML file
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

   public static void ReadXML() // Sets current material variables from a XML file without creating a keyframe.
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
        CacheAnimatedProceduralVariables();
    }

   public static void WriteAllXML() // Writes each keyframe to a XML file
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

   public static void ReadAllXML() // Read XML files and create keyframes from them.
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
                            Debug.Log(xmlEmissionColor);
                            emissionInput = xmlEmissionColor;
                            if (prefabScript)
                                prefabScript.emissionInput = emissionInput;
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
                            //AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[0]);
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
                            //AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[keyFrames]);
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
            CacheAnimatedProceduralVariables();
            CalculateAnimationLength();
        }
    }

   public static void WriteJSON() //  Write current material variables to a JSON file
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
            //AddProceduralVariablesToList(jsonDescription);// Writes current variables to a list to be saved to a XML file
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

   public static void ReadJSON()
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Read XML File");
        //var serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
        string path = EditorUtility.OpenFilePanel("", "", "json"); // 'Open' Dialog that only accepts JSON files
        if (path.Length != 0)
        {
            string dataAsJson = File.ReadAllText(path);
            var stream = new FileStream(path, FileMode.Open);
            if (stream.Length != 0)
            {
                MaterialVariableListHolder jsonContainer = JsonUtility.FromJson<MaterialVariableListHolder>(dataAsJson);
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
        CacheAnimatedProceduralVariables();
    } //  Sets current material variables from a JSON file without creating a keyframe

   public static void WriteAllJSON()
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
    } //Writes all keyframes to JSON files

   public static void ReadAllJSON() // Read JSON files and create keyframes from them.
    {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { this, substance, substanceImporter, currentSelection }, "Create Keyframes From JSON files");
        //var serializer = new XmlSerializer(typeof(MaterialVariableListHolder));
        string JsonReadFolderPath = EditorUtility.OpenFolderPanel("Load JSON files from folder", "", ""); //Creates 'Open Folder' Dialog
        if (JsonReadFolderPath.Length != 0)
        {
            string[] JsonReadFiles = Directory.GetFiles(JsonReadFolderPath);//array of selected xml file paths
            Debug.Log(JsonReadFiles.Count());
            if (JsonReadFiles.Count() > 0)
            {
                keyFrames = keyFrameTimes.Count();
                foreach (string jsonReadFile in JsonReadFiles) //for each xml file path in the list.
                {
                    Debug.Log(jsonReadFile);
                    if (jsonReadFile.EndsWith(".json"))
                    {
                        string dataAsJson = File.ReadAllText(jsonReadFile);
                        var stream = new FileStream(jsonReadFile, FileMode.Open);// defines how to use the file at the selected path
                        MaterialVariableListHolder jsonContainer = JsonUtility.FromJson<MaterialVariableListHolder>(dataAsJson);
                        //var container = serializer.Deserialize(stream) as MaterialVariableListHolder; //Puts current xml file into a list.
                        SetProceduralVariablesFromList(jsonContainer);//Sets current substance values from list
                        MainTexOffset = jsonContainer.MainTex;
                        Color jsonEmissionColor = new Color(0, 0, 0, 0);
                        jsonEmissionColor = jsonContainer.emissionColor;
                        if (rend.sharedMaterial.HasProperty("_EmissionColor"))
                        {
                            Debug.Log(jsonEmissionColor);
                            emissionInput = jsonEmissionColor;
                            if (prefabScript)
                                prefabScript.emissionInput = emissionInput;
                            rend.sharedMaterial.SetColor("_EmissionColor", jsonEmissionColor);
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
                            MaterialVariableKeyframeList[0] = jsonContainer;
                            //AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[0]);
                            keyFrames++;
                            keyFrameTimes.Add(MaterialVariableKeyframeList[0].animationTime);
                            AnimationUtility.SetKeyBroken(substanceCurve, 0, true);
                            AnimationUtility.SetKeyLeftTangentMode(substanceCurve, 0, AnimationUtility.TangentMode.Linear);
                            AnimationUtility.SetKeyRightTangentMode(substanceCurve, 0, AnimationUtility.TangentMode.Linear);
                        }
                        else if (keyFrameTimes.Count > 0)
                        {
                            MaterialVariableKeyframeList.Add(new MaterialVariableListHolder());
                            MaterialVariableKeyframeList[keyFrames] = jsonContainer;
                            MaterialVariableKeyframeDictionaryList.Add(new MaterialVariableDictionaryHolder());
                            MaterialVariableKeyframeDictionaryList[keyFrames] = new MaterialVariableDictionaryHolder();
                            //AddProceduralVariablesToDictionary(MaterialVariableKeyframeDictionaryList[keyFrames]);
                            keyFrameTimes.Add(jsonContainer.animationTime);
                            keyframeSum = 0;
                            for (int i = 0; i < MaterialVariableKeyframeList.Count() - 1; i++)  //  -1 to count here 6/10/17
                                keyframeSum += MaterialVariableKeyframeList[i].animationTime;
                            if (jsonContainer.AnimationCurveKeyframeList.Count() >= 1)
                                substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum, jsonContainer.AnimationCurveKeyframeList[0].inTangent, jsonContainer.AnimationCurveKeyframeList[0].outTangent));
                            else
                                substanceCurve.AddKey(new Keyframe(keyframeSum, keyframeSum));
                            if (keyFrames >= 1)
                                AnimationUtility.SetKeyBroken(substanceCurve, keyFrames, true);
                            keyFrames++;
                        }
                    }
                    DebugStrings.Add("Read keyframe from: " + jsonReadFile);
                }
                DebugStrings.Add(keyFrames - 1 + " Keyframes created from XML files ");
                substanceCurveBackup.keys = substanceCurve.keys;
                lastAction = MethodBase.GetCurrentMethod().Name.ToString();
            }
            else
                EditorUtility.DisplayDialog("Empty Folder", "No Files were found in the selected folder", "Ok");
            CacheAnimatedProceduralVariables();
            CalculateAnimationLength();
        }
    }
    */
}
