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
    private List<PortalTraveler> travelers;

    /*
    // Enums.
    enum PortalSide
    {
        Positive,
        Negative
    }
    */

    void Start()
    {
        // TODO player = GameObject.FindGameObjectWithTag("Player");
        travelers = new List<PortalTraveler> { };
    }

    void LateUpdate()
    {

        if (travelers.Count > 0)
        {
            for (int thisTraveler = 0; thisTraveler < travelers.Count; thisTraveler++)
            {
                PortalTraveler traveler = travelers[thisTraveler];

                int startPortalSide = System.Math.Sign(Vector3.Dot(traveler.previousPosition - transform.position, transform.right));
                int currentPortalSide = System.Math.Sign(Vector3.Dot(traveler.transform.position - transform.position, transform.right));

                if (startPortalSide != currentPortalSide)
                {
                    // Debug.Log("Travel " + name + " " + startPortalSide + " " + currentPortalSide);
                    Matrix4x4 matrix = destination.transform.localToWorldMatrix * transform.worldToLocalMatrix * traveler.transform.localToWorldMatrix;

                    traveler.Travel(matrix.GetColumn(3), matrix.rotation);

                    destination.AddTraveler(traveler);
                    RemoveTraveler(traveler);
                    thisTraveler--;
                }
            }
        }
    }

    private void MovePortalCamera()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Enter " + this.name);

        PortalTraveler traveler = other.GetComponent<PortalTraveler>();

        if (traveler)
        {
            AddTraveler(traveler);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Debug.Log("Exit" + this.name);

        PortalTraveler traveler = other.GetComponent<PortalTraveler>();

        if (traveler)
        {
            RemoveTraveler(traveler);
        }
    }

    private void AddTraveler(PortalTraveler traveler)
    {
        if (!travelers.Contains(traveler))
        {
            traveler.previousPosition = traveler.transform.position;
            travelers.Add(traveler);
            // int currentPortalSide = System.Math.Sign(Vector3.Dot(traveler.transform.position - transform.position, transform.right));
            // Debug.Log("Added traveler at side " + currentPortalSide + " in portal " + this.name);
        }
    }

    private void RemoveTraveler(PortalTraveler traveler)
    {
        if (travelers.Contains(traveler))
        {
            travelers.Remove(traveler);
            // Debug.Log("Removed traveler from portal " + this.name);
        }
    }

    private void TrackTravelers()
    {

    }

    private void MoveTraveler(PortalTraveler traveler)
    {

    }

    /*
    private PortalSide CalculatePortalSide(Vector3 travelerPosition)
    {
        PortalSide travelerSideOfPortal;
        Vector3 travelerOffsetFromPortal = travelerPosition - transform.position;

        float portalSide = Vector3.Dot(travelerOffsetFromPortal, Vector3.forward);
        if (portalSide > 0)
        {
            travelerSideOfPortal = PortalSide.Positive;
        }
        else
        {
            travelerSideOfPortal = PortalSide.Negative;
        }

        return travelerSideOfPortal;
    }
    */

    private void SetScreenRenderTexture()
    {

    }
}
