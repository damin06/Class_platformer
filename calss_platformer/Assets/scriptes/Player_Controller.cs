using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controller : MonoBehaviour
{
    [SerializeField]private float walkSpeed = 10f;
    [SerializeField]private float gravity = 20f;
    [SerializeField]private float jumpSpeed = 15f;

    //player state
    public bool isJumping;

    private bool _startJump;
    private bool _rleaseJump;

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D  _charachtercontroller;

    private void Awake()
    {
        _charachtercontroller = GetComponent<CharacterController2D >();
    }

    private void Update()
    {
        _moveDirection.x = _input.x * walkSpeed;

        if(_charachtercontroller.below) //on the ground
        {
            if(_startJump)
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
            }
        }
        else//공중에.....
        {
            if(_rleaseJump)
            {
                _rleaseJump = false;
                if(_moveDirection.y > 0)
                {
                    _moveDirection.y *=0.5f;
                }
            }
            _moveDirection.y -= gravity  * Time.deltaTime;
        }

        _charachtercontroller.Move(_moveDirection * Time.deltaTime);
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            _startJump = true;
        }
        else if(context.canceled)
        {
            _rleaseJump = true;
        }
    }
}
