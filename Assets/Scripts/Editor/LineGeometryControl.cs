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

		if(GUILayout.Button("Redraw Mesh"))
		{
			this.lineGeometry.DrawLineMesh();
		}

		base.DrawDefaultInspector();
	}
}
