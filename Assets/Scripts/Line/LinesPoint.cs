using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode, DisallowMultipleComponent]
public class LinesPoint : MonoBehaviour {

	[SerializeField] public Lines lines;
	Vector3 lastPos = Vector3.zero;
	[SerializeField] public bool showLineRotationGizmo = true;

#if UNITY_EDITOR

	void OnValidate()
	{
		if(this.lines == null)
		{
			this.lines = this.transform.parent.GetComponent<Lines>();
		}
	}

	public void AddLine()
	{
		this.lines.AddPointAtEnd();
	}

	public void OnPaste()
	{
		this.lines.OnPastePoint(this.transform);
	}

#endif

	private void Update()
	{
		if (Selection.activeTransform == this.transform)
		{
			if (this.transform.position != this.lastPos)
			{
				this.lastPos = this.transform.position;

				this.lines.PositionValuesChanged();
			}
		}
	}

	private void OnDestroy()
	{
		this.lines.RemovePoint(this.transform);
	}

}
