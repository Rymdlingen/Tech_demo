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
        Posetive,
        Negative
    }


    void Start()
    {

        // TODO player = GameObject.FindGameObjectWithTag("Player");
        travelers = new Dictionary<PortalTraveler, PortalSide> { };
    }

    void LateUpdate()
    {
        foreach (KeyValuePair<PortalTraveler, PortalSide> traveler in travelers)
        {
            // TODO change to != when the calculation method is actually calculating.
            if (traveler.Value == CalculatePortalSide(traveler.Key))
            {
                player.GetComponent<PortalTraveler>().Travel(destination);
                RemoveTraveler(traveler.Key);
            }
        }
    }

    private void MoveCamera()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");

        if (other.GetComponent<PortalTraveler>() != null)
        {
            AddTraveler(other.GetComponent<PortalTraveler>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit");

        if (travelers.ContainsKey(other.GetComponent<PortalTraveler>()))
        {
            // RemoveTraveler(other.GetComponent<PortalTraveler>());
        }
    }

    private void AddTraveler(PortalTraveler traveler)
    {
        travelers.Add(traveler, CalculatePortalSide(traveler));

        Debug.Log(travelers);
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
        // TODO
        return PortalSide.Posetive;
    }

    private void SetScreenRenderTexture()
    {

    }
}
