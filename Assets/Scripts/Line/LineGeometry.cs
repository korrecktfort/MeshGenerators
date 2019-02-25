using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Lines)), DisallowMultipleComponent, ExecuteInEditMode]
public class LineGeometry : MeshGenerator {

	[Header("Test Geometry")]
	[Range(0.0f, 25.0f)]
	[SerializeField] float scale = 1.0f;

	[SerializeField] bool seamGeometry = true;

	[Header("Connect Planes")]
	[SerializeField] bool connectPlanes;

	private Lines lines;

	[SerializeField]
	private Vector2[] currentRingPoints;

	[System.Serializable]
	public struct SegmentVertexGroup
	{
		public SegmentVertexGroup(Vector3[] segmentVertices)
		{
			this.segmentVertices = segmentVertices;
		}

		public SegmentVertexGroup(SegmentVertexGroup segmentVertices)
		{
			this.segmentVertices = segmentVertices.segmentVertices;
		}

		public Vector3[] segmentVertices;
	}

	[System.Serializable]
	public struct SegmentQuadGroup
	{
		public SegmentQuadGroup(Quad[] segmentQuads)
		{
			this.segmentQuads = segmentQuads;
		}

		public Quad[] segmentQuads;
	}

	private new void Awake()
	{
		base.Awake();
	}

	public void DrawLineMesh()
	{
		base.ClearMesh();

		if(this.currentRingPoints == null || this.currentRingPoints.Length < 2)
		{
			return;
		}

		Line[] newLines = this.lines.CurrentLines;

		if(newLines == null)
		{
			return;
		}

		BuildMesh(newLines);

		base.RedrawMesh();
	}

#if UNITY_EDITOR

	private void OnValidate()
	{
		if(this.currentRingPoints == null || this.currentRingPoints.Length < 2)
		{
			return;
		}

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

			Vector3 offset01 = l1.right * this.scale * 0.5f;
			Vector3 offset02 = l2.right * this.scale * 0.5f;

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
		List<SegmentVertexGroup> vertexGroups = new List<SegmentVertexGroup>();
		List<SegmentQuadGroup> quadGroups = new List<SegmentQuadGroup>();

		foreach(Line l in lines)
		{
			Vector3 origin = l.start;
			Vector3 right = l.right;
			Vector3 up = l.up;

			List<Vector3> vertices = new List<Vector3>();
			// Calculate All Vertices
			foreach (Vector3 v in this.currentRingPoints)
			{
				vertices.Add(origin + right * v.x * this.scale + up * v.y * this.scale);
			}

			vertexGroups.Add(new SegmentVertexGroup(vertices.ToArray()));
		}

		if (this.lines.circle)
		{
			vertexGroups[0] = new SegmentVertexGroup(vertexGroups[vertexGroups.Count - 1]);
		}

		SegmentVertexGroup[] vertexGroupArray = vertexGroups.ToArray();

		for(int i = 1; i <= lines.Length - 1; i++)
		{
			SegmentVertexGroup s1 = vertexGroupArray[i - 1];
			SegmentVertexGroup s2 = vertexGroupArray[i];
			List <Quad> quads = new List<Quad>();

			for (int v = 1; v <= ringVertexCount - 1; v++)
			{
				Quad q = new Quad(s1.segmentVertices[v - 1], s2.segmentVertices[v - 1], s1.segmentVertices[v], s2.segmentVertices[v]);
				quads.Add(q);
			}

			if (this.seamGeometry)
			{
				Quad qSeam = new Quad(s1.segmentVertices[ringVertexCount - 1], s2.segmentVertices[ringVertexCount - 1], s1.segmentVertices[0], s2.segmentVertices[0]);
				quads.Add(qSeam);
			}

			quadGroups.Add(new SegmentQuadGroup(quads.ToArray()));
		}

		quadGroups.ToArray();

		foreach(SegmentQuadGroup sQuad in quadGroups)
		{
			foreach(Quad quad in sQuad.segmentQuads)
			{
				base.AddQuad(quad);
			}
		}
	}

	#endregion

}