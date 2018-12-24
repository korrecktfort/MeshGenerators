using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
	#region Points To Rays and Lines

	// shortest distance point to line
	public static float DistancePointToLine(this Line line, Vector3 point)
	{
		return Vector3.Cross(line.forward, point - line.start).sqrMagnitude;
	}

	// shortest distance point to ray
	public static float DistancePointToRay(this Ray ray, Vector3 point)
	{
		return Vector3.Cross(ray.direction, point - ray.origin).sqrMagnitude;
	}

	// point on the line that is the shortest distance to this.point
	public static Vector3 IntersectionFromPointToLine(this Line line, Vector3 point)
	{
		return line.start + line.forward * Vector3.Dot(line.forward, point - line.start);
	}

	// point on the ray that is the shortest distance to point
	public static Vector3 IntersectionFromPointToRay(this Ray ray, Vector3 point)
	{
		return ray.origin + ray.direction * Vector3.Dot(ray.direction, point - ray.origin);
	}

	// Closest Point Between Two Lines
	public static Vector3 ClosestCenterPointBetweenTwoLines(Line l1, Line l2)
	{
		Vector3 linePoint1 = l1.start;
		Vector3 lineVec1 = l1.forward;

		Vector3 linePoint2 = l2.end;
		Vector3 lineVec2 = -l2.forward;

		float a = lineVec1.x * lineVec1.x + lineVec1.y * lineVec1.y + lineVec1.z * lineVec1.z;
		float b = lineVec1.x * lineVec2.x + lineVec1.y * lineVec2.y + lineVec1.z * lineVec2.z;
		float e = lineVec2.x * lineVec2.x + lineVec2.y * lineVec2.y + lineVec2.z * lineVec2.z;

		float d = a * e - b * b;

		if(d!= 0.0f)
		{
			Vector3 r = linePoint1 - linePoint2;
			float c = lineVec1.x * r.x + lineVec1.y * r.y + lineVec1.z * r.z;
			float f = lineVec2.x * r.x + lineVec2.y * r.y + lineVec2.z * r.z;

			float s = (b * f - c * e) / d;
			float t = (a * f - c * b) / d;

			return ((linePoint1 + lineVec1 * s) + (linePoint2 + lineVec2 * t)) * 0.5f;
		}

		return Vector3.zero;
	}

	public static Vector3 ClosestCenterPointBetweenTwoLines(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{
		float a = lineVec1.x * lineVec1.x + lineVec1.y * lineVec1.y + lineVec1.z * lineVec1.z;
		float b = lineVec1.x * lineVec2.x + lineVec1.y * lineVec2.y + lineVec1.z * lineVec2.z;
		float e = lineVec2.x * lineVec2.x + lineVec2.y * lineVec2.y + lineVec2.z * lineVec2.z;

		float d = a * e - b * b;

		if (d != 0.0f)
		{
			Vector3 r = linePoint1 - linePoint2;
			float c = lineVec1.x * r.x + lineVec1.y * r.y + lineVec1.z * r.z;
			float f = lineVec2.x * r.x + lineVec2.y * r.y + lineVec2.z * r.z;

			float s = (b * f - c * e) / d;
			float t = (a * f - c * b) / d;

			return ((linePoint1 + lineVec1 * s) + (linePoint2 + lineVec2 * t)) * 0.5f;
		} 
		
		return Vector3.zero;
	}

	#endregion

	#region Utilities.Line

	public static float Distance(this Utilities.Line line)
	{
		return (line.end - line.start).magnitude;
	} 

    public static Vector3 Forward(this Utilities.Line line)
    {
        return (line.end - line.start).normalized;
    }
	
    public static Vector3 Up(this Utilities.Line line)
    {
		return Vector3.Cross(line.Forward(), line.Right()).normalized;
    }

	public static Vector3 Right(this Utilities.Line line)
	{
		float _dot = Vector3.Dot(Vector3.up, line.Forward());

		if(Mathf.Abs(_dot) > 0.9999f)
		{
			return Vector3.Cross(Vector3.forward, line.Forward()).normalized;
		}

		return Vector3.Cross(Vector3.up, line.Forward()).normalized;
	}

	public static Quaternion LookRotation(this Utilities.Line line)
	{
		return Quaternion.LookRotation(line.Forward(), line.Up());
	}

	#endregion

	public static void ResetPosition(this Transform trans)
    {
        trans.position = Vector3.zero;
    }
}