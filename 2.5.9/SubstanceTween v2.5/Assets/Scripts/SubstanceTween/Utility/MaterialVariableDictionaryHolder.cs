using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Linq;
using System;

[Serializable]
public class MaterialVariableDictionaryHolder // this class is used to store parameters
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
}