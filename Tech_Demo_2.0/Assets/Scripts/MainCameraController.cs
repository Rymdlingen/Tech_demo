using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainCameraController : MonoBehaviour
{
    [SerializeField] private TrackablePortalTraveler target;

    public event Action positionUpdated;

    // Start is called before the first frame update
    void Start()
    {
        target.trackingUpdated += OnTargetTrackingUpdated;
        GetComponent<PortalTraveler>().traveled += OnTraveled;
    }

    // Update is called once per frame
    private void OnTargetTrackingUpdated()
    {
        transform.Translate(target.travelerPositionDelta);

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + target.travelerRotationDelta);

        positionUpdated?.Invoke();
    }

    private void OnTraveled(PortalTraveler traveler)
    {
        positionUpdated?.Invoke();
    }


    private void FixedUpdate()
    {

    }
}
