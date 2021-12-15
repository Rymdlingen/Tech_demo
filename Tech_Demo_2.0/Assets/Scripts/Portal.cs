using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Private fields.
    [SerializeField] private Portal destination;
    private Camera portalCamera;

    private RenderTexture screenRenderTexture;

    [SerializeField] private GameObject player;
    private Camera playerCamera;

    // Can it be string for a name of the portal traveler or can I just use PortalTraveler? Or will that be the same for different travelers??
    private List<PortalTraveler> travelers;

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
        // TODO player = GameObject.FindGameObjectWithTag("Player");
        portalCamera = GetComponentInChildren<Camera>();
        portalCamera.enabled = false;
        travelers = new List<PortalTraveler> { };
        playerCamera = player.GetComponentInChildren<Camera>();
        screenMeshRenderer.material.SetInt("displayMask", 1);
    }

    void LateUpdate()
    {

        if (travelers.Count > 0)
        {
            for (int thisTraveler = 0; thisTraveler < travelers.Count; thisTraveler++)
            {
                PortalTraveler traveler = travelers[thisTraveler];

                int startPortalSide = System.Math.Sign(Vector3.Dot(traveler.previousPosition - transform.position, transform.right));
                int currentPortalSide = System.Math.Sign(Vector3.Dot(traveler.transform.position - transform.position, transform.right));

                if (startPortalSide != currentPortalSide)
                {
                    // Debug.Log("Travel " + name + " " + startPortalSide + " " + currentPortalSide);
                    Matrix4x4 matrix = destination.transform.localToWorldMatrix * transform.worldToLocalMatrix * traveler.transform.localToWorldMatrix;

                    traveler.Travel(matrix.GetColumn(3), matrix.rotation);

                    destination.AddTraveler(traveler);
                    RemoveTraveler(traveler);
                    thisTraveler--;
                }
            }
        }

        Render();
    }

    public void Render()
    {
        // Don't move the camera if the player doesn't see the portal screen.
        if (!VisibleFromCamera(destination.screenMeshRenderer, playerCamera))
        {
            return;
        }

        SetScreenRenderTexture();
        screenMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        destination.screenMeshRenderer.material.SetInt("displayMask", 0);
        MovePortalCamera();
        portalCamera.Render();
        destination.screenMeshRenderer.material.SetInt("displayMask", 1);
        screenMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
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
        // Debug.Log("Moving camera next to " + name);
        Matrix4x4 playerCameraToPortal = transform.localToWorldMatrix * destination.transform.worldToLocalMatrix * playerCamera.transform.localToWorldMatrix;
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
        // Debug.Log("Enter " + this.name);

        PortalTraveler traveler = other.GetComponent<PortalTraveler>();

        if (traveler)
        {
            AddTraveler(traveler);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Debug.Log("Exit" + this.name);

        PortalTraveler traveler = other.GetComponent<PortalTraveler>();

        if (traveler)
        {
            RemoveTraveler(traveler);
        }
    }

    private void AddTraveler(PortalTraveler traveler)
    {
        if (!travelers.Contains(traveler))
        {
            traveler.previousPosition = traveler.transform.position;
            travelers.Add(traveler);
            // int currentPortalSide = System.Math.Sign(Vector3.Dot(traveler.transform.position - transform.position, transform.right));
            // Debug.Log("Added traveler at side " + currentPortalSide + " in portal " + this.name);
        }
    }

    private void RemoveTraveler(PortalTraveler traveler)
    {
        if (travelers.Contains(traveler))
        {
            travelers.Remove(traveler);
            // Debug.Log("Removed traveler from portal " + this.name);
        }
    }

    private void TrackTravelers()
    {

    }

    private void MoveTraveler(PortalTraveler traveler)
    {

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


}
