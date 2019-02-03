using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasuringLines : MonoBehaviour {

	[Header("Measuring")]
	public Lines lines;
	public float linesDistance;
	public Vector3 currentPosition;
	public Quaternion currentRotation;

	[Header("Control"), SerializeField]
	public float currentDistance;

	[Range(0.0f, 1.0f), SerializeField]
	public float normalizedDistance;
	
	private void Measure()
	{
		if(this.currentDistance < 0.0f)
		{
			if (this.lines.circle)
			{
				this.currentDistance = this.linesDistance - 0.1f;
			} else
			{
				this.currentDistance = 0.0f;
			}

		}

		if(this.currentDistance > this.linesDistance)
		{
			if (this.lines.circle)
			{
				this.currentDistance = 0.1f;
			} else
			{
				this.currentDistance = this.linesDistance - 0.00001f;
			}

		}

		this.linesDistance = this.lines.Distance;
		this.currentPosition = this.lines.PositionAtDistance(this.currentDistance);
		this.currentRotation = this.lines.RotationAtDistance(this.currentDistance);

		this.transform.position = this.currentPosition;
		this.transform.rotation = this.currentRotation;

	}
	
	private void OnValidate()
	{
		if (this.lines != null)
		{
			this.lines.OnPointsValuesChange -= Measure;	
			this.lines.OnPointsValuesChange += Measure;

			Measure();
		}

	}

}
