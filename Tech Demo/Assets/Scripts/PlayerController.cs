using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Fields.
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    private float activeSpeedSetting;

    private Rigidbody playerRigidbody;

    private bool isGrounded = false;

    // Fields.
    [SerializeField] private float airSpeed;
    [SerializeField] private float jumpForce;


    private float horizontalInput;
    private float verticalInput;

    [SerializeField] private float safeFallDistance;
    [SerializeField] private float stepHight = .2f;
    [SerializeField] private float slopeRestriction = 0;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        isGrounded = true;

        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        Vector3 input = new Vector3(horizontalInput, 0, verticalInput);

        // Check if grounded
        if (isGrounded)
        {
            //       check if do interaction (add later) (enters another state)
            //       check if jump (add later) (continues after no)
            //       check if crouch (continues after no)
            playerRigidbody.useGravity = false;
        }
        else
        {
            playerRigidbody.useGravity = true;
        }
        //    no (just continue)


        // Check if player is holding shift (speed modifier) or is in the air and set the correct speed.
        if (!isGrounded)
        {
            // Air movement.
            if (activeSpeedSetting != airSpeed)
            {
                activeSpeedSetting = airSpeed;
            }
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // Run.
            if (activeSpeedSetting != runSpeed)
            {
                activeSpeedSetting = runSpeed;
            }
        }
        else
        {
            // Walk.
            if (activeSpeedSetting != walkSpeed)
            {
                activeSpeedSetting = walkSpeed;
            }
        }


        // Check if move input
        if (verticalInput != 0 || horizontalInput != 0)
        {


            //    yes
            //       Check if player can move in that direction
            //       yes
            //          check if grounded
            //          yes
            //             move limited to speed
            //             end
            //
            //          no
            //             move a little bit in the direction (still floating in the air)
            //             end
            //
            //       no (just continue)
            /*}
            else
            {
                //    no
                //       Check if grounded
                //       yes
                //          Hold position
                //
                //       no
                //          move towards ground, fall
            }
        }*/
            // Ray looking at the point where the player is moving in ths frame.
            Vector3 movementDirection = input.normalized;
            Vector3 groundOffset = movementDirection * (activeSpeedSetting * Time.fixedDeltaTime);

            float yGroundThisFrame = GetGroundHeight(playerRigidbody.position);
            float yGroundNextFrame = GetGroundHeight(playerRigidbody.position + groundOffset);

            // Only move player if there is not a cliff. By checking this before checking if the player is grounded I make sure they player don't fall down a cliff while jumping.
            // TODO move slope check here?? or is slope check just for moving upwards?

            // ("Ground next frame: " + yGroundNextFrame);
            // Debug.Log("Ground this frame: " + yGroundThisFrame);
            // Debug.Log("Ground this frame - fall distance: " + (yGroundThisFrame - safeFallDistance));



            if (yGroundNextFrame < yGroundThisFrame - safeFallDistance)
            {
                Debug.Log("SAVED!!");
                // But if the player is in the air they should still fall down.
                playerRigidbody.velocity = Vector3.zero;
            }
            else
            {
                // If the player is on the ground move normally.
                if (isGrounded)
                {
                    Debug.Log("ground move");
                    // Player moves limited to the active speed.
                    MovePlayerOnGround(new Vector3(horizontalInput, 0, verticalInput));
                }
                // If the player is in the air, move slower.
                else
                {
                    Debug.Log("air move");
                    // Player is able to move at a lower speed (air speed) and falls to the ground.
                    MovePlayerInAir(new Vector3(horizontalInput, 0, verticalInput));
                }
            }
        }
        else
        {
            // Check if grounded
            if (isGrounded)
            {
                Debug.Log("hold");
                // Hold position.
                playerRigidbody.velocity = Vector3.zero;
            }

            // If not grounded gravity is on and will move player towards the ground.
        }
    }


    // Methods.

    // Moves player when player is in the air.
    private void MovePlayerInAir(Vector3 direction)
    {
        // Moves the player in the given direction.
        playerRigidbody.AddForce(direction.normalized * activeSpeedSetting * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    // Moves the player id player is on ground.
    private void MovePlayerOnGround(Vector3 inputs)
    {
        var collider = GetComponent<CapsuleCollider>();

        Vector3 direction = inputs.normalized;

        playerRigidbody.velocity = Vector3.zero;

        // Get the slope in the direction the player is moving.
        float radius = collider.radius;

        float farDistance = radius * Mathf.Sin(slopeRestriction * Mathf.Deg2Rad);
        float frontSlope = GetSlopeInDirection(transform.position, direction, farDistance);
        float slope = GetSlopeInDirection(transform.position, direction, 0);
        float backSlope = GetSlopeInDirection(transform.position, direction, -farDistance);


        // Add extra vertical force depending on the slope of the ground.
        Vector3 moveDirection = direction;
        moveDirection.y = (frontSlope + slope + backSlope) / 3;
        moveDirection.Normalize();

        // Next frame position.
        Vector3 groundOffset = moveDirection * (activeSpeedSetting * Time.fixedDeltaTime);

        Vector3 newPosition = transform.position + groundOffset;

        float newGroundHeight = GetGroundHeight(newPosition);
        newPosition.y = newGroundHeight + collider.height / 2;

        float allowedSlope = Mathf.Tan(slopeRestriction * Mathf.Deg2Rad);

        float wideNextPositionSlope = GetGradient(newPosition, radius).magnitude;
        float narrowNextPositionSlope = GetGradient(newPosition, 0.1f).magnitude;

        // Check if we are stepping into too steep teriritory.
        if (wideNextPositionSlope > allowedSlope && narrowNextPositionSlope > allowedSlope)
        {
            // Allow to step uo/down cliffs or stairs.
            if (frontSlope > allowedSlope)
            {
                return;
            }
        }

        transform.position = newPosition;



        // Ray on the ground, with the current slope in the direction the player is moving.
        // Close slope is blue.
        Debug.DrawRay(transform.position + Vector3.down * .9f, new Vector3(direction.x, slope, direction.z).normalized * safeFallDistance, Color.blue);
        // Far slope is red.
        Debug.DrawRay(transform.position + direction * farDistance + Vector3.down * .9f, new Vector3(direction.x, frontSlope, direction.z).normalized * safeFallDistance, Color.red);


    }


    // Check the gradient around the player.
    private Vector3 GetGradient(float x, float z, float d)
    {
        float yL = GetGroundHight(x - d, z);
        float yR = GetGroundHight(x + d, z);
        float yF = GetGroundHight(x, z + d);
        float yB = GetGroundHight(x, z - d);

        float dd = d * 2;

        return new Vector3((yR - yL) / dd, (yF - yB) / dd);
    }

    private Vector3 GetGradient(Vector3 position, float d)
    {
        return GetGradient(position.x, position.z, d);
    }

    // Checks the slope of the ground in the direction the player is moving.
    private float GetSlopeInDirection(Vector3 position, Vector3 direction, float offset)
    {
        direction.y = 0;
        float d = 0.1f;

        float yOrigin = GetGroundHeight(position + direction.normalized * offset);
        float y = GetGroundHeight(position + direction.normalized * (d + offset));


        return (y - yOrigin) / d;
    }

    // Checks the hight (y) of the ground in a specific point (x,z).
    private float GetGroundHight(float x, float z)
    {
        Vector3 origin = new Vector3(x, 100, z);
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit))
        {
            return origin.y - hit.distance;
        }

        return float.NegativeInfinity;
    }

    // Checks the hight (y) of the ground in a specific point (x,z), ignoring the y value that is sent in.
    private float GetGroundHeight(Vector3 position)
    {
        return GetGroundHight(position.x, position.z);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isGrounded && collision.gameObject.tag == "Terrain")
        {
            isGrounded = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain")
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain")
        {
            isGrounded = false;
        }
    }
}