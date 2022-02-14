using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainCameraController : MonoBehaviour
{
    // Debug player camera and main camera position to see if the main camera follows properly.
    public static void DebugSync(string message, bool always = false)
    {
        var mainCamera = Camera.main.gameObject.transform;
        var playerCamera = GameObject.Find("Player Camera").transform;

        if (mainCamera.GetComponent<PortalTraveler>().lastUsedPortal != null || always)
        {
            Debug.LogWarning(message);

            Vector3 v = mainCamera.position;
            Quaternion q = mainCamera.rotation;
            Vector3 r = q.eulerAngles;

            Debug.Log($"{v.x} {v.y} {v.z}");
            Debug.Log($"{r.x} {r.y} {r.z}");
            Debug.Log($"{q.x} {q.y} {q.z} {q.w}");

            v = playerCamera.position;
            q = playerCamera.rotation;
            r = q.eulerAngles;

            Debug.Log($"{v.x} {v.y} {v.z}");
            Debug.Log($"{r.x} {r.y} {r.z}");
            Debug.Log($"{q.x} {q.y} {q.z} {q.w}");
        }
    }

    [SerializeField] private TrackablePortalTraveler target;

    // Sending events.
    public event Action positionUpdated;
    public event Action targetTraveled;

    // Used for cameras position and rotation.
    private Vector3 unforcedPosition;
    private Vector3 unforcedRotation;
    private Vector3 forcedPosition;
    [SerializeField] private float cameraBuffer;
    private bool inForcingState = false;

    // Used for rays.
    private Vector3 cameraToPlayerInCameraSpace;
    private Vector3 rayStart;

    // Layers.
    private string[] layerNames = new string[] { "Big Environment Object", "Terrain", "Portal Frame" };
    private LayerMask environmentLayersMask;
    private LayerMask bigEnvirnomentObjectLayerMask;
    private LayerMask portalFrameLayerMask;
    private LayerMask terrainLayerMask;

    private List<GameObject> transparentObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Listening to events.
        target.trackingUpdated += OnTargetTrackingUpdated;
        GetComponent<PortalTraveler>().traveled += OnTraveled;

        // Set starting position and rotation relative to target.
        transform.position = target.trackingTarget.transform.position;
        transform.rotation = target.trackingTarget.transform.rotation;
        unforcedPosition = transform.position;
        unforcedRotation = transform.rotation.eulerAngles;

        // Used to calculate the targets position in cameras local space.
        Vector3 cameraToPlayerInWorldSpace = target.transform.position - transform.position;
        cameraToPlayerInCameraSpace = transform.worldToLocalMatrix.MultiplyVector(cameraToPlayerInWorldSpace);

        // Layer masks.
        environmentLayersMask = LayerMask.GetMask(layerNames);
        bigEnvirnomentObjectLayerMask = LayerMask.GetMask(layerNames[0]);
        portalFrameLayerMask = LayerMask.GetMask(layerNames[2]);
        terrainLayerMask = LayerMask.GetMask(layerNames[1]);
    }

    // Called if the target position have been updated. Makes sure the camera follows the target, takes care of making obstructing objects transparent and moving the camera closer to the player in some cases.
    private void OnTargetTrackingUpdated()
    {
        RaycastHit hit;
        rayStart = unforcedPosition + transform.localToWorldMatrix.MultiplyVector(cameraToPlayerInCameraSpace);

        // All ray directions. TODO needs way more rays, both wider and closer together.
        Vector3 rayDirection = unforcedPosition - rayStart;
        Vector3 rayToTheLeft = rayDirection - Camera.main.nearClipPlane * 3 * transform.right;
        Vector3 rayToTheRight = rayDirection + Camera.main.nearClipPlane * 3 * transform.right;

        Debug.DrawRay(rayStart, rayDirection, Color.green);
        Debug.DrawRay(rayStart, rayToTheLeft, Color.red);
        Debug.DrawRay(rayStart, rayToTheRight, Color.blue);

        #region Transparent objects

        // Get a list of game objects that are between the player and the camera.
        List<GameObject> obstructingObjects = GetObstructingGameObjects(
            new Ray(rayStart, rayDirection),
            new Ray(rayStart, rayToTheLeft),
            new Ray(rayStart, rayToTheLeft)
            );

        // Set obstructing objects to transparent.
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

        // Tried to use these instead of the long if condition below, but then it always gives the hit of the middle raycast (because it is last), and I couldn't figure out how I should do it nicer. 
        bool leftRaycastHit = Physics.Raycast(rayStart, rayToTheLeft, out hit, Vector3.Distance(rayStart, unforcedPosition), environmentLayersMask);
        bool rightRaycastHit = Physics.Raycast(rayStart, rayToTheRight, out hit, Vector3.Distance(rayStart, unforcedPosition), environmentLayersMask);
        bool middleRaycastHit = Physics.Raycast(rayStart, rayDirection, out hit, Vector3.Distance(rayStart, unforcedPosition), environmentLayersMask);

        // The hit is always set to the middle ray with means it is null if the camera is trying to change position if the other two rays are hitting. TODO
        // This is set to terrain layer mask instead of environment layer mask, so it only reacts to the terrain and not to, for example, the portal frame. This is wrong.
        if (Physics.Raycast(rayStart, rayDirection, out hit, Vector3.Distance(rayStart, unforcedPosition), terrainLayerMask) || Physics.Raycast(rayStart, rayToTheRight, out hit, Vector3.Distance(rayStart, unforcedPosition), terrainLayerMask) || Physics.Raycast(rayStart, rayToTheLeft, out hit, Vector3.Distance(rayStart, unforcedPosition), terrainLayerMask))
        {
            // Debug.Log("Racast hit: " + LayerMask.LayerToName(hit.transform.gameObject.layer) + ", " + hit.transform.gameObject.name);

            // If the hit is a portal, make sure tha camera moves inside the portal frame.
            if (LayerMask.GetMask(LayerMask.LayerToName(hit.collider.gameObject.layer)) == portalFrameLayerMask)
            {
                Debug.Log("Hit the portal frame");
                // Debug.Log("Racast hit: " + LayerMask.LayerToName(hit.transform.gameObject.layer) + ", " + hit.transform.gameObject.name);
                // Get the forced position and use it.
                forcedPosition = hit.point + cameraBuffer * transform.forward;
                transform.position = Vector3.Lerp(transform.position, forcedPosition, 1f * Time.deltaTime);

                // Update the unforced position.
                Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);
                unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);

                inForcingState = true;
            }
            // If the hit is environment, do normal movement.
            else if (LayerMask.GetMask(LayerMask.LayerToName(hit.collider.gameObject.layer)) == bigEnvirnomentObjectLayerMask)
            {
                inForcingState = false;
                if (transform.position != unforcedPosition)
                {
                    // transform.position = Vector3.Lerp(transform.position, unforcedPosition, .05f);
                    transform.position = Vector3.Lerp(transform.position, unforcedPosition, 5f * Time.deltaTime);
                }
                Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);
                unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);
                transform.position = unforcedPosition;

            }
            // If a hit is terrain or building, I don't know what to do really. MAKE A DECISION TODO
            else
            {
                // Debug.Log("Racast hit: " + LayerMask.LayerToName(hit.transform.gameObject.layer) + ", " + hit.transform.gameObject.name);
                forcedPosition = hit.point + cameraBuffer * transform.forward;
                transform.position = Vector3.Lerp(transform.position, forcedPosition, 5f * Time.deltaTime);

                // Update the unforced position.
                Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);
                unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);

                inForcingState = true;
            }
        }
        else
        {
            Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);
            unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);

            // This doesn't do anything since i'm setting the position below. TODO
            if (transform.position != unforcedPosition)
            {
                // transform.position = unforcedPosition;
                // transform.position = Vector3.Lerp(transform.position, unforcedPosition, .05f);
                transform.position = Vector3.Lerp(transform.position, unforcedPosition, 5f * Time.deltaTime);
            }

            transform.position = unforcedPosition;

            inForcingState = false;

        }

        #endregion

        // Update rotation, couldn't get the rotation to work as I wanted in the forced state.
        if (false && inForcingState)
        {
            transform.LookAt(target.transform);
            unforcedRotation = unforcedRotation + target.travelerRotationDelta;
        }
        else
        {
            transform.rotation = Quaternion.Euler(unforcedRotation + target.travelerRotationDelta);
            unforcedRotation = transform.rotation.eulerAngles;
        }

        // Tell that the position has been updated.
        positionUpdated?.Invoke();
    }

    // James helped me and wrote most of this method :)
    private List<GameObject> GetObstructingGameObjects(params Ray[] rays)
    {
        var gameObjects = new List<GameObject>();

        foreach (var ray in rays)
        {
            foreach (var hit in Physics.RaycastAll(ray, Vector3.Distance(rayStart, unforcedPosition), bigEnvirnomentObjectLayerMask))
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
        // Update forced and unforced positions and rotations after this object traveled.
        if (inForcingState)
        {
            forcedPosition = transform.position;
            unforcedPosition = target.trackingTarget.position;

            unforcedRotation = transform.rotation.eulerAngles;
        }
        else
        {
            unforcedPosition = transform.position;
            forcedPosition = unforcedPosition;

            unforcedRotation = transform.rotation.eulerAngles;
        }

        // Tell that the position has been updated.
        positionUpdated?.Invoke();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(unforcedPosition, Vector3.one);
        // Gizmos.DrawLine(forcedPosition, forcedPosition + cameraDirection);
    }
}

