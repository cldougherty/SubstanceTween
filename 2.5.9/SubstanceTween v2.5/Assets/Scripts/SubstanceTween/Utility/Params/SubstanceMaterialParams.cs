using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Parameters for how the tool handles procedural materials
/// </summary>
[System.Serializable]
public class SubstanceMaterialParams
{
    public Renderer rend;
    public bool rebuildSubstanceImmediately, saveOutputParameters = true, resettingValuesToDefault = true;

    public ProceduralMaterial substance; // main procedural material
    public ProceduralPropertyDescription[] materialVariables; // paramters of main procedural material
    public List<string> materialVariableNames = new List<string>();

    public List<ProceduralPropertyDescription> animatedMaterialVariables = new List<ProceduralPropertyDescription>();
    public List<MaterialVariableDictionaryHolder> MaterialVariableKeyframeDictionaryList = new List<MaterialVariableDictionaryHolder>();
    public List<MaterialVariableListHolder> MaterialVariableKeyframeList = new List<MaterialVariableListHolder>();

    public Color emissionInput;
    public Vector2 MainTexOffset;

    //parameters for the proceduralMaterial 
    public string substanceAssetName;
    public int[] textureSizeValues = { 3, 4, 5, 6, 7, 8, 9, 10 };
    public string[] textureSizeStrings = { "16", "32", "64", "128", "256", "512", "1024", "2048" };
    public int substanceHeight;
    public int substanceWidth;
    public int substanceTextureFormat;
    public int substanceLoadBehavior;

    //for overwriting parameters for every keyframe
    public bool chooseOverwriteVariables;
    public bool saveOverwriteVariables;
    public List<ProceduralPropertyDescription> variablesToOverwrite = new List<ProceduralPropertyDescription>(); 
    public Dictionary<string, string> VariableValuesToOverwrite = new Dictionary<string, string>();

    public ReorderableList reorderList;
    public int reorderIndexInt;
}