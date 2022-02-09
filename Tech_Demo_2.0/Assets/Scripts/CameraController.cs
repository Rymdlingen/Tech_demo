using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private float xRotation;
    private float yRotation;

    [SerializeField] private float cameraHorizontalSpeed;
    [SerializeField] private float cameraVerticalSpeed;
    [SerializeField] private float maxPitch;
    [SerializeField] private float minPitch;

    private float mouseXMovement;
    private float mouseYMovement;

    private Vector3 focusPointYOffset;
    private Vector3 cameraFocusPoint;
    private Transform playerCameraTransform;

    Vector3 hitPoint;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.GetComponent<MainCameraController>().targetTraveled += RestrictCameraMovement;

        // Save the start value of the camera pivots x and y rotation. 
        yRotation = transform.eulerAngles.y;
        xRotation = transform.eulerAngles.x;

        playerCameraTransform = GameObject.Find("Player Camera").transform;
        focusPointYOffset = new Vector3(0, playerCameraTransform.localPosition.y, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // Store mouse movement.
        mouseXMovement = Input.GetAxis("Mouse X");
        mouseYMovement = Input.GetAxis("Mouse Y");
        yRotation = transform.eulerAngles.y;
        // Add the mouse x movement to the y rotation in the speed of horizontal camera movement.
        if (mouseXMovement != 0)
        {
            yRotation += mouseXMovement * cameraHorizontalSpeed;
        }

        // Add the mouse y movement to the x rotation in the speed of vertical camera movement, but keep the value within the pitch restrictions.
        if (mouseYMovement != 0)
        {
            xRotation = Mathf.Clamp(xRotation + mouseYMovement * cameraVerticalSpeed, minPitch, maxPitch);
        }

        // Rotate the camera pivot around both y and x and rotate player around the y axis.
        if (mouseXMovement != 0 || mouseYMovement != 0)
        {
            player.transform.rotation = Quaternion.Euler(0, yRotation, 0);
            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }
    }

    private void RestrictCameraMovement()
    {
        // raycast from focus point to camera
        cameraFocusPoint = transform.position + focusPointYOffset;
        LayerMask forcingFrameLayerMask = LayerMask.GetMask("Forcing Frame");

        Transform thisPortalTransform = player.GetComponent<PortalTraveler>().lastUsedPortal.destination.transform;
        Ray ray = new Ray(cameraFocusPoint, playerCameraTransform.position - cameraFocusPoint);

        Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(cameraFocusPoint, playerCameraTransform.position), Color.magenta);
        Debug.Log("Ray start: " + ray.origin + " ray direction: " + ray.direction);

        Plane xyPlane = new Plane(Vector3.forward, 0);

        float distanceToPlane;
        xyPlane.Raycast(ray, out distanceToPlane);
        hitPoint = ray.origin + ray.direction * distanceToPlane;

        // if ray hits portal screen, all is good



        // if it doesnt, restrict the camera
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(cameraFocusPoint, 0.2f);
        Gizmos.DrawSphere(hitPoint, 0.2f);
    }
}
