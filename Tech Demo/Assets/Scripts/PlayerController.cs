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

            float yGroundThisFrame = GetGroundHight(playerRigidbody.position);
            float yGroundNextFrame = GetGroundHight(playerRigidbody.position + groundOffset);

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

        Vector3 direction = inputs.normalized;

        playerRigidbody.velocity = Vector3.zero;

        // Get the slope in the direction the player is moving.
        float radius = playerRigidbody.GetComponent<CapsuleCollider>().radius;

        float farDistance = radius * Mathf.Sin(slopeRestriction * Mathf.Deg2Rad);
        float frontSlope = GetSlopeInDirection(playerRigidbody.position, direction, farDistance);
        float slope = GetSlopeInDirection(playerRigidbody.position, direction, 0);
        float backSlope = GetSlopeInDirection(transform.position, direction, -farDistance);


        Vector3 groundOffset = direction * (activeSpeedSetting * Time.fixedDeltaTime);
        float farGroundHight = GetGroundHight(playerRigidbody.position + groundOffset + direction * radius);
        float closeGroundHight = GetGroundHight(playerRigidbody.position + groundOffset);


        // Ray on the ground, with the current slope in the direction the player is moving.
        // Close slope is blue.
        Debug.DrawRay(transform.position + Vector3.down * .9f, new Vector3(direction.x, slope, direction.z).normalized * safeFallDistance, Color.blue);
        // Far slope is red.
        Debug.DrawRay(transform.position + direction * farDistance + Vector3.down * .9f, new Vector3(direction.x, frontSlope, direction.z).normalized * safeFallDistance, Color.red);

        // Add extra vertical force depending on the slope of the ground.
        Vector3 forceDirection = direction;
        forceDirection.y = Mathf.Max((frontSlope + slope) / 2, slope);

        if (frontSlope >= slope)
        {
            if (farGroundHight > closeGroundHight + 0.01f)
            {
                forceDirection.y = (frontSlope + slope) / 2;
            }
            else
            {
                forceDirection.y = backSlope;
            }
        }
        else
        {
            if (farGroundHight < closeGroundHight)
            {
                forceDirection.y = backSlope;
            }
            else
            {
                forceDirection.y = slope;
            }
        }

        forceDirection.Normalize();


        /* // Ray on the ground, with the current slope in the direction the player is moving.
         // Close slope is blue.
         Debug.DrawRay(playerRigidbody.position + Vector3.down * .9f, new Vector3(flatForceDirection.x, closeSlope, flatForceDirection.z) * safeFallDistance, Color.blue, .5f);
         // Far slope is red.
         Debug.DrawRay(playerRigidbody.position + Vector3.down * .9f, new Vector3(flatForceDirection.x, farSlope, flatForceDirection.z) * safeFallDistance, Color.red, .5f);
        */



        // If the ground is to steep, the player can't move in that direction.
        /*
                float fallDistance = GetGroundHight(transform.position) - closeGroundHight;
                if (fallDistance < safeFallDistance)
                {
                    // Will continue.
                    if (forceDirection.y < 0)
                    {
                        if (GetGroundHight(transform.position) > closeGroundHight)
                        {

                        }
                        else
                        {
                            forceDirection.y = 0;
                            forceDirection.Normalize();
                        }
                    }
                }
                else if (farSlope > slopeRestriction || closeSlope > slopeRestriction)
                {
                    Debug.Log("slope stop");
                    return;
                }

                // Add extra force if the player is going upwards, so they don't lose speed upwards.
                float counteractGravityFactor = Mathf.Max(0, farSlope);
                playerRigidbody.AddForce(Vector3.up * counteractGravityFactor * 9.81f, ForceMode.Acceleration);
                // Debug.Log(counteractGravityFactor);
        */
        Debug.Log("Force direction: " + forceDirection);
        // Moves the player in the given direction.
        playerRigidbody.AddForce(forceDirection * activeSpeedSetting, ForceMode.VelocityChange);

        Debug.DrawRay(transform.position + Vector3.down * .9f, forceDirection, Color.green, .5f);
    }


    // Check the gradient around the player.
    private Vector3 GetGradient(float x, float z)
    {
        float d = 0.1f;
        float yL = GetGroundHight(x - d, z);
        float yR = GetGroundHight(x + d, z);
        float yF = GetGroundHight(x, z + d);
        float yB = GetGroundHight(x, z - d);

        float dd = d * 2;

        return new Vector3((yR - yL) / dd, (yF - yB) / dd);
    }

    // Checks the slope of the ground in the direction the player is moving.
    private float GetSlopeInDirection(Vector3 position, Vector3 direction, float offset)
    {
        direction.y = 0;
        float d = 0.1f;

        float yOrigin = GetGroundHight(position + direction.normalized * offset);
        float y = GetGroundHight(position + direction.normalized * (d + offset));


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
    private float GetGroundHight(Vector3 position)
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