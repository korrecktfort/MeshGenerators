using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Lines))]
public class LinesControl : Editor {

	Lines lineGeometry;

	public override void OnInspectorGUI()
	{
		this.lineGeometry = (Lines)target as Lines;
		
		GUILayout.BeginHorizontal();

		if(GUILayout.Button("Add Line"))
		{
			this.lineGeometry.AddLine();
		}

		if(GUILayout.Button("Remove Last"))
		{
			this.lineGeometry.RemoveLast();
		}
		GUILayout.EndHorizontal();

		base.DrawDefaultInspector();
	}
}
