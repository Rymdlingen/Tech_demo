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

    private Vector3 cameraOffset;
    private float baseCameraBackwardsOffset;
    private float desiredCameraBackwardsOffset;
    private float currentCameraBackwardsOffset;

    // Start is called before the first frame update
    void Start()
    {
        // Save the start value of the camera pivots x and y rotation. 
        yRotation = transform.eulerAngles.y;
        xRotation = transform.eulerAngles.x;

        //Debug.Log(gameObject.transform.GetChild(0).name);
        cameraOffset = gameObject.transform.GetChild(0).localPosition;
        baseCameraBackwardsOffset = transform.localPosition.z;
        desiredCameraBackwardsOffset = -(cameraOffset.z / 2);
        currentCameraBackwardsOffset = baseCameraBackwardsOffset;
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
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }


        // When the target comes close to a portal move closer to the target.
        // raycast from target, if it hits a portal, move closer

        RaycastHit hit;
        Vector3 rayStart = new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z);
        // Debug.DrawRay(rayStart, player.transform.forward * 10, Color.red, 1);
        Physics.Raycast(rayStart, player.transform.forward, out hit);

        if (hit.collider && hit.transform.CompareTag("Portal"))
        {
            // Debug.Log(hit.distance);

            if (hit.distance < 5f)
            {
                currentCameraBackwardsOffset = Mathf.Lerp(currentCameraBackwardsOffset, desiredCameraBackwardsOffset, Time.deltaTime * 1.25f);
            }
        }
        else
        {
            currentCameraBackwardsOffset = Mathf.Lerp(currentCameraBackwardsOffset, baseCameraBackwardsOffset, Time.deltaTime * 3f);
        }

        //transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, currentCameraBackwardsOffset);
    }
}
