using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 tmpRot = transform.rotation.eulerAngles;

		transform.rotation = Quaternion.Euler (new Vector3(tmpRot.x, tmpRot.y + 2, tmpRot.z));
		
	}
}
