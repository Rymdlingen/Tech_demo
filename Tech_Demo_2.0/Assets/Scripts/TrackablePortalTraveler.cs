using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackablePortalTraveler : PortalTraveler
{
    #region Fields.

    [SerializeField] public Transform trackingTarget;
    public Vector3 previousTravelerPosition { get; private set; }
    public Matrix4x4 previousTravelerWorldToLocalMatrix { get; private set; }
    public Vector3 latestTravelerPosition { get; private set; }
    public Matrix4x4 latestTravelerWorldToLocalMatrix { get; private set; }
    public Vector3 travelerPositionDelta => previousTravelerWorldToLocalMatrix.MultiplyVector(latestTravelerPosition - previousTravelerPosition);

    public Vector3 previousTravelerRotation { get; private set; }
    public Vector3 latestTravelerRotation { get; private set; }
    public Vector3 travelerRotationDelta => latestTravelerRotation - previousTravelerRotation;

    public event Action trackingUpdated;

    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ResetTracking();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateTracking();
    }

    public override void EnterPortal()
    {
        base.EnterPortal();

        ResetTracking();
    }

    private void ResetTracking()
    {
        previousTravelerPosition = trackingTarget.position;
        latestTravelerPosition = trackingTarget.position;

        previousTravelerWorldToLocalMatrix = trackingTarget.worldToLocalMatrix;
        latestTravelerWorldToLocalMatrix = trackingTarget.worldToLocalMatrix;

        previousTravelerRotation = trackingTarget.rotation.eulerAngles;
        latestTravelerRotation = trackingTarget.rotation.eulerAngles;
    }

    private void UpdateTracking()
    {
        previousTravelerPosition = latestTravelerPosition;
        latestTravelerPosition = trackingTarget.position;

        previousTravelerWorldToLocalMatrix = latestTravelerWorldToLocalMatrix;
        latestTravelerWorldToLocalMatrix = trackingTarget.worldToLocalMatrix;

        previousTravelerRotation = latestTravelerRotation;
        latestTravelerRotation = trackingTarget.rotation.eulerAngles;

        // Tell that the tracking have been updated.
        trackingUpdated?.Invoke();
    }
}
