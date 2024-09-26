using Microsoft.Kinect;
using System;

namespace KinectSensorStreamsLibrary.Postures
{
    // Class representing a posture where the right hand is raised above the head and open
    public class RightHandUp : Posture
    {
        public RightHandUp() : base()
        {
            GestureName = "RightHandUp";
        }
        // Override the TestPosture method from the base class to define the specific posture test logic
        protected override bool TestPosture(Body body)
        {
            // Get the 3D position of the right hand and head joints
            CameraSpacePoint posHandRight = body.Joints[JointType.HandRight].Position;
            CameraSpacePoint posHead = body.Joints[JointType.Head].Position;

            // Check if the right hand is open
            bool isRightHandOpen = body.HandRightState == HandState.Open;

            // Check if the right hand is above the head
            bool isHandAboveHead = posHandRight.Y > posHead.Y;

            // Return true if both conditions are met (right hand open and above the head)
            return isRightHandOpen && isHandAboveHead;
        }
    }
}
