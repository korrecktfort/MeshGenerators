using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Lines))]
public class LinesControl : Editor {

	public override void OnInspectorGUI()
	{
		Lines lines = (Lines)target as Lines;
		
		GUILayout.BeginHorizontal();

		if(GUILayout.Button("Add Point"))
		{
			lines.AddPointAtEnd();
		}

		if (GUILayout.Button("Recalculate All Lines"))
		{
			lines.BuildNewRegistration(true);
		}

		GUILayout.EndHorizontal();

		base.DrawDefaultInspector();
	}

	private void OnSceneGUI()
	{
		Lines lines = (Lines)target as Lines;

		Lines.RegisteredLine[] registeredLines = lines.RegisteredLines;

		if (registeredLines == null || registeredLines.Length == 0)
			return;

		int length = registeredLines.Length;

		for(int i = 0; i <= length - 1; i++)
		{
			Line line = registeredLines[i].line;
			if (line == null)
				continue;

			if (lines.ShowLineRotationGizmos)
			{
				Vector3 center = line.center;
				
				EditorGUI.BeginChangeCheck();
				Quaternion rot = Handles.RotationHandle(line.lookRotation, center);
				
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(target, "Rotate Line");
					registeredLines[i].line.LineRotation(rot.eulerAngles.z);

					lines.RegisteredLines = registeredLines;
					lines.OnInternalValuesChange(true);
					break;
				}
			}
		}
	}
}
