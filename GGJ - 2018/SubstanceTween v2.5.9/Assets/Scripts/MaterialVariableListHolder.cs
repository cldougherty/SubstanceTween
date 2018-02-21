using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
[System.Serializable]
[XmlRoot]
public class MaterialVariableListHolder // this class is used to store parameters
{
	public string PropertyMaterialName;
	public float animationTime;
	public List<string> PropertyName = new List<string>(); 
	public List<float> PropertyFloat = new List<float>();
	public List<Color> PropertyColor = new List<Color>();
	public List<Vector2> PropertyVector2 = new List<Vector2>();
	public List<Vector3> PropertyVector3 = new List<Vector3>();
	public List<Vector4> PropertyVector4 = new List<Vector4>();
    public List<int> PropertyEnum = new List<int>();
    public List<bool> PropertyBool = new List<bool>();
    public List<Color> stdColor = new List<Color>(); // Colors that are part of the standard unity material (ex: _EmissionColor)
    public Color emissionColor = new Color();
    public Vector2 MainTex = new Vector2();
    public List<string> myKeys = new List<string>(); // Keys and values are used for sorting parameter names and values (Cant Serialize Dictionaries to XML or prefabs.)
	public List<string> myValues = new List<string>();

    public List<string> myFloatKeys = new List<string>();
    public List<float> myFloatValues = new List<float>();

    public List<string> myColorKeys = new List<string>();
    public List<Color> myColorValues = new List<Color>();

    public List<string> myVector2Keys = new List<string>();
    public List<Vector2> myVector2Values = new List<Vector2>();

    public List<string> myVector3Keys = new List<string>();
    public List<Vector3> myVector3Values = new List<Vector3>();

    public List<string> myVector4Keys = new List<string>();
    public List<Vector4> myVector4Values = new List<Vector4>();

    public List<string> myBooleanKeys = new List<string>();
    public List<bool> myBooleanValues = new List<bool>();

    public List<string> myEnumKeys = new List<string>();
    public List<int> myEnumValues = new List<int>();

    public List<Keyframe> AnimationCurveKeyframeList = new List<Keyframe>();
    public List<Vector2> AnimationCurveTangentList = new List<Vector2>();
    public enum AnimationType  {PingPong , Loop};
	public List<AnimationType> variableAnimationType = new List<AnimationType>();
	public bool hasParametersWithoutRange = false;
}