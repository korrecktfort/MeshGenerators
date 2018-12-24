using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour {

	[System.Serializable]
	public struct Line
    {
		public Vector3 start;
		public Vector3 end;
		public Vector3 End
		{
			set
			{
				end = value;
			}
		}

		public Line(Vector3 _start, Vector3 _end)
		{
			this.start = _start;
			this.end = _end;
		}

		public Line(Vector3 _end)
		{
			this.start = Vector3.zero;
			this.end = _end;
		}

		public Line (Line _line)
		{
			this.start = _line.start;
			this.end = _line.end;
		}

    }
}
