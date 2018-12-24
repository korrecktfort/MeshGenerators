using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoDrawer : MonoBehaviour {


	[Header("Sphere")]
	[SerializeField] bool drawSphere = true;
	[SerializeField] float radius = 0.25f;

	[Header("Settings")]
	[SerializeField] bool wireframe;
	[SerializeField] Color color = Color.blue;
	
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

	}
}
