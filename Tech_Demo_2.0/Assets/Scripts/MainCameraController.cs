using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainCameraController : MonoBehaviour
{
    [SerializeField] private TrackablePortalTraveler target;

    public event Action positionUpdated;

    private Vector3 unforcedPosition;
    private Vector3 forcedPositionDelta;

    private Vector3 unforcedRotation;

    public bool inForcingState = false;
    private const float forcingRegionSize = 9;
    private const float minTeleportationDistance = 15;

    public float forcingFactor;
    public float distanceFromPortal;
    public bool playerHasTeleported;
    public Vector3 forcedPosition;
    public Vector3 cameraDirection;

    private Portal forcingPortal;


    // Start is called before the first frame update
    void Start()
    {
        target.trackingUpdated += OnTargetTrackingUpdated;
        GetComponent<PortalTraveler>().traveled += OnTraveled;

        unforcedPosition = transform.position;
        unforcedRotation = transform.rotation.eulerAngles;
    }

    private void OnTargetTrackingUpdated()
    {
        transform.Translate(target.travelerPositionDelta);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + target.travelerRotationDelta);

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

    private void OnTraveled(PortalTraveler traveler, Portal traveledFrom, Portal traveledTo)
    {
        forcingPortal = traveledTo;
        inForcingState = true;
        unforcedPosition = transform.position - forcedPositionDelta;
        positionUpdated?.Invoke();
    }


    private void FixedUpdate()
    {

    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(forcedPosition, Vector3.one);
        Gizmos.DrawLine(forcedPosition, forcedPosition + cameraDirection);
    }
}

