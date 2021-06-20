using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Fields.
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float airSpeed;

    private Rigidbody playerRigidbody;
    private bool isGrounded;

    private float horizontalInput;
    private float verticalInput;

    private float activSpeedSetting;

    [SerializeField] private float safeFallDistance;
    private float stepHight = .2f;
    private float slopeRestriction = 1; // 1:slopeRestriction (slopeRestriction 1 = 45° is walkable)

    // Properties.


    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        // Check if the player is grounded
        // if grounded, move with arrows, don't slide on slopes
        // if not grounded, fall but be able to controll directiion a bit



        // Check if grounded
        //    yes
        //       check if do interaction (add later) (enters another state)
        //       check if jump (add later) (continues after no)
        //       check if crouch (continues after no)
        //
        //    no (just continue)

        // Check if player is holding shift (speed modifier)
        //    yes
        //       speed is runSpeed
        //
        //    no
        //       speed is walkSpeed

        // Check if move input
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
        //
        //    no
        //       Check if grounded
        //       yes
        //          Hold position
        //
        //       no
        //          move towards ground

        // end

        // Calculate if players is on the ground
        // Set isGrounded to true or false.
        float yPlayer = playerRigidbody.position.y - playerRigidbody.transform.localScale.y;
        float yGround = GetGroundHight(playerRigidbody.position);

        // TODO Differs 0.000004 and doesn't work.
        print("player " + yPlayer);
        print("ground " + yGround);

        if (yPlayer > yGround)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = true;
        }

        print(isGrounded);


        // Check if grounded.
        // If player is grounded check if they are pressing any action buttons (interact, jump or crouch).
        if (isGrounded)
        {
            // check if do interaction. (add later) (enter another state)
            // check if jump. (add later) (continues after no)
            // check if crouch. (add later) (continues after no)
        }
        else
        {
            // Nothing.(just continue)
        }

        // Check if player is holding shift (speed modifier).
        // If the player is holding shift they are running, change the speed accordingly.
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // Run.
            if (activSpeedSetting != runSpeed)
            {
                activSpeedSetting = runSpeed;
            }
        }
        else
        {
            // Walk.
            if (activSpeedSetting != walkSpeed)
            {
                activSpeedSetting = walkSpeed;
            }
        }

        // Save movement input.
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Check if move input.
        // If the player is pressing the arrow/WASD-keys, move the player.
        if (horizontalInput != 0 || verticalInput != 0)
        {
            //       Check if player can move in that direction
            //       yes
            //          check if grounded
            //          yes
            //             move limited to speed
            //             end
            //
            //          no
            //             move a little bit in the direction (still floating in the air and down towards the ground)
            //             end
            //
            //       no (just continue)


            // Ray looking at the point where the player is moving in ths frame.
            Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
            Vector3 groundOffset = movementDirection * (activSpeedSetting * Time.fixedDeltaTime);

            float yGroundThisFrame = GetGroundHight(playerRigidbody.position);
            float yGroundNextFrame = GetGroundHight(playerRigidbody.position + groundOffset);
            Debug.Log(groundOffset);

            // Only move player if there is not a cliff. By checking this before checking if the player is grounded I make sure they player don't fall down a cliff while jumping.
            // TODO move slope check here?? or is slope check just for moving upwards?
            if (yGroundNextFrame < yGroundThisFrame - safeFallDistance)
            {
                // But of the player is in the air they should still fall down.
                playerRigidbody.velocity = Vector3.zero;
            }
            else
            {
                // If the player is on the ground move normally.
                if (isGrounded)
                {
                    // yes
                    //    move limited to speed
                    //    end
                    MovePlayer(new Vector3(horizontalInput, 0, verticalInput));
                }
                // If the player is in the air, move slower.
                else
                {
                    // TODO change speed earlier if the player is not grounded??

                    // no
                    //    move a little bit in the direction (still floating in the air and down towards the ground)
                    //    end
                }
            }
        }
        else
        {
            // Check if grounded
            if (isGrounded)
            {
                // hold position.
                playerRigidbody.velocity = Vector3.zero;
            }
            else
            {
                // move towards ground (so do nothing?? gravity will just take over??)
            }
        }
    }


    // Methods.

    // Moves the player.
    private void MovePlayer(Vector3 direction)
    {
        // Used to make sure there is only force added if the speed is not reached.
        float actualSpeed = playerRigidbody.velocity.magnitude;
        float speedDifference = Mathf.Max(0, activSpeedSetting - actualSpeed);
        // Debug.Log(actualSpeed);

        // Get the slope in the direction the player is moving.
        float slope = GetSlopeInDirection(playerRigidbody.position, direction);

        // If the ground is to steep, the player can't move in that direction.
        if (slope > slopeRestriction)
        {
            return;
        }

        // Add extra vertical force depending on the slope of the ground.
        Vector3 forceDirection = direction;
        forceDirection.y = 0;
        forceDirection.Normalize();
        forceDirection.y = slope;
        forceDirection.Normalize();

        // Reduce the speed if the player is going to fast (so the player doesn't go faster downhills).
        if (actualSpeed > activSpeedSetting)
        {
            playerRigidbody.velocity = forceDirection * activSpeedSetting;
            return;
        }

        // Add extra force if the player is going upwards, so they don't lose speed upwards.
        float counteractGravityFactor = Mathf.Max(0, forceDirection.y);
        playerRigidbody.AddForce(Vector3.up * counteractGravityFactor * 9.81f, ForceMode.Acceleration);

        // Ray on the ground, with the current slope in the direction the player is moving.
        Debug.DrawRay(playerRigidbody.position + Vector3.down * .9f, forceDirection * 2, Color.blue, .5f);

        // Moves the player in the given direction.
        playerRigidbody.AddForce(forceDirection * speedDifference, ForceMode.VelocityChange);
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
    private float GetSlopeInDirection(Vector3 position, Vector3 direction)
    {
        direction.y = 0;
        float d = 0.1f;

        float yOrigin = GetGroundHight(position.x, position.z);
        float y = GetGroundHight(position + direction.normalized * d);


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
}
