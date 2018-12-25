using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Lines))]
public class LinesControl : Editor {

	Lines lines;

	public override void OnInspectorGUI()
	{
		this.lines = (Lines)target as Lines;
		
		GUILayout.BeginHorizontal();

		if(GUILayout.Button("Add Line"))
		{
			this.lines.AddLine();
		}

//		if(GUILayout.Button("Remove Last"))
//		{
//			this.lines.RemoveLast();
//		}

		GUILayout.EndHorizontal();

		base.DrawDefaultInspector();
	}
}
