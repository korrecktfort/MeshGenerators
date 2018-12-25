using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GizmoDrawer : MonoBehaviour {

	[Header("Transform Directions")]
	[SerializeField] bool showDirections = false;
	[SerializeField] float lineLength = 1.0f;

	[Header("Sphere")]
	[SerializeField] bool drawSphere = true;
	[SerializeField] float radius = 0.25f;

	[Header("Settings")]
	[SerializeField] bool wireframe = false;
	[SerializeField] Color color = Color.blue;

	[System.Serializable]
	public struct GizmoSettings
	{
		public bool showDirections;
		public float lineLength;

		public bool drawSphere;
		public float radius;

		public bool wireframe;
		public Color color;
	}

	public void OverrideSettings(GizmoSettings settings){
		this.showDirections = settings.showDirections;
		this.lineLength = settings.lineLength;
		this.drawSphere = settings.drawSphere;
		this.radius = settings.radius;
		this.wireframe = settings.wireframe;
		this.color = settings.color;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = this.color;

		if (this.drawSphere)
		{
			if (wireframe)
			{
				Gizmos.DrawWireSphere(this.transform.position, this.radius);
			} else
			{
				Gizmos.DrawSphere(this.transform.position, this.radius);
			}
		}

		if (showDirections) {
			if (Tools.pivotRotation == PivotRotation.Local) {
				Transform ownTrans = this.transform;
				Vector3 ownPos = ownTrans.position;

				Gizmos.color = Color.red;
				Gizmos.DrawLine(ownPos, ownPos + ownTrans.right * this.lineLength);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(ownPos, ownPos + ownTrans.up * this.lineLength);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(ownPos, ownPos + ownTrans.forward * this.lineLength);

			} else {
				Vector3 ownPos = this.transform.position;

				Gizmos.color = Color.red;
				Gizmos.DrawLine(ownPos, ownPos + Vector3.right * this.lineLength);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(ownPos, ownPos + Vector3.up * this.lineLength);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(ownPos, ownPos + Vector3.forward * this.lineLength);
			}
		}
	}
}
