using GlobalType;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorController2D : MonoBehaviour
{
    public float raycastDistance = 0.2f;
    public LayerMask layerMask;
    public float slopAngleLimit = 45f;

    //flags
    public bool below;
    public bool above;
    public bool right;
    public bool left;

    public GroundType groundType;
        
    private Vector2 _slopNormal;
    private float _slopAngle;

    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;

    private bool _disableGroundCheck;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3];

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        _lastPosition = _rigidbody.position;

        if (_slopAngle != 0 && below)
        {
            if( (_moveAmount.x > 0f && _slopAngle > 0f) || (_moveAmount.x < 0f && _slopAngle < 0f))
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopAngle * Mathf.Deg2Rad) * _moveAmount.x);
            } 
        }

        _currentPosition = _lastPosition + _moveAmount;

        _rigidbody.MovePosition(_currentPosition);

        _moveAmount = Vector2.zero;
        if(!_disableGroundCheck) CheckGrounded();

        CheckOtherCollision();

    }
    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    private void CheckOtherCollision()
    {
        //left check
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size*0.75f, 
                                              0f, Vector2.left, raycastDistance, layerMask);

        if (leftHit.collider)
        {
            left = true;
        }
        else
        {
            left = false;
        }

        //right check
        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size*0.75f, 
                                              0f, Vector2.right, raycastDistance, layerMask);

        if (rightHit.collider)
        {
            right = true;
        }
        else
        {
            right = false;
        }
        //above check
                RaycastHit2D aboveHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size, 
                                              0f, Vector2.up, raycastDistance, layerMask);

        if (aboveHit.collider)
        {
            above = true;
        }
        else
        {
            above = false;
        }
    }  

    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, 
            CapsuleDirection2D.Vertical, 0f, Vector2.down, raycastDistance, layerMask);

        if (hit.collider)
        {
            groundType = DetermineGroundType(hit.collider);

            _slopNormal = hit.normal;
            _slopAngle = Vector2.SignedAngle(_slopNormal, Vector2.up);

            if (_slopAngle > slopAngleLimit || _slopAngle < -slopAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }
        }
        else
        {
            below = false;
            groundType = GroundType.none;
        }
    }

    private GroundType DetermineGroundType(Collider2D collider)
    {
        if (collider.GetComponent<GroundEffector>())
        {
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            return groundEffector.groundType;
        }
        else return GroundType.LevelGeom;
    }

    private void DrawDebugRays(Vector2 direction, Color color)
    {
        for(int i = 0; i < _raycastPosition.Length; i++)
        {
            Debug.DrawRay(_raycastPosition[i], direction * raycastDistance, color);
        }
    }

    public void DisableGroundCheck(float delayTime)
    {
        below = false;
        _disableGroundCheck = true;
        StartCoroutine("EnableGroundCheck",delayTime);
    }

    IEnumerator EnableGroundCheck(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        _disableGroundCheck = false;
    }
}