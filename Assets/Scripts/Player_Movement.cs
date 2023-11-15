using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GrappleMode
{
    HookShot,
    SwingShot,
    KillShot
}

public class Player_Movement : MonoBehaviour
{

    // generic movement
    private Vector2 mouse_position;
    private float horizontal;
    private float speed = 8f;
    private bool is_facing_right = true;

    private float gravity = 4f;
    private float horizontal_decceleration = 0.5f;
    private float max_fall_speed = -25f;

    // jumping
    private float normal_jump_power = 20f;
    private float double_jump_power = 10f;
    private float falling_jump_power = 15f;
    private bool double_jump = false;
    private bool has_jumped = false;

    // coyote time
    private float coyote_time = 0.2f;
    private float coyote_counter = 0f;

    // jump buffer
    private float jump_buffer_time = 0.2f;
    private float jump_buffer_counter = 0f;

    // dashing
    private bool can_dash = true;
    private bool is_dashing = false;
    private float dash_power = 24f;
    private float dash_time = 0.2f;
    private float dash_cooldown = 0.1f;

    // grappling
    RaycastHit2D hit;
    private GrappleMode grapple_mode = GrappleMode.SwingShot;
    private bool can_grapple = true;
    private bool is_grappling = false;
    private float grapple_range = 10f;
    private float grapple_length = 2.5f;
    private float grapple_hold_time = 3.0f;
    private float grapple_release_time;
    private float grapple_cooldown = 0.1f;
    private float grapple_miss_cooldown = 0f;

    // ! replace later
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Transform ground_check;
    [SerializeField] private LayerMask ground_layer;

    // Update is called once per frame
    void Update()
    {

        // facing
        horizontal = Input.GetAxisRaw("Horizontal");
        Flip();

        // setting Line render to place body
        transform.GetComponent<LineRenderer>().SetPosition(0, transform.position);

        // stop other execution while running CO-routine
        if (is_dashing)
        {
            return;
        }

        // stop other execution while running CO-routine
        if (is_grappling)
        {

            switch (grapple_mode)
            {
                case GrappleMode.HookShot:
                if (Vector2.Distance(hit.point, (Vector2)transform.position) < 1)
                {
                    grapple_release_time = -1f;
                }
                transform.GetComponent<SpringJoint2D>().distance = 0f;
                break;

                case GrappleMode.SwingShot:
                if ((grapple_length > 0 && Input.GetAxis("Mouse ScrollWheel") > 0) || (grapple_length < grapple_range && Input.GetAxis("Mouse ScrollWheel") < 0))
                {
                    grapple_length += Input.GetAxis("Mouse ScrollWheel") * -10;
                }
                transform.GetComponent<SpringJoint2D>().distance = grapple_length;
                break;

                default:
                break;
            }

            grapple_release_time -= Time.deltaTime;

            return;
        }

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

        if (Input.GetKeyDown(KeyCode.LeftShift) && can_dash)
        {
            StartCoroutine(Dash());
        }

        if (Input.GetButton("Fire1") && can_grapple)
        {
            StartCoroutine(Grapple());
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (grapple_mode == GrappleMode.HookShot) grapple_mode = GrappleMode.SwingShot;
            else grapple_mode = GrappleMode.HookShot;
        }

    }

    void FixedUpdate()
    {

        Debug.Log(body.velocity.y);


        if (is_dashing || is_grappling)
        {
            return;
        }

        if (Input.GetAxisRaw("Horizontal") == 0 && !IsGrounded())
        {
            if (body.velocity.x > 0)
            {
                 body.velocity = new Vector2(body.velocity.x - horizontal_decceleration, ((body.velocity.y < max_fall_speed) && !Input.GetKey(KeyCode.S)) ? max_fall_speed : body.velocity.y);
            }
            else if (body.velocity.x < 0)
            {
                 body.velocity = new Vector2(body.velocity.x + horizontal_decceleration, ((body.velocity.y < max_fall_speed) && !Input.GetKey(KeyCode.S)) ? max_fall_speed : body.velocity.y);
            }
            else
            {
                body.velocity = new Vector2(0f, ((body.velocity.y < max_fall_speed) && !Input.GetKey(KeyCode.S)) ? max_fall_speed : body.velocity.y);
            }
        }
        else
        {
            body.velocity = new Vector2(horizontal * speed, ((body.velocity.y < max_fall_speed) && !Input.GetKey(KeyCode.S)) ? max_fall_speed : body.velocity.y);
        }



        if (body.velocity.y < 3 && body.velocity.y > 0)
        {
            body.gravityScale = gravity / 2;
        }
        else
        {
            body.gravityScale = gravity;
        }

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

    private IEnumerator Dash()
    {
        can_dash = false;
        is_dashing = true;
        float original_gravity = body.gravityScale;
        Quaternion original_rotation = transform.rotation;
        body.gravityScale = 0f;

        // ! make var
        for(int i = 0; i < 10; i++)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y - i * 0.01f, transform.localScale.z);
        }

        if (Input.GetKey(KeyCode.W))
        {
            body.velocity = new Vector2((transform.localScale.x > 0) ? dash_power : -1 * dash_power, dash_power);
            transform.Rotate(0,0,(transform.localScale.x > 0) ? 45 : -45);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            body.velocity = new Vector2((transform.localScale.x > 0) ? dash_power : -1 * dash_power, -1 * dash_power);
            transform.Rotate(0,0,(transform.localScale.x > 0) ? -45 : 45);
        }
        else
        {
            body.velocity = new Vector2((transform.localScale.x > 0) ? dash_power : -1 * dash_power, 0f);
        }

        yield return new WaitForSeconds(dash_time);

        // ! make var
        for(int i = 0; i < 10; i++)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y + i * 0.01f, transform.localScale.z);
        }
        transform.rotation = original_rotation;
        body.gravityScale = original_gravity;
        body.velocity = new Vector2(0f, 0f);

        is_dashing = false;
        yield return new WaitForSeconds(dash_cooldown);
        can_dash = true;
    }

    private IEnumerator Grapple()
    {

        mouse_position = Input.mousePosition;
        mouse_position = Camera.main.ScreenToWorldPoint(mouse_position);

        int layerMask = ~LayerMask.GetMask("Player");
        hit = Physics2D.Raycast(transform.position, mouse_position - (Vector2)transform.position, grapple_range, layerMask);

        if (hit.collider != null)
        {

            can_grapple = false;
            is_grappling = true;
            body.gravityScale = gravity;

            mouse_position = hit.point;

            transform.GetComponent<SpringJoint2D>().enabled = true;
            transform.GetComponent<SpringJoint2D>().connectedAnchor = mouse_position;

            transform.GetComponent<LineRenderer>().enabled = true;
            transform.GetComponent<LineRenderer>().SetPosition(1, mouse_position);

            grapple_release_time = grapple_hold_time;

            yield return new WaitUntil(() => Input.GetButton("Fire2") || grapple_release_time < 0);

            transform.GetComponent<SpringJoint2D>().enabled = false;
            transform.GetComponent<LineRenderer>().enabled = false;
            is_grappling = false;
            has_jumped = false; // ! testing (like it!!!)

            yield return new WaitForSeconds(grapple_cooldown);

            can_grapple = true;

        } else {

            can_grapple = false;

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
