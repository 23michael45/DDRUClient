using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PIDController
{
    [System.Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool motor;
        public bool steering;
    }

    public class CarController : MonoBehaviour
    {
        public List<AxleInfo> axleInfos;

        //Car data
        public float maxMotorTorque = 50;
        public float maxSteeringAngle = 40f;

        //To get a more realistic behavior
        public Vector3 centerOfMassChange;

        //The difference between the center of the car and the position where we steer
        public float centerSteerDifference;
        //The position where the car is steering
        private Vector3 steerPosition;

        //All waypoints
        public Transform parentWaypoint;
        List<Transform> allWaypoints;
        //The current index of the list with all waypoints
        private int currentWaypointIndex = 0;
        //The waypoint we are going towards and the waypoint we are going from
        private Vector3 currentWaypoint;
        private Vector3 previousWaypoint;

        //Average the steering angles to simulate the time it takes to turn the wheel
        float averageSteeringAngle = 0f;

        PIDController m_PIDController;

        public Transform m_TargetSignal;

        void Start()
        {
            allWaypoints = new List<Transform>();
            foreach (var t in parentWaypoint.GetComponentsInChildren<Transform>())
            {
                if(t == parentWaypoint)
                {
                    continue;
                }
                allWaypoints.Add(t);
            }
            //Move the center of mass
            var rigid = transform.GetComponent<Rigidbody>();
            rigid.centerOfMass = rigid.centerOfMass + centerOfMassChange;

            //Init the waypoints
            currentWaypoint = allWaypoints[currentWaypointIndex].position;

            previousWaypoint = GetPreviousWaypoint();

            m_PIDController = GetComponent<PIDController>();



        }

        //Finds the corresponding visual wheel, correctly applies the transform
        void ApplyLocalPositionToVisuals(WheelCollider collider)
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

            ShowCurrentWaypoint();
        }

        void Update()
        {
            //So we can experiment with the position where the car is checking if it should steer left/right
            //doesn't have to be where the wheels are - especially if we are reversing
            steerPosition = transform.position + transform.forward * centerSteerDifference;

            //Check if we should change waypoint
            if (Math.HasPassedWaypoint(steerPosition, previousWaypoint, currentWaypoint))
            {
                currentWaypointIndex += 1;

                if (currentWaypointIndex == allWaypoints.Count)
                {
                    currentWaypointIndex = 0;
                }

                currentWaypoint = allWaypoints[currentWaypointIndex].position;

                previousWaypoint = GetPreviousWaypoint();

                ShowCurrentWaypoint();
            }
        }

        //Get the waypoint before the current waypoint we are driving towards
        Vector3 GetPreviousWaypoint()
        {
            previousWaypoint = Vector3.zero;

            if (currentWaypointIndex - 1 < 0)
            {
                previousWaypoint = allWaypoints[allWaypoints.Count - 1].position;
            }
            else
            {
                previousWaypoint = allWaypoints[currentWaypointIndex - 1].position;
            }

            return previousWaypoint;
        }

        void FixedUpdate()
        {
            float motor = maxMotorTorque;

            //Manual controls for debugging
            //float motor = maxMotorTorque * Input.GetAxis("Vertical");
            //float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

            //
            //Calculate the steering angle
            //
            //The simple but less accurate way -> will produce drunk behavior
            //float steeringAngle = maxSteeringAngle * Math.SteerDirection(transform, steerPosition, currentWaypoint);

            //Get the cross track error, which is what we want to minimize with the pid controller
            float CTE = Math.GetCrossTrackError(steerPosition, previousWaypoint, currentWaypoint);

            //But we still need a direction to steer
            CTE *= Math.SteerDirection(transform, steerPosition, currentWaypoint);

            float steeringAngle = m_PIDController.GetSteerFactorFromPIDController(CTE);

            //Limit the steering angle
            steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle);

            //Average the steering angles to simulate the time it takes to turn the steering wheel
            float averageAmount = 30f;

            averageSteeringAngle = averageSteeringAngle + ((steeringAngle - averageSteeringAngle) / averageAmount);


            //
            //Apply everything to the car 
            //
            foreach (AxleInfo axleInfo in axleInfos)
            {
                if (axleInfo.steering)
                {
                    axleInfo.leftWheel.steerAngle = averageSteeringAngle;
                    axleInfo.rightWheel.steerAngle = averageSteeringAngle;
                }
                if (axleInfo.motor)
                {
                    axleInfo.leftWheel.motorTorque = motor;
                    axleInfo.rightWheel.motorTorque = motor;
                }

                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }
        }


        void ShowCurrentWaypoint()
        {
            m_TargetSignal.position = allWaypoints[currentWaypointIndex].position;
        }
    }
}