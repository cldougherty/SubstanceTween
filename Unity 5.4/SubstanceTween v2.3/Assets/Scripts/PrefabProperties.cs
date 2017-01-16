using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PrefabProperties : MonoBehaviour { // This script automatically gets added to the prefab once it is created

	public List<string> PropertyNameLerp1 = new List<string>();
	public List<float> PropertyFloatLerp1 = new List<float>();
	public List<Color> PropertyColorLerp1 = new List<Color>();
	public List<Vector2> PropertyVector2Lerp1 = new List<Vector2> ();
	public List<Vector3> PropertyVector3Lerp1 = new List<Vector3> ();
	public List<Vector4> PropertyVector4Lerp1 = new List<Vector4> ();
	public List<Vector2> stdVector2Lerp1 = new List<Vector2> ();
	public List<Vector3> stdVector3Lerp1 = new List<Vector3> ();
	public List<Color> stdColorLerp1= new List<Color>();

	public List<string> PropertyNameLerp2 = new List<string>();
	public List<float> PropertyFloatLerp2 = new List<float>();
	public List<Color> PropertyColorLerp2 = new List<Color>();
	public List<Vector2> PropertyVector2Lerp2 = new List<Vector2> ();
	public List<Vector3> PropertyVector3Lerp2 = new List<Vector3> ();
	public List<Vector4> PropertyVector4Lerp2 = new List<Vector4> ();
	public List<Vector2> stdVector2Lerp2 = new List<Vector2> ();
	public List<Vector3> stdVector3Lerp2 = new List<Vector3> ();
	public List<Color> stdColorLerp2= new List<Color>();
	public List<string> myLerp1Keys = new List<string>();
	public List<string> myLerp1Values = new List<string>();
	public List<string> myLerp2Keys = new List<string>();
	public List<string> myLerp2Values = new List<string>();

	private ProceduralPropertyDescription[] objProperties;
	public bool lerpToggle = false ,rebuildSubstanceImmediately = false;
	public Vector2 mainTexOffset;
	public Renderer rend;
	public ProceduralMaterial substance,  defaultSubstance  ;
	public int tempLerpFloatIndex, tempLerpColorIndex, tempLerpVector2Index, tempLerpVector3Index, tempLerpVector4Index;
	public float animationTime = 2;

	// Use this for initialization
	void Start () 
	{
		rend = GetComponent<Renderer>();
		ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;
		substance = rend.sharedMaterial as ProceduralMaterial;
		objProperties = substance.GetProceduralPropertyDescriptions();
		Color emissionInput = Color.white;
		if (PropertyNameLerp2.Count > 0)
			lerpToggle = true;
		else
			lerpToggle = false;
		if (PropertyNameLerp1.Count > 0)
		{ // sort before setting variables?
			for (int i = 0; i < objProperties.Length; i++)
			{
				ProceduralPropertyDescription objProperty = objProperties[i];
				ProceduralPropertyType propType = objProperties[i].type;
				if (propType == ProceduralPropertyType.Float  )
				{
					for(int j =0; j < myLerp1Keys.Count; j++ )
					{
						if (myLerp1Keys[j] ==   objProperty.name)
						{
							if (myLerp1Keys[j] == objProperty.name)
								substance.SetProceduralFloat(objProperty.name,float.Parse(myLerp1Values[j]));
						}
					}
				}
				else if (propType == ProceduralPropertyType.Color3 )
				{  
					Debug.Log(myLerp1Keys[i] + " " + objProperty.name);
					for(int j =0; j < myLerp1Keys.Count; j++ )
					{
						if (myLerp1Keys[j] == objProperty.name)
						{
							Color curColor;
							ColorUtility.TryParseHtmlString(myLerp1Values[j],out curColor);
							substance.SetProceduralColor(objProperty.name,curColor);
						}
					}
				}
				else if (propType == ProceduralPropertyType.Color4 )
				{  
					for(int j =0; j < myLerp1Keys.Count; j++ )
					{
						if (myLerp1Keys[j] ==   objProperty.name)
						{
							Color curColor;
							ColorUtility.TryParseHtmlString(myLerp1Values[j],out curColor);
							substance.SetProceduralColor(objProperty.name,curColor);
						}
					}
				}
				else if (propType == ProceduralPropertyType.Vector2 ||propType == ProceduralPropertyType.Vector3 || propType ==  ProceduralPropertyType.Vector4)
				{
					if ( propType == ProceduralPropertyType.Vector4)
					{ // Put a for loop here for each vector type
						for(int j =0; j < myLerp1Keys.Count; j++ )
						{
							if (myLerp1Keys[j] ==   objProperty.name)
							{
								Vector4 curVector4 = StringToVector(myLerp1Values[j],4);
								substance.SetProceduralVector(objProperty.name,curVector4);
							}
						}
					}
					else if  ( propType == ProceduralPropertyType.Vector3)
					{
						for(int j =0; j < myLerp1Keys.Count; j++ )
						{
							if (myLerp1Keys[j] ==   objProperty.name)
							{
								Vector3 curVector3 = StringToVector(myLerp1Values[j],3);
								substance.SetProceduralVector(objProperty.name,curVector3);
							}
						}
					}
					else if ( propType == ProceduralPropertyType.Vector2)
					{
						for(int j =0; j < myLerp1Keys.Count; j++ )
						{
							if (myLerp1Keys[j] ==   objProperty.name)
							{
								Vector2 curVector2 = StringToVector(myLerp1Values[j],2);
								substance.SetProceduralVector(objProperty.name,curVector2);
							}
						}
					}
				}
			}
			mainTexOffset.x = stdVector2Lerp1[0].x;
			mainTexOffset.y = stdVector2Lerp1[0].y;
			Color emissionColor = new Color (0,0,0);
			substance.RebuildTextures();
		}	
	}
	// Update is called once per frame
	void Update () 
	{// use dictionary to update values
		if (lerpToggle)
		{
			float lerp = Mathf.PingPong(Time.time, animationTime) / animationTime;
			for (int i = 0; i < objProperties.Length; i++)
			{
				ProceduralPropertyDescription objProperty = objProperties[i];
				ProceduralPropertyType propType = objProperties[i].type;
				if(propType == ProceduralPropertyType.Float )
				{
					for(int j =0; j < myLerp1Keys.Count; j++ )
					{
						if (myLerp1Keys[j] == objProperty.name)
						{
							float curLerp1Float = (float) float.Parse(myLerp1Values[j]);
							float curLerp2Float = (float) float.Parse(myLerp2Values[j]);
							if (curLerp1Float != curLerp2Float)
								substance.SetProceduralFloat(objProperties[i].name, Mathf.Lerp(curLerp1Float,curLerp2Float, lerp));
						}
					}
				}
				else if (propType == ProceduralPropertyType.Color3 )
				{  
					for(int j =0; j < myLerp1Keys.Count; j++ )
					{
						if (myLerp1Keys[j] ==   objProperty.name)
						{
							Color curLerp1Color = new Color(0,0,0), curLerp2Color = new Color(0,0,0);
							ColorUtility.TryParseHtmlString(myLerp1Values[j],out curLerp1Color);
							ColorUtility.TryParseHtmlString(myLerp2Values[j],out curLerp2Color);
							if (curLerp1Color != curLerp2Color)
								substance.SetProceduralColor(objProperties[i].name, Color.Lerp(curLerp1Color, curLerp2Color, lerp));
						}
					}
				}
				else if (propType == ProceduralPropertyType.Color4 )
				{  
					for(int j =0; j < myLerp1Keys.Count; j++ )
					{
						if (myLerp1Keys[j] ==   objProperty.name)
						{
							Color curLerp1Color = new Color(0,0,0), curLerp2Color = new Color(0,0,0);
							ColorUtility.TryParseHtmlString(myLerp1Values[j],out curLerp1Color);
							ColorUtility.TryParseHtmlString(myLerp2Values[j],out curLerp2Color);
							if (curLerp1Color != curLerp2Color)
								substance.SetProceduralColor(objProperties[i].name, Color.Lerp(curLerp1Color, curLerp2Color, lerp));
						}
					}
				}
				else if (propType == ProceduralPropertyType.Vector2 ||propType == ProceduralPropertyType.Vector3 || propType ==  ProceduralPropertyType.Vector4)
				{
					Vector4 curLerp1Vector = Vector4.zero, curLerp2Vector = Vector4.zero;
					if ( propType == ProceduralPropertyType.Vector4)
					{
						for(int j =0; j < myLerp1Keys.Count; j++ )
						{
							if (myLerp1Keys[j] == objProperty.name)
							{
								curLerp1Vector = StringToVector(myLerp1Values[j],4);
								curLerp2Vector = StringToVector(myLerp2Values[j],4);
								if (curLerp1Vector != curLerp2Vector)
									substance.SetProceduralVector(objProperties[j].name, Vector4.Lerp(curLerp1Vector, curLerp2Vector, lerp));
							}
						}
					}
					else if  ( propType == ProceduralPropertyType.Vector3)
					{
						for(int j =0; j < myLerp1Keys.Count; j++ )
						{
							if (myLerp1Keys[j] == objProperty.name)
							{
								curLerp1Vector = StringToVector(myLerp1Values[j],3);
								curLerp2Vector = StringToVector(myLerp2Values[j],3);
								if (curLerp1Vector != curLerp2Vector)
									substance.SetProceduralVector(objProperties[j].name, Vector3.Lerp(curLerp1Vector, curLerp2Vector, lerp));
							}
						}
					}
					else if ( propType == ProceduralPropertyType.Vector2)
					{
						for(int j =0; j < myLerp1Keys.Count; j++ )
						{
							if (myLerp1Keys[j] == objProperty.name )
							{
								curLerp1Vector = StringToVector(myLerp1Values[j],2);
								curLerp2Vector = StringToVector(myLerp2Values[j],2);
								if (curLerp1Vector != curLerp2Vector)
									substance.SetProceduralVector(objProperties[j].name, Vector2.Lerp(curLerp1Vector, curLerp2Vector, lerp));
							}
						}
					}
				}
			}
			if (rend.material.HasProperty("_EmissionColor"))
			{
				Color curlerp1Emission = stdColorLerp1[0];
				Color curlerp2Emission =  stdColorLerp2[0];
				Color emissionInput = rend.material.GetColor("_EmissionColor");
				rend.material.SetColor("_EmissionColor", Color.Lerp(curlerp1Emission,curlerp2Emission,lerp));
			}
			if (rebuildSubstanceImmediately)
				substance.RebuildTexturesImmediately();
			else
				substance.RebuildTextures();
		}
		rend.material.SetTextureOffset("_MainTex",new Vector2 (mainTexOffset.x * Time.time,mainTexOffset.y * Time.time));
	}

	public static Vector4 StringToVector(string startVector, int VectorAmount)
	{
		if (startVector.StartsWith ("(") && startVector.EndsWith (")")) // Remove "( )" fram the string 
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