using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCamera : MonoBehaviour
{
    public float r_limit;
    public float r_speed = 8;
    public float r_a = 3;
    public float r_turn_force = 6;
    public float m_force = 100;
    public float shift_force_add = 1.6f;
    

    float r_x_real, r_y_real;

    KeyCode exit = KeyCode.P;
    Rigidbody mine;

    const float NO_MAX = -1;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        mine = GetComponent<Rigidbody>();
    }

    float lerp(float A, float As, float a, float forceTurn, float max)
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

    // Update is called once per frame
    void Update()
    {
        float r_x = Input.GetAxis("Mouse X");
        float r_y = Input.GetAxis("Mouse Y");
        if (Input.mousePosition.x > Screen.width / 3 && Cursor.lockState == CursorLockMode.Locked)
        {
            r_x_real = lerp(r_x_real, r_x, r_a, r_turn_force, NO_MAX);
            r_y_real = lerp(r_y_real, r_y, r_a, r_turn_force, NO_MAX);
        }
        else
        {
            r_x_real = 0;
            r_y_real = 0;
            r_x = 0;
            r_y = 0;
        }
        transform.Rotate(0, r_x * r_speed, 0, Space.World);
        transform.Rotate(new Vector3(-1, 0, 0) * r_y * r_speed);
        if (transform.up.y < 0)
        {
            transform.Rotate(new Vector3(-1, 0, 0) * 180);
        }
        if (Vector3.Angle(new Vector3(transform.forward.x, 0, transform.forward.z), transform.forward) > r_limit)
        {
            transform.Rotate(new Vector3(-1, 0, 0) * -Mathf.Abs(r_y) * Mathf.Sign(transform.forward.y) * r_speed);
        }

        float _mf = m_force;

        if (Input.mousePosition.x < Screen.width / 3)
        {
           
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _mf *= shift_force_add;
        }
        if (Input.GetKey(KeyCode.W))
        {
            mine.AddForce(transform.forward * Time.deltaTime * mine.mass * _mf);
        }
        if (Input.GetKey(KeyCode.S))
        {
            mine.AddForce(-transform.forward * Time.deltaTime * mine.mass * _mf);
        }
        if (Input.GetKey(KeyCode.A))
        {
            mine.AddForce(-transform.right * Time.deltaTime * mine.mass * _mf);
        }
        if (Input.GetKey(KeyCode.D))
        {
            mine.AddForce(transform.right * Time.deltaTime * mine.mass * _mf);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            mine.AddForce(transform.up * Time.deltaTime * mine.mass * _mf);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            mine.AddForce(-transform.up * Time.deltaTime * mine.mass * _mf);
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
