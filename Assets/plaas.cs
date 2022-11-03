using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plaas : MonoBehaviour
{
    private float y;
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (y < 1)
            {
                y += Time.deltaTime;
            }
            rb.AddForce(Vector3.up * y, ForceMode2D.Impulse);
        }
        else
        {
            y = 0;
        }
    }
}
