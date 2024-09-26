using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectSensorStreamsLibrary.Gestures
{


    public class HolaGesture : Gesture
    {
        public HolaGesture() : base()
        {
            GestureName = "HolaGesture";
        }

        // Variables to store initial positions of hands and hip.
        private bool oldPosture;
        CameraSpacePoint posStartHandRight;
        CameraSpacePoint posStartHandLeft;
        CameraSpacePoint posHead;
        CameraSpacePoint posStartHipRight;

        // Overrides the TestInitialConditions method from the base class to implement ClapHands-specific initial conditions.
        protected override bool TestInitialConditions(Body body)
        {
            // Check if the body is null.
            if (body == null)
                return false;

            // Retrieve initial positions of hands and hip.
            posStartHandRight = body.Joints[JointType.HandRight].Position;
            posStartHandLeft = body.Joints[JointType.HandLeft].Position;
            posHead = body.Joints[JointType.Head].Position;

            posStartHipRight = body.Joints[JointType.HipRight].Position;
            CameraSpacePoint posStartHipLeft = body.Joints[JointType.HipLeft].Position;

            // Define conditions for initial posture: hands open, hands above hip, hands width within specified range.
            bool isRightHandOpen = body.HandRightState == HandState.Open;
            bool isRightHandAboveHip = posStartHandRight.Y < posStartHipRight.Y && posStartHandRight.Y < posHead.Y;

            bool isLeftHandOpen = body.HandRightState == HandState.Open;
            bool isLeftHandAboveHip = posStartHandLeft.Y < posStartHipLeft.Y && posStartHandLeft.Y < posHead.Y;

            // Return true if all initial conditions are met.
            return isRightHandOpen && isRightHandAboveHip && isLeftHandOpen && isLeftHandAboveHip;
        }

        // Overrides the TestPosture method from the base class to implement ClapHands-specific posture conditions.
        protected override bool TestPosture(Body body)
        {
            // Check if hands are open, above hip, and within a specified width range.
            if (body.HandRightState == HandState.Open && body.HandLeftState == HandState.Open && body.Joints[JointType.HandRight].Position.Y > posStartHipRight.Y && body.Joints[JointType.HandLeft].Position.Y > posStartHipRight.Y)
            {
                // Update oldPosture and return true.
                oldPosture = true;
                return true;
            }

            // Return false if posture conditions are not met.
            return false;
        }

        // Overrides the TestRunningGesture method from the base class to implement ClapHands-specific running gesture conditions.
        protected override bool TestRunningGesture(Body body)
        {
            // Return true if oldPosture is true and TestPosture conditions are met.
            return oldPosture && TestPosture(body);
        }

        // Overrides the TestEndConditions method from the base class to implement ClapHands-specific end conditions.
        protected override bool TestEndConditions(Body body)
        {
            // End conditions: return false if running gesture conditions are met.
            if (TestRunningGesture(body))
                return false;

            // Return true if end conditions are met.
            return true;
        }
    }
}
