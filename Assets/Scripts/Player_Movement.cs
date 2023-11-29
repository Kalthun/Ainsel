using System;
using System.Collections;
using UnityEngine;


public enum GrappleMode
{
    HookShot,
    SwingShot
}

public class Player_Movement : MonoBehaviour
{

    // generic movement
    private float moving;
    private bool is_facing_right = true;

    private float move_velocity = 10f;
    private float decceleration = 1f;

    private float up_gravity = 4f;
    private float down_gravity = 8f;
    private float max_fall_speed = -15f;
    private bool force_down = false;

    // jumping
    private float normal_jump_power = 20f;
    private float falling_jump_power = 15f;
    private float double_jump_power = 10f;
    private bool has_jumped = false;
    private bool double_jump = false;

    // coyote time
    private float coyote_time = 0.2f;
    private float coyote_time_counter = 0f;

    // jump buffer
    private float jump_buffer_time = 0.2f;
    private float jump_buffer_time_counter = 0f;

    // dashing
    private bool can_dash = true;
    private bool is_dashing = false;
    private float dash_power = 20f;
    private float dash_time = 0.25f;
    private float dash_time_counter;
    private float dash_cooldown = 0.1f;

    // grappling
    private Vector2 mouse_position;
    RaycastHit2D hit;
    private GrappleMode grapple_mode = GrappleMode.SwingShot;
    private bool can_grapple = true;
    private bool is_grappling = false;
    private float grapple_range = 10f;
    private float grapple_length = 2.5f; // ? could make it equal to distance bewteen player and hit
    private float grapple_hold_time = 3.0f;
    private float grapple_hold_time_counter;
    private float grapple_cooldown = 0.1f;
    private float grapple_miss_cooldown = 0f;
    private float grapple_gravity_time = 0.5f;
    private float grapple_gravity_time_counter;
    private Vector2 grapple_hookshot_speed = new Vector2(20f, 20f);

    // ! replace later with values
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform ground_check;
    [SerializeField] private LayerMask ground_layer;


    //! Animator
    Animator animator;

    void Start(){
       animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        switch (Input.GetAxisRaw("Horizontal"))
        {
            case < 0:
                moving = -1;
                if (is_facing_right && !is_grappling)
                {
                    body.velocity = new Vector2(0f, body.velocity.y);
                }
                break;

            case > 0:
                moving = 1;
                if (!is_facing_right && !is_grappling)
                {
                    body.velocity = new Vector2(0f, body.velocity.y);
                }
                break;

            default:
                break;
        }

        Flip();

        Animate();

        // setting Line render to place body
        transform.GetComponent<LineRenderer>().SetPosition(0, transform.position);

        if (!is_dashing || grapple_gravity_time_counter > 0)
        {
            // gravity
            if (body.velocity.y < -1 || (force_down && !IsGrounded()))
            {
                animator.SetTrigger("Fall");
                body.gravityScale = down_gravity;

                if (body.velocity.y < -1)
                {
                    force_down = false;
                }
            }
            else if (body.velocity.y > 1)
            {
                body.gravityScale = up_gravity;
            }
            else
            {
                body.gravityScale = up_gravity / 2;
                if (!IsGrounded()) animator.SetTrigger("Apex");
            }
        }

        // stop other execution while running CO-routine
        if (is_grappling)
        {

            switch (grapple_mode)
            {
                case GrappleMode.HookShot:

                if (Vector2.Distance(hit.point, (Vector2)transform.position) < 1 || grapple_hold_time_counter < grapple_hold_time - 0.33f) // buffer for hookshot
                {
                    grapple_hold_time_counter = -1f;
                }
                transform.GetComponent<SpringJoint2D>().distance = 0f;
                break;

                case GrappleMode.SwingShot:
                if ((grapple_length > 0 && Input.GetAxis("Mouse ScrollWheel") > 0) || (grapple_length < grapple_range && Input.GetAxis("Mouse ScrollWheel") < 0))
                {
                    grapple_length += Input.GetAxis("Mouse ScrollWheel") * -10;
                }
                transform.GetComponent<SpringJoint2D>().distance = grapple_length;

                if (grapple_gravity_time_counter < 0)
                {
                    body.gravityScale = (up_gravity + down_gravity) / 2;
                }
                break;

                default:
                break;
            }

            grapple_gravity_time_counter -= Time.deltaTime;
            grapple_hold_time_counter -= Time.deltaTime;

            return;
        }

        if (Input.GetButton("Fire1") && can_grapple)
        {
            StartCoroutine(Grapple());
        }

        // stop other execution while running CO-routine
        if (is_dashing)
        {
            dash_time_counter -= Time.deltaTime;
            return;
        }

        if (body.velocity.y < max_fall_speed && !Input.GetKey(KeyCode.S))
        {
            body.velocity = new Vector2(body.velocity.x, max_fall_speed);
        }
        else
        {
            body.velocity = new Vector2(body.velocity.x, body.velocity.y);
        }

        // jump reset
        if (IsGrounded() && !Input.GetButton("Jump"))
        {
            has_jumped = double_jump = force_down = false;
        }

        // coyote_time
        if (IsGrounded())
        {
            coyote_time_counter = coyote_time;
            animator.SetBool("Grounded", true);
        }
        else
        {
            animator.SetBool("Grounded", false);
            coyote_time_counter -= Time.deltaTime;
        }

        // jump_buffer
        if (Input.GetButtonDown("Jump"))
        {
            jump_buffer_time_counter = jump_buffer_time;
        }
        else
        {
            jump_buffer_time_counter -= Time.deltaTime;
        }

        // jump logic
        if ((jump_buffer_time_counter > 0f && coyote_time_counter > 0f) || (Input.GetButtonDown("Jump") && double_jump) || (Input.GetButtonDown("Jump") && !has_jumped))
        {
            if (coyote_time_counter > 0 || double_jump)
            {
                body.velocity = new Vector2(body.velocity.x, double_jump ? double_jump_power : normal_jump_power);

                double_jump = !double_jump;
            }
            else
            {
                body.velocity = new Vector2(body.velocity.x, falling_jump_power);
            }

            animator.SetTrigger("Jump");

            has_jumped = true;

            jump_buffer_time_counter = 0f;
        }

        // letting go early
        if (Input.GetButtonUp("Jump") && body.velocity.y > -1f)
        {
            force_down = true;
            coyote_time_counter = 0f;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && can_dash)
        {
            StartCoroutine(Dash());
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (grapple_mode == GrappleMode.HookShot) grapple_mode = GrappleMode.SwingShot;
            else grapple_mode = GrappleMode.HookShot;
        }

    }

    void FixedUpdate()
    {

        if (is_dashing || is_grappling)
        {
            return;
        }

        if (IsGrounded())
        {
          body.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * move_velocity, body.velocity.y);
        }
        else
        {
            if (Input.GetAxisRaw("Horizontal") == 0)
            {
                if ( body.velocity.x > -0.5 && body.velocity.x < 0.5)
                {
                    body.velocity = new Vector2(0, body.velocity.y);
                }
                else
                {
                    body.velocity = new Vector2(body.velocity.x + ((body.velocity.x > 0) ? -1 * decceleration : decceleration), body.velocity.y);
                }
            }
            else
            {
                if (body.velocity.x > 0)
                {
                    if (body.velocity.x > move_velocity)
                    {
                        body.velocity = new Vector2(moving * Math.Abs(body.velocity.x), body.velocity.y);
                    }
                    else
                    {
                        body.velocity = new Vector2(moving * move_velocity, body.velocity.y);
                    }
                }
                else if (body.velocity.x < 0)
                {
                    if (body.velocity.x < -1 * move_velocity)
                    {
                        body.velocity = new Vector2(moving * Math.Abs(body.velocity.x), body.velocity.y);
                    }
                    else
                    {
                    body.velocity = new Vector2(moving * move_velocity, body.velocity.y);
                    }
                }
                else
                {
                    body.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * move_velocity, body.velocity.y);
                }
            }
        }

    }

    private void Flip()
    {
        if ((is_facing_right && moving < 0f) || (!is_facing_right && moving > 0f))
        {
            is_facing_right = !is_facing_right;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void Animate() {

        if (body.velocity.x != 0)
        {
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Walk", false);
        }

    }


    private bool IsGrounded() {
        return Physics2D.OverlapCircle(ground_check.position, 0.2f, ground_layer);
    }

    private IEnumerator Dash()
    {

        animator.SetTrigger("Dash");

        can_dash = false;
        is_dashing = true;
        dash_time_counter = dash_time;
        float original_gravity = body.gravityScale;
        Quaternion original_rotation = transform.rotation;
        body.gravityScale = 0f;
        // ! make var
        // for(int i = 0; i < 10; i++)
        // {
        //     transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y - i * 0.01f, transform.localScale.z);
        // }

        if (Input.GetKey(KeyCode.W))
        {
            body.velocity = new Vector2((moving > 0) ? dash_power : -1 * dash_power, dash_power);
            transform.Rotate(0,0,(moving > 0) ? 45 : -45);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            body.velocity = new Vector2((moving > 0) ? dash_power : -1 * dash_power, -1 * dash_power);
            transform.Rotate(0,0,(moving > 0) ? -45 : 45);
        }
        else
        {
            body.velocity = new Vector2((moving > 0) ? dash_power : -1 * dash_power, 0f);
        }

        yield return new WaitUntil(() => dash_time_counter < 0 || is_grappling);
        
        animator.SetTrigger("Dash_Exit");

        // ! make var
        // for(int i = 0; i < 10; i++)
        // {
        //     transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y + i * 0.01f, transform.localScale.z);
        // }
        transform.rotation = original_rotation;

        if (is_grappling)
        {
            body.velocity = new Vector2(moving * Math.Abs(body.velocity.x), 0f);
        }
        else
        {
            body.velocity = new Vector2(moving * move_velocity, 0f);
        }

        is_dashing = false;
        has_jumped = false;

        yield return new WaitForSeconds(dash_cooldown);

        can_dash = true;
    }

    private IEnumerator Grapple()
    {

        can_grapple = false;

        mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int layerMask = ~LayerMask.GetMask("Player");
        hit = Physics2D.Raycast(transform.position, mouse_position - (Vector2)transform.position, grapple_range, layerMask);

        if (hit.collider != null)
        {

            can_grapple = false;
            is_grappling = true;
            body.gravityScale = up_gravity;

            bool above = transform.position.y > hit.point.y;

            mouse_position = hit.point;

            transform.GetComponent<SpringJoint2D>().enabled = true;
            transform.GetComponent<SpringJoint2D>().connectedAnchor = mouse_position;

            transform.GetComponent<LineRenderer>().enabled = true;
            transform.GetComponent<LineRenderer>().SetPosition(1, mouse_position);

            grapple_hold_time_counter = grapple_hold_time;
            grapple_gravity_time_counter = grapple_gravity_time;

            yield return new WaitUntil(() => Input.GetButton("Fire2") || grapple_hold_time_counter < 0);

            grapple_gravity_time_counter = 0;

            transform.GetComponent<SpringJoint2D>().enabled = false;
            transform.GetComponent<LineRenderer>().enabled = false;

            switch (body.velocity.x)
            {
                case < 0:
                moving = -1;
                break;

                case > 0:
                moving = 1;
                break;

                default:
                moving = 0;
                break;

            }

            if (grapple_mode == GrappleMode.HookShot)
            {
                if (hit.collider.tag.Equals("Hookpoint"))
                {
                    body.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * grapple_hookshot_speed.x, grapple_hookshot_speed.y);
                }
                else
                {
                    body.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * grapple_hookshot_speed.x, 0f);
                }

            }

            is_grappling = false;
            has_jumped = false;

            yield return new WaitForSeconds(grapple_cooldown);

            can_grapple = true;

        } else {

            transform.GetComponent<LineRenderer>().enabled = true;

            Vector2 delta = (mouse_position - (Vector2)transform.position).normalized * (Vector2.Distance((Vector2)transform.position, mouse_position) - grapple_range);

            transform.GetComponent<LineRenderer>().SetPosition(1, mouse_position - delta);

            yield return new WaitForSeconds(0.33f);

            transform.GetComponent<LineRenderer>().enabled = false;

            yield return new WaitForSeconds(grapple_miss_cooldown);

            can_grapple = true;

        }

    }

}
