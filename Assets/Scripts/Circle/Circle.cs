using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Circle : MonoBehaviour {

	/// <summary>
	/// calculating points to create a circle
	/// </summary>

	public delegate void OnValuesChanged();
	public OnValuesChanged OnValueChange;

	[Range(3, 128)]
	public int vertexCount = 3;

	[Range(0.5f, 500.0f)]
	public float radius = 0.5f;

	internal Line[] lines;
	internal Vector3[] vertices;
	
	int currentEdgeCount = -1;
	float currentRadius = -1.0f;
	// Vector3 currentPosition;
	// Quaternion currentRotation;

	void Awake(){
		// this.currentPosition = this.transform.position;
		// this.currentRotation = this.transform.rotation;
	}

	public void CalcEdgePoints(){
		if (this.vertexCount < 3 || this.radius <= 0.0f)
			return;

		float angleStep = 360.0f / (float)this.vertexCount;
		Vector3 rotDir = this.transform.up;

		List<Vector3> points = new List<Vector3> ();

		points.Add (this.transform.forward * this.radius);

		for (int i = 1; i <= this.vertexCount; i++) {
			this.transform.RotateAround (Vector3.zero, rotDir, angleStep);
			points.Add(this.transform.forward * this.radius);
		}
		this.vertices = points.ToArray ();
	}

	#if UNITY_EDITOR
	void Update(){
		if (this.currentEdgeCount == this.vertexCount 
			&& this.currentRadius == this.radius
			// && this.currentPosition == this.transform.position
			// && this.currentRotation == this.transform.rotation
			)
			return;

		CalcEdgePoints ();

		if (OnValueChange != null) {
			OnValueChange ();
		}

		this.currentEdgeCount = this.vertexCount;
		this.currentRadius = this.radius;
		// this.currentPosition = this.transform.position;
		// this.currentRotation = this.transform.rotation;
	}

	#endif


}
