using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Player_Movement : MonoBehaviour
{

    private float horizontal;
    private float speed = 8f;
    private float normal_jump_power = 16f;
    private float double_jump_power = 8f;
    private float floating_jump_power = 12f;
    private bool is_facing_right = true;

    private float coyote_time = 0.2f;
    private float coyote_counter = 0f;

    private float jump_buffer_time = 0.2f;
    private float jump_buffer_counter = 0f;

    private bool double_jump = false;
    private bool has_jumped = false;

    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform ground_check;
    [SerializeField] private LayerMask ground_layer;

    // Update is called once per frame
    void Update()
    {

        Debug.Log("Double Jump: " + double_jump);
        Debug.Log("Floating Jump: " + has_jumped);
        Debug.Log("Coyote Time: " + coyote_counter);

        body.velocity = new Vector2(horizontal * speed, body.velocity.y);

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
                body.velocity = new Vector2(body.velocity.x, floating_jump_power);
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

        // facing
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
