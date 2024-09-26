using Microsoft.Kinect;
using System;

namespace KinectSensorStreamsLibrary.Postures
{
    // Class representing a posture where both hands are above the head
    public class TwoHandsUp : Posture
    {
        public TwoHandsUp():base()
        {
            GestureName = "TwoHandsUp";
        }

        // Override the TestPosture method from the base class to define the specific posture test logic
        protected override bool TestPosture(Body body)
        {

            // Check if the right hand is open
            bool rightHandOpen = body.HandRightState == HandState.Open;

            // Check if the left hand is open
            bool leftHandOpen = body.HandLeftState == HandState.Open;

            // Get the 3D position of the right hand and head joints
            CameraSpacePoint posHandRight = body.Joints[JointType.HandRight].Position;
            CameraSpacePoint posHandLeft = body.Joints[JointType.HandLeft].Position;
            CameraSpacePoint posHead = body.Joints[JointType.Head].Position;

            // Return true if both hands are open and if posHandLeft and posHandRight > posHead
            return rightHandOpen && leftHandOpen && posHandLeft.Y>posHead.Y && posHandRight.Y>posHead.Y;
        }
    }
}
