using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaCgoChild : MonoBehaviour {

	public bool MonitorPosition;
	public bool MonitorRotation;
	public bool MonitorScale;

	private Vector3 lastPostion;
	private Quaternion lastRotation;
	private Vector3 lastScale;

	private GameObjectBoundaries ultimateParent;

	public GameObjectBoundaries UltimateParent {
		get {
			return ultimateParent;
		}
		set {
				ultimateParent = value;
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (CheckPositionChanges () || CheckRotationChanges () || CheckScaleChanges ()) {

			UltimateParent.Update3dBounds ();

		}
		
	}

	// =================================================================================================================================
	// Check if position has changed since the last check
	protected bool CheckPositionChanges()
	{
		if (!MonitorPosition)
			return false;

		if (lastPostion != transform.position)
		{
			lastPostion = transform.position;
			return true;
		}

		return false;
	}

	// =================================================================================================================================
	// Check if rotation has changed since the last check
	protected bool CheckRotationChanges()
	{
		if (!MonitorRotation)
			return false;

		if (lastRotation != transform.rotation)
		{
			lastRotation = transform.rotation;
			return true;
		}

		return false;
	}

	// =================================================================================================================================
	// Check if scale has changed since the last check
	protected bool CheckScaleChanges()
	{
		if (!MonitorScale)
			return false;

		if (lastScale != transform.localScale)
		{
			lastScale = transform.localScale;
			return true;
		}

		return false;
	}
}
