using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainCameraController : MonoBehaviour
{
    [SerializeField] private TrackablePortalTraveler target;

    public event Action positionUpdated;

    [SerializeField] private Vector3 unforcedPosition;
    [SerializeField] private Vector3 forcedPositionDelta;

    private Vector3 unforcedRotation;

    private bool inForcingState = false;

    // public float forcingFactor;
    // public float distanceFromPortal;
    // public bool playerHasTeleported;
    [SerializeField] private Vector3 forcedPosition;
    // public Vector3 cameraDirection;

    [SerializeField] private Vector3 cameraToPlayerInCameraSpace;
    [SerializeField] private Vector3 rayStart;

    [SerializeField] private float cameraBuffer;

    string[] layerNames = new string[] { "Environment", "Terrain", "Portal Frame" };
    LayerMask environmentLayerMask;
    LayerMask envirnomentLayer;
    LayerMask portalFrameLayer;

    List<GameObject> transparentObjects = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        target.trackingUpdated += OnTargetTrackingUpdated;
        GetComponent<PortalTraveler>().traveled += OnTraveled;

        unforcedPosition = transform.position;
        unforcedRotation = transform.rotation.eulerAngles;

        Vector3 cameraToPlayerInWorldSpace = target.transform.position - transform.position;
        cameraToPlayerInCameraSpace = transform.worldToLocalMatrix.MultiplyVector(cameraToPlayerInWorldSpace);

        environmentLayerMask = LayerMask.GetMask(layerNames);
        envirnomentLayer = LayerMask.NameToLayer(layerNames[0]);
        portalFrameLayer = LayerMask.NameToLayer(layerNames[2]);
    }

    private void OnTargetTrackingUpdated()
    {
        // All rays.
        RaycastHit hit;
        rayStart = unforcedPosition + transform.localToWorldMatrix.MultiplyVector(cameraToPlayerInCameraSpace);
        // Main ray.
        Vector3 rayDirection = unforcedPosition - rayStart;
        Debug.DrawRay(rayStart, rayDirection, Color.green);
        // Ray to the left.
        Vector3 rayToTheLeft = rayDirection - Camera.main.nearClipPlane * transform.right;
        Debug.DrawRay(rayStart, rayToTheLeft, Color.red);
        // Ray to the right.
        Vector3 rayToTheRight = rayDirection + Camera.main.nearClipPlane * transform.right;
        Debug.DrawRay(rayStart, rayToTheRight, Color.blue);

        #region Transparent objects

        // Get a list of game objects that are between the player and the camera.
        List<GameObject> obstructingObjects = GetObstructingGameObjects(
            new Ray(rayStart, rayDirection),
            new Ray(rayStart, rayToTheLeft),
            new Ray(rayStart, rayToTheLeft)
            );

        // Set obstructing environment to transparent.
        foreach (var obstacle in obstructingObjects)
        {
            // If an object is not already in the transparent list, make it transparent and add it to the list.
            if (!transparentObjects.Contains(obstacle))
            {
                // Get all materials.
                Material[] materials = obstacle.GetComponent<MeshRenderer>().materials;
                foreach (Material material in materials)
                {
                    // Set color to transparent.
                    Color transparentColor = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
                    material.color = transparentColor;
                }
                // Add to list.
                transparentObjects.Add(obstacle);
            }
        }

        // Change the transparent objects back if they are no longer obstructing.
        for (int i = 0; i < transparentObjects.Count; i++)
        {
            // If an transparent object is not in the list of obstructing objects, make it fully visible and remove it from the transparent list.
            if (!obstructingObjects.Contains(transparentObjects[i]))
            {
                // Get all materials.
                Material[] materials = transparentObjects[i].GetComponent<MeshRenderer>().materials;
                foreach (Material material in materials)
                {
                    // Set color to fully visible.
                    Color transparentColor = new Color(material.color.r, material.color.g, material.color.b, 1f);
                    material.color = transparentColor;
                }
                // Remove from list.
                transparentObjects.Remove(transparentObjects[i]);
                i--;
            }
        }

        #endregion

        #region Move camera

        if (Physics.Raycast(rayStart, rayDirection, out hit, Vector3.Distance(rayStart, unforcedPosition), environmentLayerMask) || Physics.Raycast(rayStart, rayToTheRight, out hit, Vector3.Distance(rayStart, unforcedPosition), environmentLayerMask) || Physics.Raycast(rayStart, rayToTheLeft, out hit, Vector3.Distance(rayStart, unforcedPosition), environmentLayerMask))
        {
            // If the hit is a portal, make sure tha camera moves inside the portal frame.
            if (hit.collider.gameObject.layer == portalFrameLayer)
            {
                Debug.Log("Racast hit: " + LayerMask.LayerToName(hit.transform.gameObject.layer) + ", " + hit.transform.gameObject.name);
                forcedPosition = hit.point + cameraBuffer * transform.forward;
                forcedPositionDelta = hit.point - unforcedPosition;
                //transform.position = forcedPosition;
                transform.position = Vector3.Lerp(transform.position, forcedPosition, .05f);
                // Update the unforced position.
                Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);
                unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);

                inForcingState = true;

            }
            // If the hit is environment, make it seethrough.
            else if (hit.collider.gameObject.layer == envirnomentLayer)
            {
                if (transform.position != unforcedPosition) transform.position = unforcedPosition;
                transform.Translate(target.travelerPositionDelta);
                unforcedPosition = transform.position;
            }
            // If a hit is terrain or building, I don't know what to do really. MAKE A DECISION TODO
            else
            {
                Debug.Log("Racast hit: " + LayerMask.LayerToName(hit.transform.gameObject.layer) + ", " + hit.transform.gameObject.name);
                forcedPosition = hit.point + cameraBuffer * transform.forward;
                forcedPositionDelta = hit.point - unforcedPosition;
                //transform.position = forcedPosition;
                transform.position = Vector3.Lerp(transform.position, forcedPosition, .05f);
                // Update the unforced position.
                Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);
                unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);

                inForcingState = true;
            }
        }
        else
        {
            inForcingState = false;
            if (transform.position != unforcedPosition)
            {
                // transform.position = unforcedPosition;
                transform.position = Vector3.Lerp(transform.position, unforcedPosition, .05f);
            }
            transform.Translate(target.travelerPositionDelta);
            //unforcedPosition = transform.position;
            Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);
            unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);
        }

        #endregion

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + target.travelerRotationDelta);

        unforcedRotation = transform.rotation.eulerAngles;

        positionUpdated?.Invoke();
    }

    private List<GameObject> GetObstructingGameObjects(params Ray[] rays)
    {
        var gameObjects = new List<GameObject>();

        foreach (var ray in rays)
        {
            foreach (var hit in Physics.RaycastAll(ray, Vector3.Distance(rayStart, unforcedPosition), environmentLayerMask))
            {
                if (!gameObjects.Contains(hit.transform.gameObject) && hit.transform.GetComponent<MeshRenderer>())
                {
                    gameObjects.Add(hit.transform.gameObject);
                }
            }
        }

        return gameObjects;
    }

    private void OnTraveled(PortalTraveler traveler)
    {
        Debug.Log("OnTraveled from main camera script called");

        if (inForcingState)
        {
            forcedPosition = transform.position;
            unforcedPosition = target.trackingTarget.position;
        }
        else
        {
            unforcedPosition = transform.position;
            forcedPosition = unforcedPosition;
        }

        positionUpdated?.Invoke();
    }


    private void FixedUpdate()
    {

    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(unforcedPosition, Vector3.one);
        // Gizmos.DrawLine(forcedPosition, forcedPosition + cameraDirection);
    }
}

