﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour {

	public Vector3[] points;

	public void Reset()
	{
		points = new Vector3[] {
			transform.position + Vector3.forward,
			transform.position + Vector3.forward * 2.0f,
			transform.position + Vector3.forward * 3.0f,
			transform.position + Vector3.forward * 4.0f
		};
	}

	public Vector3 GetPoint(float t)
	{
		return transform.TransformPoint(Bezier.GetPoint(points[0], points[1], points[2], points[3], t));
	}

	public Vector3 GetVelocity(float t) {
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[0], points[1], points[2], points[3], t))
			- transform.position;
	}

	public Vector3 GetDirection(float t)
	{
		return GetVelocity(t).normalized;
	}
}
