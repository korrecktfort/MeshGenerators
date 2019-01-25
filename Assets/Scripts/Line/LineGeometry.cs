using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Lines)), DisallowMultipleComponent, ExecuteInEditMode]
public class LineGeometry : MeshGenerator {

	[Range(0.0f, 25.0f)]
	[SerializeField] float lineThickness = 1.0f;
	private float lastLineThickness = 1.0f;

	[Header("Connect Planes")]
	[SerializeField] bool connectPlanes;
	private bool lastConnectPlanes;

	private Lines lines;

	private new void Awake()
	{
		base.Awake();
		this.lines = GetComponent<Lines> ();
		lines.OnPointsValuesChange += DrawLineMesh;
	}

	public void DrawLineMesh()
	{
		base.ClearMesh();

		if (this.lines.Smooth && this.lines.CurrentPointsCount > 2 && this.lines.SmoothSteps > 1 && this.lines.SmoothDistance > 0.0f)
		{
			Line[] newLines = this.lines.CalcSmoothLines();

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
			Line[] newLines = this.lines.CalcPlacedLines();

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
			this.lines.OnInternalValuesChange ();
			this.lastLineThickness = this.lineThickness;
			return;
		}

		if (this.connectPlanes != this.lastConnectPlanes)
		{			
			DrawLineMesh();
			this.lastConnectPlanes = this.connectPlanes;
			return;
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

//			if(l1.forward == l2.forward)
//			{
//				positions.Add(v2);
//				v2 = l2.end - offset02;
//				positions.Add(v2);
//
//				positions.Add(v4);
//				v4 = l2.end + offset02;
//				positions.Add(v4);
//				continue;
//			}

			if(i == 1)
			{
				v1 = l1.start - offset01;
				v2 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(v1, l1.forward, l2.end - offset02, -l2.forward);
//				if (v2 == Vector3.zero)
//				{
//					v2 = l1.end - offset01;
//				}

				v3 = l1.start + offset01;
				v4 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(v3, l1.forward, l2.end + offset02, -l2.forward);
//				if (v4 == Vector3.zero)
//				{
//					v4 = l1.end + offset01;
//				}
				positions.Add(v1);
				positions.Add(v2);
				positions.Add(v3);
				positions.Add(v4);
			}

			if(i > 1 && i <= lines.Length - 1)
			{
				positions.Add(v2);
				v2 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(l1.start - offset01, l1.forward, l2.end - offset02, -l2.forward);
//				if (v2 == Vector3.zero)
//				{
//					v2 = l1.end - offset01;
//				}
				positions.Add(v2);

				positions.Add(v4);
				v4 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(l1.start + offset01, l1.forward, l2.end + offset02, -l2.forward);
//				if (v4 == Vector3.zero)
//				{
//					v4 = l1.end + offset01;
//				}
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

}