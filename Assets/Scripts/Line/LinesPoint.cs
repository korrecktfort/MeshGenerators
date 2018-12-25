using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode, DisallowMultipleComponent]
public class LinesPoint : MonoBehaviour {

	Lines lines;
	Vector3 lastPos = Vector3.zero;

#if UNITY_EDITOR

	private void Awake()
	{
		this.lines = this.transform.parent.GetComponent<Lines>();
	}

	public void AddLine()
	{
		this.lines.AddLine();
	}

//	public void RemoveLast()
//	{
//		this.lines.RemoveLast();
//	}

	public void OnPaste()
	{
		this.lines.OnPastePoint(this.transform);
	}
#endif

	private void Update()
	{
		if(Selection.activeTransform == this.transform)
		{
			if(this.transform.position != this.lastPos)
			{
				this.lastPos = this.transform.position;
				lines.OnInternalValuesChange();
			}
		}
	}

	private void OnDestroy()
	{
		this.lines.RemovePoint(this.transform);
		this.lines.OnInternalValuesChange();
		Selection.activeGameObject = this.lines.gameObject;
	}

}
