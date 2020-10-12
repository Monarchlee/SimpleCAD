using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    public float rotateSpeed = 3;
    public float moveSpeed = 2;
    public float scrollSpeed = 3;
    public float dragSpeed = 10;

    void FixedUpdate()
    {
        Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical") + Input.GetAxis("Mouse ScrollWheel") * scrollSpeed) * moveSpeed;

        if(Input.GetMouseButton(2))
        {
            Vector2 drag = mouse * dragSpeed;
            movement -= (Vector3.up * drag.y + Vector3.right * drag.x);
        }

        if(Input.GetMouseButton(1))
        {
            Vector2 rotate = mouse * rotateSpeed;
            transform.Rotate(Vector3.up, rotate.x, Space.World);
            transform.Rotate(transform.right, -rotate.y, Space.World);
        }

        transform.Translate(movement, Space.Self);
    }
}
