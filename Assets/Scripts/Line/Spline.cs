using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour {

	public Line[] lines;

	private void Awake()
	{
		
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		foreach (Line line in lines) {
			Gizmos.DrawLine(line.start, line.end);
		}
	}

#endif
}
