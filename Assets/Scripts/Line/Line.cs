using UnityEngine;

[System.Serializable]
public class Line{
	/// <summary>
	/// Class Line
	/// </summary>
	
	[SerializeField]
	public Vector3 start;

	[SerializeField]
	public Vector3 end;

	[SerializeField]
	private float lineRotation = 0.0f;
	public float LineRotation{
		get{ 
			return this.lineRotation;
		}

		set{ 
			this.lineRotation = value;
		}
	}

	public Line(Vector3 start, Vector3 end)
	{
		this.start = start;
		this.end = end;
	}

	public Line(Vector3 end)
	{
		this.start = Vector3.zero;
		this.end = end;
	}

	public Line (Line line)
	{
		this.start = line.start;
		this.end = line.end;
	}

	public Line(Line line, float lineRotation)
	{
		this.start = line.start;
		this.end = line.end;
		this.lineRotation = lineRotation;
	}

	public Line(Vector3 start, Vector3 end, float lineRotation)
	{
		this.start = start;
		this.end = end;
		this.lineRotation = lineRotation;
	}

	public Vector3 forward{
		get{
			return (this.end - this.start).normalized;
		}

		set{ 
			float distance = this.distance;
			this.end = this.start + value * distance;
		}
	}

	public Vector3 right{
		get{ 
			float _dot = Vector3.Dot(Vector3.up, this.forward);
			Vector3 dir = Vector3.zero;

			if(Mathf.Abs(_dot) > 0.9999f)
			{
				dir = Vector3.Cross(Vector3.forward, this.forward).normalized;
				dir = Quaternion.AngleAxis(this.lineRotation, this.forward) * dir;
				return dir;
			}

			dir = Vector3.Cross(Vector3.up, this.forward).normalized;
			dir = Quaternion.AngleAxis(this.lineRotation, this.forward) * dir;
			return dir;
		}
	}

	public Vector3 up{
		get { 
			return Vector3.Cross(this.forward, this.right).normalized;
		}
	}

	public float distance {
		get { 
			return (this.end - this.start).magnitude;
		}
		set {
			Vector3 forward = this.forward;
			end = this.start + forward * value;
		}
	}

	public Vector3 center
	{
		get	{
			Vector3 v;
			Vector3 s = this.start;
			Vector3 f = this.forward;
			float d = this.distance * 0.5f;

			v.x = s.x + f.x * d;
			v.y = s.y + f.y * d;
			v.z = s.z + f.z * d;

			return v;
		}
	}

	public Quaternion lookRotation{
		get
		{
			return Quaternion.LookRotation(this.forward, this.up);
		}
	}

	public void LookAt(Transform transform){
		this.forward = (transform.position - this.start).normalized;
	}


}
