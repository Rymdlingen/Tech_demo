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

    // Start is called before the first frame update
    void Start()
    {
        // Save the start value of the camera pivots x and y rotation. 
        xRotation = transform.rotation.x;
        yRotation = transform.rotation.y;
    }

    // Update is called once per frame
    void Update()
    {
        // Store mouse movement.
        mouseXMovement = Input.GetAxis("Mouse X");
        mouseYMovement = Input.GetAxis("Mouse Y");

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
        if (xRotation != 0 || yRotation != 0)
        {
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            player.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }
}
