using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LinesPoint))]
public class LinesPointControl : Editor {

	LinesPoint lineGeometryPoint;

	public override void OnInspectorGUI()
	{
		this.lineGeometryPoint = (LinesPoint)target as LinesPoint;

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
	}

	private void OnSceneGUI()
	{
		Event e = Event.current;
		if(this.lineGeometryPoint != null && e.type == EventType.ValidateCommand && e.commandName == "Duplicate" || e.commandName == "Paste")
		{
			this.lineGeometryPoint.OnPaste();
		}
	}
}
