using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;

    private float verticalInput;
    private float horizontalInput;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    private float currentSpeed;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Set the correct speed of the player, if the player is holding shift use run speed, if not use walk speed.
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) && currentSpeed != runSpeed)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            if (currentSpeed != walkSpeed)
            {
                currentSpeed = walkSpeed;
            }
        }

        // Save movement input.
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        // Move the character, based on the characters rotation (that is based on the cameras rotation).
        if (verticalInput != 0)
        {
            Vector3 forward = verticalInput * transform.forward;
            controller.Move(forward * Time.deltaTime * currentSpeed);
        }

        if (horizontalInput != 0)
        {
            Vector3 sideways = horizontalInput * transform.right;
            controller.Move(sideways * Time.deltaTime * currentSpeed);
        }

        // Check if the character is on the ground.
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity);
        // Debug.DrawLine(transform.position, hit.point, Color.blue);

        // Keep the character on the ground.
        transform.position = new Vector3(transform.position.x, hit.point.y + transform.lossyScale.y, transform.position.z);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(0.0f, 0.0f, 0.75f, 0.75f);

        // Convert the local coordinate values into world
        // coordinates for the matrix transformation.
        Gizmos.DrawCube(transform.Find("Camera pivot").Find("Player Camera").position, Vector3.one);
    }

}
