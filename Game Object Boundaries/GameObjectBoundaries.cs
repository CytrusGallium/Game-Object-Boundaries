using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class GameObjectBoundaries : MonoBehaviour
{

	protected static Material boundsMaterial;
	private static List<GameObject> childBuffer;

	public enum Meta2dPlaneType {XY,XZ}

    public enum MetaBoundsType
    {

        RendererBounds,
        ColliderBounds

    }

	[Header("Movement Plane")]
	public Meta2dPlaneType Used2DPlane = Meta2dPlaneType.XZ;

	[Header("Bounds")]
    public MetaBoundsType UsedBoundsType = MetaBoundsType.RendererBounds;
	public bool DrawSpaceBounds = false;
	public Color SpaceBoundsColor = Color.green;
	public bool DrawScreenSpaceBounds = false;
	public Color ScreenBoundsColor = Color.red;

	[Header("Collider settings")]
	public bool AutoGenerateMeshColliders = true;
    public bool ConvexMeshColliders = true;

	[Header("Renderer settings")]
    public bool GenerateDummyIfNoMeshFound = false;
    
	[Header("Camera settings")]
	public Camera CurrentCamera;
    
	[Header("Monitoring")]
	public bool AutoUpdateOnChanges;
	public bool MonitorPosition;
	public bool MonitorRotation;
	public bool MonitorScale;
	public bool MonitorHierarchy;
	public bool MonitorChildrenPosition;
	public bool MonitorChildrenRotation;
	public bool MonitorChildrenScale;

	private bool RemoveGOBFromChildren = false;

    private string advancedName;
    private List<GameObject> childrens;
    private Renderer[] renderers;
    private Collider[] colliders;
    private List<Collider> generatedColliders;
    private bool multiTransform = false;
    private bool multiRenderer = false;
    private bool multiCollider = false;
    private bool initialized;

    private Bounds bounds;
    private Bounds rendererBounds;
    private Bounds colliderBounds;

	private GameObject boundsGameObject;

    private bool isCalculatedRendererCenter;
    private Vector3 calculatedRendererCenter;

    private bool isCalculatedRendererDiameter;
    private float calculatedRendererDiameter;

    private bool isCalculatedRendererHeight;
    private float calculatedRendererHeight;

    private bool isCalculatedColliderCenter;
    private Vector3 calculatedColliderCenter;

    private bool isCalculatedColliderDiameter;
    private float calculatedColliderDiameter;

    private bool isCalculatedColliderHeight;
    private float calculatedColliderHeight;

    private Vector3 lastPostion;
	private Quaternion lastRotation;
	private Vector3 lastScale;
	private GameObject[] lastHierarchy;

	private Texture2D texture;

    public string AdvancedName
    {
        get
        {
            return advancedName;
        }
    }

    // ==========================================================================================================================================
	/// <summary>
	/// General Unit Test for the the Game Object Boundaries Component, Errors are displayed in Unity console.
	/// </summary>
    public static void UnitTest_General()
    {

        GameObject UnitTest_Main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        UnitTest_Main.AddComponent<GameObjectBoundaries>();

        GameObject UnitTest_Child1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        UnitTest_Child1.transform.parent = UnitTest_Main.transform;

        GameObject UnitTest_Child2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        UnitTest_Child2.transform.parent = UnitTest_Main.transform;

        GameObject UnitTest_GrandSon1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        UnitTest_GrandSon1.transform.parent = UnitTest_Child1.transform;

        GameObject UnitTest_GrandSon2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        UnitTest_GrandSon2.transform.parent = UnitTest_Child1.transform;

        GameObject UnitTest_GrandSon3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        UnitTest_GrandSon3.transform.parent = UnitTest_Child2.transform;

        int UnitTest_ChildCount = GetAllChilds(UnitTest_Main).Count;
        Assert.AreEqual(UnitTest_ChildCount, 5);
        UnitTest_ChildCount = GetAllChilds(UnitTest_Main).Count;
        Assert.AreEqual(UnitTest_ChildCount, 5);

        // ----

        Destroy(UnitTest_Main);

    }

    // ==========================================================================================================================================
    // Use this for initialization
    void Start()
    {

        Initialize();

		if (DrawSpaceBounds)
			Get3dBoundsRenderer ();
    }

    // ==========================================================================================================================================
    // Update is called once per frame
    void Update()
    {
		if (AutoUpdateOnChanges) {
		
			if (CheckPositionChanges () || CheckRotationChanges () || CheckScaleChanges () || CheckHierarchyChanges()) {

				if (DrawSpaceBounds) {
					Update3dBounds ();
				} else {
					GetBounds (true);
				}
			
			}
		
		}
    }

	// ===================================
	/// <summary>
	/// Recalculate bounds and update the visual 3D bounds.
	/// </summary>
	public void Update3dBounds()
	{
		if (DrawSpaceBounds) {
			Get3dBoundsGameObject ().transform.localScale = GetBounds (true).size;
			Get3dBoundsGameObject ().transform.position = GetCenter (true);
		}
	}

	// ===================================
	// ...
	protected void UpdateChildren()
	{
		lastHierarchy = GameObjectBoundaries.GetAllChilds (transform.gameObject).ToArray ();

		for (int i = 0; i < lastHierarchy.Length; i++) {
		
			if (lastHierarchy [i] != gameObject) {
			
				MetaCgoChild tmpChildComp = lastHierarchy [i].GetComponent<MetaCgoChild> ();

				if (tmpChildComp == null) {

					tmpChildComp = lastHierarchy [i].AddComponent<MetaCgoChild> ();
					tmpChildComp.MonitorPosition = MonitorChildrenPosition;
					tmpChildComp.MonitorRotation = MonitorChildrenRotation;
					tmpChildComp.MonitorScale = MonitorChildrenScale;
					tmpChildComp.UltimateParent = this;
				
				}
			
			}
		
		}
	}

    // ===================================

    void OnGUI()
    {

		if (DrawScreenSpaceBounds)
        {
			Meta2dBounds tmp2dBounds = GetScreenSpaceBounds(true);
			Meta2dBounds.Meta2dBoundsVisibility tmpVisibility = tmp2dBounds.GetVisibility ();

			if (tmpVisibility == Meta2dBounds.Meta2dBoundsVisibility.Full || tmpVisibility == Meta2dBounds.Meta2dBoundsVisibility.Partial) {

				float x = tmp2dBounds.topLeft.x;
				float y = tmp2dBounds.topLeft.y;

				float width = tmp2dBounds.GetWidth ();
				float height = tmp2dBounds.GetHeight ();

				DrawQuadOnScreen (new Rect (x, Screen.height - y, width, height));
			}
        }
    }

    // ==========================================================================================================================================
	/// <summary>
	/// Get currently used bounds.
	/// </summary>
    public Bounds GetBounds(bool ParamRecalculate)
    {

        if (UsedBoundsType == MetaBoundsType.ColliderBounds)
        {
            if (!ParamRecalculate && colliderBounds != null) { return colliderBounds; }
            colliderBounds = GetColliderBounds(true);
            return colliderBounds;
        }
        else if (UsedBoundsType == MetaBoundsType.RendererBounds)
        {
            if (!ParamRecalculate && rendererBounds != null) { return rendererBounds; }
            rendererBounds = GetRendererBounds(true);
            return rendererBounds;
        }

        return new Bounds();

    }

    // ==========================================================================================================================================
	/// <summary>
	/// Get specific bounds.
	/// </summary>
	public Bounds GetBounds(bool ParamRecalculate, MetaBoundsType ParamBoundsType)
    {

        if (!ParamRecalculate && bounds != null) { return bounds; }

        if (ParamBoundsType == MetaBoundsType.ColliderBounds)
        {
            bounds = GetColliderBounds(true);
        }
        else if (ParamBoundsType == MetaBoundsType.RendererBounds)
        {
            bounds = GetRendererBounds(true);
        }

        return bounds;

    }

    // ==========================================================================================================================================
    // get bounds from renderer(s)
    protected Bounds GetRendererBounds(bool ParamRecalculate)
    {

        if (!ParamRecalculate && rendererBounds != null) { return rendererBounds; }

        rendererBounds = MetaRenderer.GetBoundsFromRenderers(renderers);
        return rendererBounds;

    }

    // ==========================================================================================================================================
    // get bounds from collider(s)
    protected Bounds GetColliderBounds(bool ParamRecalculate)
    {

        if (!ParamRecalculate && colliderBounds != null) { return colliderBounds; }

        colliderBounds = MetaCollider.GetBoundsFromColliders(colliders);
        return colliderBounds;

    }

    // ==========================================================================================================================================
	/// <summary>
	/// Sync bounds center with transforms center.
	/// </summary>
	[System.Obsolete("Method deprecated", true)]
    public void SyncBounds()
    {

        if (bounds != null)
        {

            bounds.center = transform.position;

        }

    }

    // ==========================================================================================================================================
	/// <summary>
	/// Get diameter of the object on the user selected plane using the user selected boundaries.
	/// </summary>
    public float Get2dDiameter(bool ParamRecalculate)
    {
        if (UsedBoundsType == MetaBoundsType.ColliderBounds)
        {
            return GetCollider2dDiameter(ParamRecalculate);
        }
        else if (UsedBoundsType == MetaBoundsType.RendererBounds)
        {
            return GetRenderer2dDiameter(ParamRecalculate);
        }

        return 0;
    }

    // ==========================================================================================================================================
	/// <summary>
	/// Get the height of the object using the user selected boundaries.
	/// </summary>
    public float GetHeight(bool ParamRecalculate)
    {
        if (UsedBoundsType == MetaBoundsType.ColliderBounds)
        {
            return GetColliderHeight(ParamRecalculate);
        }
        else if (UsedBoundsType == MetaBoundsType.RendererBounds)
        {
            return GetRendererHeight(ParamRecalculate);
        }

        return 0;
    }

    // ==========================================================================================================================================
	/// <summary>
	/// Get the center of the object using the user selected boundaries.
	/// </summary>
    public Vector3 GetCenter(bool ParamRecalculate)
    {
        if (UsedBoundsType == MetaBoundsType.ColliderBounds)
        {
            return GetColliderCenter(ParamRecalculate);
        }
        else if (UsedBoundsType == MetaBoundsType.RendererBounds)
        {
            return GetRendererCenter(ParamRecalculate);
        }

        return Vector3.zero;
    }

    // ==========================================================================================================================================
    // Get the diameter of the object using the renderer bounds on the user selected plane
    protected float GetRenderer2dDiameter(bool ParamRecalculate)
    {

        if (isCalculatedRendererDiameter && !ParamRecalculate)
            return calculatedRendererDiameter;

        GetBounds(true, MetaBoundsType.RendererBounds);

        if (Used2DPlane == Meta2dPlaneType.XZ) { calculatedRendererDiameter = MetaVector.MaximizeVectorXZ(rendererBounds.size).x; }
        else if (Used2DPlane == Meta2dPlaneType.XY) { calculatedColliderDiameter = MetaVector.MaximizeVectorXY(rendererBounds.size).x; }

        isCalculatedRendererDiameter = true;
        return calculatedRendererDiameter;
    }

    // ==========================================================================================================================================
    // Get the height of the object using the renderer bounds
    protected float GetRendererHeight(bool ParamRecalculate)
    {

        if (isCalculatedRendererHeight && !ParamRecalculate)
            return calculatedRendererHeight;

        GetBounds(true, MetaBoundsType.RendererBounds);
        calculatedRendererHeight = bounds.size.y;
        isCalculatedRendererHeight = true;
        return calculatedRendererHeight;

    }

    // ==========================================================================================================================================
    // Get the center of the object using the renderer
    protected Vector3 GetRendererCenter(bool ParamRecalculate)
    {

        if (isCalculatedRendererCenter && !ParamRecalculate)
            return bounds.center;

        GetBounds(true, MetaBoundsType.RendererBounds);
        calculatedRendererCenter = bounds.center;
        isCalculatedRendererCenter = true;
        return calculatedRendererCenter;

    }

    // ==========================================================================================================================================
    // Get the Closest point on the bounds to the desired point
    protected Vector3 GetClosestRendererPoint(Vector3 ParamPoint)
    {

        return GetRendererBounds(true).ClosestPoint(ParamPoint);

    }

    // ==========================================================================================================================================
	// Get the diameter of the object using the collider bounds on the user selected plane
    protected float GetCollider2dDiameter(bool ParamRecalculate)
    {

        if (isCalculatedColliderDiameter && !ParamRecalculate)
            return calculatedColliderDiameter;

        GetBounds(true, MetaBoundsType.ColliderBounds);

        if (Used2DPlane == Meta2dPlaneType.XZ) { calculatedColliderDiameter = MetaVector.MaximizeVectorXZ(bounds.size).x; }
        else if (Used2DPlane == Meta2dPlaneType.XY) { calculatedColliderDiameter = MetaVector.MaximizeVectorXY(bounds.size).x; }

        isCalculatedColliderDiameter = true;
        return calculatedColliderDiameter;
    }

    // ==========================================================================================================================================
	// Get the height of the object using the collider bounds
    protected float GetColliderHeight(bool ParamRecalculate)
    {

        if (isCalculatedColliderHeight && !ParamRecalculate)
            return calculatedColliderHeight;

        GetBounds(true, MetaBoundsType.ColliderBounds);
        calculatedColliderHeight = bounds.size.y;
        isCalculatedColliderHeight = true;
        return calculatedColliderHeight;

    }

    // ==========================================================================================================================================
	// Get the center of the object using the collider
    protected Vector3 GetColliderCenter(bool ParamRecalculate)
    {

        if (isCalculatedColliderCenter && !ParamRecalculate)
            return bounds.center;

        GetBounds(true, MetaBoundsType.ColliderBounds);
        calculatedColliderCenter = bounds.center;
        isCalculatedColliderCenter = true;
        return calculatedColliderCenter;

    }

    // ==========================================================================================================================================
	// Get the Closest point on the bounds to the desired point
    protected Vector3 GetClosestColliderPoint(Vector3 ParamPoint)
    {

        return GetColliderBounds(false).ClosestPoint(ParamPoint);

    }

    // ==========================================================================================================================================
    // Initialization of the complex game object
	protected void Initialize()
    {

        // Check if already initialized
        if (initialized)
        {
            return;
        }

        // Name
        advancedName = gameObject.name;

		// ...
		if (RemoveGOBFromChildren) 
			RemoveGOBComponentsFromChildren ();

		// Hierarchy, colliders and renderers
		BuildStructure ();

        // Preparing some variables
        GetBounds(true, UsedBoundsType);
        GetColliderCenter(true);
        GetCollider2dDiameter(true);

        // Create a texture for the GUI Box
        texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, new Color(ScreenBoundsColor.r, ScreenBoundsColor.g, ScreenBoundsColor.b, ScreenBoundsColor.a));
        texture.Apply();

		// ...
		lastPostion = transform.position;
		lastRotation = transform.rotation;
		lastScale = transform.localScale;


        initialized = true;

    }

    // ==========================================================================================================================================
    // Offset the posotion of the game object
	[System.Obsolete("Method deprecated", true)]
	protected void ApplyVisualOffset(Vector3 ParamPositionOffset)
    {

        Transform main = gameObject.transform;
        List<GameObject> tmpChilds = GetAllChilds(gameObject);

        if (!multiCollider)
        {

            Collider tmpCollider = gameObject.GetComponentInChildren<Collider>();
            tmpCollider.transform.position = tmpCollider.transform.position + ParamPositionOffset;

        }
        else
        {

            foreach (GameObject child in tmpChilds)
            {

                if (child.transform.parent == main)
                {
                    child.transform.position = child.transform.position + ParamPositionOffset;
                }

            }


        }

        GetColliderCenter(true); // recalculate the collider center ? Yes ...

    }

    // ==========================================================================================================================================
	/// <summary>
	/// Brings the whole game object to the center of the collider.
	/// </summary>
	[System.Obsolete("Method deprecated", true)]
    public void AutoVisualOffsetWithCollider()
    {

        Vector3 colliderCenter = GetColliderCenter(false);
        Vector3 transformCenter = transform.position;
        float tmpX = (transformCenter.x - colliderCenter.x);
        float tmpY = (transformCenter.y - colliderCenter.y);
        float tmpZ = (transformCenter.z - colliderCenter.z);
        Vector3 offset = new Vector3(tmpX, tmpY, tmpZ);
        ApplyVisualOffset(offset);

    }

    // ==========================================================================================================================================
	/// <summary>
	/// Get childrens of the parent transform recursively.
	/// </summary>
    public static List<GameObject> GetAllChilds(GameObject ParamTargetGameObject)
    {
        childBuffer = new List<GameObject>();

        foreach (Transform t in ParamTargetGameObject.transform)
        {
            GetAllChilds_Recursion(t.gameObject);
            childBuffer.Add(t.gameObject);
        }

        return childBuffer;
    }

    // ==========================================================================================================================================
    // Get childrens of the parent transform recursively
    protected static void GetAllChilds_Recursion(GameObject ParamTargetGameObject)
    {
        foreach (Transform t in ParamTargetGameObject.transform)
        {
            GetAllChilds_Recursion(t.gameObject);
            childBuffer.Add(t.gameObject);
        }
    }

    // ==========================================================================================================================================
	/// <summary>
	/// Scale the object to fit a certain size of the selected bounds, not 100% accurate.
	/// </summary>
    public void BoundScale(float ParamX, float ParamY, float ParamZ)
    {

        Bounds tmpBounds = GetBounds(true);
		Vector3 tmpScale = gameObject.transform.localScale;

		float tmpX = Mathf.Max(tmpBounds.size.x, ParamX) / Mathf.Min(tmpBounds.size.x, ParamX);
		float tmpY = Mathf.Max(tmpBounds.size.y, ParamY) / Mathf.Min(tmpBounds.size.y, ParamY);
		float tmpZ = Mathf.Max(tmpBounds.size.z, ParamZ) / Mathf.Min(tmpBounds.size.z, ParamZ);

		gameObject.transform.localScale = new Vector3(tmpScale.x * tmpX, tmpScale.y * tmpY, tmpScale.z * tmpZ);

    }

    // ==========================================================================================================================================
	/// <summary>
	/// Position the whole game object above a certain point.
	/// </summary>
    public void PositionAbove(Vector3 ParamPosition)
    {

        Vector3 tmpTransformPos = gameObject.transform.position;
		Vector3 tmpCenter = GetCenter(true);
		Bounds tmpBounds = GetBounds (true);

		float boundsOffset = tmpBounds.extents.y;
		float correction = tmpCenter.y - tmpTransformPos.y;

		float tmpY = ParamPosition.y + boundsOffset - correction;

        gameObject.transform.position = new Vector3(ParamPosition.x, tmpY, ParamPosition.z);

    }

    // ==========================================================================================================================================
	/// <summary>
	/// Position the whole game object below a certain point.
	/// </summary>
    public void PositionBelow(Vector3 ParamPosition)
    {

		Vector3 tmpTransformPos = gameObject.transform.position;
		Vector3 tmpCenter = GetCenter(true);
		Bounds tmpBounds = GetBounds (true);

		float boundsOffset = tmpBounds.extents.y;
		float correction = tmpCenter.y - tmpTransformPos.y;

		float tmpY = ParamPosition.y - boundsOffset - correction;

		gameObject.transform.position = new Vector3(ParamPosition.x, tmpY, ParamPosition.z);

    }

    // ================================================================================================================================
	/// <summary>
	/// Get screen space boundaries.
	/// </summary>
    public Meta2dBounds GetScreenSpaceBounds(bool ParamRecalculate)
    {
        Vector3[] tmpCorners = GetCornersFromBounds(GetBounds(ParamRecalculate)).ToArray();

        float maxX = CurrentCamera.WorldToScreenPoint(tmpCorners[0]).x;
        float maxY = CurrentCamera.WorldToScreenPoint(tmpCorners[0]).y;
        float minX = CurrentCamera.WorldToScreenPoint(tmpCorners[0]).x;
        float minY = CurrentCamera.WorldToScreenPoint(tmpCorners[0]).y;

		float minZ = CurrentCamera.WorldToScreenPoint (tmpCorners [0]).z; // Lowest Z will determine if object is behinf camera

        int i;

        for (i = 1; i < tmpCorners.Length; i++)
        {
            Vector3 tmpVec = CurrentCamera.WorldToScreenPoint(tmpCorners[i]);

            if (tmpVec.x > maxX) maxX = tmpVec.x;
            if (tmpVec.x < minX) minX = tmpVec.x;

            if (tmpVec.y > maxY) maxY = tmpVec.y;
            if (tmpVec.y < minY) minY = tmpVec.y;

			if (tmpVec.z < minZ)
				minZ = tmpVec.z;
        }

        Meta2dBounds tmp2dBounds = new Meta2dBounds();

		tmp2dBounds.topLeft = new Vector3(minX, maxY, minZ);
		tmp2dBounds.topRight = new Vector3(maxX, maxY, minZ);
		tmp2dBounds.bottomRight = new Vector3(maxX, minY, minZ);
		tmp2dBounds.bottomLeft = new Vector3(minX, minY, minZ);

        return tmp2dBounds;
    }

    // ================================================================================================================================
	// This function is used to draw the screen bounds
	protected void DrawQuadOnScreen(Rect ParamPosition)
    {
        GUI.skin.box.normal.background = texture;
        GUI.Box(ParamPosition, GUIContent.none);
    }

    // ================================================================================================================================
	/// <summary>
	/// Get bounds corners as a list of Vector3.
	/// </summary>
    public List<Vector3> GetCornersFromBounds(Bounds ParamBounds)
    {
        List<Vector3> tmpList = new List<Vector3>();

        tmpList.Add(new Vector3(ParamBounds.max.x, ParamBounds.max.y, ParamBounds.max.z));
        tmpList.Add(new Vector3(ParamBounds.max.x, ParamBounds.max.y, ParamBounds.min.z));
        tmpList.Add(new Vector3(ParamBounds.max.x, ParamBounds.min.y, ParamBounds.max.z));
        tmpList.Add(new Vector3(ParamBounds.max.x, ParamBounds.min.y, ParamBounds.min.z));
        tmpList.Add(new Vector3(ParamBounds.min.x, ParamBounds.max.y, ParamBounds.max.z));
        tmpList.Add(new Vector3(ParamBounds.min.x, ParamBounds.max.y, ParamBounds.min.z));
        tmpList.Add(new Vector3(ParamBounds.min.x, ParamBounds.min.y, ParamBounds.max.z));
        tmpList.Add(new Vector3(ParamBounds.min.x, ParamBounds.min.y, ParamBounds.min.z));

        return tmpList;
    }

	// ================================================================================================================================
	/// <summary>
	/// Scale the object to cover a certain area of the screen (scaling is done only on a specific 2D plane, so not all camera angles are took in consideration).
	/// </summary>
	public void SetScreenCoverage(float ParamCoverage)
	{
		float currentCoverage = GetScreenSpaceBounds (true).GetScreenCoverage ();
		float ratio = ParamCoverage / currentCoverage;
		float sqrt_ratio = Mathf.Sqrt (ratio);
		Vector3 scale = transform.localScale;

		if (Used2DPlane == Meta2dPlaneType.XY) 
		{
			gameObject.transform.localScale = new Vector3 (scale.x * sqrt_ratio, scale.y * sqrt_ratio, scale.z);
		}
		else if (Used2DPlane == Meta2dPlaneType.XZ) 
		{
			gameObject.transform.localScale = new Vector3 (scale.x * sqrt_ratio, scale.y, scale.z * sqrt_ratio);
		}
	}

	// ================================================================================================================================
	// Get the game object that will represent the bounds, create a new one if not available
	protected GameObject Get3dBoundsGameObject ()
	{
		if (boundsGameObject == null) {

			boundsGameObject = GameObject.CreatePrimitive (PrimitiveType.Cube);

			boundsGameObject.name = transform.name + "(Bounds)";
			Destroy (boundsGameObject.GetComponent<Collider> ());

			boundsGameObject.transform.position = GetCenter (false);
			boundsGameObject.transform.localScale = GetBounds (false).size;

			boundsGameObject.GetComponent<Renderer> ().material = GetBoundsMaterial ();
			return boundsGameObject;

		} else {

			return boundsGameObject;

		}
	}

	// ================================================================================================================================
	// Draw the 3D boundaries of the game object and return the renderer of the visual bounds
	protected Renderer Get3dBoundsRenderer()
	{
		
		return Get3dBoundsGameObject().GetComponent<Renderer> ();

	}

	// ================================================================================================================================
	// Get the default bound box material, if not available create a new one
	protected Material GetBoundsMaterial ()
	{
		if (boundsMaterial == null) {

			boundsMaterial = new Material (Shader.Find ("Transparent/Diffuse"));
			boundsMaterial.color = SpaceBoundsColor;
			return boundsMaterial;
		
		} else {
		
			return boundsMaterial;

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

	// =================================================================================================================================
	// Check if the hierarchy has changed since the last check
	protected bool CheckHierarchyChanges()
	{
		if (!MonitorHierarchy)
			return false;

		GameObject[] tmpHierarchy = GameObjectBoundaries.GetAllChilds (transform.gameObject).ToArray();

		if (lastHierarchy.Length != tmpHierarchy.Length)
		{
			BuildStructure ();
			lastHierarchy = tmpHierarchy;
			return true;
		}

		for (int i = 0; i < tmpHierarchy.Length; i++) {
		
			if (lastHierarchy [i] != tmpHierarchy [i]) {
			
				BuildStructure ();
				lastHierarchy = tmpHierarchy;
				return true;

			}
		
		}

		return false;
	}

	// ==================================================================================================================================
	// ...
	protected void BuildStructure ()
	{
		// Prepare children list
		childrens = GetAllChilds(gameObject);
		if (childrens.Count == 0)
			multiTransform = false;
		else
			multiTransform = true;

		// Get renderer(s)
		if (multiTransform)
		{
			renderers = gameObject.GetComponentsInChildren<Renderer>();
			if (renderers.Length > 1)
			{
				multiRenderer = true;
			}
			else if (renderers.Length == 1)
			{
				multiRenderer = false;
			}

		}
		else
		{
			renderers = new Renderer[1];
			renderers[0] = gameObject.GetComponent<Renderer>();
		}


		// Get collider(s)
		bool colliderFound = false;
		if (multiTransform)
		{
			colliders = gameObject.GetComponentsInChildren<Collider>();
			if (colliders.Length > 1)
			{
				multiCollider = true;
				colliderFound = true;
			}
			else if (colliders.Length == 1)
			{
				colliderFound = true;
			}
		}
		else
		{
			colliders = new Collider[1];
			colliders[0] = gameObject.GetComponent<Collider>();
			if (colliders[0] != null)
				colliderFound = true;
		}

		// Check collider(s), and auto-create colliders from renderers if no collider was found
		if (!colliderFound && AutoGenerateMeshColliders)
		{

			generatedColliders = new List<Collider>();

			if (multiTransform)
			{
				if (renderers.Length == 0)
				{
					// Add non-mesh collider or attach a dummy model
					if (GenerateDummyIfNoMeshFound)
					{
						GameObject tmpGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						tmpGO.transform.position = gameObject.transform.position;
						tmpGO.transform.rotation = gameObject.transform.rotation;
						tmpGO.transform.parent = gameObject.transform;
						SphereCollider SC = tmpGO.GetComponent<SphereCollider>();
						SC.name = advancedName;
						generatedColliders.Add(SC);
					}
				}
				else if (renderers.Length == 1)
				{
					// Add a mesh collider from r0
					MeshCollider MC = renderers[0].gameObject.AddComponent<MeshCollider>();
					MC.sharedMesh = MetaRenderer.GetMesh(renderers[0]);
					MC.name = advancedName;
					MC.convex = ConvexMeshColliders;
					generatedColliders.Add(MC);
				}
				else if (renderers.Length > 1)
				{
					// loop and add mesh colliders
					int i;
					for (i = 0; i < renderers.Length; i++)
					{
						MeshCollider MC = renderers[i].gameObject.AddComponent<MeshCollider>();
						MC.sharedMesh = MetaRenderer.GetMesh(renderers[i]);
						MC.name = advancedName;
						MC.convex = ConvexMeshColliders;
						generatedColliders.Add(MC);
						multiCollider = true;
						colliders = generatedColliders.ToArray();
					}
				}

			}
			else
			{
				if (renderers[0] == null)
				{
					// Add non-mesh collider or attach a dummy model
					GameObject tmpGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					tmpGO.transform.position = gameObject.transform.position;
					tmpGO.transform.rotation = gameObject.transform.rotation;
					tmpGO.transform.parent = gameObject.transform;
					SphereCollider SC = tmpGO.GetComponent<SphereCollider>();
					SC.name = advancedName;
					generatedColliders.Add(SC);
				}
				else if (renderers[0] != null)
				{
					// Add a mesh collider from r0
					MeshCollider MC = renderers[0].gameObject.AddComponent<MeshCollider>();
					MC.sharedMesh = MetaRenderer.GetMesh(renderers[0]);
					MC.name = advancedName;
					MC.convex = ConvexMeshColliders;
					generatedColliders.Add(MC);
				}
			}
		}

		// ...
		UpdateChildren();
	}

	// ===================================================================================================================================
	// ...
	protected void RemoveGOBComponentsFromChildren()
	{
		GameObjectBoundaries[] tmpCompsInChilds = GetComponentsInChildren<GameObjectBoundaries> ();

		if (tmpCompsInChilds.Length == 0)
			return;

		for (int i = 0; i < tmpCompsInChilds.Length; i++) {

			if (tmpCompsInChilds[i].transform != transform)
				tmpCompsInChilds [i].Destroy();

		}
	}

	// ====================================================================================================================================
	/// <summary>
	/// Destroy the GOB component and the 3D bounds if available.
	/// </summary>
	public void Destroy ()
	{
		Destroy (Get3dBoundsGameObject ());
		Destroy (this);
	}
}