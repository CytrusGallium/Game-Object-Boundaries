using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meta2dBounds
{

	public enum Meta2dBoundsVisibility
	{

		Full,
		Partial,
		BehindCamera,
		NotVisible

	}

	public Vector3 topLeft;
	public Vector3 topRight;
	public Vector3 bottomRight;
	public Vector3 bottomLeft;

	public float GetWidth()
	{
		return topRight.x - topLeft.x;
	}

	public float GetHeight()
	{
		return topLeft.y - bottomLeft.y;
	}

	public float GetArea()
	{
		return GetWidth () * GetHeight ();
	}

	public Rect GetRect()
	{
		return new Rect (topLeft.x, Screen.height - topLeft.y, GetWidth (), GetHeight ());
	}

	public float GetScreenCoverage ()
	{
		Meta2dBoundsVisibility visibility = GetVisibility ();

		if (visibility == Meta2dBoundsVisibility.NotVisible || visibility == Meta2dBoundsVisibility.BehindCamera)
			return 0;

		return GetArea () / (Screen.width * Screen.height);
	}

	public Meta2dBoundsVisibility GetVisibility ()
	{

		// ---------
		Rect tmpScreenRect = new Rect (0, 0, Screen.width, Screen.height);
		int c = 0;

		// ---------
		if (tmpScreenRect.Contains (topLeft))
			c++;

		if (tmpScreenRect.Contains (topRight))
			c++;
		
		if (tmpScreenRect.Contains (bottomRight))
			c++;
		
		if (tmpScreenRect.Contains (bottomLeft))
			c++;

		// ---------
		if (c == 0)
			return Meta2dBoundsVisibility.NotVisible;
		else if (c == 4) {
			
			if (MetaVector.isBehindCamera (topLeft) && MetaVector.isBehindCamera (topRight) && MetaVector.isBehindCamera (bottomRight) && MetaVector.isBehindCamera (bottomLeft)) {
				return Meta2dBoundsVisibility.BehindCamera;
			}
			return Meta2dBoundsVisibility.Full;

		} else {
			if (MetaVector.isBehindCamera (topLeft) && MetaVector.isBehindCamera (topRight) && MetaVector.isBehindCamera (bottomRight) && MetaVector.isBehindCamera (bottomLeft)) {
				return Meta2dBoundsVisibility.BehindCamera;
			}
			return Meta2dBoundsVisibility.Partial;
		}


	}
}
