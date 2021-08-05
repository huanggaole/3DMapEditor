using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class look : MonoBehaviour
{
    public float r_limit;
    public float r_speed = 8;
    public float r_a = 3;
    public float r_turn_force = 6;
    public float change_x = 0, change_y = 0;
    public float mouse_move_limit_dof = 0.1f;
    Rigidbody mine;
    KeyCode exit = KeyCode.P;
    float r_x_real, r_y_real;

    public const float NO_MAX = -1;
    public static float lerp(float A, float As, float a, float forceTurn, float max)
    {
        if (Mathf.Abs(A - As) < a * Time.deltaTime)
        {
            return As;
        }
        if (A * As < 0)
        {
            A += (A < As ? a * Time.deltaTime : -a * Time.deltaTime) * forceTurn;
        }
        A += (A < As ? a * Time.deltaTime : -a * Time.deltaTime);
        if (max > 0)
        {
            if (A > max) A = max;
            if (A < -max) A = -max;
        }
        return A;
    }
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        float r_x = Input.GetAxis("Mouse X");
        float r_y = Input.GetAxis("Mouse Y");
        r_x_real += change_x;
        r_y_real += change_y;
        change_x = 0;
        change_y = 0;
        if (Input.mousePosition.x > Screen.width / 3 && Cursor.lockState == CursorLockMode.Locked)
        {
            r_x_real = lerp(r_x_real, r_x, r_a, r_turn_force, NO_MAX);
            r_y_real = lerp(r_y_real, r_y, r_a, r_turn_force, NO_MAX);
        }
        else
        {
            r_x_real = 0;
            r_y_real = 0;
        }
        transform.Rotate(0, r_x_real * Time.deltaTime * r_speed, 0, Space.World);
        transform.Rotate(new Vector3(-1, 0, 0) * r_y_real * Time.deltaTime * r_speed);
        if (Mathf.Abs(r_x_real) > mouse_move_limit_dof || Mathf.Abs(r_y_real) > mouse_move_limit_dof)
        {
            if (Camera.allCameras[0].GetComponent<CameraEffect>())
            {
                Camera.allCameras[0].GetComponent<CameraEffect>()._dof = 0;
            }
        }
        if (transform.up.y < 0)
        {
            transform.Rotate(new Vector3(-1, 0, 0) * 180);
        }
        if (Vector3.Angle(new Vector3(transform.forward.x, 0, transform.forward.z), transform.forward) > r_limit)
        {
            transform.Rotate(new Vector3(-1, 0, 0) * -Mathf.Abs(r_y_real) * 
                Mathf.Sign(transform.forward.y) * Time.deltaTime * r_speed);
        }
        if (Input.GetKeyDown(exit))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
