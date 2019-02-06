using UnityEngine;
using System.Collections;

public class MetaRenderer : Renderer {

	// Get Mesh from any renderer type
	public static Mesh GetMesh(Renderer ParamRenderer) {

		if (ParamRenderer is MeshRenderer) {
			MeshFilter tmpMF = ParamRenderer.GetComponent<MeshFilter> ();
			return tmpMF.mesh;
		} else if (ParamRenderer is SkinnedMeshRenderer) {
			SkinnedMeshRenderer tmpSMR = ParamRenderer as SkinnedMeshRenderer;
			return tmpSMR.sharedMesh;
		}

		return null;

	}

	// ...
	public static Bounds GetBoundsFromRenderers(Renderer[] ParamRenderers) {

		// multiple colliders
		float minX = ParamRenderers[0].bounds.min.x;
		float maxX = ParamRenderers[0].bounds.max.x;
		float minY = ParamRenderers[0].bounds.min.y;
		float maxY = ParamRenderers[0].bounds.max.y;
		float minZ = ParamRenderers[0].bounds.min.z;
		float maxZ = ParamRenderers[0].bounds.max.z;

		int tmp1 = (int)minZ;
		int tmp2 = (int)maxZ;

		int i;
		for (i = 0; i < ParamRenderers.Length; i++) {

			if (ParamRenderers [i].bounds.min.x < minX)
				minX = ParamRenderers [i].bounds.min.x;

			if (ParamRenderers [i].bounds.max.x > maxX)
				maxX = ParamRenderers [i].bounds.max.x;

			if (ParamRenderers [i].bounds.min.y < minY)
				minY = ParamRenderers [i].bounds.min.y;

			if (ParamRenderers [i].bounds.max.y > maxY)
				maxY = ParamRenderers [i].bounds.max.y;

			if (ParamRenderers [i].bounds.min.z < minZ)
				minZ = ParamRenderers [i].bounds.min.z;

			if (ParamRenderers [i].bounds.max.z > maxZ)
				maxZ = ParamRenderers [i].bounds.max.z;

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

	public static Bounds GetBoundsFromRenderers_UsingEncapsulation(Renderer[] ParamRenderers) {

		Bounds tmpBounds = new Bounds ();
		tmpBounds.center = ParamRenderers [0].bounds.center;

		int i;
		for (i = 0; i < ParamRenderers.Length; i++) {

			tmpBounds.Encapsulate (ParamRenderers [i].bounds.max);
			tmpBounds.Encapsulate (ParamRenderers [i].bounds.min);

		}

		return tmpBounds;

	}
}
