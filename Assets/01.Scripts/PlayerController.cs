using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Properties")]
    public float walkSpeed = 10;
    public float creepSpeed = 5f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;
    public float wallRunSpeed = 8f;
    public float wallSlideAmount = 0.1f;
    public float dashSpeed = 40f;
    public float dashTime = 0.2f;
    public float dashCooldownTime = 1f;


    [Header("Player Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;
    public bool canWallRun;
    public bool canWallSlide;
    public bool canAirDash;
    public bool canGroundDash;


    [Header("Player States")]
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isWallJumping;
    public bool isWallRunning;
    public bool isWallSliding;
    public bool isDucking;
    public bool isCreeping;
    public bool isDashing;

    private bool _startJump;
    private bool _realeaseJump;

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharactorController2D _charactorController;
    private CapsuleCollider2D _capsuleCollider;
    private SpriteRenderer _spriteRenderer;

    private Vector2 _originColliderSize;
    private bool _ableToWallRun;

    private void Awake()
    {
        _charactorController = GetComponent<CharactorController2D>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _originColliderSize = _capsuleCollider.size;
    }

    private void Update()
    {
        if (!isWallJumping)
        {
            PlayerMove();
            SpriteFlip();
        }
        PlayerJump();
        WallRun();
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
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if((canAirDash && !_charactorController.below) || (canGroundDash && _charactorController.below))
            {
                StartCoroutine("Dash");
            }
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
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
            isWallSliding = false;

            if (_startJump)
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
                _ableToWallRun = true;
                _charactorController.DisableGroundCheck(0.1f);
            }

            //ducking, creeping
            if (_input.y < 0f)
            {
                if (!isDucking && !isCreeping)
                {
                    isDucking = true;
                    _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, _capsuleCollider.size.y / 2);
                    transform.position = new Vector2(transform.position.x, transform.position.y - (_originColliderSize.y/4));
                    _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp_crouching");
                }
            }
            else
            {
                if (isCreeping || isDucking)
                {
                    RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale, CapsuleDirection2D.Vertical, 0f, Vector2.up, _originColliderSize.y / 2f, _charactorController.layerMask);
                    if (!hitCeiling.collider)
                    {
                        isDucking = false;
                        isCreeping = false;
                        _capsuleCollider.size = _originColliderSize;
                        _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
                    }
                }
            }

            if (isDucking && _moveDirection.x != 0)
                isCreeping = true;
            else
                isCreeping = false;
        }
        else //���߿�.... 
        {
            if ((isCreeping || isDucking) && _moveDirection.y > 0)
                StartCoroutine(ClearDuckingState());

            if (_realeaseJump) 
            {
                _realeaseJump = false;
                if (_moveDirection.y > 0)
                {
                    _moveDirection.y *= 0.5f;
                }
            }


            if (_startJump) //��������
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

    private void WallRun()
    {
        if (_charactorController.below)
        {
            isWallRunning = false;
            return;
        }

        if (canWallRun && (_charactorController.left || _charactorController.right))
        {
            if (_input.y > 0f && _ableToWallRun)
            {
                _moveDirection.y = wallRunSpeed;
            }

            StartCoroutine(WallRunWaiter());
        }
    }

    private void GravityCalculation()
    {
        if (_moveDirection.y > 0f && _charactorController.above)
        {
            _moveDirection.y = 0f;
        }

        if (canWallSlide && (_charactorController.left || _charactorController.right))
        {
            if (_charactorController.hitWallThisFrame)
                _moveDirection.y = 0f;

            if (_moveDirection.y <= 0)
                _moveDirection.y -= gravity * wallSlideAmount * Time.deltaTime;
            else
                _moveDirection.y -= gravity * Time.deltaTime;

            isWallSliding = true;
        }
        else
            _moveDirection.y -= gravity * Time.deltaTime;
    }

    private void PlayerMove()
    {
        _moveDirection.x = _input.x * walkSpeed;
    }

    private IEnumerator WallJumpWaiter()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(0.4f);
        isWallJumping = false;
    }

    private IEnumerator WallRunWaiter()
    {
        isWallRunning = true;
        yield return new WaitForSeconds(0.5f);
        isWallRunning = false;
        if (!isWallRunning)
            _ableToWallRun = false;
    }

    private IEnumerator ClearDuckingState()
    {
        yield return new WaitForSeconds(.05f);

        RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale, CapsuleDirection2D.Vertical, 0f, Vector2.up, _originColliderSize.y / 2f, _charactorController.layerMask);
        if (!hitCeiling.collider)
        {
            isDucking = false;
            isCreeping = false;
            _capsuleCollider.size = _originColliderSize;
            _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
        }
    }
}
