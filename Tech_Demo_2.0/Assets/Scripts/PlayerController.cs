using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;
    bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Ray ground =
        if (!isGrounded)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            controller.Move(new Vector3(Input.GetAxis("Horizontal"), 0, 0) * Time.deltaTime);
        }

        if (Input.GetAxis("Vertical") != 0)
        {
            controller.Move(new Vector3(0, 0, Input.GetAxis("Vertical") * Time.deltaTime));
        }
    }
}
