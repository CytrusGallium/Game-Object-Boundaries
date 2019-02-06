using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoStuffWithGOB : MonoBehaviour {

	public GameObject Tank;
	public GameObject MiniTank1;
	public GameObject MiniTank2;
	public GameObject HangingBox;

	// Use this for initialization
	void Start () {

//		print ("GOB START");

		Tank.GetComponent<GameObjectBoundaries> ().PositionAbove (transform.position);

		MiniTank1.GetComponent<GameObjectBoundaries> ().BoundScale (1.5f, 1.5f, 1.5f);
		MiniTank1.GetComponent<GameObjectBoundaries> ().PositionAbove (new Vector3(MiniTank1.transform.position.x, transform.position.y, MiniTank1.transform.position.z));

		MiniTank2.GetComponent<GameObjectBoundaries> ().PositionBelow (HangingBox.transform.position);
		
	}
	
	// Update is called once per frame
	void Update () {

		GameObject go = GameObject.Find ("Tank");

		GameObjectBoundaries component = go.GetComponent<GameObjectBoundaries> ();

		Bounds bounds = component.GetBounds (true);

		print (bounds);
		
	}

	// ...
	void OnGUI()
	{
		Meta2dBounds tmp2dBounds = Tank.GetComponent<GameObjectBoundaries> ().GetScreenSpaceBounds (true);
		float coverage = tmp2dBounds.GetScreenCoverage ();
		string visibility = tmp2dBounds.GetVisibility ().ToString ();

		GUI.skin.box.alignment = TextAnchor.MiddleLeft;
		GUI.Box (new Rect (0, 0, 256f, 48f), "Tank Screen Coverage = " + coverage.ToString () + "\n" + "Tank Visibility = " + visibility);
	}
}
