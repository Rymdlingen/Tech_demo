using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;

    private float xOffet;
    private float yOffset;

    [SerializeField] private float cameraSpeed;

    private float mouseX;
    private float mouseY;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        if (mouseX != 0)
        {
            transform.Rotate(0, mouseX, 0);
        }

        if (mouseY != 0)
        {
            transform.Rotate(mouseY, 0, 0);
        }

        transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, 0, transform.rotation.w);
    }
}
