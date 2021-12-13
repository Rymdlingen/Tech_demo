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

    // Enums.
    enum PortalSide
    {
        Positive,
        Negative
    }

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
                // PortalSide previousPortalSide = CalculatePortalSide(traveler.previousPosition);
                // PortalSide currentPortalSide = CalculatePortalSide(traveler.transform.position);

                int previousPortalSide = System.Math.Sign(Vector3.Dot(traveler.previousPosition - transform.position, Vector3.forward));
                int currentPortalSide = System.Math.Sign(Vector3.Dot(traveler.transform.position - transform.position, Vector3.forward));

                Debug.Log("previous side: " + previousPortalSide.ToString());
                Debug.Log("current side: " + currentPortalSide.ToString());

                if (previousPortalSide != currentPortalSide)
                {
                    Matrix4x4 matrix = destination.transform.localToWorldMatrix * transform.worldToLocalMatrix * traveler.transform.localToWorldMatrix;

                    traveler.Travel(matrix.GetColumn(3), matrix.rotation);

                    destination.AddTraveler(traveler);
                    RemoveTraveler(traveler);
                    thisTraveler--;
                }
                else
                {
                    //traveler.previousTransform = traveler.transform;
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

        if (travelers.Contains(traveler))
        {
            RemoveTraveler(traveler);
        }
    }

    private void AddTraveler(PortalTraveler traveler)
    {
        traveler.previousPosition = traveler.transform.position;
        travelers.Add(traveler);
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

    private void SetScreenRenderTexture()
    {

    }
}
