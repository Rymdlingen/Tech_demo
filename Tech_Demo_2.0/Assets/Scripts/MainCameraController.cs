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

    List<GameObject> seethroughObjects = new List<GameObject>();


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
        // TODO LERP

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

        var gameObjects = GetObstructingGameObjects(
            new Ray(rayStart, rayDirection),
            new Ray(rayStart, rayToTheLeft),
            new Ray(rayStart, rayToTheLeft)
            );



        // Set obstructing environment to transparent.
        foreach (var obstacle in gameObjects)
        {
            Debug.Log(obstacle.name);

            if (!seethroughObjects.Contains(obstacle))
            {
                // TODO need to set it back to not transparent somehow
                Material[] materials = obstacle.GetComponent<MeshRenderer>().materials;
                foreach (Material material in materials)
                {
                    // TODO find a way to make it transparent.
                    Color seethroughColor = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
                    material.color = seethroughColor;
                }
                seethroughObjects.Add(obstacle);
            }
        }

        for (int i = 0; i < seethroughObjects.Count; i++)
        {
            if (!gameObjects.Contains(seethroughObjects[i]))
            {
                // TODO need to set it back to not transparent somehow
                Material[] materials = seethroughObjects[i].GetComponent<MeshRenderer>().materials;
                foreach (Material material in materials)
                {
                    // TODO find a way to make it transparent.
                    Color seethroughColor = new Color(material.color.r, material.color.g, material.color.b, 1f);
                    material.color = seethroughColor;
                }
                seethroughObjects.Remove(seethroughObjects[i]);
                i--;
            }
        }

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

