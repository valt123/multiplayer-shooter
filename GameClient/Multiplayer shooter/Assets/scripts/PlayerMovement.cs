using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Jump stuff
    public float fallMultiplier;
    public float lowJumpMultiplier;
    public float jumpVelocity;

    //Movement stuff
    public float movementSpeed;
    Vector3 movement;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {

        Movement();
    }

    void Movement()
    {
        movement.x = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        movement.z = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        rb.position += movement;

        Jump();
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && rb.velocity.y == 0)
        {
            rb.velocity += Vector3.up * jumpVelocity;
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
}
