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
	public float lineRotation = 0.0f;

	[SerializeField]
	public Vector3 right, up, forward, center;

	[SerializeField]
	public Quaternion lookRotation;

	[SerializeField]
	public float distance;

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

	public Line(Line line)
	{
		this.start = line.start;
		this.end = line.end;
		this.lineRotation = line.lineRotation;
		this.forward = line.forward;
		this.right = line.right;
		this.up = line.up;
		this.center = line.center;
		this.distance = line.distance;
		this.lookRotation = line.lookRotation;
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

	public void ApplyData()
	{
		this.forward = Forward();
		this.right = Right();
		this.up = Up();
		this.center = Center();
		this.lookRotation = LookRotation();
		this.distance = Distance();
	}

	public Vector3 Forward()
	{
		return (this.end - this.start).normalized;
	}

	public void Forward(Vector3 newForward) { 
		this.end = this.start + newForward * this.distance;
		ApplyData();
	}
	
	public Vector3 Right() { 
		
		float _dot = Vector3.Dot(Vector3.up, this.forward);
		Vector3 dir = Vector3.zero;

		if(Mathf.Abs(_dot) > 0.99999f)
		{
			dir = Vector3.Cross(Vector3.forward, this.forward).normalized;
			dir = Quaternion.AngleAxis(this.lineRotation, this.forward) * dir;
			return dir;
		}

		dir = Vector3.Cross(Vector3.up, this.forward).normalized;
		dir = Quaternion.AngleAxis(this.lineRotation, this.forward) * dir;
		return dir;
	}

	public Vector3 Up(){
		return Vector3.Cross(this.forward, this.right).normalized;
	}



	public float Distance() {
		return (this.end - this.start).magnitude;
	}

	public void Distance(float newDistance) {
		this.end = this.start + this.forward * newDistance;
		ApplyData();
	}

	public Vector3 Center()
	{
		Vector3 v;
		Vector3 s = this.start;
		Vector3 f = this.forward;
		float d = this.distance * 0.5f;

		v.x = s.x + f.x * d;
		v.y = s.y + f.y * d;
		v.z = s.z + f.z * d;

		return v;
		
	}

	public Quaternion LookRotation(){
		return Quaternion.LookRotation(this.forward, this.up);
	}

	public void LookAt(Transform transform){
		this.end = transform.position;
		ApplyData();

	}

	public void LineRotation(float newRotation)
	{
		this.lineRotation = newRotation;
		ApplyData();
	}


}
