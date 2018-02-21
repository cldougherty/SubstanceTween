using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Linq;

[System.Serializable]
[XmlRoot]
public class ObjProperty // this class is used to store parameters
{
	public string PropertyMaterialName;
	public float animationTime;
	public List<string> PropertyName = new List<string>(); 
	public List<float> PropertyFloat = new List<float>();
	public List<Color> PropertyColor = new List<Color>();
	public List<Vector2> PropertyVector2 = new List<Vector2>();
	public List<Vector3> PropertyVector3 = new List<Vector3>();
	public List<Vector4> PropertyVector4 = new List<Vector4>();
	public List<Vector2> stdVector2 = new List<Vector2>(); //Vectors that are part of the standard unity material (ex: _MainTex)
	public List<Vector3> stdVector3 = new List<Vector3>();
	public List<Color> stdColor = new List<Color>(); // Colors that are part of the standard unity material (ex: _EmissionColor)
	public List<string> myKeys = new List<string>(); // Keys and values are used for sorting parameter names and values (Cant Serialize Dictionaries to XML)
	public List<string> myValues = new List<string>();
	public enum AnimationType  {PingPong , Loop};
	public List<AnimationType> variableAnimationType = new List<AnimationType>();
	public bool hasParametersWithoutRange = false;
}