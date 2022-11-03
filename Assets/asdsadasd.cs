using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class asdsadasd : MonoBehaviour
{
    public Vector2 _input;
    Vector2 _moveDirection;
    CapsuleCollider2D cap;
    BoxCollider2D downco;
    public LayerMask adsdas;
    public bool ground = false;
    public bool isleft = false;
    // Start is called before the first frame update
    void Start()
    {
        cap = GetComponent<CapsuleCollider2D>();
        downco = GameObject.Find("down").GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMove();
        Vector3 a = new Vector3(_input.x, _input.y, 0);
        transform.position += a * 5 * Time.deltaTime;
        mola();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();

    }

    private void PlayerMove()
    {
        _moveDirection.x = _input.x * 5;
    }

    private void mola()
    {
        RaycastHit2D ray = Physics2D.BoxCast(cap.bounds.center, cap.size * 0.7f, 0, Vector2.down, 0.8f, adsdas);
        if (ray.collider)
        {
            ground = true;
        }
        else
        {
            ground = false;
        }

        RaycastHit2D letf = Physics2D.BoxCast(cap.bounds.center, cap.size * 0.7f, 0, Vector2.left, 0.8f, adsdas);
        if (letf.collider)
        {
            isleft = true;
        }
        else
        {
            isleft = false;
        }
    }
}
