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

		GUILayout.EndHorizontal();
	}

	private void OnSceneGUI()
	{
		Event e = Event.current;
		if(this.lineGeometryPoint != null && e.type == EventType.ValidateCommand && e.commandName == "Duplicate" || e.commandName == "Paste")
		{
			if(Selection.objects.Length > 1)
			{
				Debug.LogWarning("Multiple Duplication Of LinesPoint Is Not Supported");
				return;
			} else
			{
				this.lineGeometryPoint.OnPaste();
				Undo.RecordObject(this.lineGeometryPoint, "Point Add");
			}

		}

		if (this.lineGeometryPoint != null && e.type == EventType.ValidateCommand && e.commandName == "Delete" || e.commandName == "SoftDelete")
		{
			
			Undo.RecordObject(this.lineGeometryPoint, "Point Delete");

		}
	}

	
}
