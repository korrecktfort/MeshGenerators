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

		if(GUILayout.Button("Add Line at Lines End"))
		{
			this.lineGeometryPoint.AddLine();
		}

		GUILayout.EndHorizontal();
	}

	private void OnSceneGUI()
	{
		if(this.lineGeometryPoint == null)
		{
			return;
		}

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

		if (!this.lineGeometryPoint.showLineRotationGizmo)
		{
			return;
		}

		Lines lines = this.lineGeometryPoint.lines;

		if(lines == null)
		{
			return;
		}

		int registeredLinesIndex = lines.GetLinesPointIndex(this.lineGeometryPoint.transform);

		if (registeredLinesIndex == -1)
		{
			return;
		}

		Lines.RegisteredLine[] registeredLines = lines.RegisteredLines;
		Line line = registeredLines[registeredLinesIndex].line;

		if (registeredLines == null || registeredLines.Length == 0 || line == null)
		{
			return;
		}

		Vector3 center = line.center;
		EditorGUI.BeginChangeCheck();
		Quaternion rot = Handles.RotationHandle(line.lookRotation, center);

		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(target, "Rotate Line Single Selection");
			registeredLines[registeredLinesIndex].line.LineRotation = rot.eulerAngles.z;

			lines.RegisteredLines = registeredLines;
			lines.OnInternalValuesChange(true);
		}
	}
}
