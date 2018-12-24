using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LineTest : MonoBehaviour {
	
	public Line line;

	private void Start()
	{
		line = new Line(Vector3.one);
	}

}
