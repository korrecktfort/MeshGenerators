using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonoLine)), CanEditMultipleObjects]
public class MonoLineControl : Editor {

	private void OnSceneGUI()
	{
		MonoLine myMonoLine = (MonoLine)target as MonoLine;
		Line line = myMonoLine.line;
		Vector3 center = line.center;

		EditorGUI.BeginChangeCheck();

		Quaternion rot = Handles.RotationHandle(line.lookRotation, center);
		
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(target, "Rotate Line");
			line.LineRotation(rot.eulerAngles.z);
		}
	}

}
