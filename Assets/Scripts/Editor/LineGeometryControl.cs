using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LineGeometry))]
public class LineGeometryControl : Editor {

	LineGeometry lineGeometry;

	public override void OnInspectorGUI()
	{
		this.lineGeometry = (LineGeometry)target as LineGeometry;
		
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

		if(GUILayout.Button("Redraw Mesh"))
		{
			this.lineGeometry.DrawLineMesh();
		}

		base.DrawDefaultInspector();
	}
}
