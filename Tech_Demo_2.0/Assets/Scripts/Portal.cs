using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Portal : MonoBehaviour
{
    #region Fields

    // Properties.
    public bool portalIsActivated
    {
        get; private set;
    }
    public Portal destination
    {
        get { return Destination; }
        private set { }
    }
    public PolygonCollider2D forcingFrame
    {
        get; private set;
    }

    // Private fields.
    [SerializeField] private Portal Destination;
    private MeshRenderer screenMeshRenderer;
    private Camera portalCamera;
    private RenderTexture screenRenderTexture;
    private Camera mainCamera;
    private PortalTraveler mainCameraTraveler;
    private bool mainCameraInTravelers;
    private List<PortalTraveler> travelers = new List<PortalTraveler>();
    private Dictionary<PortalTraveler, Vector3> travelersStartPositions = new Dictionary<PortalTraveler, Vector3>();

    #endregion

    void Start()
    {
        screenMeshRenderer = transform.Find("Portal Screen").GetComponent<MeshRenderer>();
        portalCamera = GetComponentInChildren<Camera>();
        portalCamera.enabled = false;
        mainCamera = Camera.main;
        mainCameraTraveler = mainCamera.GetComponent<PortalTraveler>();
        mainCamera.GetComponent<MainCameraController>().positionUpdated += OnMainCameraPositionUpdated;
        screenMeshRenderer.material.SetInt("displayMask", 1);

        // Portals start with being on and visible.
        portalIsActivated = true;
        screenMeshRenderer.enabled = portalIsActivated;

        // Setting up the forcing frame at world origin.
        Transform forcingFrameTransform = transform.Find("Forcing Frame");
        forcingFrame = forcingFrameTransform.GetComponent<PolygonCollider2D>();
        forcingFrameTransform.SetParent(GameObject.Find("Forcing Frame Collection").transform);
        forcingFrameTransform.position = Vector3.zero;
        forcingFrameTransform.rotation = Quaternion.identity;
        forcingFrameTransform.localScale = transform.lossyScale;
    }

    private void Update()
    {
        // Press T to switch between portals being active or not.
        if (Input.GetKeyDown(KeyCode.T))
        {
            portalIsActivated = !portalIsActivated;
            screenMeshRenderer.enabled = portalIsActivated;
        }

        // Move the traveler clone for each traveler, if necessary.
        if (travelers.Count > 0)
        {
            foreach (PortalTraveler traveler in travelers)
            {
                // If the traveler doesn't have a clone, skip this traveler.
                if (!traveler.cloneTraveler)
                {
                    continue;
                }

                // The world position of the traveler if it would be next to the destination portal instead of the portal.
                Matrix4x4 matrix = destination.transform.localToWorldMatrix * transform.worldToLocalMatrix * traveler.transform.localToWorldMatrix;

                // Use the travelers exact position for the clone if the traveler is inside the portal otherwise put the clone on the ground.
                Vector3 travelerPosition = traveler.transform.position;
                double distance = Math.Sqrt(Math.Pow(travelerPosition.x - transform.position.x, 2) + Math.Pow(travelerPosition.z - transform.position.z, 2));
                if (distance < traveler.transform.localScale.z)
                {
                    // Clone keeps travelers position.
                    traveler.cloneTraveler.transform.SetPositionAndRotation(matrix.GetColumn(3), matrix.rotation);
                }
                else
                {
                    // Clone is put on the ground.
                    Physics.Raycast(matrix.GetColumn(3), Vector3.down, out RaycastHit hit, Mathf.Infinity);
                    Vector3 clonePosition = new Vector3(matrix.GetColumn(3).x, hit.point.y + traveler.cloneTraveler.transform.lossyScale.y, matrix.GetColumn(3).z);
                    traveler.cloneTraveler.transform.SetPositionAndRotation(clonePosition, matrix.rotation);
                }
            }
        }
    }

    void LateUpdate()
    {
        // If the portal is active render the view.
        if (portalIsActivated)
        {
            Render();
        }
    }

    private void FixedUpdate()
    {
        if (portalIsActivated)
        {
            // Update the tracking of the travelers.
            if (travelers.Count > 0)
            {
                for (int traveler = 0; traveler < travelers.Count; traveler++)
                {
                    int totalTravelers = travelers.Count;
                    TrackTraveler(travelers[traveler]);

                    if (totalTravelers > travelers.Count)
                    {
                        traveler--;
                    }
                }
            }
        }
    }

    // Follows an event coming from MainCameraController when main cameras position has been updated.
    private void OnMainCameraPositionUpdated()
    {
        // Update the tracking of the main camera if it is a traveler.
        if (mainCameraInTravelers)
        {
            TrackTraveler(mainCameraTraveler);
        }

        // Update the portal screen thickness.
        ProtectScreenFromClipping();
    }

    // Update the screen texture and show it.
    private void Render()
    {
        // Only do this for portals that are visible from the main camera.
        if (!VisibleFromCamera(destination.screenMeshRenderer, mainCamera))
        {
            return;
        }

        // Set the texture and render it on the screen.
        SetScreenRenderTexture();
        screenMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        destination.screenMeshRenderer.material.SetInt("displayMask", 0);
        MovePortalCamera();
        SetNearClipPlane();
        portalCamera.Render();
        destination.screenMeshRenderer.material.SetInt("displayMask", 1);
        screenMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    // Create a render texture for the portal screen.
    private void SetScreenRenderTexture()
    {
        if (screenRenderTexture == null || screenRenderTexture.width != Screen.width || screenRenderTexture.height != Screen.height)
        {
            if (screenRenderTexture != null)
            {
                screenRenderTexture.Release();
            }

            screenRenderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            // Render the portal camera view to the screen render texture.
            portalCamera.targetTexture = screenRenderTexture;
            // Display the portal camera view on the destination portals screen.
            destination.screenMeshRenderer.material.SetTexture("_MainTex", screenRenderTexture);
        }
    }

    // Position the portal camera where the player camera is in relation to the destination portal.
    private void MovePortalCamera()
    {
        // The world position of the player camera if it would be next to the destination portal instead of the portal.
        Matrix4x4 playerCameraToPortal = transform.localToWorldMatrix * destination.transform.worldToLocalMatrix * mainCamera.transform.localToWorldMatrix;
        portalCamera.transform.SetPositionAndRotation(playerCameraToPortal.GetColumn(3), playerCameraToPortal.rotation);
    }

    // Copied from the code for Sebastians portal video. Checks if the portal is in the players view.
    public static bool VisibleFromCamera(Renderer renderer, Camera camera) // I don't understand why this is static and exactly when something should be static TODO
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    private void OnTriggerEnter(Collider other)
    {
        // When a traveler enters a portal collider add it to the list of travelers. 
        PortalTraveler traveler = other.GetComponent<PortalTraveler>();
        if (traveler)
        {
            AddTraveler(traveler);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When a traveler exits a portal collider remove it from the list of travelers. 
        PortalTraveler traveler = other.GetComponent<PortalTraveler>();
        if (traveler)
        {
            RemoveTraveler(traveler);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Keep the clone visible as long as it is in the portal.
        PortalTraveler traveler = other.GetComponent<PortalTraveler>();
        if (traveler && traveler.cloneTraveler && !traveler.cloneTraveler.activeInHierarchy)
        {
            traveler.cloneTraveler.SetActive(true);
        }
    }

    private void AddTraveler(PortalTraveler traveler)
    {
        if (!travelers.Contains(traveler))
        {
            travelers.Add(traveler);
            travelersStartPositions[traveler] = traveler.transform.position;
            traveler.EnterPortal();

            if (traveler == mainCameraTraveler)
            {
                mainCameraInTravelers = true;
            }
        }
    }

    private void RemoveTraveler(PortalTraveler traveler)
    {
        if (travelers.Contains(traveler))
        {
            travelers.Remove(traveler);
            travelersStartPositions.Remove(traveler);
            traveler.ExitPortal();

            if (traveler == mainCameraTraveler)
            {
                mainCameraInTravelers = false;
            }
        }
    }

    // Update the tracking of the traveler.
    private void TrackTraveler(PortalTraveler traveler)
    {
        // Check if the traveler is on the same side of the portal as it started on.
        int travelerStartPortalSide = Math.Sign(Vector3.Dot(travelersStartPositions[traveler] - transform.position, transform.forward));
        int travelerCurrentPortalSide = Math.Sign(Vector3.Dot(traveler.transform.position - transform.position, transform.forward));

        // If the traveler went to the other side of the portal, through the portal, make it travel!
        if (travelerStartPortalSide != travelerCurrentPortalSide)
        {
            RemoveTraveler(traveler);

            Matrix4x4 travelerLocalToDestinationWorldMatrix = destination.transform.localToWorldMatrix * transform.worldToLocalMatrix * traveler.transform.localToWorldMatrix;
            traveler.Travel(travelerLocalToDestinationWorldMatrix.GetColumn(3), travelerLocalToDestinationWorldMatrix.rotation, this, destination);

            destination.AddTraveler(traveler);
        }
    }

    // Change the screen thickness to make sure it doesn't clip with the camera.
    private void ProtectScreenFromClipping()
    {
        float halfHeight = mainCamera.nearClipPlane * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * mainCamera.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, mainCamera.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner * 2f;

        Transform screenT = screenMeshRenderer.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - mainCamera.transform.position) > 0;
        screenT.localScale = new Vector3(screenThickness / transform.localScale.x, screenT.localScale.y, screenT.localScale.z);
        screenT.localPosition = Vector3.forward * (screenThickness / transform.localScale.x) * (camFacingSameDirAsPortal ? 0.5f : -0.5f) + new Vector3(0, screenT.localPosition.y, 0);
    }

    // Set the portal cameras near clip plane at the position of and parallel to the portal screen to hide objects that are inbetween the portal and the portal camera.
    private void SetNearClipPlane()
    {
        Transform clipPlane = screenMeshRenderer.transform;
        int dot = Math.Sign(Vector3.Dot(clipPlane.right, screenMeshRenderer.transform.position - portalCamera.transform.position));

        Vector3 camSpacePos = portalCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCamera.worldToCameraMatrix.MultiplyVector(clipPlane.right) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + 0.05f;

        if (Mathf.Abs(camSpaceDst) > 0.2f)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
            portalCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCamera.projectionMatrix = mainCamera.projectionMatrix;
        }
    }

    private void OnDrawGizmos()
    {
        /*
        Matrix4x4 playerCameraToPortal = transform.localToWorldMatrix * destination.transform.worldToLocalMatrix * mainCamera.transform.localToWorldMatrix;

        // Convert the local coordinate values into world
        // coordinates for the matrix transformation.
        Gizmos.matrix = playerCameraToPortal;
        //Gizmos.DrawCube(player.transform.Find("Camera pivot").Find("Player Camera").position, Vector3.one);

        Gizmos.matrix = Matrix4x4.TRS(destination.portalCamera.transform.position, destination.portalCamera.transform.rotation, Vector3.one);

        Gizmos.DrawFrustum(Vector3.zero, destination.portalCamera.fieldOfView, destination.portalCamera.farClipPlane, destination.portalCamera.nearClipPlane, destination.portalCamera.aspect);
        */

        Gizmos.color = new Color(0.0f, 0.0f, 0.75f, 0.75f);

        Vector2[] points;
        if (!Application.isPlaying)
        {
            points = GetComponentInChildren<PolygonCollider2D>().points;
        }
        else
        {
            points = forcingFrame.points;
        }

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 startPoint = transform.localToWorldMatrix.MultiplyPoint(points[i]);
            Vector3 endPoint = transform.localToWorldMatrix.MultiplyPoint(points[(i + 1) % points.Length]);

            Gizmos.DrawLine(startPoint, endPoint);
        }
    }

    // Supposed to be used for only showing the part of an traveler that is on this side of the portal, I didn't finish implementing it but I will leave it here if I want it later.
    void UpdateSliceParams(PortalTraveler traveler)
    {
        // Calculate slice normal
        int side = Math.Sign(Vector3.Dot(travelersStartPositions[traveler] - transform.position, transform.right));
        Vector3 sliceNormal = transform.right * -side;
        Vector3 cloneSliceNormal = destination.transform.right * side;

        // Calculate slice centre
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = destination.transform.position;

        /*
        // Adjust slice offset so that when player standing on other side of portal to the object, the slice doesn't clip through
        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;
        float screenThickness = screenMeshRenderer.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal(playerCam.transform.position, traveler.transform.position);
        if (side > 0)

        if (!playerSameSideAsTraveller)
        {
            sliceOffsetDst = -screenThickness;
        }
        bool playerSameSideAsCloneAppearing = side != destination.SideOfPortal(playerCamera.transform.position);
        if (!playerSameSideAsCloneAppearing)
        {
            cloneSliceOffsetDst = -screenThickness;
        }
        */

        // Apply parameters
        for (int i = 0; i < traveler.playerMaterials.Length; i++)
        {
            traveler.playerMaterials[i].SetVector("sliceCentre", slicePos);
            traveler.playerMaterials[i].SetVector("sliceNormal", sliceNormal);
            // traveler.originalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);

            traveler.cloneMaterials[i].SetVector("sliceCentre", cloneSlicePos);
            traveler.cloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
            // traveler.cloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);

        }

    }
}
