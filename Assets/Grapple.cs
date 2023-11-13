using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{

    Vector2 mousePos;

    private float grapple_range;

    // Start is called before the first frame update
    void Start()
    {
        grapple_range = 20f;
    }

    // Update is called once per frame
    void Update()
    {

        transform.GetComponent<LineRenderer>().SetPosition(0, transform.position);

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
            }

        }
        else if (Input.GetButton("Fire2"))
        {
            transform.GetComponent<SpringJoint2D>().enabled = false;
            transform.GetComponent<LineRenderer>().enabled = false;
        }

    }
}

