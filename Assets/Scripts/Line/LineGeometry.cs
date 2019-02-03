using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Lines)), DisallowMultipleComponent, ExecuteInEditMode]
public class LineGeometry : MeshGenerator {

	[Header("Test Geometry")]
	[Range(0.0f, 25.0f)]
	[SerializeField] float lineThickness = 1.0f;

	[Header("Connect Planes")]
	[SerializeField] bool connectPlanes;

	private Lines lines;

	Vector3[] currentRingPoints;

	private new void Awake()
	{
		base.Awake();
	}

	public void DrawLineMesh()
	{
		base.ClearMesh();

		Line[] newLines = this.lines.CurrentLines;

		if(newLines == null || this.currentRingPoints == null)
		{
			return;
		}

		BuildMesh(newLines);

		base.RedrawMesh();
	}

#if UNITY_EDITOR

	private void OnValidate()
	{
		
		float offset = this.lineThickness * 0.5f;
		Vector3 v1 = Vector3.right * -offset;
		Vector3 v2 = Vector3.up * offset;
		Vector3 v3 = Vector3.right * offset;
		this.currentRingPoints = new Vector3[3] { v1, v2, v3 };
		
		if(this.lines == null)
		{
			this.lines = GetComponent<Lines>();
		}

		lines.OnPointsValuesChange -= DrawLineMesh;
		lines.OnPointsValuesChange += DrawLineMesh;
			
		DrawLineMesh();	
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

			if(i == 1)
			{
				v1 = l1.start - offset01;
				v2 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(v1, l1.forward, l2.end - offset02, -l2.forward);


				v3 = l1.start + offset01;
				v4 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(v3, l1.forward, l2.end + offset02, -l2.forward);

				positions.Add(v1);
				positions.Add(v2);
				positions.Add(v3);
				positions.Add(v4);
			}

			if(i > 1 && i <= lines.Length - 1)
			{
				positions.Add(v2);
				v2 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(l1.start - offset01, l1.forward, l2.end - offset02, -l2.forward);

				positions.Add(v2);

				positions.Add(v4);
				v4 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(l1.start + offset01, l1.forward, l2.end + offset02, -l2.forward);

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

	void BuildMesh(Line[] lines)
	{
		int ringVertexCount = this.currentRingPoints.Length;
		List<Vector3> list = new List<Vector3>();

		foreach(Line l in lines)
		{
			Vector3 origin = l.start;
			Vector3 right = l.right;
			Vector3 up = l.up;

			foreach(Vector3 v in this.currentRingPoints)
			{
				Vector3 v1 = origin + right * v.x + up * v.y;		

				list.Add(v1);
			}
		}

		Vector3[] array = list.ToArray();
		
		for (int i = 0; i <= array.Length - 1; i+=ringVertexCount)
		{
			if(i + 3 > array.Length - 1)
			{
				break;
			}
			Vector3 v1 = array[i];
			Vector3 v3 = array[i + 1];


			Vector3 v2 = array[i + 2];
			Vector3 v4 = array[i + 3];

			Quad q = new Quad(v1, v2, v3, v4);
			
			base.AddQuad(q);
		}
	}
	
	#endregion

}