using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    // Forcing the camera.
    private float haveTraveledThreshold;
    private Vector3 hitPoint;
    private Vector3 desiredHitPoint;
    private Plane xyPlane;
    private PolygonCollider2D forcingFrame;
    private Transform thisPortalTransform;
    private Vector3 localHitPoint;
    private Vector3 localDesiredHitPoint;

    // Start is called before the first frame update
    void Start()
    {
        // Save the start value of the camera pivots x and y rotation. 
        yRotation = transform.eulerAngles.y;
        xRotation = transform.eulerAngles.x;

        // Find camera focus point.
        playerCameraTransform = GameObject.Find("Player Camera").transform;
        focusPointYOffset = new Vector3(0, playerCameraTransform.localPosition.y, 0);

        haveTraveledThreshold = (playerCameraTransform.position - transform.position).magnitude + 1f;
        Debug.Log(haveTraveledThreshold);
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
            xRotation = Mathf.Clamp(xRotation - mouseYMovement * cameraVerticalSpeed, minPitch, maxPitch);
        }

        // Rotate the camera pivot around both y and x and rotate player around the y axis.
        if (mouseXMovement != 0 || mouseYMovement != 0)
        {
            player.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }

        Debug.Log((Camera.main.transform.position - transform.position).magnitude);

        // If the distance between the camera and the player is bigger than threshold, restrict the camera movement.
        if ((Camera.main.transform.position - transform.position).magnitude > haveTraveledThreshold)
        {
            RestrictCameraMovement();
        }
        // If they are close, stop saving the portal.
        else
        {
            if (thisPortalTransform != null)
            {
                thisPortalTransform = null;
            }
        }
    }

    private void RestrictCameraMovement()
    {
        // Choose either the characters or the players last used portal.
        if (thisPortalTransform == null)
        {
            thisPortalTransform = player.GetComponent<PortalTraveler>().lastUsedPortal ? player.GetComponent<PortalTraveler>().lastUsedPortal.destination.transform : Camera.main.GetComponent<PortalTraveler>().lastUsedPortal.transform;
        }

        // Raycast from focus point to camera.
        cameraFocusPoint = transform.position + focusPointYOffset;
        Vector3 directionFromFocusToPlayerCamera = playerCameraTransform.position - cameraFocusPoint;
        Ray ray = new Ray(cameraFocusPoint, directionFromFocusToPlayerCamera);

        Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(cameraFocusPoint, playerCameraTransform.position), Color.magenta);
        // Debug.Log("Ray start: " + ray.origin + " ray direction: " + ray.direction);

        xyPlane = new Plane(Vector3.forward, 0); // why is this vector3 forward and not the local forward for the portal? TODO ask Matej

        // Check if the ray hit the plane, if it didn't invert the local z position of the start point for the ray.
        float distanceToPlane;
        Ray rayInPortalLocalSpace = new Ray(thisPortalTransform.worldToLocalMatrix.MultiplyPoint(ray.origin), thisPortalTransform.worldToLocalMatrix.MultiplyVector(ray.direction));
        bool didHitThePlane = xyPlane.Raycast(rayInPortalLocalSpace, out distanceToPlane);

        if (didHitThePlane)
        {
            hitPoint = ray.origin + ray.direction * distanceToPlane;
            localHitPoint = thisPortalTransform.worldToLocalMatrix.MultiplyPoint(hitPoint);
        }
        else
        {
            // Move tha starting point of the ray.
            Vector3 localOrigin = thisPortalTransform.worldToLocalMatrix.MultiplyPoint(ray.origin);
            Vector3 newLocalOrigin = new Vector3(localOrigin.x, localOrigin.y, -localOrigin.z);

            rayInPortalLocalSpace = new Ray(newLocalOrigin, thisPortalTransform.worldToLocalMatrix.MultiplyVector(ray.direction));
            xyPlane.Raycast(rayInPortalLocalSpace, out distanceToPlane);

            hitPoint = ray.origin + ray.direction * distanceToPlane;
            localHitPoint = thisPortalTransform.worldToLocalMatrix.MultiplyPoint(hitPoint);
        }

        // Check if the hit point is inside the forcing frame, if not force the rotation of the pivot.
        forcingFrame = thisPortalTransform.GetComponent<Portal>().forcingFrame;
        Vector2 localHitPoint2D = localHitPoint;
        Vector2 localDesiredHitPoint2D = forcingFrame.ClosestPoint(localHitPoint2D);
        localDesiredHitPoint = localDesiredHitPoint2D;
        desiredHitPoint = thisPortalTransform.localToWorldMatrix.MultiplyPoint(localDesiredHitPoint);

        // Debug.Log("Hit point: " + hitPoint + " in frame: " + desiredHitPoint);
        // Debug.Log("Local hit point: " + localHitPoint + " local in frame: " + localDesiredHitPoint);

        if (localDesiredHitPoint != localHitPoint)
        {
            Vector3 desiredDirection = (cameraFocusPoint - desiredHitPoint).normalized;
            Matrix4x4 lookAtMatrix = Matrix4x4.LookAt(Vector3.zero, desiredDirection, Vector3.up);
            Vector3 desiredEulerAngles = lookAtMatrix.rotation.eulerAngles;

            yRotation = desiredEulerAngles.y;
            // Debug.Log(desiredEulerAngles.y);
            xRotation = Mathf.Min(xRotation, desiredEulerAngles.x);

            // player.transform.localRotation = Quaternion.Lerp(player.transform.localRotation, Quaternion.Euler(0, yRotation, 0), 5f * Time.deltaTime);
            // transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(xRotation, 0, 0), 5f * Time.deltaTime);
            player.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(cameraFocusPoint, 0.2f);
        if (thisPortalTransform)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(thisPortalTransform.localToWorldMatrix.MultiplyPoint(localHitPoint), Vector3.one * 0.3f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(thisPortalTransform.localToWorldMatrix.MultiplyPoint(localDesiredHitPoint), Vector3.one * 0.3f);
        }

#if UNITY_EDITOR
        if (Application.isPlaying && xyPlane.normal != Vector3.zero && thisPortalTransform != null)
        {
            Handles.DrawWireDisc(thisPortalTransform.position, thisPortalTransform.forward, 10f);
        }
#endif
    }
}
