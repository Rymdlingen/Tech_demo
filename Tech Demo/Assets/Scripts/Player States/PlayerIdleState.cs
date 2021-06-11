using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController player) : base(player)
    {

    }

    public override void EnterState()
    {
        // what to put here?
    }

    public override void Update()
    {
        // Transition to the move state if there is movemnet input (WASD or arrow keys).
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            player.TransitionToState(player.moveState);
        }
    }

    public override void FixedUpdate()
    {

    }
}
