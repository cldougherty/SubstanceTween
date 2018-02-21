using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;


[System.Serializable]

[XmlRoot]
public class ObjProperty  
{
	public string PropertyMaterialName;
    
	public List<string> PropertyName = new List<string>();
	//public List<int> PropertyInt = new List<int>(); 
	public List<float> PropertyFloat = new List<float>();
	public List<Color> PropertyColor = new List<Color>();
	public List<Vector2> PropertyVector2 = new List<Vector2>();
	public List<Vector3> PropertyVector3 = new List<Vector3>();
	public List<Vector4> PropertyVector4 = new List<Vector4>();
	public List<Vector2> stdVector2 = new List<Vector2>();
	public List<Vector3> stdVector3 = new List<Vector3>();
	public List<Color> stdColor = new List<Color>();

	public class ThingWithArrays
	{
		public ArrayList colorTestProp = new ArrayList();
	}
}
