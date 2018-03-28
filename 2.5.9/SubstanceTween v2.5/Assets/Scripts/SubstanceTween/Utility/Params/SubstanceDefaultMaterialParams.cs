using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parameters for handling material parameters at startup
/// </summary>
public class SubstanceDefaultMaterialParams
{
    public bool resettingValuesToDefault = true;
    public ProceduralMaterial defaultSubstance;
    public List<MaterialVariableListHolder> defaultSubstanceObjProperties = new List<MaterialVariableListHolder>();
    public List<ProceduralMaterial>selectedStartupMaterials = new List<ProceduralMaterial>();
}
