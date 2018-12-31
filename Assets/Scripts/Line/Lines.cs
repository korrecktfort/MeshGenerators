using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode, DisallowMultipleComponent]
public class Lines : MonoBehaviour {

	#region Variables

	[Header("Gizmo Settings")]
	[SerializeField] float labelDistance = 0.5f;
	[SerializeField] bool enableSmoothGizmos;
	[SerializeField] bool overrideGizmoSettingsInPoints;
	[SerializeField] GizmoDrawer.GizmoSettings gizmoOverrideSettings;

	[Header("Smoothing")]
	[SerializeField] bool smooth;
	internal bool Smooth{
		get{
			return this.smooth;
		}
	}
	private bool lastSmooth = false;

	[SerializeField] bool smoothEnds;
	private bool lastSmoothEnds = false;

	[Range(0, 10)]
	[SerializeField] int smoothSteps = 5;
	internal int SmoothSteps{
		get { 
			return this.smoothSteps;
		}
	}
	private int lastSmoothSteps = 5;

	[Range(0.0f, 1.0f)]
	[SerializeField] float smoothDistance = 1.0f;
	internal float SmoothDistance{
		get{ 
			return this.smoothDistance;
		}
	}
	private float lastSmoothDistance = 1.0f;

	[Range(0.0f, 1.0f)]
	public float smoothDistanceFactor = 0.8f;
	private float lastSmoothDistanceFactor = 0.8f;

	private List<Vector3> currentSmoothPositions = new List<Vector3>();
	
	// [Header("Line Geometry Settings")]
	public Line[] currentPlacedLines;
	private Line[] currentSmoothLines;

	private bool autoSelectNewPoint = false;
	private List<Transform> points = new List<Transform>();
	internal int CurrentPointsCount{
		get{ 
			return this.points.Count;
		}
	}

	private Transform copy;
	private string pointBaseName = "Line Point";
	public string PointBaseName
	{
		get
		{
			return this.pointBaseName;
		}
		set
		{
			this.pointBaseName = value;
			ResortPoints();
		}
	}

	#endregion

	public delegate void LineActions();
	public LineActions OnLineValuesChange;

	[Range(-1.0f, 1.0f)]
	public float dotProductValue = 0.0f;
	private float lastDotProductValue;

#if UNITY_EDITOR

	public void OnInternalValuesChange(bool calcLines = false)
	{
		if (calcLines) {
			if (smooth) {
				CalcSmoothLinesSingleBezier ();
			} else {
				CalcPlacedLines ();
			}
		}

		if(OnLineValuesChange != null)
		{
			OnLineValuesChange();
		}
	}

	private void Update()
	{
		if (this.dotProductValue != this.lastDotProductValue) {
			OnInternalValuesChange (true);
			this.lastDotProductValue = this.dotProductValue;
			return;
		}

		if (this.smooth != this.lastSmooth)
		{
			OnInternalValuesChange (true);
			this.lastSmooth = this.smooth;
			return;
		}

		if (this.smooth && this.smoothSteps != this.lastSmoothSteps)
		{
			OnInternalValuesChange (true);
			this.lastSmoothSteps = this.smoothSteps;
			return;
		}

		if (this.smooth && this.smoothDistance != this.lastSmoothDistance)
		{
			OnInternalValuesChange (true);
			this.lastSmoothDistance = this.smoothDistance;
			return;
		}

		if (this.smooth && this.smoothDistanceFactor != this.lastSmoothDistanceFactor)
		{
			OnInternalValuesChange (true);
			this.lastSmoothDistanceFactor = this.smoothDistanceFactor;
			return;
		}

		if (this.smoothEnds != this.lastSmoothEnds)
		{
			OnInternalValuesChange (true);
			this.lastSmoothEnds = this.smoothEnds;
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
				if (this.points[i - 1] != null && this.points[i] != null)
				{
					Gizmos.DrawLine(this.points[i - 1].position, this.points[i].position);
				}

				Handles.Label(this.points[i - 1].position + Vector3.up * this.labelDistance, i + "/" + count);
			}
			Handles.Label(this.points[count - 1].position + Vector3.up * this.labelDistance, count + "/" + count);
		}

		if (this.smooth && this.enableSmoothGizmos)
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

		if (this.overrideGizmoSettingsInPoints) {
			foreach (GizmoDrawer g in GetComponentsInChildren<GizmoDrawer>(true)) {
				g.OverrideSettings (this.gizmoOverrideSettings);
			}
		}
	}

#endif

	#region Calculate Smooth Lines

	internal Line[] CalcPlacedLines()
	{
		List<Line> newLines = new List<Line>();

		for (int i = 1; i <= this.points.Count - 1; i++)
		{
			Vector3 start = this.points[i - 1].position;
			Vector3 end = this.points[i].position;

			start = this.transform.InverseTransformPoint(start);
			end = this.transform.InverseTransformPoint(end);

			Line l = new Line(start, end);

			if (l.distance < 0.1f)
			{
				continue;
			}
				
			if(newLines.Count > 0 
				&& (Vector3.Dot (newLines[0].forward, l.forward) < 0.0f)){
				l.LineRotation = 180.0f;
			}

			Transform currentPoint = this.points [i - 1];
			if (i == this.points.Count - 1) {
				currentPoint = this.points [i];
			}

			currentPoint.forward = l.forward;

			newLines.Add(l);
		}

		currentPlacedLines = newLines.ToArray ();

		return currentPlacedLines;
	}

	internal Line[] CalcSmoothLinesSingleBezier()
	{
		Line[] lines = CalcPlacedLines();
		List<Vector3> smoothPositions = new List<Vector3>();
		ResortPoints ();

		for (int currentLine = 1; currentLine <= lines.Length - 1; currentLine++)
		{
			bool startSectionAvailable = (currentLine - 1 == 0);
			bool endSectionAvailable = (currentLine == lines.Length - 1);
			Line l1 = lines[currentLine - 1];
			Line l2 = lines[currentLine];
			float endSmoothDistanceFactorBezierStart = this.smoothDistanceFactor;
			float endSmoothDistanceFactorBezierEnd = this.smoothDistanceFactor;

			if (startSectionAvailable && this.smoothEnds)
			{
				endSmoothDistanceFactorBezierStart = 1.0f;
			}

			if (endSectionAvailable && this.smoothEnds)
			{
				endSmoothDistanceFactorBezierEnd = 1.0f;
			}

			if (startSectionAvailable)
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

			if(smoothLines.Count > 0 
				&& (Vector3.Dot (smoothLines[0].forward, l.forward) < this.dotProductValue)){
				l.LineRotation = 180.0f;
			}
				
			smoothLines.Add(l);
		}

		this.currentSmoothLines = smoothLines.ToArray ();

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
		GameObject obj = new GameObject(this.pointBaseName + " (" + (count + 1) + ")");
		obj.transform.SetParent(this.transform);
		Vector3 offset = this.transform.position;

		if (count == 1)
		{
			offset += Vector3.forward * 2.0f;
		}

		if (count > 1)
		{
			offset = this.points[count - 1].position + (this.points[count - 1].position - this.points[count - 2].position).normalized;
		}
		obj.transform.position = offset;
		obj.AddComponent<LinesPoint>();
		obj.AddComponent<GizmoDrawer>();
		points.Add(obj.transform);

		if (autoSelectNewPoint)
		{
			Selection.activeGameObject = obj;
		}

		if (OnLineValuesChange != null)
		{
			OnLineValuesChange();
		}
	}

	public void ResortPoints()
	{
		List<Transform> newPoints = new List<Transform>();

		foreach (Transform t in points)
		{
			if (t != null)
			{
				t.name = this.pointBaseName + " (" + (newPoints.Count + 1) + ")";
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

		ResortPoints ();
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

		foreach (Transform t in this.points)
		{
			t.SetAsLastSibling();
		}

		if (OnLineValuesChange != null)
		{
			OnLineValuesChange();
		}
	}

	#endregion
}
