using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;

    public bool isImpostor = false;
    public bool isAlive = true;
    public float ghostMoveSpeed = 20f;
    private Vector3 center = new Vector3(220.19f, 10.918f, 247.17f);
    //public float maxDistance = 200f;

    private bool[] inputs;
    private float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;

    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;

        inputs = new bool[5];
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        if (!isAlive)
        {
            this.transform.GetComponent<CharacterController>().GetComponent<Collider>().enabled = false;
            jumpSpeed = 0f;
            gravity = 0f;
        }

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }
        Move(_inputDirection);

    }
    private void Update()
    {
        //so player not wander to far
        if (!isAlive)
        {
            float distance = Vector3.Distance(transform.position, center);
            if (distance > 90f)
            {
                transform.position = center;
            }
        }
    }

    /// <summary>
    /// when player dies we disable its collider, the Conntroller.Move() method is unavailable when there is no collider
    /// there for we create our own "scuffed" move method.
    /// </summary>
    /// <param name="moveDirection">the directions passed by the Move method</param>
    /// <param name="moveSpeed">move speed</param>
    public void GhostMove(Vector3 moveDirection, float moveSpeed)
    {
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    /// <param name="_inputDirection">imputs</param>
    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        if (isAlive)
        {
            controller.Move(_moveDirection);

        }
        else
        {
            GhostMove(_moveDirection,ghostMoveSpeed);
        }
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }
    /// <summary>
    /// Teleport the player to the map when the game starts       
    /// </summary>
    public void TeleportPlayerToMap()
    {
        controller.enabled = false;
        transform.position = Constants.mapSpwanLocations[id-1];
        controller.enabled = true;
        ServerSend.PlayerRotation(this);


    }

}