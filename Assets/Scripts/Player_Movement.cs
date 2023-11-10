using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Player_Movement : MonoBehaviour
{

    private float horizontal;
    private float speed = 8f;
    private float jumping_power = 16f;
    private bool is_facing_right = true;

    private float coyote_time = 0.2f;
    private float coyote_counter = 0f;

    private float jump_buffer_time = 0.2f;
    private float jump_buffer_counter = 0f;

    private bool double_jump = false;

    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform ground_check;
    [SerializeField] private LayerMask ground_layer;

    // Update is called once per frame
    void Update()
    {

        body.velocity = new Vector2(horizontal * speed, body.velocity.y);

        horizontal = Input.GetAxisRaw("Horizontal");

        if (IsGrounded() && !Input.GetButton("Jump"))
        {
            double_jump = false;
        }

        if (IsGrounded())
        {
            coyote_counter = coyote_time;
        }
        else
        {
            coyote_counter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jump_buffer_counter = jump_buffer_time;
        }
        else
        {
            jump_buffer_counter -= Time.deltaTime;
        }

        if ((jump_buffer_counter > 0f && coyote_counter > 0f) || (Input.GetButtonDown("Jump") && double_jump))
        {
            body.velocity = new Vector2(body.velocity.x, jumping_power);

            double_jump = !double_jump;

            jump_buffer_counter = 0f;
        }

        if (Input.GetButtonUp("Jump") && body.velocity.y > 0f)
        {
            body.velocity = new Vector2(body.velocity.x, body.velocity.y * 0.5f);

            coyote_counter = 0f;
        }

        Flip();

    }

    private bool IsGrounded() {
        return Physics2D.OverlapCircle(ground_check.position, 0.2f, ground_layer);
    }

    private void Flip()
    {
        if ((is_facing_right && horizontal < 0f) || (!is_facing_right && horizontal > 0f))
        {
            is_facing_right = !is_facing_right;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }
}
