using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] private GameObject player;


    private float xOffset;
    private float yOffset;

    [SerializeField] private float cameraSpeed = 1;

    private float mouseX;
    private float mouseY;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        xOffset = transform.rotation.x;
        yOffset = transform.rotation.y;
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        Debug.Log($"{mouseX}, {mouseY}");

        if (mouseX != 0)
        {
            xOffset += mouseX * cameraSpeed;
            player.transform.rotation = Quaternion.Euler(0, xOffset, 0);
            //transform.Rotate(0, mouseX, 0);
        }

        if (mouseY != 0)
        {
            yOffset = Mathf.Clamp(yOffset + mouseY, 0.0f, 35f);
            //transform.Rotate(mouseY, 0, 0);
        }

        transform.rotation = Quaternion.Euler(yOffset, xOffset, 0);

    }
}
