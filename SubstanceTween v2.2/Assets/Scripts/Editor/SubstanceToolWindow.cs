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

public class SubstanceToolWindow :  EditorWindow {
	//SubstanceTween Ver 2.2 - 11/29/2016 
	//Written by: Chris Dougherty
	//https://www.linkedin.com/in/archarchaic
	//chris.ll.dougherty@gmail.com
	private GameObject currentSelection;

	public bool substanceLerp;

	public Vector2 offset;
	public float lerpTime = 5;
	public Renderer rend;

	public ProceduralMaterial substance ;
	public ProceduralMaterial defaultSubstance;
	private ProceduralPropertyDescription[] objProperties;
	private ProceduralPropertyDescription[] defaultObjProperties;
	private ProceduralPropertyDescription[] newObjProperties;

	public Vector2 scrollVal;
	public Vector2 textureScrollSpeed = new Vector2(0f, 0f);

	public List<float> defaultMatFloatValues = new List<float>();
	public List<Vector2> defaultMatVector2Values = new List<Vector2>();
	public List<Vector3> defaultMatVector3Values = new List<Vector3>();
	public List<Vector4> defaultMatVector4Values = new List<Vector4>();
	public List<Color> defaultMatColorValues = new List<Color>();
	[SerializeField]

	public Vector2 defaultStdOffset;
	public Color defaultStdEmissionColor;

	public List<float> customXmlFloatValues = new List<float>();
	public int tempLerpColorIndex;
	public int tempLerpVector2Index;
	public int tempLerpVector3Index;
	public int tempLerpVector4Index;

	public int tempFloatIndex;
	public int tempColorIndex;
	public int tempVector2Index;
	public int tempVector3Index;
	public int tempVector4Index;

	public int defaultSubstanceIndex= 0;

	public Color emissionInput;

	[SerializeField]
	int numOfColProps =0;

	public String xmlFieldFolderWritePath = "";
	public String xmlFieldFileWritePath = "";
	public String xmlFieldReadPath = "";
	public List<ProceduralMaterial> selectedStartupMaterials = new List<ProceduralMaterial>();
	public List<GameObject> selectedStartupGameObjects = new List<GameObject>();
	public List<ObjProperty> defaultSubstanceObjProperties = new List<ObjProperty>();

	public ObjProperty lerp1Description;
	public ObjProperty lerp2Description;
	public ObjProperty Description;

	private bool defaultPropNameFoldout;
	private bool defaultPropColorFoldout;
	public bool UpdatingStartVariables = true; 
	public bool saveDefaultSubstanceVars = true;
	public bool restartTool = false;


	public List<string> DebugStrings = new List<string>();

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

	//void OnInspectorUpdate(){}//void OnDestroy(){}//private void OnLostFocus(){}

	private void OnFocus()
	{
		if (currentSelection  == null && Selection.activeGameObject)
		{
			currentSelection = Selection.activeGameObject;//currentSelection = Selection.objects[0] as GameObject;
			Debug.Log(currentSelection);

			if (currentSelection)
			rend = currentSelection.GetComponent<Renderer>();
			
			if( rend && UpdatingStartVariables)
			{
				DebugStrings.Add("Opened Tool");
				substance = rend.sharedMaterial as ProceduralMaterial;
				UpdatingStartVariables = false;
				selectedStartupMaterials.Add(substance);
				selectedStartupGameObjects.Add(currentSelection);
				DebugStrings.Add ("First object selected: " + currentSelection + " Selected objects material  name: " + rend.sharedMaterial.name);
			}
			if (substance)
			substance.RebuildTextures();
			Repaint();
		}
	}
		
	void OnSelectionChange()
	{
		// if selectedobj.name != tmpobjectname
		if (!restartTool)
		{
			restartTool = true;
			restartTool = false;

			currentSelection = Selection.activeGameObject;//currentSelection = Selection.objects[0] as GameObject;
			rend = currentSelection.GetComponent<Renderer>();

			if (rend)
			{
				DebugStrings.Add("Selected: " + currentSelection  + "Selected objects material  name: " + rend.sharedMaterial.name); 
				Debug.Log("Selected: " + currentSelection  + "Selected objects material  name: " + rend.sharedMaterial.name); 
			}

			if (selectedStartupMaterials.Count >0)
			{
				bool checkMaterial = true;

				if (rend)
				substance = rend.sharedMaterial as ProceduralMaterial;

				for(int i = 0; i < selectedStartupMaterials.Count; i++)
				{
					if (currentSelection.name  == selectedStartupGameObjects[i].name)
					{
						substance = selectedStartupMaterials[i];
						checkMaterial = false;
						if (substance)
						Debug.Log(substance.name);
						//Debug.Log("1/Selected: " + currentSelection  + "Selected objects material  name: " + rend.sharedMaterial.name); 
						DebugStrings.Add (currentSelection.name + " = " + selectedStartupMaterials[i].name );
					}
					DebugStrings.Add("Material " + i + ": " + selectedStartupMaterials[i]);
				}

				if (checkMaterial)
				{
					if (rend)
						substance = rend.sharedMaterial as ProceduralMaterial;
					selectedStartupMaterials.Add(substance );
					selectedStartupGameObjects.Add(currentSelection);
					//Debug.Log("2/Selected: " + currentSelection  + "Selected objects material  name: " + rend.sharedMaterial.name); 

					defaultSubstanceObjProperties.Add(new ObjProperty());
					defaultSubstance = rend.sharedMaterial as ProceduralMaterial;
					defaultSubstance.CopyPropertiesFromMaterial(substance);
					defaultObjProperties = substance.GetProceduralPropertyDescriptions();
					objProperties = substance.GetProceduralPropertyDescriptions();
					tempFloatIndex = 0;
					tempColorIndex = 0;
					tempVector2Index = 0;
					tempVector3Index = 0;
					tempVector4Index = 0;
					defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName = defaultSubstance.name;

					for(int i = 0 ; i < objProperties.Length; i++)//loop through properties.
					{
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;

						if (propType == ProceduralPropertyType.Float)
						{
							float propFloat = substance.GetProceduralFloat(objProperty.name);
							defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyFloat.Add(propFloat);
						}

						if(propType == ProceduralPropertyType.Color3 ||  propType == ProceduralPropertyType.Color4)
						{
							Color propColor =  substance.GetProceduralColor(objProperty.name);
							defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyColor.Add(propColor);
						}

						if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
						{
							if (propType == ProceduralPropertyType.Vector4)
							{
								Vector4 propVector4 = substance.GetProceduralVector(objProperty.name);
								defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyVector4.Add(propVector4);
							}

							else if  (propType == ProceduralPropertyType.Vector3)
							{
								Vector3 propVector3 = substance.GetProceduralVector(objProperty.name);
								defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyVector3.Add(propVector3);
							}

							else if (propType == ProceduralPropertyType.Vector2)
							{
								Vector2 propVector2 = substance.GetProceduralVector(objProperty.name);
								defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyVector2.Add(propVector2);
							}
						}
					}

					defaultSubstanceObjProperties[defaultSubstanceIndex].stdVector2.Add(offset);
					defaultSubstanceObjProperties[defaultSubstanceIndex].stdColor.Add(emissionInput);

					DebugStrings.Add("Default substance material "  + defaultSubstanceIndex + ": " +  defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName );
					defaultSubstanceIndex++;
				}
			}

			if (lerp1Description != null   && (lerp1Description.PropertyName.Count > 0 && lerp2Description == null))
			{
				lerp1Description.PropertyName.Clear();
				lerp1Description.PropertyFloat.Clear();
				lerp1Description.PropertyVector2.Clear();
				lerp1Description.PropertyVector3.Clear();
				lerp1Description.PropertyVector4.Clear();
			}
		}
	}
		
	public void OnGUI()
	{
		if( EditorApplication.isPlaying &&  currentSelection != null && !restartTool && rend )
		{
			EditorGUILayout.BeginVertical();
			scrollVal = GUILayout.BeginScrollView(scrollVal);
			EditorGUILayout.LabelField("Currently selected gameobject:");
			EditorGUILayout.LabelField (currentSelection.name);
			currentSelection.transform.position =  EditorGUILayout.Vector3Field("At Position", currentSelection.transform.position);

			ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;

			if ( substance && saveDefaultSubstanceVars)
			{
				defaultSubstanceObjProperties.Add(new ObjProperty());
				defaultSubstance = rend.sharedMaterial as ProceduralMaterial;
				defaultSubstance.CopyPropertiesFromMaterial(substance);
				defaultObjProperties = substance.GetProceduralPropertyDescriptions();
				objProperties = substance.GetProceduralPropertyDescriptions();
				tempFloatIndex = 0;
				tempColorIndex = 0;
				tempVector2Index = 0;
				tempVector3Index = 0;
				tempVector4Index = 0;

				defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName = defaultSubstance.name;
				DebugStrings.Add("Default substance material "  + defaultSubstanceIndex + ": " + /* defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyMaterialName*/ selectedStartupMaterials[0] );

				for(int i = 0 ; i < objProperties.Length; i++)//loop through properties.
				{
					ProceduralPropertyDescription objProperty = objProperties[i];
					ProceduralPropertyType propType = objProperties[i].type;

					if (propType == ProceduralPropertyType.Float)
					{
						float propFloat = substance.GetProceduralFloat(objProperty.name);
						defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyFloat.Add(propFloat);
					}

					if(propType == ProceduralPropertyType.Color3 ||  propType == ProceduralPropertyType.Color4)
					{
						Color propColor =  substance.GetProceduralColor(objProperty.name);
						defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyColor.Add(propColor);
					}

					if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
					{
						if (propType == ProceduralPropertyType.Vector4)
						{
							Vector4 propVector4 = substance.GetProceduralVector(objProperty.name);
							defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyVector4.Add(propVector4);
						}

						else if  (propType == ProceduralPropertyType.Vector3)
						{
							Vector3 propVector3 = substance.GetProceduralVector(objProperty.name);
							defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyVector3.Add(propVector3);
						}

						else if (propType == ProceduralPropertyType.Vector2)
						{
							Vector2 propVector2 = substance.GetProceduralVector(objProperty.name);
							defaultSubstanceObjProperties[defaultSubstanceIndex].PropertyVector2.Add(propVector2);
						}
					}
					defaultSubstanceObjProperties[defaultSubstanceIndex].stdVector2.Add(offset);
					defaultSubstanceObjProperties[defaultSubstanceIndex].stdColor.Add(emissionInput);
				}

				defaultSubstanceIndex++;
				saveDefaultSubstanceVars = false;
			}

			if (substance)
			{
				objProperties = substance.GetProceduralPropertyDescriptions();

				for(int i = 0 ; i < objProperties.Length; i++)//loop through properties.//create sliders
				{
					ProceduralPropertyDescription objProperty = objProperties[i];
					ProceduralPropertyType propType = objProperties[i].type;

					if(propType == ProceduralPropertyType.Float)
					{
						if (objProperty.hasRange)
						{
							EditorGUI.BeginChangeCheck();
							GUILayout.BeginHorizontal();
							GUILayout.Label(objProperty.name);
							float propFloat = substance.GetProceduralFloat(objProperty.name);
							float oldfloat =  propFloat;
							string propFloatTextField =  propFloat.ToString();
							propFloat = EditorGUILayout.Slider( propFloat, objProperty.minimum, objProperty.maximum);
							GUILayout.TextField( propFloatTextField, 5 ,  GUILayout.Width(200));//float propFloat = float.Parse(float propFloatTextField);//float propFloat = float.Parse(GUILayout.TextField(float propFloatTextField, 999 ),System.Globalization.NumberStyles.Any  );
							substance.SetProceduralFloat(objProperty.name,  propFloat);
							GUILayout.EndHorizontal();

							if ( EditorGUI.EndChangeCheck()  )
							{DebugStrings.Add( objProperty.name + " Was " + oldfloat +  " is now: " + propFloat );}
						}
					}

					else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4) 
					{
						EditorGUI.BeginChangeCheck();
						GUILayout.Label(objProperty.name);
						int colorComponentAmount = ((propType == ProceduralPropertyType.Color3) ? 3 : 4);
						Color colorInput = substance.GetProceduralColor(objProperty.name);
						Color oldColorInput = colorInput;
					
						colorInput = EditorGUILayout.ColorField(colorInput);

						if (colorInput != oldColorInput)
							substance.SetProceduralColor(objProperty.name, colorInput);

						if ( EditorGUI.EndChangeCheck() )
						{DebugStrings.Add( objProperty.name + " Was " + oldColorInput +  " is now: " + colorInput );}
					}

					else if(propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
					{
						if (objProperty.hasRange)
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

							if ( EditorGUI.EndChangeCheck()  )
							{DebugStrings.Add( objProperty.name + " Was " + oldInputVector +  " is now: " + inputVector );}
						}
					}
				}

				if ( rend && rend.sharedMaterial.HasProperty("_MainTex"))
				{
					EditorGUI.BeginChangeCheck();
					GUILayout.Label("_MainTex");
					if (offset != null)
					{
						Vector2 oldOffset = offset;
						offset.x = EditorGUILayout.Slider(offset.x,-10f,10.0f); 
						offset.y =  EditorGUILayout.Slider(offset.y,-10f,10.0f); 
						if ( EditorGUI.EndChangeCheck())
						{DebugStrings.Add( "_MainTex" + " Was " + oldOffset +  " is now: " + offset );}
					}
				}

				if (rend && rend.sharedMaterial.HasProperty("_EmissionColor"))
				{
					EditorGUI.BeginChangeCheck();
					GUILayout.Label("Emission:");
					emissionInput = EditorGUILayout.ColorField(emissionInput);
					Color oldEmissionInput = emissionInput;
					rend.sharedMaterial.SetColor("_EmissionColor", emissionInput);
					if ( EditorGUI.EndChangeCheck())
					{DebugStrings.Add( "_EmissionColor" + " Was " + oldEmissionInput +  " is now: " + emissionInput );}

				}
				EditorGUILayout.Space();

				Rect r = EditorGUILayout.BeginHorizontal ("Button"); //if (GUI.Button(r,"Write XML values to file",buttonStyle))
				if (GUILayout.Button("Write XMl"))
				{
					GUILayout.Space(9);
					GUILayout.Label("Write XMl label");
					customXmlFloatValues.RemoveRange(0, customXmlFloatValues.Count);
					ArrayList customXmlValues = new ArrayList(); //Needs to be an array list because it accepts more than one type of object? (Floats,Vec2,Vec3,Vec4, etc)
					StreamWriter writer;
					var path = EditorUtility.SaveFilePanel("Save Data", "","", "xml");
					FileInfo fInfo = new FileInfo(path);
					AssetDatabase.Refresh();
					if (!fInfo.Exists )
						writer = fInfo.CreateText();
					else
					{
						Debug.Log("Overwriting File");
						writer = fInfo.CreateText();
					}

					Description = new ObjProperty();
					XmlSerializer serializer = new XmlSerializer(typeof(ObjProperty));

					for (int i = 0; i < objProperties.Length; i++)
					{   
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;
						float propFloat = substance.GetProceduralFloat(objProperty.name);
						Color propColor =  substance.GetProceduralColor(objProperty.name);
					
						customXmlFloatValues.Add( propFloat);
						Description.PropertyName.Add(objProperty.name);
						Description.PropertyFloat.Add( propFloat);
						Description.PropertyName[i] =objProperty.name ;
						if (propType == ProceduralPropertyType.Float)
							Description.PropertyFloat[i] =  propFloat;

						if(propType == ProceduralPropertyType.Color3 ||  propType == ProceduralPropertyType.Color4)
						{
							Description.PropertyColor.Add(propColor);
						}

						if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
						{
							if (propType == ProceduralPropertyType.Vector4)
							{
								Vector4 propVector4 = substance.GetProceduralVector(objProperty.name);
								Description.PropertyVector4.Add(propVector4);
							}

							else if  (propType == ProceduralPropertyType.Vector3)
							{
								Vector3 propVector3 = substance.GetProceduralVector(objProperty.name);
								Description.PropertyVector3.Add(propVector3);
							}

							else if (objProperty.hasRange && propType == ProceduralPropertyType.Vector2)
							{
								Vector2 propVector2 = substance.GetProceduralVector(objProperty.name);
								Description.PropertyVector2.Add(propVector2);
							}
						}
						customXmlValues.Add(Description);
					}

					Description.stdColor.Add(emissionInput);
					Description.stdVector2.Add(offset);

					// Sort description values here? 

					serializer.Serialize(writer, Description);
					writer.Close();
					Debug.Log(fInfo +  ": File written.");

					DebugStrings.Add("-----------------------------------");
					DebugStrings.Add( "Wrote XML file " + " to: " + fInfo + ", File has: "  );
					DebugStrings.Add(Description.PropertyName.Count + " Total Properties (including uneditable properties)");
					DebugStrings.Add(Description.PropertyFloat.Count + " Float Properties (includes r value for each color for some reason )" );
					DebugStrings.Add(Description.PropertyColor.Count + " Color Properties");
					DebugStrings.Add(Description.PropertyVector4.Count + " Vector4 Properties");
					DebugStrings.Add(Description.PropertyVector3.Count + " Vector3 Properties");
					DebugStrings.Add(Description.PropertyVector2.Count + " Vector2 Properties");
					if (emissionInput != null)
						DebugStrings.Add("_EmissionColor = " + emissionInput);
					if (offset != null)
						DebugStrings.Add("_MainTex = " + offset);
					DebugStrings.Add("-----------------------------------");
				}

				EditorGUILayout.Space();EditorGUILayout.Space();

				if (GUILayout.Button("Read XML"))
				{
					//xmlFieldReadPath = EditorUtility.OpenFilePanel("","","xml");
					ArrayList customXmlValues = new ArrayList();
					var serializer = new XmlSerializer(typeof(ObjProperty));
					var stream = new FileStream(EditorUtility.OpenFilePanel("","","xml"), FileMode.Open);
					var container = serializer.Deserialize(stream) as ObjProperty;
					numOfColProps = 0;
					tempVector2Index = 0;
					tempVector3Index = 0;
					tempVector4Index = 0;

					for (int i = 0; i < objProperties.Length; i++)
					{
						Debug.Log("Object : " + this.name + ". Property " + objProperties[i].name.ToString() + " Is of type " + objProperties[i].type.ToString() + " and the max Value for this property is: " + objProperties[i].maximum);
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;
						if (propType == ProceduralPropertyType.Float)
						{
							float curFloat = (float)container.PropertyFloat[i];
							substance.SetProceduralFloat(objProperty.name, curFloat);
						}

						else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
						{
							int totalNumOfColProps = container.PropertyColor.Count;
							if (numOfColProps <= totalNumOfColProps)
							{
								Color colorInput = substance.GetProceduralColor(objProperty.name);
								Color propColor = container.PropertyColor[numOfColProps];
								Debug.Log(propColor);                  
								substance.SetProceduralColor(objProperty.name, propColor);
								numOfColProps++;
							}
						}

						else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
						{
							if (objProperty.hasRange && propType == ProceduralPropertyType.Vector2)
							{
								Vector2 curVector2 = new Vector2(0,0);
								curVector2 = container.PropertyVector2[tempVector2Index];
								Debug.Log(curVector2);
								substance.SetProceduralVector(objProperty.name,curVector2);
								tempVector2Index++;
							}

							if (propType == ProceduralPropertyType.Vector3)
							{
								Vector3 curVector3 = new Vector3(0,0,0);
								curVector3 = container.PropertyVector3[tempVector3Index];
								Debug.Log(curVector3);
								substance.SetProceduralVector(objProperty.name,curVector3);
								tempVector3Index++;
							}
							if (propType == ProceduralPropertyType.Vector4)
							{
								Vector4 curVector4 = new Vector4(0,0,0,0);
								curVector4 = container.PropertyVector4[tempVector4Index];
								substance.SetProceduralVector(objProperty.name,curVector4);
								tempVector4Index++;
							}
						}//Debug.Log(i + " out of " + objProperties.Length);
					}

					offset = container.stdVector2[0];
					Color stdEmissionColor = new Color(0,0,0,0);
					stdEmissionColor = container.stdColor[0];

					if (rend.sharedMaterial.HasProperty("_EmissionColor"))
					{
						Debug.Log("tst");
						emissionInput = stdEmissionColor;
						rend.sharedMaterial.SetColor("_EmissionColor", stdEmissionColor);
					}
					stream.Close();

					DebugStrings.Add("-----------------------------------");
					DebugStrings.Add( "Read XML file " + " from: " + stream + ", File has: "  );
					DebugStrings.Add(container.PropertyName.Count + " Total Properties (including uneditable properties)");
					DebugStrings.Add(container.PropertyFloat.Count + " Float Properties (includes r value for each color for some reason )");
					DebugStrings.Add(container.PropertyColor.Count + " Color Properties  ");
					DebugStrings.Add(container.PropertyVector4.Count + " Vector4 Properties");
					DebugStrings.Add(container.PropertyVector3.Count + " Vector3 Properties");
					DebugStrings.Add(container.PropertyVector2.Count + " Vector2 Properties");
					if (emissionInput != null)
						DebugStrings.Add("_EmissionColor = " + emissionInput);
					if (offset != null)
						DebugStrings.Add("_MainTex = " + offset);
					DebugStrings.Add("-----------------------------------");
				}

				EditorGUILayout.EndHorizontal(); // ends button

				if (GUILayout.Button("Set All Procedural Values To The Minimum: "))
				{
					for (int i = 0; i < objProperties.Length; i++)
					{
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;
						if (propType == ProceduralPropertyType.Float)
						{
							Debug.Log(objProperties[i].name.ToString() + " Is of type " + objProperties[i].type.ToString() + " and equals " + objProperties[i].maximum);
							substance.SetProceduralFloat(objProperty.name, objProperties[i].minimum);
						}

						if (propType == ProceduralPropertyType.Vector2)
						{
							substance.SetProceduralVector(objProperty.name,new Vector2(objProperties[i].minimum,objProperties[i].minimum));
						}

						if (propType == ProceduralPropertyType.Vector3)
						{
							substance.SetProceduralVector(objProperty.name,new Vector3(objProperties[i].minimum,objProperties[i].minimum,objProperties[i].minimum));
						}

						if (propType == ProceduralPropertyType.Vector4)
						{
							substance.SetProceduralVector(objProperty.name,new Vector4(objProperties[i].minimum,objProperties[i].minimum,objProperties[i].minimum,objProperties[i].minimum));
						}
					}
					DebugStrings.Add("Set all properties to the minimum");
				}

				if (GUILayout.Button("Set All Procedural Values To The Max: "))
				{
					for (int i = 0; i < objProperties.Length; i++)
					{
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;
						if (propType == ProceduralPropertyType.Float)
						{
							substance.SetProceduralFloat(objProperty.name, objProperties[i].maximum);
						}
							
						if (propType == ProceduralPropertyType.Vector2)
						{
							//Vector2 curVector2 = substance.GetProceduralVector(objProperty.name);
							//substance.SetProceduralFloat(objProperty.name,objProperties[i].maximum);
							substance.SetProceduralVector(objProperty.name,new Vector2(objProperties[i].maximum,objProperties[i].maximum));
						}

						if (propType == ProceduralPropertyType.Vector3)
						{
							substance.SetProceduralVector(objProperty.name,new Vector3(objProperties[i].maximum,objProperties[i].maximum,objProperties[i].maximum));
						}

						if (propType == ProceduralPropertyType.Vector4)
						{
							substance.SetProceduralVector(objProperty.name,new Vector4(objProperties[i].maximum,objProperties[i].maximum,objProperties[i].maximum,objProperties[i].maximum));
						}
					}
					DebugStrings.Add("Set all properties to the maximum");
				}

				if (GUILayout.Button("Reset ALL Values To Default: "))
				{
					tempFloatIndex = 0;
					tempColorIndex = 0;
					tempVector2Index = 0;
					tempVector3Index = 0;
					tempVector4Index = 0;

					for (int i =  0; i < defaultSubstanceObjProperties.Count; i++)
					{
						//Debug.Log("compare " +  substance.name); Debug.Log("compare 2  " +  defaultSubstanceObjProperties[i].PropertyMaterialName);
						if ((substance.name == defaultSubstanceObjProperties[i].PropertyMaterialName) || (rend.sharedMaterial.name == defaultSubstanceObjProperties[i].PropertyMaterialName) )
						{
							tempFloatIndex = 0;
							tempColorIndex = 0;
							tempVector2Index = 0;
							tempVector3Index = 0;
							tempVector4Index = 0;

							for (int j = 0; j < objProperties.Length; j++)
							{
								ProceduralPropertyDescription objProperty = objProperties[j];
								ProceduralPropertyType propType = objProperties[j].type;
								//Debug.Log(defaultMatVector2Values.Count + " " + defaultMatVector3Values.Count + " " + defaultMatVector4Values.Count  );

								if (propType == ProceduralPropertyType.Float)
								{
									float curFloat = defaultSubstanceObjProperties[i].PropertyFloat[tempFloatIndex];
									substance.SetProceduralFloat(objProperty.name, curFloat);
									tempFloatIndex++;
								}

								else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
								{  
									Color colorInput = defaultSubstanceObjProperties[i].PropertyColor[tempColorIndex];
									Color oldColorInput = colorInput;
									substance.SetProceduralColor(objProperty.name, colorInput);
									tempColorIndex++;
								}

								else if (propType == ProceduralPropertyType.Vector2 ||propType == ProceduralPropertyType.Vector3 || propType ==  ProceduralPropertyType.Vector4)
								{
									if ( propType == ProceduralPropertyType.Vector4)
									{
										Vector4 curVector4 = substance.GetProceduralVector(objProperty.name);
										curVector4 =  defaultSubstanceObjProperties[i].PropertyVector4[tempVector4Index];
										substance.SetProceduralVector(objProperty.name, curVector4 );
										tempVector4Index++;
									}

									else if  ( propType == ProceduralPropertyType.Vector3)
									{
										Vector3 curVector3 = substance.GetProceduralVector(objProperty.name);
										curVector3 =  defaultSubstanceObjProperties[i].PropertyVector3[tempVector3Index];
										substance.SetProceduralVector(objProperty.name, curVector3 );
										tempVector3Index++;
									}

									else if ( propType == ProceduralPropertyType.Vector2)
									{
										Vector2 curVector2 = substance.GetProceduralVector(objProperty.name);
										curVector2 = defaultSubstanceObjProperties[i].PropertyVector2[tempVector2Index];
										substance.SetProceduralVector(objProperty.name, curVector2 );
										tempVector2Index++;
									}
								}
							}
							offset = defaultSubstanceObjProperties[i].stdVector2[0];

							if (rend.sharedMaterial.HasProperty("_EmissionColor"))
							{
								emissionInput = defaultSubstanceObjProperties[i].stdColor[0]; // update or OnGUI calls this which is why i dont use SetColor here
							}
							tempFloatIndex = 0;
							tempColorIndex = 0;
							tempVector2Index = 0;
							tempVector3Index = 0;
							tempVector4Index = 0;
							DebugStrings.Add("Reset all values to default");
							return;
						}
					}
				}

				GUILayout.Label("LerpTime:");
				lerpTime = EditorGUILayout.FloatField(lerpTime);

				if (GUILayout.Button("Create 1st transition based on current selction()"))
				{
					lerp1Description = new ObjProperty();
					for (int i = 0; i < objProperties.Length; i++)
					{   
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;
						float propFloat = substance.GetProceduralFloat(objProperty.name);
						lerp1Description.PropertyName.Add(objProperty.name);
						lerp1Description.PropertyFloat.Add( propFloat);
						lerp1Description.PropertyName[i] =objProperty.name ;

						if (propType == ProceduralPropertyType.Float)
							lerp1Description.PropertyFloat[i] =  propFloat;

						if(propType == ProceduralPropertyType.Color3 ||  propType == ProceduralPropertyType.Color4)
						{
							int colorComponentAmount = ((propType == ProceduralPropertyType.Color3) ? 3 : 4);
							Color propColor =  substance.GetProceduralColor(objProperty.name);
							lerp1Description.PropertyColor.Add(propColor);
							Debug.Log(propColor + " " + propType + " " + objProperty.name);
						}

						if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
						{
							if (propType == ProceduralPropertyType.Vector4)
							{
								Vector4 propVector4 = substance.GetProceduralVector(objProperty.name);
								lerp1Description.PropertyVector4.Add(propVector4);
							}

							else if  (propType == ProceduralPropertyType.Vector3)
							{
								Vector3 propVector3 = substance.GetProceduralVector(objProperty.name);
								lerp1Description.PropertyVector3.Add(propVector3);
							}

							else if (propType == ProceduralPropertyType.Vector2)
							{
								Vector2 propVector2 = substance.GetProceduralVector(objProperty.name);
								lerp1Description.PropertyVector2.Add(propVector2);
							}
						}
					}
					lerp1Description.stdColor.Add(emissionInput);;
					lerp1Description.stdVector2.Add(offset);
					DebugStrings.Add("Created 1st transition");
				}

				if (GUILayout.Button("Create 2nd transition based on current selction and lerp between the 2 transitions"))
				{
					newObjProperties = substance.GetProceduralPropertyDescriptions();
					customXmlFloatValues.Clear();
					for (int i = 0; i < objProperties.Length; i++)
					{
						float propFloat = substance.GetProceduralFloat(objProperties[i].name);
						customXmlFloatValues.Add(propFloat);
						newObjProperties[i].name = objProperties[i].name;
					}
					lerp2Description = new ObjProperty();

					for (int i = 0; i < objProperties.Length; i++)
					{   
						ProceduralPropertyDescription objProperty = objProperties[i];
						ProceduralPropertyType propType = objProperties[i].type;
						float propFloat = substance.GetProceduralFloat(objProperty.name);

						lerp2Description.PropertyName.Add(objProperty.name);
						lerp2Description.PropertyFloat.Add(propFloat);
						lerp2Description.PropertyName[i] =objProperty.name ;

						if (propType == ProceduralPropertyType.Float)
							lerp2Description.PropertyFloat[i] =  propFloat;

						if(propType == ProceduralPropertyType.Color3 ||  propType == ProceduralPropertyType.Color4)
						{
							int colorComponentAmount = ((propType == ProceduralPropertyType.Color3) ? 3 : 4);
							Color propColor =  substance.GetProceduralColor(objProperty.name);
							lerp2Description.PropertyColor.Add(propColor);
							Debug.Log(propColor + " " + propType + " " + objProperty.name);
						}

						if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
						{

							if (propType == ProceduralPropertyType.Vector4)
							{
								Vector4 propVector4 = substance.GetProceduralVector(objProperty.name);
								lerp2Description.PropertyVector4.Add(propVector4);
							}

							else if  (propType == ProceduralPropertyType.Vector3)
							{
								Vector3 propVector3 = substance.GetProceduralVector(objProperty.name);
								lerp2Description.PropertyVector3.Add(propVector3);
							}

							else if (propType == ProceduralPropertyType.Vector2)
							{
								Vector2 propVector2 = substance.GetProceduralVector(objProperty.name);
								lerp2Description.PropertyVector2.Add(propVector2);
							}
						}
						substanceLerp = true;
					}
					lerp2Description.stdColor.Add(emissionInput);
					lerp2Description.stdVector2.Add(offset);
					DebugStrings.Add("Created 2nd transition");
				}

				if (GUILayout.Button("Toggle Lerp On/Off"))
				{
					if (substanceLerp)
					{
						substanceLerp = false;
						DebugStrings.Add("Turned off lerp");
					}

					else if (!substanceLerp)
					{
						substanceLerp = true;
						DebugStrings.Add("Turned on lerp");
					}
				}
					
				if (GUILayout.Button ("Save Object as prefab:")) 
				{
					GameObject prefab = PrefabUtility.CreatePrefab(EditorUtility.SaveFilePanelInProject ("", "","prefab", ""), (GameObject)currentSelection.gameObject, ReplacePrefabOptions.Default);
					prefab.AddComponent<PrefabProperties>();
					Material prefabMat = new Material(Shader.Find("Standard"));
					PrefabProperties prefabProperties = prefab.GetComponent<PrefabProperties>();

					tempFloatIndex = 0;
					tempColorIndex = 0;
					tempVector2Index = 0;
					tempVector3Index = 0;
					tempVector4Index = 0;
					prefabProperties.substance = substance ;
					Renderer prefabRend = prefab.GetComponent<Renderer>();
					prefabRend.material = prefabProperties.substance;
					prefabRend.material = substance;

					if (substanceLerp)  //lerp is enabled, will save 2 sets of properties instead of just one.
					{
						for (int i = 0; i < objProperties.Length; i++)
						{
							ProceduralPropertyDescription objProperty = objProperties[i];
							ProceduralPropertyType propType = objProperties[i].type;
							float propFloat = substance.GetProceduralFloat(objProperty.name);
							prefabProperties.PropertyNameLerp1.Add(objProperty.name);
							prefabProperties.PropertyNameLerp2.Add(objProperty.name);
							// prefabProperties.PropertyFloatLerp1.Add(float propFloat);
							prefabProperties.PropertyNameLerp1[i] =objProperty.name ;

							if (propType == ProceduralPropertyType.Float)
							{
								//prefabProperties.PropertyFloatLerp1[i] = float propFloat;
								prefabProperties.PropertyFloatLerp1.Add((float)lerp1Description.PropertyFloat[tempFloatIndex]);
								prefabProperties.PropertyFloatLerp2.Add((float)lerp2Description.PropertyFloat[tempFloatIndex]);
								tempFloatIndex++;

							}
							else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
							{
								Color propColor = substance.GetProceduralColor(objProperty.name);
								prefabProperties.PropertyColorLerp1.Add((Color)lerp1Description.PropertyColor[tempColorIndex]);
								// prefabProperties.PropertyColorLerp1.Add(propColor);
								prefabProperties.PropertyColorLerp2.Add((Color)lerp2Description.PropertyColor[tempColorIndex]);
								tempColorIndex++;
							}

							else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
							{
								if (propType == ProceduralPropertyType.Vector4)
								{
									Vector4 propVector4 = substance.GetProceduralVector(objProperty.name);
									prefabProperties.PropertyVector4Lerp1.Add((Vector4)lerp1Description.PropertyVector4[tempVector4Index]);
									prefabProperties.PropertyVector4Lerp2.Add((Vector4)lerp2Description.PropertyVector4[tempVector4Index]);
									tempVector4Index++;
								}

								else if (propType == ProceduralPropertyType.Vector3)
								{
									Vector3 propVector3 = substance.GetProceduralVector(objProperty.name);
									prefabProperties.PropertyVector3Lerp1.Add((Vector3)lerp1Description.PropertyVector3[tempVector3Index]);
									prefabProperties.PropertyVector3Lerp2.Add((Vector3)lerp2Description.PropertyVector3[tempVector3Index]);
									tempVector3Index++;
								}

								else if (propType == ProceduralPropertyType.Vector2)
								{
									Vector2 propVector2 = substance.GetProceduralVector(objProperty.name);
									prefabProperties.PropertyVector2Lerp1.Add((Vector2)lerp1Description.PropertyVector2[tempVector2Index]);
									prefabProperties.PropertyVector2Lerp2.Add((Vector2)lerp2Description.PropertyVector2[tempVector2Index]);
									tempVector2Index++;
								}
							}
						}
						if (rend.material.HasProperty("_EmissionColor"))
						{
							prefabProperties.stdColorLerp1.Add(lerp1Description.stdColor[0]);
							prefabProperties.stdColorLerp2.Add(lerp2Description.stdColor[0]);
						}
						Vector2 offsetVector = new Vector2(offset.x, offset.y);
						prefabProperties.stdVector2Lerp1.Add(offsetVector);
					}
					else
					{
						for (int i = 0; i < objProperties.Length; i++)
						{   
							ProceduralPropertyDescription objProperty = objProperties[i];
							ProceduralPropertyType propType = objProperties[i].type;
							float propFloat = substance.GetProceduralFloat(objProperty.name);
							prefabProperties.PropertyNameLerp1.Add(objProperty.name);
							prefabProperties.PropertyFloatLerp1.Add( propFloat);
							prefabProperties.PropertyNameLerp1[i] =objProperty.name ;

							if (propType == ProceduralPropertyType.Float)
								prefabProperties.PropertyFloatLerp1[i] =  propFloat;

							else if(propType == ProceduralPropertyType.Color3 ||  propType == ProceduralPropertyType.Color4)
							{
								Color propColor =  substance.GetProceduralColor(objProperty.name);
								prefabProperties.PropertyColorLerp1.Add(propColor);
							}

							else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
							{
								if (propType == ProceduralPropertyType.Vector4)
								{
									Vector4 propVector4 = substance.GetProceduralVector(objProperty.name);
									prefabProperties.PropertyVector4Lerp1.Add(propVector4);
								}

								else if  (propType == ProceduralPropertyType.Vector3)
								{
									Vector3 propVector3 = substance.GetProceduralVector(objProperty.name);
									prefabProperties.PropertyVector3Lerp1.Add(propVector3);
								}

								else if (propType == ProceduralPropertyType.Vector2)
								{
									Vector2 propVector2 = substance.GetProceduralVector(objProperty.name);
									prefabProperties.PropertyVector2Lerp1.Add(propVector2);
								}
							}
						}
						prefabProperties.stdColorLerp1.Add(emissionInput);
						prefabProperties.stdColorLerp2.Add(emissionInput);
						Vector2 offsetVector = new Vector2(offset.x,offset.y);
						prefabProperties.stdVector2Lerp1.Add(offsetVector);
					}
					Debug.Log(prefabProperties.PropertyNameLerp1.Count);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();

					if (prefabProperties.PropertyNameLerp1.Count  <=1 )
					{
						Debug.Log("Something went wrong with creating a prefab, Trying again!");
					}
					DebugStrings.Add("Created prefab: " + prefab.name );
					if (prefabRend.sharedMaterial)
						DebugStrings.Add("Prefab material: " + prefabRend.sharedMaterial.name );
					DebugStrings.Add("Prefab transition 1  property count " + prefabProperties.PropertyNameLerp1.Count );
				}

				if (GUILayout.Button ("Write Debug Text File")) 
				{
					StreamWriter debugWrite;
					var path = EditorUtility.SaveFilePanel("Save Data", "","", "txt");
					FileInfo fInfo = new FileInfo(path);
					if (!fInfo.Exists )
					{
						debugWrite = fInfo.CreateText();
					}
					else
					{
						Debug.Log("Overwriting File");
						debugWrite = fInfo.CreateText();
					}
					debugWrite.WriteLine(System.DateTime.Now +   " Debug:");
					for (int i = 0; i < DebugStrings.Count; i++)
					{
						debugWrite.WriteLine(DebugStrings[i]);
					}

					debugWrite.WriteLine("Created debug log: " + path );
					debugWrite.Close();
					AssetDatabase.Refresh();
				}

				if (rend)
				rend.sharedMaterial.SetTextureOffset("_MainTex",new Vector2 (offset.x * Time.time,offset.y * Time.time));
				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();
				//substance.RebuildTextures();
				substance.RebuildTexturesImmediately();
			}
		}
		else if (EditorApplication.isPlaying && selectedStartupMaterials.Count <1)
		{
			if(!rend)
			{
				EditorUtility.DisplayDialog("Error", " No renderer attached to object.", "OK");
				this.Close();
				return;
			}

			if (!substance)
			{
				EditorUtility.DisplayDialog("Error", " No ProceduralMaterial attached to object.", "OK");
				this.Close();
				return;
			}
				
			EditorGUILayout.LabelField("Select a game object in the Hierarchy that has a Substance material first, then select this window.(Make sure that you are in play mode)");
		
			this.Close();
			EditorUtility.DisplayDialog("Error", " Select a game object in the Hierarchy that has a Substance material first, then select the SubstanceTween window.(Make sure that you are in play mode)", "OK");

		}

		else if ( EditorApplication.isPlaying && !rend)
		{
			this.Close();
			EditorUtility.DisplayDialog("error", " Object has no renderer attached", "OK");
		}

		else 
		{
			EditorGUILayout.LabelField("Select a game object in the Hierarchy that has a Substance material first, then select this window.(Make sure that you are in play mode)");
		}
			
		if ( selectedStartupMaterials.Count >=1 && !substance)
		{
			EditorGUILayout.LabelField("No ProceduralMaterial attached to object, Select another object)");
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}
		Repaint();
	}

	void Update ()
	{
		if(EditorApplication.isPlaying)
		{
			if (rend)
				rend.sharedMaterial.SetTextureOffset("_MainTex",new Vector2 (offset.x * Time.time,offset.y *Time.time));
			currentSelection = Selection.activeGameObject;
			if (substance && substanceLerp )
			{
				float lerp = Mathf.PingPong(Time.time, lerpTime) / lerpTime;

				if (objProperties != null)
				for(int i = 0; i < objProperties.Length; i++)
				{
					ProceduralPropertyDescription objProperty = objProperties[i];
					ProceduralPropertyType propType = objProperties[i].type;

					if(propType == ProceduralPropertyType.Float)
					{
						float curLerp1Float = (float)lerp1Description.PropertyFloat[i];
						float curLerp2Float = (float)lerp2Description.PropertyFloat[i];
						substance.SetProceduralFloat(objProperty.name, Mathf.Lerp(curLerp1Float,curLerp2Float, lerp));
					}

					else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
					{
						if (tempLerpColorIndex >= lerp1Description.PropertyColor.Count)
							tempLerpColorIndex = 0;
						int colorComponentAmount = ((propType == ProceduralPropertyType.Color3) ? 3 : 4);
						Color curLerp1Color = new Color(0,0,0);
						Color curLerp2Color = new Color(0,0,0);

						if ( (objProperties[i].type == ProceduralPropertyType.Color3 || objProperties[i].type == ProceduralPropertyType.Color4) )
						{
							curLerp1Color = (Color)lerp1Description.PropertyColor[tempLerpColorIndex];
							curLerp2Color = (Color)lerp2Description.PropertyColor[tempLerpColorIndex];
							tempLerpColorIndex++;
						}
						substance.SetProceduralColor(objProperties[i].name, Color.Lerp(curLerp1Color, curLerp2Color, lerp));
					}

					else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
					{
						if (tempLerpVector2Index >= lerp1Description.PropertyVector2.Count )
							tempLerpVector2Index = 0;
						if (tempLerpVector3Index >= lerp1Description.PropertyVector3.Count )
							tempLerpVector3Index = 0;
						if (tempLerpVector4Index >= lerp1Description.PropertyVector4.Count )
							tempLerpVector4Index = 0;

						Vector4 curlerp1Vector = Vector4.zero;
						Vector4 curlerp2Vector = Vector4.zero;

						if (propType == ProceduralPropertyType.Vector2 ) // one conditional for each vector type?
						{
							curlerp1Vector = (Vector2)lerp1Description.PropertyVector2[tempLerpVector2Index];
							curlerp2Vector = (Vector2)lerp2Description.PropertyVector2[tempLerpVector2Index];
							tempLerpVector2Index++;
							substance.SetProceduralVector(objProperties[i].name, Vector2.Lerp(curlerp1Vector, curlerp2Vector, lerp));
						}

						if (propType == ProceduralPropertyType.Vector3 ) // one conditional for each vector type?
						{
							curlerp1Vector = (Vector3)lerp1Description.PropertyVector3[tempLerpVector3Index];
							curlerp2Vector = (Vector3)lerp2Description.PropertyVector3[tempLerpVector3Index];
							tempLerpVector3Index++;
							substance.SetProceduralVector(objProperties[i].name, Vector3.Lerp(curlerp1Vector, curlerp2Vector, lerp));
						}

						if (propType == ProceduralPropertyType.Vector4 ) // one conditional for each vector type?
						{
							curlerp1Vector = (Vector4)lerp1Description.PropertyVector4[tempLerpVector4Index];
							curlerp2Vector = (Vector4)lerp2Description.PropertyVector4[tempLerpVector4Index];
							tempLerpVector4Index++;
							substance.SetProceduralVector(objProperties[i].name, Vector4.Lerp(curlerp1Vector, curlerp2Vector, lerp));
						}
					}
				}
				if (rend.sharedMaterial.HasProperty("_EmissionColor"))
				{
					Color curlerp1Emission = lerp1Description.stdColor[0];
					Color curlerp2Emission = lerp2Description.stdColor[0];
					Color emissionInput = rend.sharedMaterial.GetColor("_EmissionColor");
					rend.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(curlerp1Emission,curlerp2Emission,lerp));
				}
				substance.RebuildTexturesImmediately();
			}
		}

		else 
		{
			this.Close();
			EditorUtility.DisplayDialog("error", " This tool only works in play mode", "OK");
		}


	}
}