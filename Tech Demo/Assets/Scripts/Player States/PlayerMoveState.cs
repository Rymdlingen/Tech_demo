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
    private float safeFallDistance = 20;
    private float stepHight = .2f;

    private float lookForCliffOffSet = 1f;

    // Constructor.
    public PlayerMoveState(PlayerController player) : base(player)
    {
        playerRigidbody = player.GetComponent<Rigidbody>();
    }

    // Methods.
    public override void EnterState()
    {
        playerRigidbody.useGravity = true;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
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

        Vector3 topOfPlayer = new Vector3(player.transform.position.x, player.transform.position.y + player.transform.lossyScale.y, player.transform.position.z);
        Vector3 bottomOfPlayer = new Vector3(player.transform.position.x, player.transform.position.y - player.transform.lossyScale.y, player.transform.position.z);

        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
        {
            player.TransitionToState(player.idleState);
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Ray looking for cliffs in the direction the player is moving.
        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

        Vector3 groundOffset = movementDirection * (speed * Time.fixedDeltaTime);

        float yOrigin = GetGroundHight(playerRigidbody.position);
        float y = GetGroundHight(playerRigidbody.position + groundOffset);

        Debug.Log(groundOffset);

        Vector3 rayStart2 = new Vector3(bottomOfPlayer.x, topOfPlayer.y, bottomOfPlayer.z) + groundOffset;

        // Raycast for keeping player on the ground.
        Physics.Raycast(rayStart2, Vector3.down, out toGroundHit);
        //Debug.DrawLine(rayStart2, toGroundHit.point, Color.red, 2f);


        // Ray for detecting to high falls and stoping the player.
        Vector3 offSet = movementDirection * lookForCliffOffSet;

        Vector3 rayStart = topOfPlayer + offSet;

        Ray checkForCliff = new Ray(rayStart, Vector3.down);

        Physics.Raycast(checkForCliff, out lookForCliffs/*, (player.transform.localScale.y * 2) + safeFallDistance*/);

        //Debug.DrawLine(rayStart, lookForCliffs.point, Color.blue, .5f);

        // use normal for looking for slope.
        /*if (lookForCliffs.normal.y)
        {

        }*/



        // Only move player if ground is not to steep or there is not a cliff.
        if (y < yOrigin - 4)
        {
            playerRigidbody.velocity = Vector3.zero;
        }
        else
        {
            float moveToY = 0;

            if (toGroundHit.point.y < bottomOfPlayer.y)
            {
                moveToY = -9.81f;
                Debug.Log("going downhill");
            }
            else if (toGroundHit.point.y > bottomOfPlayer.y)
            {
                moveToY = 0;
                Debug.Log("going uphill");
            }


            //Debug.Log(moveToY);

            MovePlayer(new Vector3(horizontalInput, moveToY, verticalInput), toGroundHit.point.y);
        }
    }

    private void MovePlayer(Vector3 direction, float groundY)
    {
        float currentSpeed = playerRigidbody.velocity.magnitude;
        float speedDifference = Mathf.Max(0, speed - currentSpeed);



        Debug.Log(currentSpeed);

        float slope = GetSlopeInDirection(playerRigidbody.position, direction);

        if (slope > 2)
        {
            return;
        }

        Vector3 forceDirection = direction;
        forceDirection.y = 0;
        forceDirection.Normalize();
        forceDirection.y = slope;
        forceDirection.Normalize();

        if (currentSpeed > speed)
        {
            playerRigidbody.velocity = forceDirection * speed;
            return;
        }

        //forceDirection = new Vector3(0, 1, 0).normalized;

        float counteractGravityFactor = Mathf.Max(0, forceDirection.y);
        playerRigidbody.AddForce(Vector3.up * counteractGravityFactor * 9.81f, ForceMode.Acceleration);



        Debug.DrawRay(playerRigidbody.position + Vector3.down * .9f, forceDirection * 2, Color.blue, .5f);

        // Zeros out the players current movement (otherwise the movemnet would just keep adding up)
        //playerRigidbody.velocity = new Vector3(0, 0, 0);

        // Keeps the player on the ground.
        //playerRigidbody.MovePosition(new Vector3(player.transform.position.x, groundY + player.transform.lossyScale.y, player.transform.position.z));

        // Moves the player in the given direction.
        playerRigidbody.AddForce(forceDirection * speedDifference, ForceMode.VelocityChange);
    }

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

    private float GetSlopeInDirection(Vector3 position, Vector3 direction)
    {
        direction.y = 0;
        float d = 0.1f;

        float yOrigin = GetGroundHight(position.x, position.z);
        float y = GetGroundHight(position + direction.normalized * d);


        return (y - yOrigin) / d;
    }

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

    private float GetGroundHight(Vector3 position)
    {
        return GetGroundHight(position.x, position.z);
    }


}
