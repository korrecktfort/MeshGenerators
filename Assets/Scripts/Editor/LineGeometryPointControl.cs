using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LineGeometryPoint))]
public class LineGeometryPointControl : Editor {

	LineGeometryPoint lineGeometryPoint;

	public override void OnInspectorGUI()
	{
		this.lineGeometryPoint = (LineGeometryPoint)target as LineGeometryPoint;

		base.DrawDefaultInspector();

		GUILayout.BeginHorizontal();

		if(GUILayout.Button("Add Line"))
		{
			this.lineGeometryPoint.AddLine();
		}

		if(GUILayout.Button("Remove Last"))
		{
			this.lineGeometryPoint.RemoveLast();
		}
		GUILayout.EndHorizontal();

		if(GUILayout.Button("Recalculate Mesh"))
		{
			this.lineGeometryPoint.DrawLineMesh();
		}
	}

	private void OnSceneGUI()
	{
		Event e = Event.current;
		if(e.type == EventType.ValidateCommand && e.commandName == "Duplicate" || e.commandName == "Paste")
		{
			this.lineGeometryPoint.OnPaste();
		}
	}
}
