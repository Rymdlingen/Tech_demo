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
    Vector3 hitPoint;
    Vector3 desiredHitPoint;
    Plane xyPlane;
    PolygonCollider2D forcingFrame;
    float saveZPosition;
    Transform thisPortalTransform;
    Vector3 localHitPoint;
    Vector3 localDesiredHitPoint;

    // Start is called before the first frame update
    void Start()
    {
        // Camera.main.GetComponent<MainCameraController>().targetTraveled += RestrictCameraMovement;

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
        //xRotation = transform.eulerAngles.x;
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

        if ((Camera.main.transform.position - transform.position).magnitude > 10)
        {
            RestrictCameraMovement();
        }
    }

    private void RestrictCameraMovement()
    {
        // raycast from focus point to camera
        cameraFocusPoint = transform.position + focusPointYOffset;
        LayerMask forcingFrameLayerMask = LayerMask.GetMask("Forcing Frame");

        thisPortalTransform = player.GetComponent<PortalTraveler>().lastUsedPortal.destination.transform;
        // Vector3 rayDirection = playerCameraTransform.position - cameraFocusPoint;
        // rayDirection = new Vector3(Mathf.Abs(rayDirection.x), rayDirection.y, rayDirection.z);
        Ray ray = new Ray(cameraFocusPoint, playerCameraTransform.position - cameraFocusPoint);

        Debug.DrawRay(ray.origin, ray.direction * Vector3.Distance(cameraFocusPoint, playerCameraTransform.position), Color.magenta);
        // Debug.Log("Ray start: " + ray.origin + " ray direction: " + ray.direction);

        xyPlane = new Plane(Vector3.forward, 0); // why is this vector3 forward and not the local forward for the portal? TODO ask Matej

        float distanceToPlane;
        Ray rayInPortalLocalSpace = new Ray(thisPortalTransform.worldToLocalMatrix.MultiplyPoint(ray.origin), thisPortalTransform.worldToLocalMatrix.MultiplyVector(ray.direction));
        xyPlane.Raycast(rayInPortalLocalSpace, out distanceToPlane);
        hitPoint = ray.origin + ray.direction * distanceToPlane;
        localHitPoint = thisPortalTransform.worldToLocalMatrix.MultiplyPoint(hitPoint);

        forcingFrame = thisPortalTransform.GetComponent<Portal>().forcingFrame;
        Vector2 localHitPoint2D = localHitPoint;
        Vector2 localDesiredHitPoint2D = forcingFrame.ClosestPoint(localHitPoint2D);
        localDesiredHitPoint = localDesiredHitPoint2D;
        desiredHitPoint = thisPortalTransform.localToWorldMatrix.MultiplyPoint(localDesiredHitPoint);
        // Debug.Log("Hit point: " + hitPoint + " in frame: " + desiredHitPoint);
        // Debug.Log("Local hit point: " + localHitPoint + " local in frame: " + localDesiredHitPoint);

        if (localDesiredHitPoint != localHitPoint)
        {
            Vector3 desiredDirection = (desiredHitPoint - cameraFocusPoint).normalized;
            Matrix4x4 lookAtMatrix = Matrix4x4.LookAt(Vector3.zero, -desiredDirection, Vector3.up);
            Vector3 desiredEulerAngles = lookAtMatrix.rotation.eulerAngles;



            yRotation = desiredEulerAngles.y;
            // Debug.Log(desiredEulerAngles.y);
            xRotation = Mathf.Min(xRotation, desiredEulerAngles.x);

            player.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }




        // compare the directions and get the angles between points
        // change pivot rotation by the difference angle


        // if ray hits portal screen, all is good



        // if it doesnt, restrict the camera
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
        if (Application.isPlaying && xyPlane.normal != Vector3.zero)
        {
            Handles.DrawWireDisc(player.GetComponent<PortalTraveler>().lastUsedPortal.destination.transform.position, player.GetComponent<PortalTraveler>().lastUsedPortal.destination.transform.forward, 10f);
        }
#endif
    }
}
