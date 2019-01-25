using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MonoLine : MonoBehaviour {

	[SerializeField]
	Line line = null;
	public Line Line
	{
		get
		{
			return this.line;
		}
	}

	private void OnValidate()
	{
		if(this.line == null)
		{
			this.line = new Line(Vector3.forward);
		}		
	}

	private void OnDrawGizmosSelected()
	{
		if(line != null)
		{
			Gizmos.DrawLine(this.line.start, this.line.end);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(this.line.end, this.line.end + this.line.right * 0.1f);

			Gizmos.color = Color.blue;
			Gizmos.DrawLine(this.line.end, this.line.end + this.line.forward * 0.1f);

			Gizmos.color = Color.green;
			Gizmos.DrawLine(this.line.end, this.line.end + this.line.up * 0.1f);
		}
	}

}
