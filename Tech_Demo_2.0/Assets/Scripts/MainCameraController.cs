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




    // Start is called before the first frame update
    void Start()
    {
        target.trackingUpdated += OnTargetTrackingUpdated;
        GetComponent<PortalTraveler>().traveled += OnTraveled;

        unforcedPosition = transform.position;
        unforcedRotation = transform.rotation.eulerAngles;

        Vector3 cameraToPlayerInWorldSpace = target.transform.position - transform.position;
        cameraToPlayerInCameraSpace = transform.worldToLocalMatrix.MultiplyVector(cameraToPlayerInWorldSpace);
    }

    private void OnTargetTrackingUpdated()
    {


        string[] layerNames = new string[] { "Environment", "Terrain", "Portal Frame" };
        LayerMask environmentLayerMask = LayerMask.GetMask(layerNames);
        LayerMask envirnomentLayer = LayerMask.NameToLayer(layerNames[0]);
        LayerMask portalFrameLayer = LayerMask.NameToLayer(layerNames[2]);

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
                // Move camera
                Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);
                unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);
                inForcingState = true;

            }
            // If the hit is environment, make it seethrough.
            else if (hit.collider.gameObject.layer == envirnomentLayer && false)
            {
                Material[] materials = hit.collider.gameObject.GetComponent<MeshRenderer>().materials;
                foreach (Material material in materials)
                {
                    // TODO find a way to make it transparent.
                    Color seethroughColor = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
                    material.color = seethroughColor;
                }

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
                // Move camera
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
    /*
        private void OnTargetTrackingUpdated()
        {
            Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);

            unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);
            unforcedRotation += target.travelerRotationDelta;
             playerHasTeleported = Vector3.Distance(transform.position, target.transform.position) > minTeleportationDistance;



            if (!inForcingState && playerHasTeleported)
            {
                inForcingState = true;
                forcingPortal = target.lastUsedPortal;
            }

            if (inForcingState && false)
            {
                Transform portalTransform = forcingPortal.transform;

                Vector3 unforcedPositionInPortalSpace = portalTransform.worldToLocalMatrix.MultiplyPoint(unforcedPosition);
                Vector3 forcedPositionInPortalSpace = Vector3.Scale(unforcedPositionInPortalSpace, Vector3.right);


                forcedPosition = portalTransform.localToWorldMatrix.MultiplyPoint(forcedPositionInPortalSpace);

                cameraDirection = portalTransform.position - forcedPosition;

                if (!playerHasTeleported)
                {
                    Vector3 targetPositionInPortalSpace = portalTransform.worldToLocalMatrix.MultiplyPoint(target.transform.position);

                    cameraDirection = portalTransform.localToWorldMatrix.MultiplyVector(targetPositionInPortalSpace - forcedPositionInPortalSpace);

                    cameraDirection *= -1;
                }

                Vector3 forcedRotation = Quaternion.LookRotation(cameraDirection, Vector3.up).eulerAngles;

                distanceFromPortal = Mathf.Abs(forcedPositionInPortalSpace.x);
                forcingFactor = Mathf.Clamp01(1 - distanceFromPortal / forcingRegionSize);

                transform.position = Vector3.Lerp(unforcedPosition, forcedPosition, forcingFactor);
                transform.rotation = Quaternion.Lerp(Quaternion.Euler(unforcedRotation), Quaternion.Euler(forcedRotation), forcingFactor);

                forcedPositionDelta = forcedPosition - unforcedPosition;

                if (forcingFactor == 0)
                {
                    inForcingState = false;
                }
            }
            else
            {
            transform.position = unforcedPosition;
            transform.rotation = Quaternion.Euler(unforcedRotation);
            }

            positionUpdated?.Invoke();
        }
    */

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

