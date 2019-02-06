using UnityEngine;
using System.Collections;

public class MetaVector : Object {

	// Maximize vector
	public static Vector3 MaximizeVector (Vector3 ParamVector) {
		float tmpMax = ParamVector.x;
		if (ParamVector.y > tmpMax)
			tmpMax = ParamVector.y;
		if (ParamVector.z > tmpMax)
			tmpMax = ParamVector.z;

		Vector3 result = new Vector3 (tmpMax, tmpMax, tmpMax);
		return result;
	}

	// Maximize vector using x and z length
	public static Vector3 MaximizeVectorXZ (Vector3 ParamVector) {

		float tmpMax = ParamVector.x;

		if (ParamVector.z > tmpMax)
			tmpMax = ParamVector.z;

		Vector3 result = new Vector3 (tmpMax, tmpMax, tmpMax);
		return result;
	}

	// Maximize vector using x and y length
	public static Vector3 MaximizeVectorXY (Vector3 ParamVector) {

		float tmpMax = ParamVector.x;

		if (ParamVector.y > tmpMax)
			tmpMax = ParamVector.y;

		Vector3 result = new Vector3 (tmpMax, tmpMax, tmpMax);
		return result;
	}

	// Get 2D polar coordinates from a vector
	public static MetaPolarCoords Get2DPolarCoords(Vector3 ParamVector) {
	
		MetaPolarCoords tmpPolarCoords = new MetaPolarCoords ();
		tmpPolarCoords.Distance = Mathf.Sqrt ((ParamVector.x * ParamVector.x) + (ParamVector.z * ParamVector.z));
		tmpPolarCoords.Angle = Mathf.Atan (ParamVector.z / ParamVector.x);


		return tmpPolarCoords;
	
	}

	// Get 2D rectangular coordinates from polar coordinates
	public static Vector3 Get2DCartesianCoords(MetaPolarCoords ParamPolarCoords) {

		Vector3 tmpCartesianCoords = new Vector3 (ParamPolarCoords.Distance*Mathf.Cos(ParamPolarCoords.Angle), 0, ParamPolarCoords.Distance*Mathf.Sin(ParamPolarCoords.Angle));
		return tmpCartesianCoords;

	}

	// Distance between two points in the XZ plane
	public static float DistanceXZ (Vector3 ParamA, Vector3 ParamB) {
	
		float x = ParamB.x - ParamA.x;
		float z = ParamB.z - ParamA.z;
		return Mathf.Sqrt (((x) * (x)) + ((z) * (z)));
	
	}

	// is point visible on screen (after using world to screen space convertion)
	public static bool isBehindCamera(Vector3 ParamScreenPoint)
	{
		if (ParamScreenPoint.z < 0) {
			return true;
		} else {
			return false;
		}
	}
}

public class MetaPolarCoords : Object {

	public float Distance;
	public float Angle;

}




