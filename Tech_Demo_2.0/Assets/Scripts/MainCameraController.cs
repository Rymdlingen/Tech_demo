using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainCameraController : MonoBehaviour
{
    [SerializeField] private TrackablePortalTraveler target;

    public event Action positionUpdated;

    [SerializeField] private Vector3 playerCameraPosition;
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

    private void Update()
    {


    }

    private void OnTargetTrackingUpdated()
    {

        playerCameraPosition = target.trackingTarget.transform.position;
        RaycastHit hit;
        rayStart = unforcedPosition + transform.localToWorldMatrix.MultiplyVector(cameraToPlayerInCameraSpace);
        // rayStart = unforcedPosition + (transform.right * directionFromCameraToPlayer.x + transform.up * directionFromCameraToPlayer.y + transform.forward * directionFromCameraToPlayer.z);
        Debug.DrawRay(rayStart, unforcedPosition - rayStart, Color.red);
        string[] layerNames = new string[] { "Environment", "Terrain" };
        LayerMask environmentLayerMask = LayerMask.GetMask(layerNames);


        if (Physics.Raycast(rayStart, unforcedPosition - rayStart, out hit, Vector3.Distance(target.transform.position, unforcedPosition), environmentLayerMask))
        {
            Debug.Log("Racast hit: " + LayerMask.LayerToName(hit.transform.gameObject.layer));
            forcedPosition = hit.point;
            forcedPositionDelta = hit.point - unforcedPosition;
            transform.position = forcedPosition;
            // Move camera
            Matrix4x4 unforcedWorldMatrix = Matrix4x4.TRS(unforcedPosition, Quaternion.Euler(unforcedRotation), Vector3.one);
            unforcedPosition += unforcedWorldMatrix.MultiplyVector(target.travelerPositionDelta);
        }
        else
        {

            transform.position = unforcedPosition;


            transform.Translate(target.travelerPositionDelta);
            unforcedPosition = transform.position;
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
        inForcingState = true;
        unforcedPosition = transform.position;
        forcedPosition = unforcedPosition;
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

