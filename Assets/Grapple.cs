using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GrappleMode
{
    HookShot,
    SwingShot,
    KillShot
}

public class Grapple : MonoBehaviour
{

    Vector2 mousePos;

    private float grapple_range;

    private GrappleMode grapple_mode;

    // Start is called before the first frame update
    void Start()
    {
        grapple_range = 20f;
        grapple_mode = GrappleMode.SwingShot;
    }

    // Update is called once per frame
    void Update()
    {

        transform.GetComponent<LineRenderer>().SetPosition(0, transform.position);

        switch (grapple_mode)
        {
            case(GrappleMode.HookShot):
                HookShot();
            break;

            case(GrappleMode.SwingShot):
                SwingShot();
            break;

            default:
            break;
        }

    }


    private void HookShot()
    {

        transform.GetComponent<SpringJoint2D>().distance = 0.1f;

        if (Input.GetButton("Fire1"))
        {
            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            Debug.Log(Math.Min(grapple_range, Vector2.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position)));

            int layerMask = ~LayerMask.GetMask("Player");
            RaycastHit2D hit = Physics2D.Raycast(transform.position, mousePos - (Vector2)transform.position, grapple_range, layerMask);

            if (hit.collider != null)
            {
                mousePos = hit.point;

                transform.GetComponent<SpringJoint2D>().enabled = true;
                transform.GetComponent<SpringJoint2D>().connectedAnchor = mousePos;

                transform.GetComponent<LineRenderer>().enabled = true;
                transform.GetComponent<LineRenderer>().SetPosition(1, mousePos);

                transform.GetComponent<Player_Movement>().enabled = false;
            }

        }
        else if (Input.GetButton("Fire2"))
        {
            transform.GetComponent<SpringJoint2D>().enabled = false;
            transform.GetComponent<LineRenderer>().enabled = false;

            transform.GetComponent<Player_Movement>().enabled = true;
        }
    }

    private void SwingShot()
    {

        if ((transform.GetComponent<SpringJoint2D>().distance >= 0.1 && Input.GetAxis("Mouse ScrollWheel") < 0) || (transform.GetComponent<SpringJoint2D>().distance <= grapple_range && Input.GetAxis("Mouse ScrollWheel") > 0))
        {
            transform.GetComponent<SpringJoint2D>().distance += Input.GetAxis("Mouse ScrollWheel") * 10;
        }

        if (Input.GetButton("Fire1"))
        {
            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            int layerMask = ~LayerMask.GetMask("Player");
            RaycastHit2D hit = Physics2D.Raycast(transform.position, mousePos - (Vector2)transform.position, grapple_range, layerMask);

            if (hit.collider != null)
            {
                mousePos = hit.point;

                transform.GetComponent<SpringJoint2D>().enabled = true;
                transform.GetComponent<SpringJoint2D>().connectedAnchor = mousePos;

                transform.GetComponent<LineRenderer>().enabled = true;
                transform.GetComponent<LineRenderer>().SetPosition(1, mousePos);

                transform.GetComponent<Player_Movement>().enabled = false;
            }

        }
        else if (Input.GetButton("Fire2"))
        {
            transform.GetComponent<SpringJoint2D>().enabled = false;
            transform.GetComponent<LineRenderer>().enabled = false;

            transform.GetComponent<Player_Movement>().enabled = true;
        }

    }

}

