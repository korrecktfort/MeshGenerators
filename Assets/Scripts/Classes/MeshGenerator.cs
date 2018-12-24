using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour {

	/// <summary>
	/// Basic Class to create Triangles and Squares
	/// Able To Flip Triangles Directions
	/// </summary>

	public struct Triangle{
		/// <summary>
		/// 	v2
		/// 	* *
		/// 	*  *
		/// 	v1**v3
		/// </summary>

		public Triangle(Vector3 v1, Vector3 v2, Vector3 v3){
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
		}
		public Vector3 v1;
		public Vector3 v2;
		public Vector3 v3;

		public Vector3 normal{
			get { 
				return new Plane(v1, v2, v3).normal;
			}
		}
	}

	public struct Quad{

		public Quad(Triangle t1, Triangle t2){
			this.t1 = t1;
			this.t2 = t2;
		}

		public Quad(Triangle t1, Vector3 v1){
			this.t1 = t1;
			this.t2 = new Triangle(t1.v3, t1.v2, v1);
		}

		/// <summary>
		/// 	v2******v4
		/// 	* *  t2 *
		/// 	*   *   *
		/// 	* t1  * *
		/// 	v1******v3
		/// </summary>

		public Quad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4){
			this.t1 = new Triangle(v1, v2, v3);
			this.t2 = new Triangle(v3, v2, v4);
		}

		public Triangle t1;
		public Triangle t2;
	}
		
	public struct GeneratedLineReferences
	{
		public GeneratedLineReferences(Line line, int t1Index, int t2Index){
			this.line = line;
			this.t1Index = t1Index;
			this.t2Index = t2Index;
		}

		public Line line;
		public int t1Index;
		public int t2Index;
	}

	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangleIndices = new List<int> ();
	List <Triangle> triangles = new List<Triangle> ();
	List<GeneratedLineReferences> genLinesRefs = new List<GeneratedLineReferences> ();
	
	MeshFilter meshFilter;
	MeshRenderer meshRenderer;
	Mesh mesh;
	int submeshIndex = 0;

	[Header("Mesh Generator Settings")]
	private Material material;
	
	// Gizmos Vertice Normals
	//public bool displayNormals = false;
	//public Color normalColor = Color.blue;
	//public float normalLength = 0.125f;


	internal void Awake(){
		this.meshFilter = GetComponent<MeshFilter> ();
		this.meshRenderer = GetComponent<MeshRenderer> ();
		this.material = Resources.Load<Material>("EditorMaterials/custom_standart");
		this.meshRenderer.material = this.material;
		this.mesh = new Mesh ();
		this.mesh.name = "generatedMesh";
		this.meshFilter.mesh = mesh;
	}

	internal void RedrawMesh(){
		mesh.Clear ();
		mesh.SetVertices (this.vertices);
		mesh.SetTriangles (this.triangleIndices, this.submeshIndex);
		mesh.RecalculateNormals ();
		// this.meshRenderer.material = this.material;
	}

	internal void ClearMesh(){
		this.triangleIndices.Clear ();
		this.genLinesRefs.Clear ();
		this.triangles.Clear ();
		this.vertices.Clear();
		this.mesh.Clear ();
	}
		
	#region ChangeTriangles


	#endregion

	#region Adding

	public void AddLine(Line line, float thickness){
		thickness *= 0.5f;
		Vector3 offset = line.right * thickness;

		Vector3 q1 = line.start - offset;
		Vector3 q2 = line.end - offset;
		Vector3 q3 = line.start + offset;
		Vector3 q4 = line.end + offset;
		
		AddQuad (new Quad (q1, q2, q3, q4));

		this.genLinesRefs.Add (new GeneratedLineReferences(line, this.triangles.Count - 2, this.triangles.Count - 1));
	
		RedrawMesh();
	}

	public void AddLineNoRedraw(Line line, float thickness)
	{
		thickness *= 0.5f;
		Vector3 offset = line.right * thickness;

		Vector3 q1 = line.start - offset;
		Vector3 q2 = line.end - offset;
		Vector3 q3 = line.start + offset;
		Vector3 q4 = line.end + offset;

		AddQuad(new Quad(q1, q2, q3, q4));

		this.genLinesRefs.Add(new GeneratedLineReferences(line, this.triangles.Count - 2, this.triangles.Count - 1));
	}

	//public void AddLinesNoRedrawMethod01(Line[] lines, float thickness)
	//{
	//	thickness *= 0.5f;

	//	Vector3 q1 = Vector3.zero;
	//	Vector3 q2 = Vector3.zero;
	//	Vector3 q3 = Vector3.zero;
	//	Vector3 q4 = Vector3.zero;

	//	for (int i = 1; i <= lines.Length - 1; i++)
	//	{
	//		Line l1 = lines[i - 1];
	//		Line l2 = lines[i];

	//		Vector3 offset1 = l1.right * thickness;
	//		Vector3 offset2 = l2.right * thickness;

	//		if (i == 1) {
	//			q1 = l1.start - offset1;
	//			q2 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(q1, l1.forward, l2.end - offset2, -l2.forward);
	//			q3 = l1.start + offset1;
	//			q4 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(q3, l1.forward, l2.end + offset2, -l2.forward);

	//			AddQuad(new Quad(q1, q2, q3, q4));
	//		}

	//		if (i > 1 && i <= lines.Length - 1)
	//		{
	//			q1 = q2;
	//			q2 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(q1, l1.forward, l2.end - offset2, -l2.forward);
	//			q3 = q4;
	//			q4 = ExtensionMethods.ClosestCenterPointBetweenTwoLines(q3, l1.forward, l2.end + offset2, -l2.forward);

	//			AddQuad(new Quad(q1, q2, q3, q4));
	//		}

	//		if (i == lines.Length - 1)
	//		{
	//			q1 = q2;
	//			q2 = l2.end - offset2;
	//			q3 = q4;
	//			q4 = l2.end + offset2;
	//			AddQuad(new Quad(q1, q2, q3, q4));
	//		}

	//	}
	//}

	public void AddQuadPositionsNoRedraw(Vector3[] positions)
	{
		for (int i = 0; i <= positions.Length - 1; i+=4)
		{
			AddQuad(new Quad(positions[i], positions[i + 1], positions[i + 2], positions[i + 3]));
		}
	}

	internal void AddTriangle(Triangle triangle){
		this.vertices.Add (triangle.v1);
		this.vertices.Add (triangle.v2);
		this.vertices.Add (triangle.v3);

		int index = triangles.Count * 3;

		this.triangleIndices.Add (index);
		this.triangleIndices.Add (index + 1);
		this.triangleIndices.Add (index + 2);
	
		this.triangles.Add (triangle);

		RedrawMesh ();
	}

	void AddTriangleNoRedraw(Triangle triangle){
		this.vertices.Add (triangle.v1);
		this.vertices.Add (triangle.v2);
		this.vertices.Add (triangle.v3);

		int index = triangles.Count * 3;

		this.triangleIndices.Add (index);
		this.triangleIndices.Add (index + 1);
		this.triangleIndices.Add (index + 2);

		this.triangles.Add (triangle);
	}
		
	internal void AddQuad(Quad quad){
		AddTriangleNoRedraw (quad.t1);
		AddTriangleNoRedraw (quad.t2);
	}

	void AddVertex(Vector3 v){
		if (this.triangles.Count <= 0)
			return;
		
		Quad quad = new Quad (this.triangles [this.triangles.Count - 1], v);
		AddTriangle (quad.t2);
	}

	#endregion

	#region Cleaning
	void FlipTriangleIndices(int index){
		index *= 3;

		int v1 = this.triangleIndices [index];
		int v2 = this.triangleIndices [index + 1];
		int v3 = this.triangleIndices [index + 2];

		this.triangleIndices [index] = v3;
		this.triangleIndices [index + 2] = v1;

		RedrawMesh ();
	}

	void FlipTriangleVectors(int index){
		Triangle t = this.triangles [index];
		Triangle newT = new Triangle (t.v3, t.v2, t.v1);
		this.triangles [index] = newT;

		RedrawMesh ();
	}
	#endregion

	//void OnDrawGizmos(){
	//	if (displayNormals)
	//	{
	//		Gizmos.color = this.normalColor;
	//		float length = this.normalLength;
	//		Vector3 p = this.transform.position;

	//		foreach (Triangle t in this.triangles)
	//		{
	//			Gizmos.DrawLine(p + t.v1, p + t.v1 + t.normal * length);
	//			Gizmos.DrawLine(p + t.v2, p + t.v2 + t.normal * length);
	//			Gizmos.DrawLine(p + t.v3, p + t.v3 + t.normal * length);
	//		}
	//	} 
	//}

}
