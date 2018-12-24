using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LineGeometry : MeshGenerator {

	[Header("Gizmo Settings")]
	[SerializeField] float labelDistance = 0.5f;
	[SerializeField] bool enableSmoothGizmos;

	[Header("Line Geometry Settings")]
	[SerializeField] bool autoSelectNewPoint = false;

	[Range(0.0f, 25.0f)]
	[SerializeField] float lineThickness = 1.0f;
	private float lastLineThickness = 1.0f;

	[Header("Connect Planes")]
	[SerializeField] bool connectPlanes;
	private bool lastConnectPlanes;
	
	[SerializeField] List<Transform> points = new List<Transform>();

	[Header("Smoothing")]
	[SerializeField] bool smooth;
	private bool lastSmooth = false;

	[SerializeField] bool smoothEnds;
	private bool lastSmoothEnds = false;

	[Range(0,10)]
	[SerializeField] int smoothSteps = 5;
	private int lastSmoothSteps = 5;

	[Range(0.0f, 1.0f)]
	[SerializeField] float smoothDistance = 1.0f;
	private float lastSmoothDistance = 1.0f;

	[Range(0.0f, 1.0f)]
	public float smoothDistanceFactor = 0.8f;
	private float lastSmoothDistanceFactor = 0.8f;

	private List<Vector3> currentSmoothPositions = new List<Vector3>();
	private Vector3[] currentSmoothPositionsDouble;
	private Transform copy;
	private string pointBaseName = "LG Point";

	private new void Awake()
	{
		base.Awake();
	}

	public void DrawLineMesh()
	{
		base.ClearMesh();
		ResortPoints();

		if (this.smooth && this.points.Count > 2 && this.smoothSteps > 1 && this.smoothDistance > 0.0f)
		{
			Line[] newLines = CalcSmoothLinesSingleBezier();

			if (this.connectPlanes)
			{
				base.AddQuadPositionsNoRedraw(CalcMeshPositions(newLines));
			} else
			{
				foreach (Line l in newLines)
				{
					base.AddLineNoRedraw(l, this.lineThickness);
				}
			}

		} else
		{
			Line[] newLines = CalcPlacedLines();

			if (connectPlanes)
			{
				base.AddQuadPositionsNoRedraw(CalcMeshPositions(newLines));
			} 
			else 
			{
				foreach (Line l in newLines)
				{
					base.AddLineNoRedraw(l, this.lineThickness);
				}
			}
		}

		base.RedrawMesh();

	}

#if UNITY_EDITOR

	private void Update()
	{
		if (this.lastLineThickness != this.lineThickness)
		{
			DrawLineMesh();
			this.lastLineThickness = this.lineThickness;
			return;
		}

		if (this.smooth != this.lastSmooth)
		{
			DrawLineMesh();
			this.lastSmooth = this.smooth;
			return;
		}

		if (this.smooth && this.smoothSteps != this.lastSmoothSteps)
		{
			DrawLineMesh();
			this.lastSmoothSteps = this.smoothSteps;
			return;
		}

		if (this.smooth && this.smoothDistance != this.lastSmoothDistance)
		{
			DrawLineMesh();
			this.lastSmoothDistance = this.smoothDistance;
			return;
		}

		if(this.smooth && this.smoothDistanceFactor != this.lastSmoothDistanceFactor)
		{
			DrawLineMesh();
			this.lastSmoothDistanceFactor = this.smoothDistanceFactor;
			return;
		}

		if (this.smoothEnds != this.lastSmoothEnds)
		{
			DrawLineMesh();
			this.lastSmoothEnds = this.smoothEnds;
			return;
		}

		if (this.connectPlanes != this.lastConnectPlanes)
		{			
			DrawLineMesh();
			this.lastConnectPlanes = this.connectPlanes;
			return;
		}

	}

	private void OnDrawGizmos()
	{
		
		if (this.points != null && this.points.Count > 0)
		{
			Gizmos.color = Color.blue;
			Handles.color = Color.black;
			int count = this.points.Count;

			for (int i = 1; i <= count - 1; i++)
			{
				if(this.points[i - 1] != null && this.points[i] != null)
				{
					Gizmos.DrawLine(this.points[i - 1].position, this.points[i].position);
				}

				Handles.Label(this.points[i - 1].position + Vector3.up * this.labelDistance, i + "/" + count);
			}
			Handles.Label(this.points[count - 1].position + Vector3.up * this.labelDistance, count + "/" + count);
		}
		
		
		if(this.smooth && this.enableSmoothGizmos)
		{
			Gizmos.color = Color.red;

			if (this.currentSmoothPositions != null && this.currentSmoothPositions.Count > 0)
			{
				foreach (Vector3 v in this.currentSmoothPositions)
				{
					Vector3 p = this.transform.TransformPoint(v);
					Gizmos.DrawSphere(p, 0.15f);
				}

				for (int i = 1; i <= this.currentSmoothPositions.Count - 1; i++)
				{
					Vector3 v1 = this.currentSmoothPositions[i - 1];
					Vector3 v2 = this.currentSmoothPositions[i];

					v1 = this.transform.TransformPoint(v1);
					v2 = this.transform.TransformPoint(v2);

					Gizmos.DrawLine(v1, v2);
				}
			}
		}

	}

#endif

	#region Calculate Positions for the Mesh Generator

	Vector3[] CalcMeshPositions(Line[] lines)
	{
		List<Vector3> positions = new List<Vector3>();

		Vector3 v1 = Vector3.zero;
		Vector3 v2 = Vector3.zero;
		Vector3 v3 = Vector3.zero;
		Vector3 v4 = Vector3.zero;

		for (int i = 1; i <= lines.Length - 1; i++)
		{
			Line l1 = lines[i - 1];
			Line l2 = lines[i];

			Vector3 offset01 = l1.right * this.lineThickness * 0.5f;
			Vector3 offset02 = l2.right * this.lineThickness * 0.5f;

			if(l1.forward == l2.forward)
			{
				positions.Add(v2);
				v2 = l2.end - offset02;
				positions.Add(v2);

				positions.Add(v4);
				v4 = l2.end + offset02;
				positions.Add(v4);
				continue;
			}

			if(i == 1)
			{
				v1 = l1.start - offset01;
				v2 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(v1, l1.forward, l2.end - offset02, -l2.forward);
				if (v2 == Vector3.zero)
				{
					v2 = l1.end - offset01;
				}

				v3 = l1.start + offset01;
				v4 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(v3, l1.forward, l2.end + offset02, -l2.forward);
				if (v4 == Vector3.zero)
				{
					v4 = l1.end + offset01;
				}
				positions.Add(v1);
				positions.Add(v2);
				positions.Add(v3);
				positions.Add(v4);
			}

			if(i > 1 && i <= lines.Length - 1)
			{
				positions.Add(v2);
				v2 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(l1.start - offset01, l1.forward, l2.end - offset02, -l2.forward);
				if (v2 == Vector3.zero)
				{
					v2 = l1.end - offset01;
				}
				positions.Add(v2);

				positions.Add(v4);
				v4 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(l1.start + offset01, l1.forward, l2.end + offset02, -l2.forward);
				if (v4 == Vector3.zero)
				{
					v4 = l1.end + offset01;
				}
				positions.Add(v4);

			}

			if (i == lines.Length - 1)
			{
				positions.Add(v2);
				v2 = l2.end - offset02;
				positions.Add(v2);

				positions.Add(v4);
				v4 = l2.end + offset02;
				positions.Add(v4);
			}
		}

		return positions.ToArray(); 

	}
	
	#endregion

	#region Calculate Smooth Lines

	Line[] CalcPlacedLines()
	{
		List<Line> newLines = new List<Line>();

		for (int i = 1; i <= this.points.Count - 1; i++)
		{
			Vector3 start = this.points[i - 1].position;
			Vector3 end = this.points[i].position;

			start = this.transform.InverseTransformPoint(start);
			end = this.transform.InverseTransformPoint(end);

			Line l = new Line(start, end);

			if(l.distance < 0.1f)
			{
				continue;
			}

			this.points[i].rotation = l.lookRotation;

			newLines.Add(l);
		}

		return newLines.ToArray();
	}

	Line[] CalcSmoothLinesSingleBezier()
	{
		Line[] lines = CalcPlacedLines();
		List<Vector3> smoothPositions = new List<Vector3>();
		
		for (int currentLine = 1; currentLine <= lines.Length - 1; currentLine++)
		{
			bool startSectionAvailable = (currentLine - 1 == 0);
			bool endSectionAvailable = (currentLine == lines.Length - 1);
			Line l1 = lines[currentLine - 1];
			Line l2 = lines[currentLine];
			float endSmoothDistanceFactorBezierStart = this.smoothDistanceFactor;
			float endSmoothDistanceFactorBezierEnd = this.smoothDistanceFactor;

			if(startSectionAvailable && this.smoothEnds)
			{
				endSmoothDistanceFactorBezierStart = 1.0f;
			}

			if (endSectionAvailable && this.smoothEnds)
			{
				endSmoothDistanceFactorBezierEnd = 1.0f;
			}
			
			if(startSectionAvailable)
			{
				smoothPositions.Add(l1.start);
			}

			if (!this.smoothEnds && startSectionAvailable)
			{
				smoothPositions.Add(l1.center);
			}

			// Bezier Calc Start
			Vector3 bezierStart = l1.end - l1.forward * l1.distance * this.smoothDistance * endSmoothDistanceFactorBezierStart;
			Vector3 bezierCenter = l1.end;
			Vector3 bezierEnd = l2.start + l2.forward * l2.distance * this.smoothDistance * endSmoothDistanceFactorBezierEnd;

			for (int s = 1; s <= this.smoothSteps - 1; s++)
			{
				smoothPositions.Add(Bezier.GetPoint(bezierStart, bezierCenter, bezierEnd, s / (float)this.smoothSteps));
			}
			// Bezier Calc End

			if (!this.smoothEnds && endSectionAvailable)
			{
				smoothPositions.Add(l2.center);
			}

			if (endSectionAvailable)
			{
				smoothPositions.Add(l2.end);
			}
		}

		this.currentSmoothPositions = smoothPositions;

		List<Line> smoothLines = new List<Line>();

		for (int i = 1; i <= smoothPositions.Count - 1; i++)
		{
			Vector3 start = smoothPositions[i - 1];
			Vector3 end = smoothPositions[i];

			Line l = new Line(start, end);
			smoothLines.Add(l);
		}

		return smoothLines.ToArray();
	}

	#endregion

	#region Adding, Duplicating, Deleting and Sorting Points

	public void AddLine()
	{
		foreach (Transform t in this.points)
		{
			if (t == null)
			{
				ResortPoints();
				break;
			}
		}

		int count = this.points.Count;
		GameObject obj = new GameObject(this.pointBaseName + " (" + count + ")");
		obj.transform.SetParent(this.transform);
		Vector3 offset = this.transform.position;

		if(count == 1)
		{
			offset += Vector3.forward * 2.0f;
		}

		if(count > 1)
		{
			offset = this.points[count - 1].position + (this.points[count - 1].position - this.points[count - 2].position).normalized;
		}
		obj.transform.position = offset;
		obj.AddComponent<LineGeometryPoint>();
		obj.AddComponent<GizmoDrawer>();
		points.Add(obj.transform);
		
		if (autoSelectNewPoint)
		{
			Selection.activeGameObject = obj;
		}

		DrawLineMesh();
	}

	public void RemoveLast()
	{
		if(this.points.Count <= 0)
		{
			return;
		}

		foreach (Transform t in this.points)
		{
			if (t == null)
			{
				ResortPoints();
				break;
			}
		}

		DestroyImmediate(this.points[this.points.Count - 1].gameObject);
		this.points.RemoveAt(this.points.Count - 1);

		if (autoSelectNewPoint)
		{
			Selection.activeGameObject = this.points[this.points.Count - 1].gameObject;
		}

		DrawLineMesh();
	}

	public void ResortPoints()
	{
		List<Transform> newPoints = new List<Transform>();

		foreach(Transform t in points)
		{
			if(t != null)
			{
				t.name = this.pointBaseName + " (" + newPoints.Count +")";
				newPoints.Add(t);
			}
		}

		points = newPoints;
	}

	public void RemovePoint(Transform t)
	{
		if (points.Contains(t))
		{
			points.RemoveAt(points.IndexOf(t));
		}
	}

	public void OnPastePoint(Transform copy)
	{
		this.copy = copy;
		EditorApplication.delayCall += SortInPasted;
			
	}

	void SortInPasted()
	{
		Transform myTransform = this.transform;
		Transform pasted = myTransform.GetChild(myTransform.childCount - 1);

		int copyIndex = this.points.IndexOf(copy);
		points.Insert(copyIndex + 1, pasted);

		ResortPoints();

		foreach(Transform t in this.points)
		{
			t.SetAsLastSibling();
		}

		DrawLineMesh();
	}

	#endregion

}