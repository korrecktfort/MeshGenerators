using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode, DisallowMultipleComponent]
public class Lines : MonoBehaviour {

	#region Variables

	#region Gizmos & Interaction Settings
	[Header("Gizmo Settings")]
	private float labelDistance = 0.5f;

	[SerializeField] bool showLineRotationGizmos = true;
	public bool ShowLineRotationGizmos
	{
		get
		{
			return this.showLineRotationGizmos;
		}
	}

	private bool autoSelectNewPoint = false;

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

	#region smoothing
	[Header("Smoothing")]
	[SerializeField] bool smooth = true;
	internal bool Smooth {
		get {
			return this.smooth;
		}
	}

	[SerializeField] bool smoothEnds = true;

	[Range(0, 10)]
	[SerializeField] int smoothSteps = 5;
	internal int SmoothSteps {
		get {
			return this.smoothSteps;
		}
	}

	[Range(0.0f, 1.0f)]
	float smoothDistance = 0.741f;
	internal float SmoothDistance {
		get {
			return this.smoothDistance;
		}
	}

	[Range(0.0f, 1.0f)]
	private float smoothDistanceFactor = 0.66f;

	private List<Vector3> currentSmoothPositions = new List<Vector3>();

	private Line[] currentSmoothLines;
	public Line[] SmoothLines
	{
		get
		{
			return CalcSmoothLines();
		}
	}

	#endregion

	#region Registration

	[System.Serializable]
	public struct RegisteredLine
	{
		public RegisteredLine(Transform start, Transform end)
		{
			this.start = start;
			this.end = end;
			this.line = new Line(start.position, end.position);
		}

		public RegisteredLine(float lineRotation, Transform start, Transform end)
		{
			this.start = start;
			this.end = end;
			this.line = new Line(start.position, end.position);
			this.line.LineRotation = lineRotation;
		}

		public RegisteredLine(RegisteredLine registeredLine)
		{
			this.start = registeredLine.start;
			this.end = registeredLine.end;
			this.line = new Line(start.position, end.position);
			this.line.LineRotation = registeredLine.line.LineRotation;
		}

		public void Refresh()
		{
			if(this.line != null && this.start != null && this.end != null)
			{
				line.start = this.start.position;
				line.end = this.end.position;
			}
		}

		public Line line;
		public Transform start;
		public Transform end;
	}

	[SerializeField]
	RegisteredLine[] registeredLines;
	public RegisteredLine[] RegisteredLines
	{
		get
		{
			return this.registeredLines;
		}

		set
		{
			registeredLines = value;
		}
	}

	public Line[] PlacedLines
	{
		get
		{
			return CalcPlacedLines();
		}
	}

	#endregion

	#region Placed Lines & Points

	[SerializeField]
	List<Transform> points = new List<Transform>();

	internal int CurrentPointsCount {
		get {
			return this.points.Count;
		}
	}
	int lastPointsCount;

	#endregion

	#endregion

	#region Properties & Returns

	public Line[] CurrentLines{
		get
		{
			Line[] currentLines = this.smooth ? this.SmoothLines : this.PlacedLines;

			if(currentLines != null && currentLines.Length <= 0)
			{
				return null;
			}

			return currentLines;
		}
	}

	public struct DeltaLine
	{
		public DeltaLine(float deltaDistance, Line line, bool valid = true)
		{
			this.deltaDistance = deltaDistance;
			this.line = line;
			this.valid = valid;
		}

		public DeltaLine(bool valid)
		{
			this.deltaDistance = 0.0f;
			this.line = new Line(Vector3.forward);
			this.valid = valid;
		}

		public Vector3 DeltaPosition()
		{
			return line.start + line.forward * deltaDistance;
		}

		public Quaternion DeltaRotation()
		{
			return line.lookRotation;
		}

		public float deltaDistance;
		public Line line;
		public bool valid;
	}

	public float Distance
	{
		get
		{
			float distance = 0.0f;

			Line[] currentLines = this.CurrentLines;

			if (currentLines != null)
			{
				foreach (Line l in currentLines)
				{
					distance += l.distance;
				}
			}

			return distance;
		}
	}

	public Vector3 PositionAtDistance(float distance)
	{
		DeltaLine deltaLine = CurrentDeltaLine(distance);

		if(!deltaLine.valid)
		{
			return Vector3.zero;
		}

		if (distance <= 0.0f)
		{
			return deltaLine.line.start;
		}

		if (distance >= this.Distance)
		{
			return deltaLine.line.end;
		}

		return deltaLine.DeltaPosition();
	}

	public Vector3 PositionAtNormalizedDistance(float normalizedDistance)
	{
		float distance = this.Distance * Mathf.Clamp01(normalizedDistance);
		return PositionAtDistance(distance);
	}

	public Quaternion RotationAtDistance(float distance)
	{
		DeltaLine deltaLine = CurrentDeltaLine(distance);

		if (!deltaLine.valid)
		{
			return Quaternion.identity;
		}

		return deltaLine.line.lookRotation;
	}

	public Quaternion RotationAtNormalizedDistance(float normalizedDistance)
	{
		float distance = this.Distance * Mathf.Clamp01(normalizedDistance);
		return RotationAtDistance(distance);
	}

	private DeltaLine CurrentDeltaLine(float distance)
	{
		Line[] currentLines = this.CurrentLines;

		if(currentLines == null)
		{
			return new DeltaLine(false);
		}

		if (distance <= 0.0f)
		{
			return new DeltaLine(0.0f, currentLines[0]);
		}

		if (distance >= this.Distance)
		{
			Line l = currentLines[currentLines.Length - 1];
			return new DeltaLine(l.distance, l);
		}

		// when in between
		float currentDistance = 0.0f;

		for (int i = 0; i <= this.registeredLines.Length; i++)
		{
			float currentLineDistance = currentLines[i].distance;

			if ((currentDistance + currentLineDistance) >= distance)
			{
				float deltaDistance = distance - currentDistance;
				return new DeltaLine(deltaDistance, currentLines[i]);
			}

			currentDistance += currentLineDistance;
		}

		return new DeltaLine(false);
	}

	#endregion

	public delegate void LineActions();
	public LineActions OnPointsValuesChange;

	public void OnInternalValuesChange(bool calcLines = false)
	{
		RefreshRegisteredLines();

		if (calcLines)
		{
			if (smooth)
			{
				CalcSmoothLines();
			}
			else
			{
				CalcPlacedLines();
			}
		}

		if (OnPointsValuesChange != null)
		{
			OnPointsValuesChange();
		}
	}

	#region Messages

#if UNITY_EDITOR

	public void OnValidate()
	{
		OnInternalValuesChange(true);
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
					Handles.Label(this.points[i - 1].position + Vector3.up * this.labelDistance, i + "/" + count);
				}
			}

			if(this.points[count - 1] != null)
			{
				Handles.Label(this.points[count - 1].position + Vector3.up * this.labelDistance, count + "/" + count);
			}
		}

		if (this.smooth && this.currentSmoothLines != null && this.currentSmoothLines.Length > 0)
		{

			float smoothDirSize = 0.25f;

			foreach(Line l in this.currentSmoothLines)
			{
				if (l == null)
					continue;

				Vector3 center = l.center;
				// Gizmos.DrawSphere(l.start, smoothDirSize);

				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(l.start, l.end);

				Gizmos.color = Color.green;
				Gizmos.DrawLine(center, center + l.up * smoothDirSize);

				Gizmos.color = Color.red;
				Gizmos.DrawLine(center, center + l.right * smoothDirSize);

				Gizmos.color = Color.blue;
				Gizmos.DrawLine(center, center + l.forward * smoothDirSize);
			}
			
		}
	}

#endif

	#endregion

	#region Registration Action

	public void PositionValuesChanged(Transform t)
	{
		OnInternalValuesChange(true);
	}

	public void BuildNewRegistration()
	{
		this.registeredLines = null;

		List<RegisteredLine> newRegisteredLines = new List<RegisteredLine>();

		for(int i = 1; i <= this.points.Count - 1; i++)
		{
			RegisteredLine newRegistration = new RegisteredLine(this.points[i - 1], this.points[i]);
			newRegisteredLines.Add(newRegistration);
		}

		this.registeredLines = newRegisteredLines.ToArray();
	}

	public void AddedPointAtEnd()
	{

		List<RegisteredLine> newRegisteredLines = this.registeredLines.ToList();
		
		int index = this.points.Count;
		RegisteredLine newRegistration = new RegisteredLine(this.points[index - 2], this.points[index - 1]);
		newRegisteredLines.Add(newRegistration);

		this.registeredLines = newRegisteredLines.ToArray();
	}

	public void PastedPointInBetween(int startIndex, int newIndex, int endIndex)
	{
		List<RegisteredLine> newRegisteredLines = new List<RegisteredLine>();

		if(startIndex > 0)
		{
			for(int i = 0; i <= startIndex - 1; i++)
			{
				newRegisteredLines.Add(new RegisteredLine(this.registeredLines[i]));
			}		
		}

		RegisteredLine firstRegistration = new RegisteredLine(this.points[startIndex], this.points[newIndex]);
		newRegisteredLines.Add(firstRegistration);
		
		if(newIndex < this.points.Count - 1)
		{
			RegisteredLine secondRegistration = new RegisteredLine(this.points[newIndex], this.points[endIndex]);
			newRegisteredLines.Add(secondRegistration);

			for (int h = newIndex; h <= this.registeredLines.Length - 1; h++)
			{
				newRegisteredLines.Add(new RegisteredLine(this.registeredLines[h]));
			}
		}

		this.registeredLines = newRegisteredLines.ToArray();
	}

	public void RemovedPointInBetween(int removeIndex)
	{
		List<RegisteredLine> newRegisteredLines = new List<RegisteredLine>();

		for (int i = 0; i <= removeIndex - 2; i++)
		{
			newRegisteredLines.Add(new RegisteredLine(this.registeredLines[i]));
		}

		RegisteredLine newRegistration = new RegisteredLine(this.registeredLines[removeIndex].line.LineRotation, this.points[removeIndex - 1], this.points[removeIndex + 1]);
		newRegisteredLines.Add(newRegistration);

		for (int i = removeIndex + 2; i <= this.points.Count - 1; i++)
		{
			RegisteredLine reRegister = new RegisteredLine(this.registeredLines[i - 1].line.LineRotation, this.points[i - 1], this.points[i]);
			newRegisteredLines.Add(reRegister);
		}

		this.registeredLines = newRegisteredLines.ToArray();
	}

	public void RefreshRegisteredLines()
	{
		if(this.registeredLines != null && this.registeredLines.Length > 0)
		{
			foreach(RegisteredLine r in this.registeredLines)
			{
				if(r.line != null)
				{
					r.Refresh();
				}
			}
		}
	}

	#endregion

	#region Calculate Smooth Lines

	internal Line[] CalcPlacedLines()
	{
		List<Line> newLines = new List<Line>();
		
		if(this.registeredLines != null && this.registeredLines.Length > 0)
		{
			foreach(RegisteredLine r in this.registeredLines)
			{
				newLines.Add(new Line(r.line, r.line.LineRotation));
			}

			return newLines.ToArray();
		}

		return null;
	}

	internal Line[] CalcSmoothLines()
	{
		Line[] lines = CalcPlacedLines();
			   
		List<Vector3> smoothPositions = new List<Vector3>();
		List<float> lineRotations = new List<float>();
		
		if(lines == null || lines.Length <= 0)
		{
			return null;
		}

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
				lineRotations.Add(l1.LineRotation);
			}

			if (!this.smoothEnds && startSectionAvailable)
			{
				smoothPositions.Add(l1.center);
				lineRotations.Add(l1.LineRotation);
			}

			// Bezier Calc Start
			Vector3 bezierStart = l1.end - l1.forward * l1.distance * this.smoothDistance * endSmoothDistanceFactorBezierStart;
			Vector3 bezierCenter = l1.end;
			Vector3 bezierEnd = l2.start + l2.forward * l2.distance * this.smoothDistance * endSmoothDistanceFactorBezierEnd;

			for (int s = 1; s <= this.smoothSteps - 1; s++)
			{
				float currentStep = s / (float)this.smoothSteps;
				smoothPositions.Add(Bezier.GetPoint(bezierStart, bezierCenter, bezierEnd, currentStep));
				lineRotations.Add(Mathf.Lerp(l1.LineRotation, l2.LineRotation, currentStep));
			}
			// Bezier Calc End

			if (!this.smoothEnds && endSectionAvailable)
			{
				smoothPositions.Add(l2.center);
				lineRotations.Add(l2.LineRotation);
			}

			if (endSectionAvailable)
			{
				smoothPositions.Add(l2.end);
				lineRotations.Add(l2.LineRotation);
			}
		}
			
		this.currentSmoothPositions = smoothPositions;

		List<Line> smoothLines = new List<Line>();

		for (int i = 1; i <= smoothPositions.Count - 1; i++)
		{
			Vector3 start = smoothPositions[i - 1];
			Vector3 end = smoothPositions[i];

			Line l = new Line(start, end);
			l.LineRotation = lineRotations[i - 1];

			smoothLines.Add(l);
		}

		this.currentSmoothLines = smoothLines.ToArray ();

		return smoothLines.ToArray();
	}

	#endregion

	#region Adding, Duplicating, Deleting and Sorting Points
	
	public void AddPointAtEnd()
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

		int currentCount = this.points.Count;

		if(currentCount == 2 && this.lastPointsCount == 1)
		{
			BuildNewRegistration();
		}

		if(currentCount > 2 && this.lastPointsCount >= 2)
		{
			AddedPointAtEnd();
		}

		if(currentCount < 2 && this.lastPointsCount >= 2)
		{
			this.registeredLines = null;
		}

		OnInternalValuesChange(true);

		this.lastPointsCount = currentCount;
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
			int removeIndex = this.points.IndexOf(t);
			int lastIndex = this.points.Count - 1;

			if(removeIndex == 0)
			{
				List<RegisteredLine> newRegisteredLines = this.registeredLines.ToList();

				if(newRegisteredLines != null && newRegisteredLines.Count > 0)
				{
					newRegisteredLines.RemoveAt(0);
				}
				this.registeredLines = newRegisteredLines.ToArray();
			}
			else if(removeIndex == lastIndex)
			{
				List<RegisteredLine> newRegisteredLines = this.registeredLines.ToList();
				if (newRegisteredLines != null && newRegisteredLines.Count > 0)
				{
					newRegisteredLines.RemoveAt(lastIndex - 1);
				}
				this.registeredLines = newRegisteredLines.ToArray();
			}
			else
			{
				RemovedPointInBetween(removeIndex);
			}
			
			this.points.RemoveAt(removeIndex);

			if (autoSelectNewPoint)
			{
				Selection.activeGameObject = this.points[removeIndex].gameObject;
			}

			ResortPoints ();

			OnInternalValuesChange(true);

			this.lastPointsCount = this.points.Count;

			
		}

	}

	public void OnPastePoint(Transform copy)
	{
		EditorApplication.delayCall -= SortInPasted;

		this.copy = copy;

		RefreshRegisteredLines();

		EditorApplication.delayCall += SortInPasted;

	}

	void SortInPasted()
	{
		Transform myTransform = this.transform;
		Transform pasted = myTransform.GetChild(myTransform.childCount - 1);

		int copyIndex = this.points.IndexOf(copy);
		points.Insert(copyIndex + 1, pasted);
		
		foreach (Transform t in this.points)
		{
			t.SetAsLastSibling();
		}

		PastedPointInBetween(copyIndex , copyIndex + 1, copyIndex + 2);
	
		ResortPoints();

		OnInternalValuesChange(true);

		this.lastPointsCount = this.points.Count;
	}

	#endregion
}