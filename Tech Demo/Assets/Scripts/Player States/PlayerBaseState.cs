using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerController player;

    public PlayerBaseState(PlayerController player)
    {
        this.player = player;
    }

    public abstract void EnterState();

    public abstract void Update();

}
