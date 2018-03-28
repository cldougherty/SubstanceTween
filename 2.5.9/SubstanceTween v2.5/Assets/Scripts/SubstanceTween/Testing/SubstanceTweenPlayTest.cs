
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;



public class SubstanceTweenPlayTest
{
    ProceduralMaterial currentMaterial;
    
   public SubstanceTweenTesterParams testingScript;
   public  ProceduralMaterial materialBeforeChange;
    ProceduralPropertyDescription[] materialVariablesBeforeChange;
    public List<string> materialKeysBeforeChange = new List<string>(); // Keys and values are used for sorting parameter names and values (Cant Serialize Dictionaries to XML)
    public List<string> materialValuesBeforeChange = new List<string>();

    public ProceduralMaterial materialAtStartup;
    ProceduralPropertyDescription[] materialVariablesAtStartup;
    public List<string> materialKeysAtStartup = new List<string>(); // Keys and values are used for sorting parameter names and values (Cant Serialize Dictionaries to XML)
    public List<string> materialValuesAtStartup = new List<string>();

    List<ProceduralMaterial> proceduralMaterialsTesting = new List<ProceduralMaterial>();
    List<string> proceduralMaterialsNamesTesting = new List<string>();

    GameObject spawnedObject;
    [Test]
	public void SubstanceTweenPlayTestSimplePasses()
    {
		// Use the Assert class to test conditions.
	}

	// A UnityTest behaves like a coroutine in PlayMode
	// and allows you to yield null to skip a frame in EditMode
	[UnityTest, Timeout(100000000)]
	public IEnumerator SubstanceTweenPlayTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame

        //SETUP SCENE/OBJECT
        SetupScene();
        SpawnAndSetupObject();
        yield return new WaitForSeconds(0.5f);

        //LOAD PROCEDURAL MATERIALS 
        LoadAllProceduralMaterialsFromFolder();
        SetSpecificProceduralMaterial("Crystal1-11");
        yield return new WaitForSeconds(1);

        // ADD MAIN TOOL TO SELECTED OBJECT
        AddEditorToolToObject();

        //REMEMBER CURRENT MATERIAL AS THE STARTUP/BEFORE CHANGE 
        materialVariablesBeforeChange = materialBeforeChange.GetProceduralPropertyDescriptions();
        AddSetProceduralVariablesBeforeChangeToKeyValue(materialBeforeChange, materialVariablesBeforeChange, materialKeysBeforeChange, materialValuesBeforeChange);
        AddSetProceduralVariablesBeforeChangeToKeyValue(materialAtStartup, materialVariablesAtStartup, materialKeysAtStartup, materialValuesAtStartup);
        materialVariablesAtStartup = materialAtStartup.GetProceduralPropertyDescriptions();
        yield return new WaitForSeconds(0.5f);

        // RANDOMIZE VALUES
        SubstanceTweenSetParameterUtility.RandomizeProceduralValues(testingScript.substanceMaterialParams, testingScript.randomizeSettings);
        yield return new WaitForSeconds(0.5f);

        // SET TO MINIMUM
        AddSetProceduralVariablesBeforeChangeToKeyValue(materialBeforeChange, materialVariablesBeforeChange, materialKeysBeforeChange, materialValuesBeforeChange);
        SubstanceTweenSetParameterUtility.SetAllProceduralValuesToMin(testingScript.substanceMaterialParams);
        yield return new WaitForSeconds(0.5f);

        //SET TO MAXIMUM
        CheckForChangedMaterialParams(materialVariablesBeforeChange);
        AddSetProceduralVariablesBeforeChangeToKeyValue(materialBeforeChange, materialVariablesBeforeChange, materialKeysBeforeChange, materialValuesBeforeChange);
        SubstanceTweenSetParameterUtility.SetAllProceduralValuesToMax(testingScript.substanceMaterialParams);
        CheckForChangedMaterialParams(materialVariablesBeforeChange);
        yield return new WaitForSeconds(0.5f);

        // RESET MATERIAL
        SubstanceTweenSetParameterUtility.ResetAllProceduralValues(testingScript.substanceDefaultMaterialParams, testingScript.substanceMaterialParams, testingScript.animationParams , testingScript.substanceToolParams);
        CheckForNoChangeInMaterialParams(materialAtStartup, materialVariablesAtStartup, materialKeysAtStartup, materialValuesAtStartup);
        AddSetProceduralVariablesBeforeChangeToKeyValue(materialBeforeChange, materialVariablesBeforeChange, materialKeysBeforeChange, materialValuesBeforeChange);
        yield return new WaitForSeconds(0.5f);
        
        //CREATE KEYFRAMES 
        SetSpecificMaterialParamsAndCreateKeyframes(5, "Crystal_Disorder", "Grunge_Map5_Contrast", "Base_Color");
        CheckForChangedMaterialParams(materialVariablesBeforeChange);
        ReWriteAllKeyframeTimes(1, testingScript.substanceMaterialParams, testingScript.animationParams);

        testingScript.substanceToolParams.selectedPrefabScript.animationToggle = true;
        yield return new WaitForSeconds(40f);

        // START TESTING CURVES 
        testingScript.substanceToolParams.testingCurve = true;
        float addKey = 19;
        testingScript.animationParams.substanceCurve.AddKey(addKey, addKey); // ADD KEY TO CURVE
        Assert.That(testingScript.animationParams.substanceCurve.keys.Count() != testingScript.animationParams.substanceCurveBackup.keys.Count(), "ERROR: Curve key count matches backup ");
        SubstanceTweenKeyframeUtility.CheckForAddKeyFromCurveEditor(testingScript.substanceMaterialParams, testingScript.animationParams, testingScript.substanceToolParams, testingScript.flickerValues); // checks if a key was added on the curve. if a curve key was added I add a keyframe for the material
        Assert.That(testingScript.animationParams.substanceCurve.keys.Count() == testingScript.animationParams.substanceCurveBackup.keys.Count(), "ERROR: Curve key does not count matches backup ");
        for (int i = 0; i <= testingScript.animationParams.substanceCurve.keys.Count() - 1; i++ )
        {
            if (testingScript.animationParams.substanceCurve.keys[i].time == addKey)
            {
                testingScript.animationParams.substanceCurve.RemoveKey(i);
            }
        }
        Assert.That(testingScript.animationParams.substanceCurve.keys.Count() != testingScript.animationParams.substanceCurveBackup.keys.Count(), "ERROR: Curve key count matches backup ");
        SubstanceTweenKeyframeUtility.CheckForRemoveOrEditFromCurveEditor(testingScript.substanceMaterialParams, testingScript.animationParams, testingScript.substanceToolParams, testingScript.flickerValues); // checks if a key was removed/edited on the curve. if a curve key was removed/edited I reflect the changes to the material keyframes. 

        Assert.That(testingScript.animationParams.substanceCurve.keys.Count() == testingScript.animationParams.substanceCurveBackup.keys.Count(), "ERROR: Curve key does not count matches backup ");

        //CREATE UNDO STATE
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { testingScript.referenceToEditorScript  }, "Edited Curve");

        //SIMULATE EDITING MULTIPLE CURVE KEYS TO A NEW VALUE THAT DOES NOT ALREADY EXIST ON THE CURVE
        testingScript.animationParams.substanceCurve.RemoveKey(2);
        testingScript.animationParams.substanceCurve.RemoveKey(1);
        testingScript.animationParams.substanceCurve.AddKey(4, 4);

        Assert.That(testingScript.animationParams.substanceCurve.keys.Count() != testingScript.animationParams.substanceCurveBackup.keys.Count(), "ERROR: Curve key count matches backup ");
        SubstanceTweenKeyframeUtility.CheckForRemoveOrEditFromCurveEditor(testingScript.substanceMaterialParams, testingScript.animationParams, testingScript.substanceToolParams, testingScript.flickerValues);
        Assert.That(testingScript.animationParams.substanceCurve.keys.Count() == testingScript.animationParams.substanceCurveBackup.keys.Count(), "ERROR: Curve key does not count matches backup ");

        yield return new WaitForSeconds(10);

        // UNDO TO A PREVIOUS STATE
        Undo.PerformUndo();

        // STOP TESTING CURVES
        testingScript.substanceToolParams.testingCurve = false;
        
        yield return new WaitForSeconds(50);

        //END TEST
        yield break;
	}

    void SetupScene() // adds light to the scene
    {
        GameObject lightGameObject = new GameObject("Light");
        Light lightComp = lightGameObject.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.transform.rotation = Quaternion.Euler(50, 0, 0);
    }

    void SpawnAndSetupObject()//creates object and adds testing script
    {
        spawnedObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Selection.activeGameObject = spawnedObject;
        testingScript = spawnedObject.AddComponent<SubstanceTweenTesterParams>();
    }

    void AddEditorToolToObject()// Load substance tool.
    {
        spawnedObject.AddComponent<SubstanceTool>();
        Assert.IsTrue(spawnedObject.GetComponent<SubstanceTool>() != null);
        SubstanceTool substanceTool = spawnedObject.GetComponent<SubstanceTool>();
    }

    void  LoadAllProceduralMaterialsFromFolder() // loads all procedural materials from a resources folder
    {
       // List<ProceduralMaterial> proceduralMaterialsTesting = new List<ProceduralMaterial>();
        //List<string> proceduralMaterialsNamesTesting = new List<string>();
        Object[] procedureMaterialObjects = Resources.LoadAll("Testing/ProceduralMaterial/", typeof(ProceduralMaterial));
        for (int i = 0; i <= procedureMaterialObjects.Length - 1; i++)
        {
            proceduralMaterialsTesting.Add(procedureMaterialObjects[i] as ProceduralMaterial);
            proceduralMaterialsNamesTesting.Add(procedureMaterialObjects[i].name);
        }
    }

    void SetSpecificProceduralMaterial(string materialName) // sets current material from one of the materials in the resources folder 
    {
         currentMaterial = proceduralMaterialsTesting[proceduralMaterialsNamesTesting.IndexOf(materialName)] as ProceduralMaterial;
        materialBeforeChange = proceduralMaterialsTesting[proceduralMaterialsNamesTesting.IndexOf(materialName)] as ProceduralMaterial;
        materialAtStartup = proceduralMaterialsTesting[proceduralMaterialsNamesTesting.IndexOf(materialName)] as ProceduralMaterial;

        spawnedObject.GetComponent<MeshRenderer>().sharedMaterial = currentMaterial;
        Assert.That(spawnedObject.GetComponent<MeshRenderer>().sharedMaterial == currentMaterial);
    }

    void CheckForChangedMaterialParams (ProceduralPropertyDescription[] materialVariablesListBeforeChange)
    { // check if material variables have changed after the beforeChange was saved 
        int variablesChanged = 0;
        for (int i = 0; i <= materialVariablesListBeforeChange.Count() - 1; i++)
        {
            ProceduralPropertyDescription materialVariableCurrent = testingScript.substanceMaterialParams.materialVariables[i];
            ProceduralPropertyDescription materialVariableBeforeChange = materialVariablesListBeforeChange[i];
            ProceduralPropertyType propType = testingScript.substanceMaterialParams.materialVariables[i].type;
     
            if (propType == ProceduralPropertyType.Float)
            {
                float propFloatCurrent = testingScript.substanceMaterialParams.substance.GetProceduralFloat(materialVariableCurrent.name);
                float propFloatBeforeChange = float.Parse(materialValuesBeforeChange[materialKeysBeforeChange.IndexOf(materialVariableCurrent.name)]);
                if (propFloatCurrent != propFloatBeforeChange)
                    variablesChanged++;
            }
            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                Color propColorCurrent = testingScript.substanceMaterialParams.substance.GetProceduralColor(materialVariableCurrent.name);
                Color curColor;
                ColorUtility.TryParseHtmlString(materialValuesBeforeChange[materialKeysBeforeChange.IndexOf(materialVariableCurrent.name)], out curColor);
                Color propColorBeforeChange = curColor;
                if (propColorCurrent != propColorBeforeChange)
                    variablesChanged++;
            }
            else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    Vector4 propVector4Current = testingScript.substanceMaterialParams.substance.GetProceduralVector(materialVariableCurrent.name);
                    Vector4 propVector4BeforeChange = StringToVector(materialValuesBeforeChange[materialKeysBeforeChange.IndexOf(materialVariableCurrent.name)], 4);
                    if (propVector4Current != propVector4BeforeChange)
                        variablesChanged++;
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    Vector3 propVector3Current = testingScript.substanceMaterialParams.substance.GetProceduralVector(materialVariableCurrent.name);
                    Vector3 propVector3BeforeChange = StringToVector(materialValuesBeforeChange[materialKeysBeforeChange.IndexOf(materialVariableCurrent.name)], 3);
                    if (propVector3Current != propVector3BeforeChange)
                        variablesChanged++;
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    Vector2 propVector2Current = testingScript.substanceMaterialParams.substance.GetProceduralVector(materialVariableCurrent.name);
                    Vector2 propVector2BeforeChange = StringToVector(materialValuesBeforeChange[materialKeysBeforeChange.IndexOf(materialVariableCurrent.name)], 2);
                    if (propVector2Current != propVector2BeforeChange)
                        variablesChanged++;
                }
            }
            else if (propType == ProceduralPropertyType.Enum)
            {
                int propEnumCurrent = testingScript.substanceMaterialParams.substance.GetProceduralEnum(materialVariableCurrent.name);
                int propEnumBeforeChange =int.Parse(materialValuesBeforeChange[materialKeysBeforeChange.IndexOf(materialVariableCurrent.name)]);
                if (propEnumCurrent != propEnumBeforeChange)
                    variablesChanged++;
            }
            else if (propType == ProceduralPropertyType.Boolean)
            {
                bool propBoolCurrent = testingScript.substanceMaterialParams.substance.GetProceduralBoolean(materialVariableCurrent.name);
                bool propBoolBeforeChange = materialBeforeChange.GetProceduralBoolean(materialVariableBeforeChange.name);
                if (propBoolCurrent != propBoolBeforeChange)
                    variablesChanged++;
            }
            if (variablesChanged > 0)
            {
                Assert.True(variablesChanged > 0 );
                break;
            }
            if (i == materialVariablesBeforeChange.Count() - 1 && variablesChanged == 0 )
                Assert.Fail("Material did not change");
        }
    }

    void CheckForNoChangeInMaterialParams (ProceduralMaterial inputMaterialBeforeChange, ProceduralPropertyDescription[] materialVariablesListBeforeChange, List<string> keysList, List<string> valuesList)
    {// checks if the material variables have not changed from a given material. 
        int variablesChanged = 0;
        for (int i = 0; i <= materialVariablesListBeforeChange.Count() - 1; i++)
        {
            ProceduralPropertyDescription materialVariableCurrent = testingScript.substanceMaterialParams.materialVariables[i];
            ProceduralPropertyDescription materialVariableBeforeChange = materialVariablesListBeforeChange[i];
            ProceduralPropertyType propType = testingScript.substanceMaterialParams.materialVariables[i].type;
     
            if (propType == ProceduralPropertyType.Float)
            {
                float propFloatCurrent = testingScript.substanceMaterialParams.substance.GetProceduralFloat(materialVariableCurrent.name);
                float propFloatBeforeChange = float.Parse(valuesList[keysList.IndexOf(materialVariableCurrent.name)]);
                if (propFloatCurrent != propFloatBeforeChange)
                    variablesChanged++;
            }
            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                Color propColorCurrent = testingScript.substanceMaterialParams.substance.GetProceduralColor(materialVariableCurrent.name);
                Color curColor;
                ColorUtility.TryParseHtmlString(valuesList[keysList.IndexOf(materialVariableCurrent.name)], out curColor);
                Color propColorBeforeChange = curColor;
                if (propColorCurrent != propColorBeforeChange)
                    variablesChanged++;
            }
            else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    Vector4 propVector4Current = testingScript.substanceMaterialParams.substance.GetProceduralVector(materialVariableCurrent.name);
                    Vector4 propVector4BeforeChange = StringToVector(valuesList[keysList.IndexOf(materialVariableCurrent.name)], 4);
                    if (propVector4Current != propVector4BeforeChange)
                        variablesChanged++;
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    Vector3 propVector3Current = testingScript.substanceMaterialParams.substance.GetProceduralVector(materialVariableCurrent.name);
                    Vector3 propVector3BeforeChange = StringToVector(valuesList[keysList.IndexOf(materialVariableCurrent.name)], 3);
                    if (propVector3Current != propVector3BeforeChange)
                        variablesChanged++;
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    Vector2 propVector2Current = testingScript.substanceMaterialParams.substance.GetProceduralVector(materialVariableCurrent.name);
                    Vector2 propVector2BeforeChange = StringToVector(valuesList[keysList.IndexOf(materialVariableCurrent.name)], 2);
                    if (propVector2Current != propVector2BeforeChange)
                        variablesChanged++;
                }
            }
            else if (propType == ProceduralPropertyType.Enum)
            {
                int propEnumCurrent = testingScript.substanceMaterialParams.substance.GetProceduralEnum(materialVariableCurrent.name);
                int propEnumBeforeChange =int.Parse(valuesList[keysList.IndexOf(materialVariableCurrent.name)]);
                if (propEnumCurrent != propEnumBeforeChange)
                    variablesChanged++;
            }
            else if (propType == ProceduralPropertyType.Boolean)
            {
                bool propBoolCurrent = testingScript.substanceMaterialParams.substance.GetProceduralBoolean(materialVariableCurrent.name);
                bool propBoolBeforeChange = materialBeforeChange.GetProceduralBoolean(materialVariableBeforeChange.name);
                if (propBoolCurrent != propBoolBeforeChange)
                    variablesChanged++;
            }
            if (variablesChanged > 0)
            {
                Assert.Fail("Material not the same as original");
                break;
            }
            if (i == materialVariablesBeforeChange.Count() - 1 && variablesChanged == 0)
                Debug.Log("Material same as original, Success?");
        }
    }

    void AddSetProceduralVariablesBeforeChangeToKeyValue(ProceduralMaterial inputMaterialBeforeChange, ProceduralPropertyDescription[] materialVariablesListBeforeChange, List<string> keysList, List<string>valuesList )
    { // Specify current material variables as the 'beforeChange' material and what to check against after a change happens
        if (keysList.Count > 0)
            keysList.Clear();
        if (valuesList.Count > 0)
            valuesList.Clear();
        for (int i = 0; i < materialVariablesBeforeChange.Length; i++)//loop through properties and make them the default properties for this object
        {
            ProceduralPropertyDescription materialVariable = materialVariablesBeforeChange[i];
            ProceduralPropertyType propType = materialVariablesBeforeChange[i].type;
            if (propType == ProceduralPropertyType.Float /*&& (materialVariable.hasRange )*/)
            {
               float propFloat = inputMaterialBeforeChange.GetProceduralFloat(materialVariable.name);
               keysList.Add(materialVariable.name);
               valuesList.Add(propFloat.ToString());             
            }
            if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                Color propColor = inputMaterialBeforeChange.GetProceduralColor(materialVariable.name);
                keysList.Add(materialVariable.name);
                valuesList.Add("#" + ColorUtility.ToHtmlStringRGBA(propColor));
            }
            if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)/* && (objProperty.hasRange || (saveParametersWithoutRange && !objProperty.hasRange))*/)
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    Vector4 propVector4 = inputMaterialBeforeChange.GetProceduralVector(materialVariable.name);
                    keysList.Add(materialVariable.name);
                    valuesList.Add(propVector4.ToString());
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    Vector3 propVector3 = inputMaterialBeforeChange.GetProceduralVector(materialVariable.name);
                    keysList.Add(materialVariable.name);
                    valuesList.Add(propVector3.ToString());
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    Vector2 propVector2 = inputMaterialBeforeChange.GetProceduralVector(materialVariable.name);
                    keysList.Add(materialVariable.name);
                    valuesList.Add(propVector2.ToString());
                }
            }
            else if (propType == ProceduralPropertyType.Enum)
            {
                int propEnum = inputMaterialBeforeChange.GetProceduralEnum(materialVariable.name);
                keysList.Add(materialVariable.name);
                valuesList.Add(propEnum.ToString());
            }
            else if (propType == ProceduralPropertyType.Boolean)
            {
                bool propBoolean = inputMaterialBeforeChange.GetProceduralBoolean(materialVariable.name);
                keysList.Add(materialVariable.name);
                valuesList.Add(propBoolean.ToString());
            }
        }
        // materialVariables.animationTime = animationTime;

        // sets a procedural material based on custom dictionary. I do this because copies of Procedural Materials get changed with their originals. This does not happen if i set values based on parsed values.   
        for (int i = 0; i < materialVariablesBeforeChange.Length; i++)
        {
            ProceduralPropertyDescription materialVariables = materialVariablesBeforeChange[i];
            ProceduralPropertyType propType = materialVariablesBeforeChange[i].type;
            if (propType == ProceduralPropertyType.Float && (materialVariables.hasRange /*|| (saveParametersWithoutRange && !materialVariables.hasRange) || resettingValuesToDefault)*/))
            {
                for (int j = 0; j < keysList.Count; j++)
                {
                    if (keysList[j] == materialVariables.name)
                    {
                        if (keysList[j] == materialVariables.name)
                            inputMaterialBeforeChange.SetProceduralFloat(materialVariables.name, float.Parse(valuesList[j]));
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
            {
                for (int j = 0; j < keysList.Count; j++)
                {
                    if (keysList[j] == materialVariables.name)
                    {
                        Color curColor;
                        ColorUtility.TryParseHtmlString(valuesList[j], out curColor);
                        inputMaterialBeforeChange.SetProceduralColor(materialVariables.name, curColor);
                    }
                }
            }
            else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (materialVariables.hasRange /*|| (saveParametersWithoutRange && !materialVariables.hasRange) || resettingValuesToDefault)*/))
            {
                if (propType == ProceduralPropertyType.Vector4)
                {
                    for (int j = 0; j < keysList.Count; j++)
                    {
                        if (keysList[j] == materialVariables.name)
                        {
                            Vector4 curVector4 = StringToVector(valuesList[j], 4);
                            inputMaterialBeforeChange.SetProceduralVector(materialVariables.name, curVector4);
                        }
                    }
                }
                else if (propType == ProceduralPropertyType.Vector3)
                {
                    for (int j = 0; j < keysList.Count; j++)
                    {
                        if (keysList[j] == materialVariables.name)
                        {
                            Vector3 curVector3 = StringToVector(valuesList[j], 3);
                            inputMaterialBeforeChange.SetProceduralVector(materialVariables.name, curVector3);
                        }
                    }
                }
                else if (propType == ProceduralPropertyType.Vector2)
                {
                    for (int j = 0; j < keysList.Count; j++)
                    {
                        if (keysList[j] == materialVariables.name)
                        {
                            Vector2 curVector2 = StringToVector(valuesList[j], 2);
                            inputMaterialBeforeChange.SetProceduralVector(materialVariables.name, curVector2);
                        }
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Enum)
            {
                for (int j = 0; j < keysList.Count; j++)
                {
                    if (keysList[j] == materialVariables.name)
                    {
                        int curEnum = int.Parse(valuesList[j]);
                        inputMaterialBeforeChange.SetProceduralEnum(materialVariables.name, curEnum);
                    }
                }
            }
            else if (propType == ProceduralPropertyType.Boolean)
            {
                for (int j = 0; j < keysList.Count; j++)
                {
                    if (keysList[j] == materialVariables.name)
                    {
                        bool curBool = bool.Parse(valuesList[j]);
                        inputMaterialBeforeChange.SetProceduralBoolean(materialVariables.name, curBool);
                    }
                }
            }
        }
    }

    void SetSpeceficMaterialParams(params string[] paramNames)
    {

    }

    void SetSpecificMaterialParamsAndCreateKeyframes(float keyframesToCreate, params string[] paramsNames) // needs float for division
    {//  creates a certain amount of keyframes from a amount of parameter names. sets them based on 
        for (int i = 0; i  <= keyframesToCreate-1; i++  )
        {
            foreach (string param in paramsNames)
            {
              if (testingScript.substanceMaterialParams.materialVariableNames.Contains(param))
              {
                    int index = testingScript.substanceMaterialParams.materialVariableNames.IndexOf(param);
                    ProceduralPropertyDescription materialVariable = testingScript.substanceMaterialParams.materialVariables[index];
                    ProceduralPropertyType propType = testingScript.substanceMaterialParams.materialVariables[index].type;
                    if (propType == ProceduralPropertyType.Float)
                    {
                        testingScript.substanceMaterialParams.substance.SetProceduralFloat(param, (materialVariable.maximum / keyframesToCreate) * i); // set current parameter to (max value of parameter divided by the total # of keyframes to create) multiplyed by current keyframe(i) 
                    }
                    else if (propType == ProceduralPropertyType.Color3)
                    {
                        testingScript.substanceMaterialParams.substance.SetProceduralColor(param, new Color(i / keyframesToCreate, 0, 0));
                    }
                    else if (propType == ProceduralPropertyType.Color4)
                    {
                        testingScript.substanceMaterialParams.substance.SetProceduralColor(param, new Color(i / keyframesToCreate, 0, 0, 254));
                    }
                    else if (propType == ProceduralPropertyType.Vector2)
                    {
                        testingScript.substanceMaterialParams.substance.SetProceduralVector(param,new Vector2(materialVariable.minimum + (materialVariable.maximum / keyframesToCreate) * i, materialVariable.minimum + (materialVariable.maximum / keyframesToCreate) * i) );

                    }
                    else if (propType == ProceduralPropertyType.Vector3)
                    {
                        testingScript.substanceMaterialParams.substance.SetProceduralVector(param, new Vector3(materialVariable.minimum + (materialVariable.maximum / keyframesToCreate) * i, materialVariable.minimum + (materialVariable.maximum / keyframesToCreate) * i, materialVariable.minimum + (materialVariable.maximum / keyframesToCreate) * i));

                    }
                    else if (propType == ProceduralPropertyType.Vector4)
                    {
                        testingScript.substanceMaterialParams.substance.SetProceduralVector(param, new Vector4(materialVariable.minimum + (materialVariable.maximum / keyframesToCreate) * i, materialVariable.minimum + (materialVariable.maximum / keyframesToCreate) * i, materialVariable.minimum + (materialVariable.maximum / keyframesToCreate) * i, materialVariable.minimum + (materialVariable.maximum / keyframesToCreate) * i));

                    }
                    else if (propType == ProceduralPropertyType.Boolean)
                    {
                        if (i % 2 == 0)
                            testingScript.substanceMaterialParams.substance.SetProceduralBoolean(param, true);
                        else
                            testingScript.substanceMaterialParams.substance.SetProceduralBoolean(param, false);
                    }
                    else if (propType == ProceduralPropertyType.Enum)
                    {
                        testingScript.substanceMaterialParams.substance.SetProceduralEnum(param, (int)Random.Range(materialVariable.minimum, materialVariable.maximum));
                    }
              }
                //ProceduralPropertyDescription materialVariable = testingScript.substanceMaterialParams.materialVariables[ArrayUtility.IndexOf( testingScript.substanceMaterialParams.materialVariables,param)];
            }
            CreateKeyframe();
        }
    }

    void CreateKeyframe()
    {
        SubstanceTweenKeyframeUtility.CreateKeyframe(testingScript.substanceMaterialParams, testingScript.animationParams, testingScript.substanceToolParams);
    }

    void ReWriteAllKeyframeTimes(float desiredTime, SubstanceMaterialParams substanceMaterialParams, SubstanceAnimationParams animationParams )
    {
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
                for (int k = 0; k < animationParams.keyFrameTimes.Count() - 1; k++)
                {
                    animationParams.keyFrameTimes[k] = desiredTime;
                    substanceMaterialParams.MaterialVariableKeyframeList[k].animationTime = desiredTime;
                }
                animationParams.keyframeSum += desiredTime;
        }

        /*for (int k = 0; k < animationParams.keyFrameTimes.Count() - 1; k++)
        {
            animationParams.keyFrameTimes[k] = desiredTime;
            substanceMaterialParams.MaterialVariableKeyframeList[k].animationTime = desiredTime;
        }
        animationParams.keyframeSum += desiredTime;*/
    }

    public static Vector4 StringToVector(string startVector, int VectorAmount)
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
}
