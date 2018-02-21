using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class MaterialVariableScriptableObject : ScriptableObject 
{
    public string PropertyMaterialName;
    public float animationTime;
    public Dictionary<string, object> PropertyDictionary = new Dictionary<string, object>();
    [SerializeField]
    public Dictionary<string, float> PropertyFloatDictionary = new Dictionary<string, float>();
    public Dictionary<string, Color> PropertyColorDictionary = new Dictionary<string, Color>();
    public Dictionary<string, Vector2> PropertyVector2Dictionary = new Dictionary<string, Vector2>();
    public Dictionary<string, Vector3> PropertyVector3Dictionary = new Dictionary<string, Vector3>();
    public Dictionary<string, Vector4> PropertyVector4Dictionary = new Dictionary<string, Vector4>();
    public Dictionary<string, int> PropertyEnumDictionary = new Dictionary<string, int>();
    public Dictionary<string, bool> PropertyBoolDictionary = new Dictionary<string, bool>();
    public List<Keyframe> AnimationCurveKeyframeList = new List<Keyframe>();
    public List<string> PropertyName = new List<string>();
    public Color emissionColor = new Color();
    public Vector2 MainTex = new Vector2();
    public enum AnimationType { PingPong, Loop };
    public List<AnimationType> variableAnimationType = new List<AnimationType>();
    public bool hasParametersWithoutRange = false;




    public static void Save(MaterialVariableDictionaryHolder dict)
    {
        Debug.Log("Saving game...");
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create("Assets");
        bf.Serialize(file, dict);
        file.Close();
    }



}
