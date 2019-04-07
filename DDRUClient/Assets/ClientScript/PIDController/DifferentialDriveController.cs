using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PIDController;

public class DifferentialDriveController : MonoBehaviour
{

    public WheelCollider mFrontWheel;
    public WheelCollider mLeftWheel;
    public WheelCollider mRightWheel;

    public bool mannualControl = false;
    public float mLeftMoterTarget = 0;
    public float mRightMotorTarget = 0;
    

    float leftTargetRPM = 0;
    float rightTargetRPM = 0;


    public float maxMotorTorque;

    public PIDController.PIDController leftPID;
    public PIDController.PIDController rightPID;

    Rigidbody mRigid;

    private void Start()
    {
        leftPID = mLeftWheel.GetComponent<PIDController.PIDController>();
        rightPID = mRightWheel.GetComponent<PIDController.PIDController>();
        mRigid = gameObject.GetComponent<Rigidbody>();
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
        float leftTorque = 0;
        float rightTorque = 0;

        if (mannualControl)
        {
            mLeftMoterTarget = maxMotorTorque * Input.GetAxis("LeftMotor");
            mRightMotorTarget = maxMotorTorque * Input.GetAxis("RightMotor");

        }


        float curLeftRPM = mLeftWheel.rpm;
        leftTargetRPM = mLeftMoterTarget;
        if (System.Single.IsNaN(curLeftRPM))
        {
            curLeftRPM = 0;
        }
        if (System.Single.IsInfinity(curLeftRPM))
        {
            curLeftRPM = maxMotorTorque;
        }
        leftTorque = leftPID.GetSteerFactorFromPIDController(leftTargetRPM - curLeftRPM);

        float curRightRPM = mRightWheel.rpm;
        rightTargetRPM = mRightMotorTarget;
        if (System.Single.IsNaN(curRightRPM))
        {
            curRightRPM = 0;
        }
        if (System.Single.IsInfinity(curRightRPM))
        {
            curRightRPM = maxMotorTorque;
        }
        rightTorque = rightPID.GetSteerFactorFromPIDController(rightTargetRPM - curRightRPM);




        if (Mathf.Abs(leftTorque) > 0 || Mathf.Abs(rightTorque) > 0)
        {
            mLeftWheel.motorTorque = leftTorque;
            mLeftWheel.brakeTorque = 0;

            mRightWheel.motorTorque = rightTorque;
            mRightWheel.brakeTorque = 0;
        }
        else
        {
            mLeftWheel.motorTorque = 0;
            mLeftWheel.brakeTorque = Mathf.Infinity;

            mRightWheel.motorTorque = 0;
            mRightWheel.brakeTorque = Mathf.Infinity;
        }


        if (leftTorque == 0 && rightTorque == 0)
        {
            mFrontWheel.brakeTorque = Mathf.Infinity;

            mRigid.velocity = Vector3.zero;
            mRigid.angularVelocity = Vector3.zero;
        }
        else
        {
            mFrontWheel.brakeTorque = 0;

        }




        //Debug.Log(string.Format("{0} {1} {2} {3} {4} {5} {6}", mLeftWheel.motorTorque, mRightWheel.motorTorque, mLeftWheel.rpm, mRightWheel.rpm, mFrontWheel.brakeTorque, mLeftMoterTarget, mRightMotorTarget));

        Debug.Log(string.Format("leftTorque:{0} rightTorque:{1} leftTargetRPM:{2} rightTargetRPM:{3} left rpm:{4} right rpm{5}", leftTorque, rightTorque, leftTargetRPM, rightTargetRPM, mLeftWheel.rpm, mRightWheel.rpm));

        ApplyLocalPositionToVisuals(mLeftWheel);
        ApplyLocalPositionToVisuals(mRightWheel);
        ApplyLocalPositionToVisuals(mFrontWheel);

    }

    public void SetMotorTorque(float left,float right)
    {

        leftTargetRPM = left;
        rightTargetRPM = right;

    }
}
