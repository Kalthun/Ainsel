using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Player_Movement : MonoBehaviour
{
    //
    private float horizontal;
    private float speed = 8f;
    private bool is_facing_right = true;

    private float normal_jump_power = 16f;
    private float double_jump_power = 8f;
    private float falling_jump_power = 12f;
    private bool double_jump = false;
    private bool has_jumped = false;

    private float coyote_time = 0.2f;
    private float coyote_counter = 0f;

    private float jump_buffer_time = 0.2f;
    private float jump_buffer_counter = 0f;

    private bool can_dash = true;
    private bool is_dashing = false;
    private float dash_power = 24f;
    private float dash_time = 0.2f;
    private float dash_cooldown = 1f;

    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform ground_check;
    [SerializeField] private LayerMask ground_layer;

    // Update is called once per frame
    void Update()
    {

        if (is_dashing)
        {
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (IsGrounded() && !Input.GetButton("Jump"))
        {
            has_jumped = double_jump = false;
        }

        // coyote_time
        if (IsGrounded())
        {
            coyote_counter = coyote_time;
        }
        else
        {
            coyote_counter -= Time.deltaTime;
        }

        // jump_buffer
        if (Input.GetButtonDown("Jump"))
        {
            jump_buffer_counter = jump_buffer_time;
        }
        else
        {
            jump_buffer_counter -= Time.deltaTime;
        }

        // jump logic
        if ((jump_buffer_counter > 0f && coyote_counter > 0f) || (Input.GetButtonDown("Jump") && double_jump) || (Input.GetButtonDown("Jump") && !has_jumped))
        {
            if (coyote_counter > 0 || double_jump)
            {
                body.velocity = new Vector2(body.velocity.x, double_jump ? double_jump_power : normal_jump_power);

                double_jump = !double_jump;
            }
            else
            {
                body.velocity = new Vector2(body.velocity.x, falling_jump_power);
            }

            has_jumped = true;

            jump_buffer_counter = 0f;
        }

        // letting go early
        if (Input.GetButtonUp("Jump") && body.velocity.y > 0f)
        {
            body.velocity = new Vector2(body.velocity.x, body.velocity.y * 0.5f);

            coyote_counter = 0f;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && can_dash) // ! make input action
        {
            StartCoroutine(Dash());
        }

        // facing
        Flip();

    }

    private void FixedUpdate()
    {
        if (is_dashing)
        {
            return;
        }

        body.velocity = new Vector2(horizontal * speed, body.velocity.y);
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

    private IEnumerator Dash()
    {
        can_dash = false;
        is_dashing = true;
        float original_gravity = body.gravityScale;
        body.gravityScale = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            body.velocity = new Vector2(transform.localScale.x * dash_power, Math.Abs(transform.localScale.x * dash_power));
        }
        else if (Input.GetKey(KeyCode.S))
        {
            body.velocity = new Vector2(transform.localScale.x * dash_power, -1 * Math.Abs(transform.localScale.x * dash_power));
        }
        else
        {
            body.velocity = new Vector2(transform.localScale.x * dash_power, 0f);
        }

        yield return new WaitForSeconds(dash_time);
        body.gravityScale = original_gravity;
        body.velocity = new Vector2(transform.localScale.x, 0f);
        is_dashing = false;
        yield return new WaitForSeconds(dash_cooldown);
        can_dash = true;
    }
}
