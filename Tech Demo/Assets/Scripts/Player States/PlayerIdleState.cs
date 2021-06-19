using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    private Rigidbody playerRigidbody;

    public PlayerIdleState(PlayerControllerOld player) : base(player)
    {
        playerRigidbody = player.GetComponent<Rigidbody>();
    }

    public override void EnterState()
    {
        playerRigidbody.useGravity = false;

        // what to put here?
    }

    public override void Update()
    {


    }

    public override void FixedUpdate()
    {
        // Transition to the move state if there is movemnet input (WASD or arrow keys).
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            player.TransitionToState(player.moveState);
            return;
        }

        playerRigidbody.velocity = Vector3.zero;
    }
}
