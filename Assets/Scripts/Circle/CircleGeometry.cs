using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Circle))]
public class CircleGeometry : MeshGenerator {

	/// <summary>
	/// Class that creates 3D Circles Combining Circle Class
	/// and Mesh Generator Class
	/// </summary>

	private Circle circle;

	private new void Awake(){
		base.Awake ();
		this.circle = GetComponent<Circle> ();
	}

	void Start(){
		this.circle.OnValueChange += this.DrawCircle;
	}

	void DrawCircle(){
		base.ClearMesh ();
		for (int i = 1; i < this.circle.vertices.Length; i++) {
			base.AddTriangle(new Triangle(Vector3.zero, this.circle.vertices[i - 1], this.circle.vertices[i]));
		}
		base.AddTriangle (new Triangle (Vector3.zero, this.circle.vertices [this.circle.vertices.Length - 1], this.circle.vertices [0]));
		base.RedrawMesh ();
	}

}
