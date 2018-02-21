using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;
//[CustomEditor(typeof(ProceduralMaterial))]
//[CanEditMultipleObjects]
public class SubstanceToolTest : MaterialEditor
{

	// Use this for initialization
	void Start () {
		
	}
	
  public override void OnEnable()
        {
        SubstanceTool.CreateEditor(this);
            base.OnEnable();
            Undo.undoRedoPerformed += new Undo.UndoRedoCallback(this.UndoRedoPerformed);
        }

    public override void OnInspectorGUI()
    {
        //base.DrawDefaultInspector();
       // base.OnInspectorGUI();
        //DrawDefaultInspector();
    }

}
