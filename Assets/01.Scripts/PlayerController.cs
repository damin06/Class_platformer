using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
        [Header("Player Properties")]
    public float walkSpeed = 10;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;

    [Header("Player Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;

    [Header("Player States")]
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isWallJumping;

    private bool _startJump;
    private bool _realeaseJump;

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharactorController2D _charactorController;

    private void Awake()
    {
        _charactorController = GetComponent<CharactorController2D>();
    }

    private void Update()
    {
        if (!isWallJumping)
        {
            PlayerMove();
            SpriteFlip();
        }
        PlayerJump();
    }
    
    private void GravityCalculation()
    {
        if (_moveDirection.y > 0f && _charactorController.above)
        {
            _moveDirection.y = 0f;
        }
        _moveDirection.y -= gravity * Time.deltaTime;
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _startJump = true;
            _realeaseJump = false;
        }

        else if (context.canceled)
        {
            _startJump = false;
            _realeaseJump = true;
        }
    }

    private void SpriteFlip()
    {
        if (_input.x > 0)
            transform.localScale = Vector3.one;
        else if (_input.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = transform.localScale;
    }

    private void PlayerJump()
    {
        if (_charactorController.below) //on the ground
        {
            _moveDirection.y = 0;
            isJumping = false;
            isDoubleJumping = false;
            isTripleJumping = false;
            isWallJumping = false;

            if (_startJump)
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
                _charactorController.DisableGroundCheck(0.1f);
            }
        }
        else
        {
            if (_realeaseJump) //공중에.... 
            {
                _realeaseJump = false;
                if (_moveDirection.y > 0)
                {
                    _moveDirection.y *= 0.5f;
                }
            }


            if (_startJump) //더블점프
            {
                if (canTripleJump && (!_charactorController.left && !_charactorController.right))
                {
                    if (isDoubleJumping && !isTripleJumping)
                    {
                        _moveDirection.y = doubleJumpSpeed;
                        isTripleJumping = true;
                    }
                }


                if (canDoubleJump && (!_charactorController.left && !_charactorController.right))
                {
                    if (!isDoubleJumping)
                    {
                        _moveDirection.y = doubleJumpSpeed;
                        isDoubleJumping = true;
                    }
                }

                if (canWallJump && (_charactorController.left || _charactorController.right))
                {
                    if (_charactorController.right && _moveDirection.x <= 0)
                    {
                        _moveDirection.x = -xWallJumpSpeed;
                        _moveDirection.y = yWallJumpSpeed;
                        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    }
                    else if (_charactorController.left && _moveDirection.x >= 0)
                    {
                        _moveDirection.x = xWallJumpSpeed;
                        _moveDirection.y = yWallJumpSpeed;
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                    isWallJumping = true;

                    StartCoroutine(WallJumpWaiter());
                }

                _startJump = false;
            }
            

            GravityCalculation();
        }

        _charactorController.Move(_moveDirection * Time.deltaTime);

        /*if (_charactorController.above)
            _realeaseJump = true;*/
    }

    private void PlayerMove()
    {
        _moveDirection.x = _input.x * walkSpeed;
    }

    IEnumerator WallJumpWaiter()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(0.4f);
        isWallJumping = false;
    }
}
