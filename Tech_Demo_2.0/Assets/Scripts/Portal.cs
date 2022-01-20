using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Portal : MonoBehaviour
{
    // Properties.
    public bool isActivated
    {
        get; private set;
    }
    public MeshRenderer screenMeshRenderer
    {
        get; private set;
    }

    // Private fields. TODO can I change this to find all the values it needs through the script?
    [SerializeField] private Portal destination;
    private Camera portalCamera;

    private RenderTexture screenRenderTexture;

    [SerializeField] private GameObject player;
    private Camera mainCamera;

    private List<PortalTraveler> travelers = new List<PortalTraveler>();
    private Dictionary<PortalTraveler, Vector3> travelersStartPositions = new Dictionary<PortalTraveler, Vector3>();

    /*
    // Enums.
    enum PortalSide
    {
        Positive,
        Negative
    }
    */

    void Start()
    {
        screenMeshRenderer = transform.Find("Portal Screen").GetComponent<MeshRenderer>();
        portalCamera = GetComponentInChildren<Camera>();
        portalCamera.enabled = false;
        mainCamera = Camera.main;
        screenMeshRenderer.material.SetInt("displayMask", 1);
        // Portals start with being on and visible.
        isActivated = true;
        screenMeshRenderer.enabled = isActivated;
    }

    private void Update()
    {
        // Press T to switch between portals being active or not.
        if (Input.GetKeyDown(KeyCode.T))
        {
            isActivated = !isActivated;
            screenMeshRenderer.enabled = isActivated;
        }

        // Move the traveler clone.
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
        if (isActivated)
        {
            Render();
        }
    }

    private void FixedUpdate()
    {
        if (isActivated)
        {
            // Move travelers.
            if (travelers.Count > 0)
            {
                for (int traveler = 0; traveler < travelers.Count; traveler++)
                {
                    int totalTravelers = travelers.Count;
                    MoveTraveler(travelers[traveler]);

                    if (totalTravelers > travelers.Count)
                    {
                        traveler--;
                    }
                }
            }
        }
    }

    public void Render()
    {
        // Don't move the camera if the player doesn't see the portal screen.
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

        ProtectScreenFromClipping(mainCamera.transform.position);
    }

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

    private void MovePortalCamera()
    {
        // The world position of the player camera if it would be next to the destination portal instead of the portal.
        Matrix4x4 playerCameraToPortal = transform.localToWorldMatrix * destination.transform.worldToLocalMatrix * mainCamera.transform.localToWorldMatrix;
        portalCamera.transform.SetPositionAndRotation(playerCameraToPortal.GetColumn(3), playerCameraToPortal.rotation);
    }

    // Copied from the code for Sebastians portal video.
    public static bool VisibleFromCamera(Renderer renderer, Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    private void OnTriggerEnter(Collider other)
    {
        PortalTraveler traveler = other.GetComponent<PortalTraveler>();

        if (traveler)
        {
            AddTraveler(traveler);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PortalTraveler traveler = other.GetComponent<PortalTraveler>();

        if (traveler)
        {
            RemoveTraveler(traveler);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        PortalTraveler traveler = other.GetComponent<PortalTraveler>();
        // Keep the clone visible as long as it is in the portal.
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
        }
    }

    private void RemoveTraveler(PortalTraveler traveler)
    {
        if (travelers.Contains(traveler))
        {
            travelers.Remove(traveler);
            travelersStartPositions.Remove(traveler);
            traveler.ExitPortal();
        }



    }

    private void TrackTravelers()
    {

    }

    private void MoveTraveler(PortalTraveler traveler)
    {

        int travelerStartPortalSide = Math.Sign(Vector3.Dot(travelersStartPositions[traveler] - transform.position, transform.right));
        int travelerCurrentPortalSide = Math.Sign(Vector3.Dot(traveler.transform.position - transform.position, transform.right));

        Matrix4x4 cameraMatrix = destination.transform.localToWorldMatrix * transform.worldToLocalMatrix * traveler.transform.localToWorldMatrix;

        if (travelerStartPortalSide != travelerCurrentPortalSide)
        {
            traveler.Travel(cameraMatrix.GetColumn(3), cameraMatrix.rotation);

            destination.AddTraveler(traveler);
            RemoveTraveler(traveler);
        }
    }

    /*
    private PortalSide CalculatePortalSide(Vector3 travelerPosition)
    {
        PortalSide travelerSideOfPortal;
        Vector3 travelerOffsetFromPortal = travelerPosition - transform.position;

        float portalSide = Vector3.Dot(travelerOffsetFromPortal, Vector3.forward);
        if (portalSide > 0)
        {
            travelerSideOfPortal = PortalSide.Positive;
        }
        else
        {
            travelerSideOfPortal = PortalSide.Negative;
        }

        return travelerSideOfPortal;
    }
    */

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Matrix4x4 playerCameraToPortal = transform.localToWorldMatrix * destination.transform.worldToLocalMatrix * mainCamera.transform.localToWorldMatrix;

        Gizmos.color = new Color(0.0f, 0.0f, 0.75f, 0.75f);

        // Convert the local coordinate values into world
        // coordinates for the matrix transformation.
        Gizmos.matrix = playerCameraToPortal;
        //Gizmos.DrawCube(player.transform.Find("Camera pivot").Find("Player Camera").position, Vector3.one);


        Gizmos.matrix = Matrix4x4.TRS(destination.portalCamera.transform.position, destination.portalCamera.transform.rotation, Vector3.one);

        Gizmos.DrawFrustum(Vector3.zero, destination.portalCamera.fieldOfView, destination.portalCamera.farClipPlane, destination.portalCamera.nearClipPlane, destination.portalCamera.aspect);
    }

    void UpdateSliceParams(PortalTraveler traveler)
    {
        // Calculate slice normal
        int side = Math.Sign(Vector3.Dot(travelersStartPositions[traveler] - transform.position, transform.right));
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = destination.transform.forward * side;

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


    // TODO make the screen not clip.
    float ProtectScreenFromClipping(Vector3 viewPoint)
    {
        float halfHeight = mainCamera.nearClipPlane * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * mainCamera.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, mainCamera.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = screenMeshRenderer.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.right, transform.position - mainCamera.transform.position) > 0;
        screenT.localScale = new Vector3(screenThickness, screenT.localScale.y, screenT.localScale.z);
        screenT.localPosition = Vector3.right * screenThickness * (camFacingSameDirAsPortal ? 0.5f : -0.5f) + new Vector3(0, screenT.localPosition.y, 0);
        return screenThickness;
    }
}
