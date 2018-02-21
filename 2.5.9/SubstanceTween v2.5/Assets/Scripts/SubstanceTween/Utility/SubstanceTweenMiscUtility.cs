using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SubstanceTweenMiscUtility {

    public static Vector4 StringToVector(string startVector, int VectorAmount)//Converts Strings to Vectors.
    {
        if (startVector.StartsWith("(") && startVector.EndsWith(")")) // Remove "()" from the string 
            startVector = startVector.Substring(1, startVector.Length - 2);
        string[] sArray = startVector.Split(',');
        if (VectorAmount == 2)
        {
            Vector2 result = new Vector2(float.Parse(sArray[0]), float.Parse(sArray[1]));
            return result;
        }
        else if (VectorAmount == 3)
        {
            Vector3 result = new Vector3(float.Parse(sArray[0]), float.Parse(sArray[1]), float.Parse(sArray[2]));
            return result;
        }
        else if (VectorAmount == 4)
        {
            Vector4 result = new Vector4(float.Parse(sArray[0]), float.Parse(sArray[1]), float.Parse(sArray[2]), float.Parse(sArray[3]));
            return result;
        }
        else
            return new Vector4(0, 0, 0, 0);
    }
}
