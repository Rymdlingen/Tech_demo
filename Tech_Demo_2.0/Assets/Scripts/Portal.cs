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

    // Private fields.
    [SerializeField] private Portal destination;
    private Camera portalCamera;
    private MeshRenderer screenMeshRenderer;
    private RenderTexture screenRenderTexture;

    [SerializeField] private GameObject player;
    private Camera playerCamera;

    // Can it be string for a name of the portal traveler or can I just use PortalTraveler? Or will that be the same for different travelers??
    private Dictionary<PortalTraveler, PortalSide> travelers;

    // Enums.
    enum PortalSide
    {
        Positive,
        Negative
    }

    void Start()
    {

        // TODO player = GameObject.FindGameObjectWithTag("Player");
        travelers = new Dictionary<PortalTraveler, PortalSide> { };
    }

    void LateUpdate()
    {
        // Transform travelerTransform = player.transform;
        // Vector3 position = travelerTransform.position - transform.position;
        // Debug.Log(position);

        if (travelers.Count > 0)
        {
            foreach (KeyValuePair<PortalTraveler, PortalSide> traveler in travelers)
            {
                if (traveler.Value != CalculatePortalSide(traveler.Key))
                {
                    Debug.Log("Travel!");
                    traveler.Key.Travel(destination);
                }
            }
        }

    }

    private void MovePortalCamera()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        PortalTraveler traveler = other.GetComponent<PortalTraveler>();

        if (traveler)
        {
            AddTraveler(traveler);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PortalTraveler traveler = other.GetComponent<PortalTraveler>();

        if (travelers.ContainsKey(traveler))
        {
            RemoveTraveler(traveler);
        }
    }

    private void AddTraveler(PortalTraveler traveler)
    {
        travelers.Add(traveler, CalculatePortalSide(traveler));
    }

    private void RemoveTraveler(PortalTraveler traveler)
    {
        travelers.Remove(traveler);
    }

    private void TrackTravelers()
    {

    }

    private void MoveTraveler(PortalTraveler traveler)
    {

    }

    private PortalSide CalculatePortalSide(PortalTraveler traveler)
    {
        PortalSide travelerSideOfPortal;
        Transform travelerTransform = traveler.transform;
        Vector3 travelerOffsetFromPortal = travelerTransform.position - transform.position;

        float portalSide = Vector3.Dot(travelerOffsetFromPortal, Vector3.forward);
        if (portalSide > 0)
        {
            travelerSideOfPortal = PortalSide.Positive;
        }
        else if (portalSide < 0)
        {
            travelerSideOfPortal = PortalSide.Negative;
        }
        else
        {
            travelerSideOfPortal = travelers[traveler];
        }

        return travelerSideOfPortal;
    }

    private void SetScreenRenderTexture()
    {

    }
}
