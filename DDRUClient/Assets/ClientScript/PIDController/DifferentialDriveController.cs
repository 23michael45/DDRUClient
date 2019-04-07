using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PIDController;

public class DifferentialDriveController : MonoBehaviour
{

    public WheelCollider mLeftWheel;
    public WheelCollider mRightWheel;

    public bool mannualControl = false;
    public float leftmotor = 0;
    public float rightmotor = 0;

    float leftTargetRPM = 0;
    float rightTargetRPM = 0;


    public float maxMotorTorque;

    public PIDController.PIDController leftPID;
    public PIDController.PIDController rightPID;

    private void Start()
    {
        leftPID = mLeftWheel.GetComponent<PIDController.PIDController>();
        rightPID = mRightWheel.GetComponent<PIDController.PIDController>();
    }

    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    public void FixedUpdate()
    {
        if (mannualControl)
        {
            leftmotor = maxMotorTorque * Input.GetAxis("LeftMotor");
            rightmotor = maxMotorTorque * Input.GetAxis("RightMotor");

        }
        else
        {


            float curLeftRPM = mLeftWheel.rpm;
            if (System.Single.IsNaN(curLeftRPM))
            {
                curLeftRPM = 0;
            }
            leftmotor = leftPID.GetSteerFactorFromPIDController(leftTargetRPM - curLeftRPM);

            float curRightRPM = mRightWheel.rpm;
            if (System.Single.IsNaN(curRightRPM))
            {
                curRightRPM = 0;
            }
            rightmotor = rightPID.GetSteerFactorFromPIDController(rightTargetRPM - curRightRPM);

        }


        mLeftWheel.motorTorque = leftmotor;
        mRightWheel.motorTorque = rightmotor;

        Debug.Log(string.Format("{0} {1} {2} {3} {4} {5}", mLeftWheel.motorTorque, mRightWheel.motorTorque, mLeftWheel.rpm, mRightWheel.rpm,leftmotor,rightmotor));

        ApplyLocalPositionToVisuals(mLeftWheel);
        ApplyLocalPositionToVisuals(mRightWheel);

    }

    public void SetMotorTorque(float left,float right)
    {

        leftTargetRPM = left;
        rightTargetRPM = right;

    }
}
