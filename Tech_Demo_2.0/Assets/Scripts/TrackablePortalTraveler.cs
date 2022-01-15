using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackablePortalTraveler : PortalTraveler
{
    [SerializeField] private Transform trackingTarget;
    public Vector3 previousTravelerPosition { get; private set; }
    public Vector3 latestTravelerPosition { get; private set; }
    public Vector3 travelerPositionDelta => latestTravelerPosition - previousTravelerPosition;

    public Vector3 previousTravelerRotation { get; private set; }
    public Vector3 latestTravelerRotation { get; private set; }
    public Vector3 travelerRotationDelta => latestTravelerRotation - previousTravelerRotation;


    // Start is called before the first frame update
    void Start()
    {
        ResetTracking();
    }

    // Update is called once per frame
    void Update()
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

        previousTravelerRotation = trackingTarget.rotation.eulerAngles;
        latestTravelerRotation = trackingTarget.rotation.eulerAngles;
    }

    private void UpdateTracking()
    {
        previousTravelerPosition = latestTravelerPosition;
        latestTravelerPosition = trackingTarget.position;

        previousTravelerRotation = latestTravelerRotation;
        latestTravelerRotation = trackingTarget.rotation.eulerAngles;
    }
}
