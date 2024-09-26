using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectSensorStreamsLibrary.Gestures
{
    // Represents a specific gesture: SwipeRightHand, inheriting from the Gesture class.
    public class SwipeRightHand : Gesture
    {
        public SwipeRightHand() : base()
        {
            GestureName = "SwipeRightHand";
        }
        // Variables to store initial positions of the right hand, right hip, and end position of the right hand.
        private bool oldPosture;
        CameraSpacePoint posStartHandRight;
        CameraSpacePoint posStartHipRight;
        CameraSpacePoint posEndHandRight;

        // Overrides the TestInitialConditions method from the base class to implement SwipeRightHand-specific initial conditions.
        protected override bool TestInitialConditions(Body body)
        {
            // Check if the body is null.
            if (body == null)
                return false;

            // Retrieve initial positions of the right hand, right hip, and end position of the right hand.
            posStartHandRight = body.Joints[JointType.HandRight].Position;
            posStartHipRight = body.Joints[JointType.HipRight].Position;
            posEndHandRight = body.Joints[JointType.Head].Position;

            // Return true if the right hand is open and below the right hip.
            return body.HandRightState == HandState.Open && posStartHandRight.Y < posStartHipRight.Y;
        }

        // Overrides the TestPosture method from the base class to implement SwipeRightHand-specific posture conditions.
        protected override bool TestPosture(Body body)
        {
            // Check if the right hand is open, above the right hip, and below the end position of the right hand.
            if (body.HandRightState == HandState.Open && body.Joints[JointType.HandRight].Position.Y > posStartHipRight.Y && body.Joints[JointType.Head].Position.Y < posEndHandRight.Y)
            {
                // Update oldPosture and return true.
                oldPosture = true;
                return true;
            }

            // Return false if posture conditions are not met.
            return false;
        }

        // Overrides the TestRunningGesture method from the base class to implement SwipeRightHand-specific running gesture conditions.
        protected override bool TestRunningGesture(Body body)
        {
            // Return true if oldPosture is true and TestPosture conditions are met.
            return oldPosture && TestPosture(body);
        }

        // Overrides the TestEndConditions method from the base class to implement SwipeRightHand-specific end conditions.
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
