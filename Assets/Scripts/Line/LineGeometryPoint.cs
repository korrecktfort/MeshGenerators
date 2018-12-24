using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode, DisallowMultipleComponent]
public class LineGeometryPoint : MonoBehaviour {

	LineGeometry lineGeometry;

	Vector3 lastPos = Vector3.zero;

	private void Awake()
	{
		this.lineGeometry = this.transform.parent.GetComponent<LineGeometry>();
	}

	public void AddLine()
	{
		this.lineGeometry.AddLine();
	}

	public void RemoveLast()
	{
		this.lineGeometry.RemoveLast();
	}

	public void DrawLineMesh()
	{
		this.lineGeometry.DrawLineMesh();
	}

#if UNITY_EDITOR
	private void Update()
	{
		if(Selection.activeTransform == this.transform)
		{
			if(this.transform.position != this.lastPos)
			{
				this.lastPos = this.transform.position;
				DrawLineMesh();
			}
		}
	}
#endif

	private void OnDestroy()
	{
		this.lineGeometry.RemovePoint(this.transform);
		this.lineGeometry.DrawLineMesh();
		Selection.activeGameObject = this.lineGeometry.gameObject;
	}

	public void OnPaste()
	{
		this.lineGeometry.OnPastePoint(this.transform);
	}
}
