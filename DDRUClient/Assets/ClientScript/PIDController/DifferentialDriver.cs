using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifferentialDriver : MonoBehaviour
{
    public float rRPM;
    public float lRPM;

    public float rightRadius;
    public float leftRadius;

    public Transform mLeftWheel;


    public float WheelBase;

    float Vr;
    float Vl;

    float V;
    float W;
    float R;

    Rigidbody mRigidbody;


    float x = 0;
    float y = 0;
    float theta = 0;

    // Start is called before the first frame update
    void Start()
    {
        mRigidbody = GetComponent<Rigidbody>();


        x = transform.position.x;
        y = transform.position.z;



        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z);
        theta = Vector3.Angle(Vector3.right, forward) * Mathf.Deg2Rad;

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void Move(float x,float y)
    {
        Vector3 movement = new Vector3(x, transform.position.y, y);//new Vector3((-y)+26, 0f, (x)-20);
        mRigidbody.MovePosition(movement);
    }


    private void Turn(float theta)
    {
        float theta_deg = -Mathf.Rad2Deg * theta + 90f;
        Quaternion rotate = Quaternion.Euler(0f, theta_deg, 0f);
        mRigidbody.MoveRotation(rotate);

    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;


        float _vr = (rRPM / 60) * 2 * Mathf.PI * rightRadius;
        float _vl = (lRPM / 60) * 2 * Mathf.PI * leftRadius;


        V = (_vr + _vl) / 2f;
        W = (_vr - _vl) / WheelBase;




        float _theta_change = W * deltaTime;
        if (_vl == _vr)
        {
            x = x + _vr * deltaTime * Mathf.Cos(theta);
            y = y + _vr * deltaTime * Mathf.Sin(theta);
            theta = theta + _theta_change;
        }
        else
        {
            //float R = (B / 2f) * ((_vr + _vl) / (_vr - _vl));
            x = x - (V / W) * Mathf.Sin(theta) + (V / W) * Mathf.Sin(theta + _theta_change);
            y = y + (V / W) * Mathf.Cos(theta) - (V / W) * Mathf.Cos(theta + _theta_change);
            theta = theta + _theta_change;
        }

        Move(x, y);
        Turn(theta);

    }
}
