using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;
    private bool isGrounded;

    private float verticalInput;
    private float horizontalInput;

    [SerializeField] float speed;
    [SerializeField] float quickSpeed;

    private float currentSpeed;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        // Set the correct speed of the player, based on if shift is pressed.
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) && currentSpeed != quickSpeed)
        {
            currentSpeed = quickSpeed;
        }
        else
        {
            if (currentSpeed != speed)
            {
                currentSpeed = speed;
            }
        }

        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity);
        // Debug.DrawRay(transform.position, Vector3.down * 10, Color.red, 2);
        // Debug.DrawLine(transform.position, hit.point, Color.blue);

        // Make sure the player is on the ground.
        if (!isGrounded)
        {
            //transform.position = new Vector3(transform.position.x, hit.point.y + transform.lossyScale.y, transform.position.z);
        }

        if (horizontalInput != 0)
        {
            Vector3 sideways = horizontalInput * transform.right;
            controller.Move(sideways * Time.deltaTime * currentSpeed);
        }

        if (verticalInput != 0)
        {
            Vector3 forward = verticalInput * transform.forward;
            controller.Move(forward * Time.deltaTime * currentSpeed);
        }

        transform.position = new Vector3(transform.position.x, hit.point.y + transform.lossyScale.y, transform.position.z);
        Debug.Log(isGrounded);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain")
        {
            Debug.Log("enter");
            isGrounded = true;
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain")
        {
            Debug.Log("exit");
            isGrounded = false;
        }
    }
}
