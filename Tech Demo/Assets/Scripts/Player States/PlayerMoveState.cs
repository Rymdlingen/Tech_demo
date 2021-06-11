using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    // Fields.
    private float speed;

    private float horizontalInput;
    private float verticalInput;

    private Rigidbody playerRigidbody;

    private RaycastHit toGroundHit;
    private RaycastHit lookForCliffs;

    // Move to controller? TODO
    private float safeFallDistance = 10;

    private float lookAheadOffSet = .75f;

    // Constructor.
    public PlayerMoveState(PlayerController player) : base(player)
    {

    }

    // Methods.
    public override void EnterState()
    {
        playerRigidbody = player.GetComponent<Rigidbody>();
    }

    public override void Update()
    {
        // If the player is holding shift they are running, so change the speed.
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // Run.
            if (speed != player.runSpeed)
            {
                speed = player.runSpeed;
            }
        }
        else
        {
            // Walk.
            if (speed != player.walkSpeed)
            {
                speed = player.walkSpeed;
            }
        }

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            // Raycast for keeping player on the ground.
            Physics.Raycast(player.transform.position, Vector3.down, out toGroundHit);

            // Ray looking for cliffs in the direction the player is moving.
            Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

            // clean up these variiablrs, do i need all of them? could the offset be a float?
            Vector3 offSet = movementDirection * lookAheadOffSet;

            Vector3 rayStart = new Vector3(player.transform.position.x, player.transform.lossyScale.y * 2, player.transform.position.z) + offSet;

            Ray checkForCliff = new Ray(rayStart, Vector3.down);

            Physics.Raycast(checkForCliff, out lookForCliffs);

            Debug.DrawLine(rayStart, lookForCliffs.point, Color.blue, .5f);

        }
        else
        {
            player.TransitionToState(player.idleState);
        }
    }

    public override void FixedUpdate()
    {
        MovePlayer(new Vector3(horizontalInput, -toGroundHit.point.y, verticalInput));
    }

    private void MovePlayer(Vector3 direction)
    {
        playerRigidbody.velocity = Vector3.zero;
        playerRigidbody.AddForce(direction * speed, ForceMode.Impulse);
    }


}
