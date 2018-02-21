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

public class SubstanceToolWindow : EditorWindow {
	//SubstanceTween Ver 2.4 - 1/29/2016 
	//Written by: Chris Dougherty
	//https://www.linkedin.com/in/archarchaic
	//chris.ll.dougherty@gmail.com
	//https://www.artstation.com/artist/archarchaic

	private GameObject currentSelection;
	public Renderer rend;
	public bool UpdatingStartVariables = true , saveDefaultSubstanceVars = true, rebuildSubstanceImmediately, gameIsPaused, substanceLerp, saveParametersWithoutRange = true, resettingValuesToDefault = true, showKeyframes, animateBackwards;
	public ProceduralMaterial substance, defaultSubstance;
	private ProceduralPropertyDescription[] objProperties, defaultObjProperties;
	public ObjProperty xmlDescription;
	public List<ObjProperty> keyframeDescriptions = new List<ObjProperty>();
	public List<ObjProperty> defaultSubstanceObjProperties = new List<ObjProperty>();
	public List<float> defaultMatFloatValues = new List<float>();
	public List<Vector2> defaultMatVector2Values = new List<Vector2>();
	public List<Vector3> defaultMatVector3Values = new List<Vector3>();
	public List<Vector4> defaultMatVector4Values = new List<Vector4>();
	public List<Color> defaultMatColorValues = new List<Color>();
	public List<string> DebugStrings = new List<string>();
	public List<ProceduralMaterial> selectedStartupMaterials = new List<ProceduralMaterial>();
	public List<GameObject> selectedStartupGameObjects = new List<GameObject>();
	public List<float> keyFrameTimes = new List<float>();
	public Color defaultStdEmissionColor, emissionInput;
	public Vector2 scrollVal , defaultStdOffset, MainTexOffset;
	public int defaultSubstanceIndex, keyFrames, currentKeyframeIndex;
	public float animationTime = 5, currentAnimationTime= 0;
	public enum AnimationType { Loop, BackAndForth };
	public AnimationType animationType;

	[MenuItem("Window/SubstanceTween")]
	static void Init()
	{
		var window = (SubstanceToolWindow)GetWindow(typeof(SubstanceToolWindow));
		var content = new GUIContent ();
		content.text = "SubstanceTween";
		var icon = new Texture2D (16,16);
		content.image = icon;
		window.titleContent = content;
	}

	private void OnFocus()
	{
		if (currentSelection == null && Selection.activeGameObject) 
		{
			currentSelection = Selection.activeGameObject;
			if (currentSelection)
			rend = currentSelection.GetComponent<Renderer>();
			if(rend && UpdatingStartVariables)// should only be called after the first time you open the tool
			{
				DebugStrings.Add("Opened Tool");
				substance = rend.sharedMaterial as ProceduralMaterial;
				UpdatingStartVariables = false;
				selectedStartupMaterials.Add(substance); // First selected object material 
				selectedStartupGameObjects.Add(currentSelection); // First selected game object
				DebugStrings.Add ("First object selected: " + currentSelection + " Selected objects material name: " + rend.sharedMaterial.name);
			}
			if (substance)
			substance.RebuildTextures();
			Repaint();
		}
	}
		
	void OnSelectionChange() // Gets called whenever you change objects 
	{
		if (currentSelection != Selection.activeGameObject)
			substanceLerp = false;
		currentSelection = Selection.activeGameObject;
		if (!gameIsPaused)
		{
			if(currentSelection)
				rend = currentSelection.GetComponent<Renderer>();
			if (rend && currentSelection)
			{
				DebugStrings.Add("Selected: " + currentSelection + "Selected objects material name: " + rend.sharedMaterial.name); 
				Debug.Log("Selected: " + currentSelection + "Selected objects material name: " + rend.sharedMaterial.name + "Material has:" + objProperties.Count() + " Properties");
			}
			if (selectedStartupMaterials.Count >0)
			{
				bool materialExists = false;
				if (rend)
					substance = rend.sharedMaterial as ProceduralMaterial;
				for(int i = 0; i < selectedStartupMaterials.Count; i++) // goes through every material that you have selected in the past 
				{
					if (currentSelection.name == selectedStartupGameObjects[i].name) // if currently selected object name = one of the gameobjects that you have already selected before
					{
						substance = selectedStartupMaterials[i];
						materialExists = true; // object has already been selected before 
						DebugStrings.Add (currentSelection.name + " = " + selectedStartupMaterials[i].name);
					}
						DebugStrings.Add("Material " + i + ": " + selectedStartupMaterials[i]);
				}
				if (!materialExists) //Object has not been selected before so save all of the current object variables as this object's default variables 
				{
					if (rend)
						substance = rend.sharedMaterial as ProceduralMaterial;
					saveParametersWithoutRange = true;
					selectedStartupMaterials.Add(substance);
					selectedStartupGameObjects.Add(currentSelection);
					defaultSubstanceObjProperties.Add(new ObjProperty());
					defaultSubstance = rend.sharedMaterial as ProceduralMaterial;
					if (substance)
					{
						defaultSubstance.CopyPropertiesFromMaterial(substance);
						defaultObjProperties = substance.GetProceduralPropertyDescriptions();
						objProperties = substance.GetProceduralPropertyDescriptions();
						defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName = defaultSubstance.name;
						AddProceduralVariablesToList(defaultSubstanceObjProperties[defaultSubstanceIndex]);
						defaultSubstanceObjProperties[defaultSubstanceIndex].stdVector2.Add(MainTexOffset);
						defaultSubstanceObjProperties[defaultSubstanceIndex].stdColor.Add(emissionInput);
						DebugStrings.Add("Default substance material " + defaultSubstanceIndex + ": " + defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName);
						defaultSubstanceIndex++;
					}
				}
			}
			if (keyframeDescriptions.Count == 1 && (keyframeDescriptions[0].PropertyName.Count > 0))
			{
				keyframeDescriptions[0].PropertyName.Clear(); keyframeDescriptions[0].PropertyFloat.Clear(); keyframeDescriptions[0].PropertyColor.Clear(); 
				keyframeDescriptions[0].PropertyVector2.Clear();keyframeDescriptions[0].PropertyVector3.Clear();keyframeDescriptions[0].PropertyVector4.Clear();
				keyframeDescriptions[0].myKeys.Clear(); keyframeDescriptions[0].myValues.Clear();
			}
		}
	}
		
	public void OnGUI()
	{
		if(EditorApplication.isPlaying && gameIsPaused == false && currentSelection != null && rend)
		{
			var styleAnimationButton = new GUIStyle(GUI.skin.button);
			if (keyframeDescriptions.Count >=2 && substanceLerp)
				styleAnimationButton.normal.textColor = Color.green; // green for animating
			else if(keyframeDescriptions.Count >=2 && !substanceLerp)
				styleAnimationButton.normal.textColor = new Color(255,204,0); // yellow for pause/standby
			else
				styleAnimationButton.normal.textColor = Color.red; // red for not ready to animate (needs 2 keyframes)
			var styleMainHeader = new GUIStyle();
			styleMainHeader.fontSize = 25;
			styleMainHeader.alignment = TextAnchor.UpperCenter;
			var styleH2 = new GUIStyle();
			styleH2.fontSize = 20;
			styleH2.alignment = TextAnchor.UpperCenter;
			EditorGUILayout.BeginVertical();
			scrollVal = GUILayout.BeginScrollView(scrollVal);
			EditorGUILayout.LabelField("SubstanceTween 2.4",styleMainHeader);EditorGUILayout.Space();
			EditorGUILayout.LabelField("Currently selected Material:",styleH2);EditorGUILayout.Space();
			if(substance)
			EditorGUILayout.LabelField (substance.name,styleH2);EditorGUILayout.Space();
			currentSelection.transform.position = EditorGUILayout.Vector3Field("At Position", currentSelection.transform.position);
			ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;
			if (substance && saveDefaultSubstanceVars)
			{
				defaultSubstanceObjProperties.Add(new ObjProperty());
				defaultSubstance = rend.sharedMaterial as ProceduralMaterial;
				defaultSubstance.CopyPropertiesFromMaterial(substance);
				defaultObjProperties = substance.GetProceduralPropertyDescriptions();
				objProperties = substance.GetProceduralPropertyDescriptions();
				defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName = defaultSubstance.name;
				DebugStrings.Add("Default substance material " + defaultSubstanceIndex + ": " + selectedStartupMaterials[0]);
				AddProceduralVariablesToList(defaultSubstanceObjProperties[defaultSubstanceIndex]);
				defaultSubstanceObjProperties[defaultSubstanceIndex].stdVector2.Add(MainTexOffset);
				defaultSubstanceObjProperties[defaultSubstanceIndex].stdColor.Add(emissionInput);
				defaultSubstanceIndex++;
				saveDefaultSubstanceVars = false;
			}
			if (substance)//if object has a procedural material, loop through properties and create sliders based on type
			{
				objProperties = substance.GetProceduralPropertyDescriptions();
				for(int i = 0 ; i < objProperties.Length; i++)
				{
					ProceduralPropertyDescription objProperty = objProperties[i];
					ProceduralPropertyType propType = objProperties[i].type;
					if(propType == ProceduralPropertyType.Float)
					{
						EditorGUI.BeginChangeCheck();
						GUILayout.BeginHorizontal();
						GUILayout.Label(objProperty.name);
						float propFloat = substance.GetProceduralFloat(objProperty.name);
						float oldfloat = propFloat;
						string propFloatTextField = propFloat.ToString();
						propFloat = EditorGUILayout.Slider(propFloat, objProperty.minimum, objProperty.maximum);
						substance.SetProceduralFloat(objProperty.name, propFloat);
						GUILayout.EndHorizontal();
						if (EditorGUI.EndChangeCheck()) // anytime you change a slider it will save the old/new value to the debug text file
							{DebugStrings.Add(objProperty.name + " Was " + oldfloat + " is now: " + propFloat);}
					}
					else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4) 
					{
						EditorGUI.BeginChangeCheck();
						GUILayout.Label(objProperty.name);
						Color colorInput = substance.GetProceduralColor(objProperty.name);
						Color oldColorInput = colorInput;
						colorInput = EditorGUILayout.ColorField(colorInput);
						if (colorInput != oldColorInput)
							substance.SetProceduralColor(objProperty.name, colorInput);
						if (EditorGUI.EndChangeCheck())
							{DebugStrings.Add(objProperty.name + " Was " + oldColorInput + " is now: " + colorInput);}
					}
					else if(propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
					{
						EditorGUI.BeginChangeCheck();
						GUILayout.Label(objProperty.name);
						int vectorComponentAmount = 4;
						if (propType== ProceduralPropertyType.Vector2)
							vectorComponentAmount = 2;
						if (propType == ProceduralPropertyType.Vector3)
							vectorComponentAmount = 3;
						Vector4 inputVector = substance.GetProceduralVector(objProperty.name);
						Vector4 oldInputVector = inputVector;
						int c = 0; 
						while (c < vectorComponentAmount) 
						{ 
							inputVector[c] = EditorGUILayout.Slider(inputVector[c], objProperty.minimum,objProperty.maximum); c++;
						} 
						if (inputVector != oldInputVector) 
							substance.SetProceduralVector(objProperty.name, inputVector);
						if (EditorGUI.EndChangeCheck())
							{DebugStrings.Add(objProperty.name + " Was " + oldInputVector + " is now: " + inputVector);}
					}
				}
				if (rend && rend.sharedMaterial.HasProperty("_MainTex"))
				{
					EditorGUI.BeginChangeCheck();
					GUILayout.Label("_MainTex");
					if (MainTexOffset != null)
					{
						Vector2 oldOffset = MainTexOffset;
						MainTexOffset.x = EditorGUILayout.Slider(MainTexOffset.x,-10f,10.0f); 
						MainTexOffset.y = EditorGUILayout.Slider(MainTexOffset.y,-10f,10.0f); 
						if (EditorGUI.EndChangeCheck())
							{DebugStrings.Add("_MainTex" + " Was " + oldOffset + " is now: " + MainTexOffset);}
					}
				}
				if (rend && rend.sharedMaterial.HasProperty("_EmissionColor"))
				{
					EditorGUI.BeginChangeCheck();
					GUILayout.Label("Emission:");
					emissionInput = EditorGUILayout.ColorField(emissionInput);
					Color oldEmissionInput = emissionInput;
					rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
					if (EditorGUI.EndChangeCheck())
						{DebugStrings.Add("_EmissionColor" + " Was " + oldEmissionInput + " is now: " + emissionInput);}
				}
				EditorGUILayout.Space();
				Rect r = EditorGUILayout.BeginHorizontal ("Button");
				if (GUILayout.Button("Write XMl"))
				{
					GUILayout.Space(10);
					GUILayout.Label("Write XMl label");
					StreamWriter writer;
					var path = EditorUtility.SaveFilePanel("Save Data", "","", "xml");
					FileInfo fInfo = new FileInfo(path);
					AssetDatabase.Refresh();
					if (!fInfo.Exists)
						writer = fInfo.CreateText();
					else
					{
						writer = fInfo.CreateText(); Debug.Log("Overwriting File");
					}
					xmlDescription = new ObjProperty();
					XmlSerializer serializer = new XmlSerializer(typeof(ObjProperty));
					AddProceduralVariablesToList(xmlDescription);
					xmlDescription.PropertyMaterialName = substance.name;
					xmlDescription.stdColor.Add(emissionInput);
					xmlDescription.stdVector2.Add(MainTexOffset);
					serializer.Serialize(writer, xmlDescription);
					writer.Close();
					Debug.Log(fInfo + ": File written.");
					DebugStrings.Add("-----------------------------------");
					DebugStrings.Add("Wrote XML file " + " to: " + fInfo + ", File has: ");
					DebugStrings.Add(xmlDescription.PropertyName.Count + " Total Properties ");
					DebugStrings.Add(xmlDescription.PropertyFloat.Count + " Float Properties");
					DebugStrings.Add(xmlDescription.PropertyColor.Count + " Color Properties");
					DebugStrings.Add(xmlDescription.PropertyVector4.Count + " Vector4 Properties");
					DebugStrings.Add(xmlDescription.PropertyVector3.Count + " Vector3 Properties");
					DebugStrings.Add(xmlDescription.PropertyVector2.Count + " Vector2 Properties");
					DebugStrings.Add(xmlDescription.myKeys.Count + " Keys");
					DebugStrings.Add(xmlDescription.myValues.Count + " Values");
					DebugStrings.Add("Material Name: " + xmlDescription.PropertyMaterialName);
					if (emissionInput != null)
						DebugStrings.Add("_EmissionColor = " + emissionInput);
					if (MainTexOffset != null)
						DebugStrings.Add("_MainTex = " + MainTexOffset);
					DebugStrings.Add("-----------------------------------");
				}
				if (GUILayout.Button("Read XML"))
				{
					var serializer = new XmlSerializer(typeof(ObjProperty));
					var stream = new FileStream(EditorUtility.OpenFilePanel("","","xml"), FileMode.Open);
					var container = serializer.Deserialize(stream) as ObjProperty;
					SetProceduralVariablesFromList(container);
					MainTexOffset = container.stdVector2[0];
					Color stdEmissionColor = new Color(0,0,0,0);
					stdEmissionColor = container.stdColor[0];
					if (rend.sharedMaterial.HasProperty("_EmissionColor"))
					{
						emissionInput = stdEmissionColor;
						rend.sharedMaterial.SetColor("_EmissionColor", stdEmissionColor);
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
					DebugStrings.Add(container.myKeys.Count + " Keys");
					DebugStrings.Add(container.myValues.Count + " Values");
					if (emissionInput != null)
						DebugStrings.Add("_EmissionColor = " + emissionInput);
					if (MainTexOffset != null)
						DebugStrings.Add("_MainTex = " + MainTexOffset);
					DebugStrings.Add("-----------------------------------");
					substance.RebuildTexturesImmediately();
				}
				EditorGUILayout.EndHorizontal(); // ends button
				if (GUILayout.Button("Set All Procedural Values To The Minimum: ")) //except color
				{
					for (int i = 0; i < objProperties.Length; i++)
					{
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;
						if (propType == ProceduralPropertyType.Float)
							substance.SetProceduralFloat(objProperty.name, objProperties[i].minimum);
						if (propType == ProceduralPropertyType.Vector2 && objProperty.hasRange)
							substance.SetProceduralVector(objProperty.name,new Vector2(objProperties[i].minimum,objProperties[i].minimum));
						if (propType == ProceduralPropertyType.Vector3 && objProperty.hasRange)
							substance.SetProceduralVector(objProperty.name,new Vector3(objProperties[i].minimum,objProperties[i].minimum,objProperties[i].minimum));
						if (propType == ProceduralPropertyType.Vector4 && objProperty.hasRange)
							substance.SetProceduralVector(objProperty.name,new Vector4(objProperties[i].minimum,objProperties[i].minimum,objProperties[i].minimum,objProperties[i].minimum));
					}DebugStrings.Add("Set all properties to the minimum");
					substance.RebuildTexturesImmediately();
				}
				if (GUILayout.Button("Set All Procedural Values To The Max: ")) // except color 
				{
					for (int i = 0; i < objProperties.Length; i++)
					{
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;
						if (propType == ProceduralPropertyType.Float)
							substance.SetProceduralFloat(objProperty.name, objProperties[i].maximum);
						if (propType == ProceduralPropertyType.Vector2 && objProperty.hasRange)
							substance.SetProceduralVector(objProperty.name,new Vector2(objProperties[i].maximum,objProperties[i].maximum));
						if (propType == ProceduralPropertyType.Vector3 && objProperty.hasRange)
							substance.SetProceduralVector(objProperty.name,new Vector3(objProperties[i].maximum,objProperties[i].maximum,objProperties[i].maximum));
						if (propType == ProceduralPropertyType.Vector4 && objProperty.hasRange)
							substance.SetProceduralVector(objProperty.name,new Vector4(objProperties[i].maximum,objProperties[i].maximum,objProperties[i].maximum,objProperties[i].maximum));
					}DebugStrings.Add("Set all properties to the maximum");
					substance.RebuildTexturesImmediately();
				}
				if (GUILayout.Button("Reset ALL Values To Default"))
				{
					for (int i = 0; i < defaultSubstanceObjProperties.Count; i++)
					{
						if ((substance.name == defaultSubstanceObjProperties[i].PropertyMaterialName) || (rend.sharedMaterial.name == defaultSubstanceObjProperties[i].PropertyMaterialName))
						{
							resettingValuesToDefault = true;
							SetProceduralVariablesFromList(defaultSubstanceObjProperties[i]);
							MainTexOffset = defaultSubstanceObjProperties[i].stdVector2[0];
							if (rend.sharedMaterial.HasProperty("_EmissionColor"))
								emissionInput = defaultSubstanceObjProperties[i].stdColor[0]; // update or OnGUI calls this which is why i dont use SetColor here
							DebugStrings.Add("Reset all values to default");
							substance.RebuildTexturesImmediately();
							resettingValuesToDefault = false;
							return;
						}
					}
				}
				GUILayout.Label("Animation Time:");
				animationTime = EditorGUILayout.FloatField(animationTime);
				EditorGUILayout.Space();
				if (GUILayout.Button("Create Keyframe"))
				{
					if (animationTime >0)
					{
						if (keyFrameTimes.Count == 0)
						{
							DebugStrings.Add("Created Keyframe 1:");
							keyframeDescriptions.Add(new ObjProperty());
							keyframeDescriptions[0] = new ObjProperty();
							AddProceduralVariablesToList(keyframeDescriptions[0]);
							keyframeDescriptions[0].PropertyMaterialName = substance.name;
							keyframeDescriptions[0].stdColor.Add(emissionInput);
							keyframeDescriptions[0].stdVector2.Add(MainTexOffset);
							keyFrames++;
							keyFrameTimes.Add(keyframeDescriptions[0].animationTime);
							if (saveParametersWithoutRange)
								keyframeDescriptions[0].hasParametersWithoutRange = true;
							else
								keyframeDescriptions[0].hasParametersWithoutRange = false;
						}
						else if (keyFrameTimes.Count > 0)
						{	for(int i = 0; i <= keyFrameTimes.Count -1; i++) 
							{// Goes through each key frame and checks if the keyframe that you are trying to create has the same number of parametrs as the rest and if they all save Output parameters or not.
								if (objProperties.Length != keyframeDescriptions[i].PropertyName.Count)
								{
									EditorUtility.DisplayDialog("Error", "Cannot Create Keyframe because the number of variables does not match keyframe: " + (i+1), "OK");
									return;
								}
								if (saveParametersWithoutRange && keyframeDescriptions[i].hasParametersWithoutRange == false)
								{
									EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are saving Parameters without a range however keyframe " + (i+1) + " does " +
									"not save these variables. To fix this uncheck \"Save Output Parameters\" on this frame and try again or check \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
									return;
								}
								if (!saveParametersWithoutRange && keyframeDescriptions[i].hasParametersWithoutRange == true)
								{
									EditorUtility.DisplayDialog("Error", "Could not save keyframe because you are not saving Parameters without a range however keyframe " + i + "does " +
									"save these variables. To fix this check \"Save Output Parameters\" on this frame and try again or uncheck \"Save Output Parameters\" then select and overWrite on every other frame. ", "OK");
									return;
								}
							}
							keyframeDescriptions.Add(new ObjProperty());
							DebugStrings.Add("Created KeyFrame: " + keyframeDescriptions.Count);
							keyframeDescriptions[keyFrames] = new ObjProperty();
							AddProceduralVariablesToList(keyframeDescriptions[keyFrames]);
							keyframeDescriptions[keyFrames].PropertyMaterialName = substance.name;
							keyframeDescriptions[keyFrames].stdColor.Add(emissionInput);
							keyframeDescriptions[keyFrames].stdVector2.Add(MainTexOffset);
							keyFrameTimes.Add(keyframeDescriptions[keyFrames].animationTime);
							if (saveParametersWithoutRange)
								keyframeDescriptions[keyFrames].hasParametersWithoutRange = true;
							else
								keyframeDescriptions[keyFrames].hasParametersWithoutRange = false;
							keyFrames++;
						}
					}
					else
						EditorUtility.DisplayDialog("error", "Animation time cannot be 0 or less", "OK");
				}
					
				if (GUILayout.Button("Toggle Animation On/Off(needs Two transitions)",styleAnimationButton))
				{
					if (keyframeDescriptions[0].PropertyMaterialName == substance.name && keyframeDescriptions.Count >= 2) // if you have 2 transitions and the name of the selected substance matches the name on keyframe 1
					{
						if (substanceLerp)
						{
							substanceLerp = false;
							DebugStrings.Add("Turned off animation");
						}
						else if (!substanceLerp)
						{
							substanceLerp = true;
							currentKeyframeIndex = 0;
							DebugStrings.Add("Turned on animation");
						}
					}
					else if (keyframeDescriptions.Count >= 2)
					{
						DebugStrings.Add("Tried to animate object: " + currentSelection + " but the Transition Material name " + keyframeDescriptions[0].PropertyMaterialName + " did not match the current Procedural Material name: " + substance.name);
						var renameMaterialOption =	EditorUtility.DisplayDialog(
							"error", 
							"Transition Material name " + keyframeDescriptions[0].PropertyMaterialName + " does not match current Procedural Material name: " + substance.name + ". Would you like to rename " + keyframeDescriptions[0].PropertyMaterialName + " to " + substance.name + "?"
							+ " (Only do this if you are sure the materials are the same and only have different names)","Yes","No");
						if(renameMaterialOption)
						{
							DebugStrings.Add("Renamed Material: " + keyframeDescriptions[0].PropertyMaterialName + " To: " + substance.name);
							keyframeDescriptions[0].PropertyMaterialName = substance.name;

							for(int i = 0; i <= keyframeDescriptions.Count -1; i++)
								keyframeDescriptions[i].PropertyMaterialName = substance.name;
							substanceLerp = true;
						}
						else
							DebugStrings.Add("Did not rename or take any other action.");
					}
					else 
						EditorUtility.DisplayDialog("error", "You do not have two keyframes", "OK");
				}
				animationType = (AnimationType) EditorGUILayout.EnumPopup ("Animation Type:", animationType);
				GUILayout.Space(10);
				if (GUILayout.Button ("Save Object as prefab:")) 
				{
					GameObject prefab = PrefabUtility.CreatePrefab(EditorUtility.SaveFilePanelInProject ("", "","prefab", ""), (GameObject)currentSelection.gameObject, ReplacePrefabOptions.Default);
					prefab.AddComponent<PrefabProperties>();
					PrefabProperties prefabProperties = prefab.GetComponent<PrefabProperties>();
					prefabProperties.substance = substance ;
					Renderer prefabRend = prefab.GetComponent<Renderer>();
					prefabRend.material = substance;
					DebugStrings.Add("Created prefab: " + prefab.name);
					if (prefabRend.sharedMaterial)
						DebugStrings.Add("Prefab material: " + prefabRend.sharedMaterial.name);
					if (keyframeDescriptions.Count >=1)
					{
						for(int i = 0; i <= keyframeDescriptions.Count-1; i++)
						{
							if (animationTime > 0)
							{
								if (prefabProperties.keyFrameTimes.Count == 0)
								{
									prefabProperties.keyframeDescriptions.Add(new ObjProperty());
									prefabProperties.keyframeDescriptions[0] = keyframeDescriptions[0];
									prefabProperties.keyframeDescriptions[0].PropertyMaterialName = substance.name;
									prefabProperties.keyframeDescriptions[0].stdColor.Add(emissionInput);
									prefabProperties.keyframeDescriptions[0].stdVector2.Add(MainTexOffset);
									prefabProperties.keyFrames++;
									prefabProperties.keyFrameTimes.Add(keyframeDescriptions[0].animationTime);
									if (keyframeDescriptions[0].hasParametersWithoutRange)
										prefabProperties.keyframeDescriptions[0].hasParametersWithoutRange = true;
									else
										prefabProperties.keyframeDescriptions[0].hasParametersWithoutRange = false;
								}
								else // After one keyframe is created create the rest
								{
									prefabProperties.keyframeDescriptions.Add(new ObjProperty());
									prefabProperties.keyframeDescriptions[prefabProperties.keyFrames] = keyframeDescriptions[prefabProperties.keyFrames];
									prefabProperties.keyframeDescriptions[prefabProperties.keyFrames].PropertyMaterialName = substance.name;
									prefabProperties.keyframeDescriptions[prefabProperties.keyFrames].stdColor.Add(emissionInput);
									prefabProperties.keyframeDescriptions[prefabProperties.keyFrames].stdVector2.Add(MainTexOffset);
									prefabProperties.keyFrameTimes.Add(keyframeDescriptions[prefabProperties.keyFrames].animationTime);
									if (saveParametersWithoutRange)
										prefabProperties.keyframeDescriptions[prefabProperties.keyFrames].hasParametersWithoutRange = true;
									else
										prefabProperties.keyframeDescriptions[prefabProperties.keyFrames].hasParametersWithoutRange = false;
									prefabProperties.keyFrames++;
								}
							}
						}
					}
					DebugStrings.Add(prefab.name + " has " + prefabProperties.keyframeDescriptions.Count + " keyframes");
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}

				if (GUILayout.Button ("Write Debug Text File")) 
				{
					StreamWriter debugWrite;
					var path = EditorUtility.SaveFilePanel("Save Data", "","", "txt");
					FileInfo fInfo = new FileInfo(path);
					if (!fInfo.Exists)
						debugWrite = fInfo.CreateText();
					else
					{
						debugWrite = fInfo.CreateText();Debug.Log("Overwriting File");
					}
					debugWrite.WriteLine(System.DateTime.Now + " Debug:");
					for (int i = 0; i < DebugStrings.Count; i++)
					{
						if (DebugStrings.Count >1 && i > 1 && DebugStrings[i] != DebugStrings[i-1])
						debugWrite.WriteLine(DebugStrings[i]);
					}
					debugWrite.WriteLine("Created debug log: " + path);
					debugWrite.Close();
					AssetDatabase.Refresh();
				}
				saveParametersWithoutRange = EditorGUILayout.Toggle("Save Output Parameters ", saveParametersWithoutRange);
				rebuildSubstanceImmediately = EditorGUILayout.Toggle("Rebuild Substance Immediately", rebuildSubstanceImmediately);

				showKeyframes = EditorGUILayout.Foldout(showKeyframes,"Keyframes");
				if (showKeyframes && keyFrames >= 1)
				{
					for (int i = 0; i <= keyFrameTimes.Count -1; i++) 
					{
						GUILayout.BeginHorizontal();
						if (!substanceLerp)
						{
							if (GUILayout.Button("Remove") && !substanceLerp)
							{
								keyFrameTimes.RemoveAt(i);
								if(keyFrames >0)
									keyFrames--;
									Repaint();
								return; // Without returning i get a error when deleting the last keyframe
							}
						}
							if (GUILayout.Button("Select"))
							{
								SetProceduralVariablesFromList(keyframeDescriptions[i]);
								Repaint();
							}
							if (GUILayout.Button("Overwrite"))
							{
								if (keyFrameTimes.Count >= 1)
								{
									int lastKeyframeIndex = i-1;
									if (i >= 0)
									{
										keyframeDescriptions.Remove(keyframeDescriptions[i]);
										keyframeDescriptions.Insert(i, new ObjProperty());
										keyFrameTimes.RemoveAt(i);
										AddProceduralVariablesToList(keyframeDescriptions[i]);
										keyframeDescriptions[i].PropertyMaterialName = substance.name;
										keyframeDescriptions[i].stdColor.Clear();
										keyframeDescriptions[i].stdColor.Add(emissionInput);
										keyframeDescriptions[i].stdVector2.Clear();
										keyframeDescriptions[i].stdVector2.Add(MainTexOffset);
										keyFrameTimes.Add(keyframeDescriptions[i].animationTime);
										if (saveParametersWithoutRange)
											keyframeDescriptions[i].hasParametersWithoutRange = true;
										else
											keyframeDescriptions[i].hasParametersWithoutRange = false;
									DebugStrings.Add("OverWrote Keyframe: " + (i+1));
										Repaint();
									}
								}
							}
						GUILayout.Label("Keyframe: " + (i+1));
						Repaint();
						if ((i != keyFrameTimes.Count-1 || keyFrameTimes.Count == 1))
						{
							keyFrameTimes[i] = EditorGUILayout.FloatField(keyFrameTimes[i]);
							keyframeDescriptions[i].animationTime = keyFrameTimes[i] ;
						}
						GUILayout.EndHorizontal();
					}
				}
				if (rend)
					rend.sharedMaterial.SetTextureOffset("_MainTex",new Vector2 (MainTexOffset.x * Time.time,MainTexOffset.y * Time.time));
				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();
			}
		}
		else if (EditorApplication.isPlaying && selectedStartupMaterials.Count <1)
		{
			if(!rend && !currentSelection)
			{
				this.Close(); EditorUtility.DisplayDialog("Error", " No object selected.", "OK"); return;
			} 
			else if(!rend)
			{
				this.Close(); EditorUtility.DisplayDialog("Error", " No renderer attached to object.", "OK"); return;
			}
			if (!substance)
			{
				EditorUtility.DisplayDialog("Error", " No ProceduralMaterial attached to object.", "OK"); this.Close(); return;
			}
			EditorGUILayout.LabelField("Select a game object in the Hierarchy that has a Substance material first, then select this window.(Make sure that you are in play mode)");
			this.Close();
			EditorUtility.DisplayDialog("Error", " Select a game object in the Hierarchy that has a Substance material first, then select the SubstanceTween window.(Make sure that you are in play mode)", "OK");
		}
		else if (EditorApplication.isPlaying && !rend)
		{
			this.Close(); EditorUtility.DisplayDialog("error", " Object has no renderer attached", "OK");
		}
		else 
			EditorGUILayout.LabelField("Select a game object in the Hierarchy that has a Substance material first, then select this window.(Make sure that you are in play mode)");
		if (selectedStartupMaterials.Count >=1 && !substance)
		{
			EditorGUILayout.LabelField("No ProceduralMaterial attached to object, Select another object)");
			EditorGUILayout.EndScrollView(); EditorGUILayout.EndVertical();
		}
		Repaint();
		#if UNITY_5_3 
			if (substance && !substanceLerp)
				substance.RebuildTexturesImmediately();
		#endif
		#if UNITY_5_4_OR_NEWER // In Unity 5.4 and above I am able to use RebuildTextures() which is faster but it is not compatible with 5.3 
			if (substance && !substanceLerp)
	 			 substance.RebuildTextures();
		#endif
	}
		
	void Update ()
	{
		if(EditorApplication.isPlaying)
		{
			if (rend)
				rend.sharedMaterial.SetTextureOffset("_MainTex",new Vector2 (MainTexOffset.x * Time.time, MainTexOffset.y * Time.time));
			if (substance && substanceLerp && currentSelection == Selection.activeGameObject)
			{ 
				float currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
				if (animationType == AnimationType.BackAndForth && animateBackwards && currentKeyframeIndex <= keyFrameTimes.Count-1 && currentKeyframeIndex > 0)
				currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex-1];
				float lerp = 5;
				if (animationType == AnimationType.Loop) 
				{
					currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
					currentAnimationTime += Time.deltaTime;
					if (keyFrameTimes.Count > 2 && currentAnimationTime > currentKeyframeAnimationTime && currentKeyframeIndex <= keyFrameTimes.Count) 
					{
						currentAnimationTime = 0;
						currentKeyframeIndex++;
					}
					else if(keyFrameTimes.Count > 2 && currentKeyframeIndex >= keyFrameTimes.Count-1)
					{
						currentAnimationTime = 0;
						currentKeyframeIndex = 0;
					}
					else if (keyFrameTimes.Count == 2 && currentAnimationTime > currentKeyframeAnimationTime)
					{
						currentAnimationTime = 0;
						currentKeyframeIndex = 0;
					}
					if (animationTime > 0)
						lerp = currentAnimationTime / currentKeyframeAnimationTime;
				}

				else if (animationType == AnimationType.BackAndForth) 
				{
					if (!animateBackwards) 
						currentAnimationTime += Time.deltaTime;
					else if (animateBackwards)
						currentAnimationTime -= Time.deltaTime;
					if (keyFrameTimes.Count > 2 && !animateBackwards && currentAnimationTime > currentKeyframeAnimationTime && currentKeyframeIndex < keyFrameTimes.Count -1) // reach next keyframe when going forwards
					{
						currentKeyframeIndex++;
						currentAnimationTime = 0;
					}
					else if(keyFrameTimes.Count > 2 && !animateBackwards && currentKeyframeIndex >= keyFrameTimes.Count-1) // if you reach the last keyframe when going forwards go backwards.
					{
						animateBackwards = true;
						currentAnimationTime = 0;
					}
					else if (keyFrameTimes.Count > 2 && animateBackwards && currentAnimationTime <= 0 && currentKeyframeIndex <= keyFrameTimes.Count -1 && currentKeyframeIndex >0) // reach next keyframe when going backwards
					{
						currentAnimationTime = currentKeyframeAnimationTime ;
						currentKeyframeIndex--;
					}
					else if (keyFrameTimes.Count == 2 && currentAnimationTime > currentKeyframeAnimationTime)
					{
						animateBackwards = true;
						currentAnimationTime = currentKeyframeAnimationTime;
					}
					if (animateBackwards && currentKeyframeIndex == 0 && currentAnimationTime < 0) // if you reach the last keyframe when going backwards go forwards.
						animateBackwards = false;
					if (animationTime > 0)
					{
						if (!animateBackwards)
							lerp = currentAnimationTime / currentKeyframeAnimationTime;
						else if (keyFrameTimes.Count > 2 && animateBackwards && currentAnimationTime != currentKeyframeAnimationTime)
							lerp = Mathf.InverseLerp(0, 1,currentAnimationTime / keyFrameTimes[currentKeyframeIndex]);
						else if (keyFrameTimes.Count == 2 && animateBackwards && currentAnimationTime != currentKeyframeAnimationTime)
							lerp = currentAnimationTime / currentKeyframeAnimationTime;
					}
				}

				if (objProperties != null) 
					for(int i = 0; i < objProperties.Length; i++)
					{
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;
						if(propType == ProceduralPropertyType.Float)
						{
							for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++)
							{
								if (keyframeDescriptions[currentKeyframeIndex].myKeys[j] == objProperty.name)
								{
									if(currentKeyframeIndex + 1 <= keyframeDescriptions.Count -1)
									{
										float curLerp1Float = (float) float.Parse(keyframeDescriptions[currentKeyframeIndex].myValues[j]);
										float curLerp2Float = (float) float.Parse(keyframeDescriptions[currentKeyframeIndex+1].myValues[j]);
										substance.SetProceduralFloat(objProperty.name, Mathf.Lerp(curLerp1Float,curLerp2Float, lerp));
									}
								}
							}
						}
						else if (propType == ProceduralPropertyType.Color3)
						{ 
							for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++)
							{
								if (keyframeDescriptions[currentKeyframeIndex].myKeys[j] == objProperty.name)
								{
									if(currentKeyframeIndex + 1 <= keyframeDescriptions.Count -1)
									{
										Color curLerp1Color = new Color(0,0,0), curLerp2Color = new Color(0,0,0);
										ColorUtility.TryParseHtmlString(keyframeDescriptions[currentKeyframeIndex].myValues[j],out curLerp1Color);
										ColorUtility.TryParseHtmlString(keyframeDescriptions[currentKeyframeIndex+1].myValues[j],out curLerp2Color);
										substance.SetProceduralColor(objProperties[j].name, Color.Lerp(curLerp1Color, curLerp2Color, lerp));
									}
								}
							}
						}
						else if (propType == ProceduralPropertyType.Color4)
						{ 
							for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++)
							{
								if (keyframeDescriptions[currentKeyframeIndex].myKeys[j] == objProperty.name)
								{
									if(currentKeyframeIndex + 1 <= keyframeDescriptions.Count -1)
									{
										Color curLerp1Color = new Color(0,0,0), curLerp2Color = new Color(0,0,0);
										ColorUtility.TryParseHtmlString(keyframeDescriptions[currentKeyframeIndex].myValues[j],out curLerp1Color);
										ColorUtility.TryParseHtmlString(keyframeDescriptions[currentKeyframeIndex+1].myValues[j],out curLerp2Color);
										substance.SetProceduralColor(objProperties[i].name, Color.Lerp(curLerp1Color, curLerp2Color, lerp));
									}
								}
							}
						}
						else if (propType == ProceduralPropertyType.Vector2 ||propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
						{
							Vector4 curLerp1Vector = Vector4.zero, curLerp2Vector = Vector4.zero;
							if (propType == ProceduralPropertyType.Vector4)
							{
								for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++)
								{
									if (keyframeDescriptions[currentKeyframeIndex].myKeys[j] == objProperty.name)
									{
										if(currentKeyframeIndex + 1 <= keyframeDescriptions.Count -1)
										{
											curLerp1Vector = StringToVector(keyframeDescriptions[currentKeyframeIndex].myValues[j],4);
											curLerp2Vector = StringToVector(keyframeDescriptions[currentKeyframeIndex + 1].myValues[j],4);
											substance.SetProceduralVector(objProperties[j].name, Vector4.Lerp(curLerp1Vector, curLerp2Vector, lerp));
										}
									}
								}
							}
							else if (propType == ProceduralPropertyType.Vector3)
							{
								for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++)
								{
									if (keyframeDescriptions[currentKeyframeIndex].myKeys[j] == objProperty.name)
									{
										if(currentKeyframeIndex + 1 <= keyframeDescriptions.Count -1)
										{
											curLerp1Vector = StringToVector(keyframeDescriptions[currentKeyframeIndex].myValues[j],3);
											curLerp2Vector = StringToVector(keyframeDescriptions[currentKeyframeIndex+1].myValues[j],3);
											substance.SetProceduralVector(objProperties[j].name, Vector3.Lerp(curLerp1Vector, curLerp2Vector, lerp));
										}
									}
								}
							}
							else if (propType == ProceduralPropertyType.Vector2)
							{
								for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++)
								{
									if (keyframeDescriptions[currentKeyframeIndex].myKeys[j] == objProperty.name)
									{
										if(currentKeyframeIndex + 1 <= keyframeDescriptions.Count -1)
										{
											curLerp1Vector = StringToVector(keyframeDescriptions[currentKeyframeIndex].myValues[j],2);
											curLerp2Vector = StringToVector(keyframeDescriptions[currentKeyframeIndex+1].myValues[j],2);
											substance.SetProceduralVector(objProperties[j].name, Vector2.Lerp(curLerp1Vector, curLerp2Vector, lerp));
										}
									}
								}
							}
						}
					}
				if (rend.sharedMaterial.HasProperty("_EmissionColor") && currentKeyframeIndex + 1 < keyframeDescriptions.Count -1)
				{
					Color curlerp1Emission = keyframeDescriptions[currentKeyframeIndex].stdColor[0];
					Color curlerp2Emission = keyframeDescriptions[currentKeyframeIndex+1].stdColor[0];
					Color emissionInput = rend.sharedMaterial.GetColor("_EmissionColor");
						rend.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(curlerp1Emission,curlerp2Emission,lerp));
				}
				if (rebuildSubstanceImmediately)
					substance.RebuildTexturesImmediately();
				else
					substance.RebuildTextures();
			}
		}
		else 
		{
			this.Close(); EditorUtility.DisplayDialog("error", " This tool only works in play mode", "OK");
		}
		if (EditorApplication.isPaused)
			gameIsPaused = true;
		else
			gameIsPaused = false;
		if (gameIsPaused)
			substanceLerp = false;
	}

	void AddProceduralVariablesToList(ObjProperty materialVariables) // Adds substance values to a list 
	{
		DebugStrings.Add("----------------------------------");
		for(int i = 0 ; i < objProperties.Length; i++)//loop through properties and make them the default properties for this object
		{
			ProceduralPropertyDescription objProperty = objProperties[i];
			ProceduralPropertyType propType = objProperties[i].type;
			materialVariables.PropertyName.Add(objProperty.name);
			if (propType == ProceduralPropertyType.Float && (objProperty.hasRange || (saveParametersWithoutRange && !objProperty.hasRange)))
			{
				float propFloat = substance.GetProceduralFloat(objProperty.name);
				materialVariables.PropertyFloat.Add(propFloat);
				materialVariables.myKeys.Add(objProperty.name);
				materialVariables.myValues.Add(propFloat.ToString());
				DebugStrings.Add(i + " " + objProperty.name + ": " + propFloat.ToString());
			}
			if(propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
			{
				Color propColor = substance.GetProceduralColor(objProperty.name);
				materialVariables.PropertyColor.Add(propColor);
				materialVariables.myKeys.Add(objProperty.name);
				materialVariables.myValues.Add("#" + ColorUtility.ToHtmlStringRGBA(propColor));
				DebugStrings.Add(i + " " + objProperty.name + ": #" + ColorUtility.ToHtmlStringRGBA(propColor));
			}
			if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (objProperty.hasRange || (saveParametersWithoutRange && !objProperty.hasRange)))
			{
				if (propType == ProceduralPropertyType.Vector4)
				{
					Vector4 propVector4 = substance.GetProceduralVector(objProperty.name);
					materialVariables.PropertyVector4.Add(propVector4);
					materialVariables.myKeys.Add(objProperty.name);
					materialVariables.myValues.Add(propVector4.ToString());
					DebugStrings.Add(i + " " + objProperty.name + ": " + propVector4.ToString());
				}
				else if (propType == ProceduralPropertyType.Vector3)
				{
					Vector3 propVector3 = substance.GetProceduralVector(objProperty.name);
					materialVariables.PropertyVector3.Add(propVector3);
					materialVariables.myKeys.Add(objProperty.name);
					materialVariables.myValues.Add(propVector3.ToString());
					DebugStrings.Add(i + " " + objProperty.name + ": " + propVector3.ToString());
				}
				else if (propType == ProceduralPropertyType.Vector2)
				{
					Vector2 propVector2 = substance.GetProceduralVector(objProperty.name);
					materialVariables.PropertyVector2.Add(propVector2);
					materialVariables.myKeys.Add(objProperty.name);
					materialVariables.myValues.Add(propVector2.ToString());
					DebugStrings.Add(i + " " + objProperty.name + ": " + propVector2.ToString());
				}
			}
		}
		materialVariables.animationTime = animationTime;
		DebugStrings.Add("Animation Time: " + animationTime + " Seconds");
		DebugStrings.Add("-----------------------------------");
	}

	void SetProceduralVariablesFromList(ObjProperty propertyList) // Sets current substance parameters from a List
	{
		for (int i = 0; i < objProperties.Length; i++)
		{ 
			ProceduralPropertyDescription objProperty = objProperties[i];
			ProceduralPropertyType propType = objProperties[i].type;
			if (propType == ProceduralPropertyType.Float && (objProperty.hasRange || (saveParametersWithoutRange && !objProperty.hasRange) || resettingValuesToDefault))
			{
				for(int j =0; j < propertyList.myKeys.Count(); j++)
				{
					if (propertyList.myKeys[j] == objProperty.name)
					{
						if (propertyList.myKeys[j] == objProperty.name)
							substance.SetProceduralFloat(objProperty.name,float.Parse(propertyList.myValues[j]));
					}
				}
			}
			else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
			{ 
				for(int j =0; j < propertyList.myKeys.Count(); j++)
				{
					if (propertyList.myKeys[j] == objProperty.name)
					{
						Color curColor;
						ColorUtility.TryParseHtmlString(propertyList.myValues[j],out curColor);
						substance.SetProceduralColor(objProperty.name,curColor);
					}
				}
			}
			else if ((propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4) && (objProperty.hasRange || (saveParametersWithoutRange && !objProperty.hasRange) || resettingValuesToDefault))
			{
				if (propType == ProceduralPropertyType.Vector4)
				{ 
					for(int j =0; j < propertyList.myKeys.Count(); j++)
					{
						if (propertyList.myKeys[j] == objProperty.name)
						{
							Vector4 curVector4 = StringToVector(propertyList.myValues[j],4);
							substance.SetProceduralVector(objProperty.name,curVector4);
						}
					}
				}
				else if (propType == ProceduralPropertyType.Vector3)
				{
					for(int j =0; j < propertyList.myKeys.Count(); j++)
					{
						if (propertyList.myKeys[j] == objProperty.name)
						{
							Vector3 curVector3 = StringToVector(propertyList.myValues[j],3);
							substance.SetProceduralVector(objProperty.name,curVector3);
						}
					}
				}
				else if (propType == ProceduralPropertyType.Vector2)
				{
					for(int j =0; j < propertyList.myKeys.Count(); j++)
					{
						if (propertyList.myKeys[j] == objProperty.name)
						{
							Vector2 curVector2 = StringToVector(propertyList.myValues[j],2);
							substance.SetProceduralVector(objProperty.name,curVector2);
						}
					}
				}
			}
		}
	}

	public static Vector4 StringToVector(string startVector, int VectorAmount)
	{
		if (startVector.StartsWith ("(") && startVector.EndsWith (")")) // Remove "()" from the string 
			startVector = startVector.Substring(1, startVector.Length-2);
		string[] sArray = startVector.Split(',');
		if (VectorAmount == 2)
		{
			Vector2 result = new Vector2(float.Parse(sArray[0]),float.Parse(sArray[1]));
			return result;
		}
		else if (VectorAmount == 3)
		{
			Vector3 result = new Vector3(float.Parse(sArray[0]),float.Parse(sArray[1]),float.Parse(sArray[2]));
			return result;
		}
		else if (VectorAmount == 4)
		{
			Vector4 result = new Vector4(float.Parse(sArray[0]),float.Parse(sArray[1]),float.Parse(sArray[2]),float.Parse(sArray[3]));
			return result;
		}
		else
			return new Vector4 (0,0,0,0);
	}
}