using UnityEngine;
using System.Collections;
using System.Collections.Generic;



[System.Serializable]
public class PrefabProperties : MonoBehaviour {

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

	private ProceduralPropertyDescription[] objProperties;

	public bool lerpToggle;

	public Vector2 offset;
	public Renderer rend;

	public ProceduralMaterial substance ;
	public ProceduralMaterial defaultSubstance;

    public int tempLerpFloatIndex;
    public int tempLerpColorIndex;
	public int tempLerpVector2Index;
	public int tempLerpVector3Index;
	public int tempLerpVector4Index;

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
		{
			for (int i = 0; i < objProperties.Length; i++)
			{
				ProceduralPropertyDescription objProperty = objProperties[i];
				float propFloat = substance.GetProceduralFloat(objProperty.name);
				ProceduralPropertyType propType = objProperties[i].type;

				if( i < PropertyFloatLerp1.Count &&  propType == ProceduralPropertyType.Float)
				{
                    float curLerp1Float = (float)PropertyFloatLerp1[i];
                    //float curLerp2Float = (float)PropertyFloatLerp2[i];
                    substance.SetProceduralFloat(objProperty.name,PropertyFloatLerp1[i] );
                   // tempLerpFloatIndex++;
                }

				else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
				{
					if (tempLerpVector2Index >= PropertyVector2Lerp1.Count )
						tempLerpVector2Index = 0;
					if (tempLerpVector3Index >= PropertyVector3Lerp1.Count )
						tempLerpVector3Index = 0;
					if (tempLerpVector4Index >= PropertyVector3Lerp1.Count )
						tempLerpVector4Index = 0;

					Vector4 curlerp1Vector = Vector4.zero;
					Vector4 curlerp2Vector = Vector4.zero;

					if (propType == ProceduralPropertyType.Vector2 ) // one conditional for each vector type?
					{
						curlerp1Vector = (Vector2)PropertyVector2Lerp1[tempLerpVector2Index];
						tempLerpVector2Index++;
						substance.SetProceduralVector(objProperties[i].name, curlerp1Vector);
					}

					if (propType == ProceduralPropertyType.Vector3 ) // one conditional for each vector type?
					{
						curlerp1Vector = (Vector3)PropertyVector3Lerp1[tempLerpVector3Index];
						tempLerpVector3Index++;
						substance.SetProceduralVector(objProperties[i].name, curlerp1Vector);
					}

					if (propType == ProceduralPropertyType.Vector4 ) // one conditional for each vector type?
					{
						curlerp1Vector = (Vector4)PropertyVector4Lerp1[tempLerpVector4Index];
						tempLerpVector4Index++;
						substance.SetProceduralVector(objProperties[i].name, curlerp1Vector);
					}
				}
				else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
				{
					if (tempLerpColorIndex >= PropertyColorLerp1.Count)
						tempLerpColorIndex = 0;
					int colorComponentAmount = ((propType == ProceduralPropertyType.Color3) ? 3 : 4);
					Color curLerp1Color = new Color(0,0,0);
					Color curLerp2Color = new Color(0,0,0);

					if ( (objProperties[i].type == ProceduralPropertyType.Color3 || objProperties[i].type == ProceduralPropertyType.Color4) )
					{
						curLerp1Color = (Color)PropertyColorLerp1[tempLerpColorIndex];
						tempLerpColorIndex++;
					}
					substance.SetProceduralColor(objProperties[i].name, curLerp1Color);
				}
				//lerp1Description.stdVector2.Add(offsetx);
				//lerp1Description.stdVector2.Add(offsety);
			}
			offset.x = stdVector2Lerp1[0].x;
			offset.y = stdVector2Lerp1[0].y;
			/*emissionInput = stdColorLerp2[0];

			stdColorLerp2.Add(emissionInput);

			emissionInput = stdColorLerp1[0];

			stdColorLerp1.Add(emissionInput);
*/
			Color emissionColor = new Color (0,0,0);


			substance.RebuildTextures();
		}	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (lerpToggle)
		{
			float lerp = Mathf.PingPong(Time.time, 2) / 2;
				for (int i = 0; i < objProperties.Length; i++)
				{
					ProceduralPropertyDescription objProperty = objProperties[i];
					float propFloat = substance.GetProceduralFloat(objProperty.name);
					ProceduralPropertyType propType = objProperties[i].type;

					if(i < PropertyFloatLerp1.Count && propType == ProceduralPropertyType.Float)
					{
						float curLerp1Float = (float)PropertyFloatLerp1[i];
						float curLerp2Float = (float)PropertyFloatLerp2[i];
                        //float curLerp2Float = (float)PropertyFloatLerp2[i];
                        substance.SetProceduralFloat(objProperties[i].name, Mathf.Lerp(curLerp1Float, curLerp2Float, lerp));
                       // substance.SetProceduralFloat(objProperty.name,PropertyFloatLerp1[i] );
					}
					
					else if (propType == ProceduralPropertyType.Vector2 || propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
					{
						if (tempLerpVector2Index >= PropertyVector2Lerp1.Count )
							tempLerpVector2Index = 0;
						if (tempLerpVector3Index >= PropertyVector3Lerp1.Count )
							tempLerpVector3Index = 0;
						if (tempLerpVector4Index >= PropertyVector3Lerp1.Count )
							tempLerpVector4Index = 0;

						Vector4 curlerp1Vector = Vector4.zero;
						Vector4 curlerp2Vector = Vector4.zero;

						if (propType == ProceduralPropertyType.Vector2 ) // one conditional for each vector type?
						{
							curlerp1Vector = (Vector2)PropertyVector2Lerp1[tempLerpVector2Index];
							curlerp2Vector = (Vector2)PropertyVector2Lerp2[tempLerpVector2Index];
							tempLerpVector2Index++;
							substance.SetProceduralVector(objProperties[i].name, Vector2.Lerp(curlerp1Vector, curlerp2Vector, lerp));
						}

						if (propType == ProceduralPropertyType.Vector3 ) // one conditional for each vector type?
						{
							curlerp1Vector = (Vector3)PropertyVector3Lerp1[tempLerpVector3Index];
							curlerp2Vector = (Vector3)PropertyVector3Lerp2[tempLerpVector3Index];
							tempLerpVector3Index++;
							substance.SetProceduralVector(objProperties[i].name, Vector3.Lerp(curlerp1Vector, curlerp2Vector, lerp));
						}

						if (propType == ProceduralPropertyType.Vector4 ) // one conditional for each vector type?
						{
							curlerp1Vector = (Vector4)PropertyVector4Lerp1[tempLerpVector4Index];
							curlerp2Vector = (Vector4)PropertyVector4Lerp2[tempLerpVector4Index];
							tempLerpVector4Index++;
							substance.SetProceduralVector(objProperties[i].name, Vector4.Lerp(curlerp1Vector, curlerp2Vector, lerp));
						}
					}
					else if (propType == ProceduralPropertyType.Color3 || propType == ProceduralPropertyType.Color4)
					{
						if (tempLerpColorIndex >= PropertyColorLerp1.Count)
							tempLerpColorIndex = 0;
						int colorComponentAmount = ((propType == ProceduralPropertyType.Color3) ? 3 : 4);
						Color curLerp1Color = new Color(0,0,0);
						Color curLerp2Color = new Color(0,0,0);

						if ( (objProperties[i].type == ProceduralPropertyType.Color3 || objProperties[i].type == ProceduralPropertyType.Color4) )
						{
							curLerp1Color = (Color)PropertyColorLerp1[tempLerpColorIndex];
							curLerp2Color = (Color)PropertyColorLerp2[tempLerpColorIndex];
							tempLerpColorIndex++;
						}
						substance.SetProceduralColor(objProperties[i].name, Color.Lerp(curLerp1Color, curLerp2Color, lerp));
					}
					//lerp1Description.stdVector2.Add(offsetx);
					//lerp1Description.stdVector2.Add(offsety);
				}

			rend.material.SetTextureOffset("_MainTex",new Vector2 (offset.x * Time.time,offset.y *Time.time));
				
				if (rend.material.HasProperty("_EmissionColor"))
				{
					//Color curlerp1Emission = PropertyColorLerp1[3];
					//Color curlerp2Emission =  PropertyColorLerp2[3];
					Color curlerp1Emission = stdColorLerp1[0];
					Color curlerp2Emission =  stdColorLerp2[0];
					Color emissionInput = rend.material.GetColor("_EmissionColor");
					rend.material.SetColor("_EmissionColor", Color.Lerp(curlerp1Emission,curlerp2Emission,lerp));
				}
			//substance.RebuildTextures();
			substance.RebuildTexturesImmediately();
		}
	}
}
