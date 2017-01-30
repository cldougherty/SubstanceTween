using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PrefabProperties : MonoBehaviour { // This script automatically gets added to the prefab once it is created

	private ProceduralPropertyDescription[] objProperties;
	public bool lerpToggle, rebuildSubstanceImmediately, animateBackwards;
	public Vector2 mainTexOffset;
	public Renderer rend;
	public ProceduralMaterial substance, defaultSubstance;
	public int keyFrames, currentKeyframeIndex;
	public float animationTime = 2, currentAnimationTime;
	public List<float> keyFrameTimes = new List<float>();
	public List<float> ReversedKeyFrameTimes = new List<float>();
	public List<ObjProperty> keyframeDescriptions = new List<ObjProperty>();
	public enum AnimationType { Loop, BackAndForth }; 
	public AnimationType animationType;

	// Use this for initialization
	void Start () 
	{
		rend = GetComponent<Renderer>();
		ProceduralMaterial.substanceProcessorUsage = ProceduralProcessorUsage.All;
		substance = rend.sharedMaterial as ProceduralMaterial;
		objProperties = substance.GetProceduralPropertyDescriptions();
		Color emissionInput = Color.white;
		if (keyframeDescriptions.Count>=2)
			lerpToggle = true;
		else
			lerpToggle = false;
		if (keyframeDescriptions.Count > 0 ) 
		{// if you have 1 keyframe set the substance parameters from that keyframe if you have more than 1 it will animate in Update()
			for (int i = 0; i < objProperties.Length; i++)
			{
				ProceduralPropertyDescription objProperty = objProperties[i];
				ProceduralPropertyType propType = objProperties[i].type;
				if (propType == ProceduralPropertyType.Float)
				{
					for(int j =0; j < keyframeDescriptions[0].myValues.Count; j++ )
					{
						if (keyframeDescriptions[0].myKeys[j] == objProperty.name)
						{
							if (keyframeDescriptions[0].myKeys[j] == objProperty.name)
								substance.SetProceduralFloat(objProperty.name,float.Parse(keyframeDescriptions[0].myValues[j]));
						}
					}
				}
				else if (propType == ProceduralPropertyType.Color3 )
				{
					for(int j =0; j < keyframeDescriptions[0].myKeys.Count; j++ )
					{
						if (keyframeDescriptions[0].myKeys[j] == objProperty.name)
						{
							Color curColor;
							ColorUtility.TryParseHtmlString(keyframeDescriptions[0].myValues[j],out curColor);
							substance.SetProceduralColor(objProperty.name,curColor);
						}
					}
				}
				else if (propType == ProceduralPropertyType.Color4 )
				{
					for(int j =0; j < keyframeDescriptions[0].myKeys.Count; j++ )
					{
						if (keyframeDescriptions[0].myKeys[j] == objProperty.name)
						{
							Color curColor;
							ColorUtility.TryParseHtmlString(keyframeDescriptions[0].myValues[j],out curColor);
							substance.SetProceduralColor(objProperty.name,curColor);
						}
					}
				}
				else if (propType == ProceduralPropertyType.Vector2 ||propType == ProceduralPropertyType.Vector3 || propType == ProceduralPropertyType.Vector4)
				{
					if (propType == ProceduralPropertyType.Vector4)
					{
						for(int j =0; j < keyframeDescriptions[0].myKeys.Count; j++ )
						{
							if (keyframeDescriptions[0].myKeys[j] == objProperty.name)
							{
								Vector4 curVector4 = StringToVector(keyframeDescriptions[0].myValues[j],4);
								substance.SetProceduralVector(objProperty.name,curVector4);
							}
						}
					}
					else if (propType == ProceduralPropertyType.Vector3)
					{
						for(int j =0; j < keyframeDescriptions[0].myKeys.Count; j++ )
						{
							if (keyframeDescriptions[0].myKeys[j] == objProperty.name)
							{
								Vector3 curVector3 = StringToVector(keyframeDescriptions[0].myValues[j],3);
								substance.SetProceduralVector(objProperty.name,curVector3);
							}
						}
					}
					else if (propType == ProceduralPropertyType.Vector2)
					{
						for(int j =0; j < keyframeDescriptions[0].myKeys.Count; j++ )
						{
							if (keyframeDescriptions[0].myKeys[j] == objProperty.name)
							{
								Vector2 curVector2 = StringToVector(keyframeDescriptions[0].myValues[j],2);
								substance.SetProceduralVector(objProperty.name,curVector2);
							}
						}
					}
				}
			}
			mainTexOffset.x = keyframeDescriptions[0].stdVector2[0].x;
			mainTexOffset.y = keyframeDescriptions[0].stdVector2[0].y;
			Color emissionColor = new Color (0,0,0);
			substance.RebuildTextures();
		}	
	}
	// Update is called once per frame
	void Update () 
	{
		if (lerpToggle )
		{
				float currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
				if (animationType == AnimationType.BackAndForth && animateBackwards && currentKeyframeIndex <= keyFrameTimes.Count-1 && currentKeyframeIndex > 0)
					currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex-1];
				float lerp = 5;
				if (animationType == AnimationType.Loop) 
				{
					currentKeyframeAnimationTime = keyFrameTimes[currentKeyframeIndex];
					currentAnimationTime += Time.deltaTime;
					if (keyFrameTimes.Count > 2 && currentAnimationTime > currentKeyframeAnimationTime && currentKeyframeIndex <= keyFrameTimes.Count -1 ) 
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

				else if (animationType == AnimationType.BackAndForth ) 
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
					if ( animateBackwards && currentKeyframeIndex == 0 && currentAnimationTime < 0 ) // if you reach the last keyframe when going backwards go forwards.
						animateBackwards = false;
					if (animationTime > 0)
					{
						if (!animateBackwards)
							lerp = currentAnimationTime / currentKeyframeAnimationTime;
						else if (keyFrameTimes.Count > 2 && animateBackwards && currentAnimationTime != currentKeyframeAnimationTime)
							lerp = Mathf.InverseLerp( 0, 1,currentAnimationTime / keyFrameTimes[currentKeyframeIndex]);
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
							for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++ )
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
							for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++ )
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
						else if (propType == ProceduralPropertyType.Color4 )
						{ 
							for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++ )
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
							if ( propType == ProceduralPropertyType.Vector4)
							{
								for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++ )
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
								for(int j =0; j < keyframeDescriptions[currentKeyframeIndex].myKeys.Count(); j++ )
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
					if (curlerp1Emission != curlerp2Emission)
						rend.sharedMaterial.SetColor("_EmissionColor", Color.Lerp(curlerp1Emission,curlerp2Emission,lerp));
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