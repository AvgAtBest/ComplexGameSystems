using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    //Distance the camera is from world zero
    public float distance = 10f;
    //x and y rotation speed
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    //x and y rotation limits
    public float yMin = 15f;
    public float yMax = 85f;
    //Current x and y rotation
    private float x = 0.0f;
    private float y = 0.0f;

    private void Start()
    {
        //Get current rotation of camera
        Vector3 euler = transform.eulerAngles;
        x = euler.y;
        y = euler.x;
    }
    private void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            //offset rotation with mouse x and y offsets
            x += mouseX * xSpeed * Time.deltaTime;
            y -= mouseY * ySpeed * Time.deltaTime;
            y = Mathf.Clamp(y, yMin, yMax);


        }
        else
        {
            Cursor.visible = true;
        }
        //rotates the board
        transform.rotation = Quaternion.Euler(y, x, 0);
        transform.position = -transform.forward * distance;
    }
}
