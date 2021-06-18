using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Fields
    private PlayerBaseState _currentState;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;

    // Properties
    public float walkSpeed { get => _walkSpeed; }
    public float runSpeed { get => _runSpeed; }

    public PlayerBaseState currentState { get => _currentState; }

    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }

    // Start is called before the first frame update.
    void Start()
    {
        idleState = new PlayerIdleState(this);
        moveState = new PlayerMoveState(this);

        TransitionToState(idleState);
    }

    // Update is called once per frame.
    void Update()
    {
        _currentState.Update();
    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdate();
    }

    public void TransitionToState(PlayerBaseState state)
    {
        _currentState = state;
        _currentState.EnterState();
    }

    // Raycast for checking if player can go there
    // WASD input
    // Shift for running
}
