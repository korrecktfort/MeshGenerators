using UnityEngine;

[System.Serializable]
public class Line : MonoBehaviour{
	/// <summary>
	/// Class Line
	/// </summary>
	public Vector3 start;
	public Vector3 end;

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

			if(Mathf.Abs(_dot) > 0.9999f)
			{
				return Vector3.Cross(Vector3.forward, this.forward).normalized;
			}

			return Vector3.Cross(Vector3.up, this.forward).normalized;
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
		get{ 
			return Quaternion.LookRotation(this.forward, this.up);
		}
	}

	public void LookAt(Transform transform){
		this.forward = (transform.position - this.start).normalized;
	}
}
