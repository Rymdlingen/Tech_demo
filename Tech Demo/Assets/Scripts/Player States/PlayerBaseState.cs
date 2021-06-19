using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerControllerOld player;

    public PlayerBaseState(PlayerControllerOld player)
    {
        this.player = player;
    }

    public abstract void EnterState();

    public abstract void Update();

    public abstract void FixedUpdate();
}
