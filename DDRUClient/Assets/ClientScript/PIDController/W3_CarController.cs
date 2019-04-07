using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WheelTutorial
{


    [System.Serializable]
    public class WheelInfo
    {
        public WheelCollider Wheel;
        public bool motor;
    }

    public class W3_CarController : MonoBehaviour
    {
        public WheelInfo mFrontWheel;
        public WheelInfo mLeftWheel;
        public WheelInfo mRightWheel;

        public bool mannualControl = false;
        public float leftmotor = 0;
        public float rightmotor = 0;


        public float maxMotorTorque;

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
            if(mannualControl)
            {
                leftmotor = maxMotorTorque * Input.GetAxis("LeftMotor");
                rightmotor = maxMotorTorque * Input.GetAxis("RightMotor");

            }



            mLeftWheel.Wheel.motorTorque = leftmotor;
            mRightWheel.Wheel.motorTorque = rightmotor;

            Debug.Log(string.Format("L {0} R {1}", leftmotor, rightmotor));

            ApplyLocalPositionToVisuals(mLeftWheel.Wheel);
            ApplyLocalPositionToVisuals(mRightWheel.Wheel);
            //ApplyLocalPositionToVisuals(mFrontWheel.Wheel);
        }
    }
}