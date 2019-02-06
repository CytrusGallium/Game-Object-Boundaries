using UnityEngine;
using System.Collections;

public class MetaCollider : Collider {

	// ...
	public static Bounds GetBoundsFromColliders(Collider[] ParamColliders) {
	
		// multiple colliders
		float minX = ParamColliders[0].bounds.min.x;
		float maxX = ParamColliders[0].bounds.max.x;
		float minY = ParamColliders[0].bounds.min.y;
		float maxY = ParamColliders[0].bounds.max.y;
		float minZ = ParamColliders[0].bounds.min.z;
		float maxZ = ParamColliders[0].bounds.max.z;

		int i;
		for (i = 0; i < ParamColliders.Length; i++) {

			if (ParamColliders [i].bounds.min.x < minX)
				minX = ParamColliders [i].bounds.min.x;

			if (ParamColliders [i].bounds.max.x > maxX)
				maxX = ParamColliders [i].bounds.max.x;

			if (ParamColliders [i].bounds.min.y < minY)
				minY = ParamColliders [i].bounds.min.y;

			if (ParamColliders [i].bounds.max.y > maxY)
				maxY = ParamColliders [i].bounds.max.y;

			if (ParamColliders [i].bounds.min.z < minZ)
				minZ = ParamColliders [i].bounds.min.z;

			if (ParamColliders [i].bounds.max.z > maxZ)
				maxZ = ParamColliders [i].bounds.max.z;

		}

		Bounds tmpBounds = new Bounds ();

		float sizeX = maxX - minX;
		float sizeY = maxY - minY;
		float sizeZ = maxZ - minZ;
		tmpBounds.size = new Vector3 (sizeX, sizeY, sizeZ);

		float avgX = (maxX + minX) / 2;
		float avgY = (maxY + minY) / 2;
		float avgZ = (maxZ + minZ) / 2;

		tmpBounds.center = new Vector3 (avgX, avgY, avgZ);

		return tmpBounds;
	
	}
}
