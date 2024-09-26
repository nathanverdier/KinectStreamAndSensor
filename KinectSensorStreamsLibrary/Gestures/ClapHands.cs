using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectSensorStreamsLibrary.Gestures
{

    // Represents a specific gesture: ClapHands, inheriting from the Gesture class.
    public class ClapHands : Gesture
    {
        public ClapHands() : base()
        {
            GestureName = "ClapHands";
        }
        // Variables to store initial positions of hands and hip.
        private bool startPosture = false;
        private bool oldPosture = false;
        CameraSpacePoint posStartHandRight;
        CameraSpacePoint posStartHandLeft;
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

            /*posStartHipRight = body.Joints[JointType.HipRight].Position;
            CameraSpacePoint posStartHipLeft = body.Joints[JointType.HipLeft].Position;

            // Define conditions for initial posture: hands open, hands above hip, hands width within specified range.
            bool isRightHandOpen = body.HandRightState == HandState.Open;
            bool isRightHandAboveHip = posStartHandRight.Y > posStartHipRight.Y && posStartHandRight.Y < body.Joints[JointType.Head].Position.Y;
            bool isRightHandWidth = posStartHandRight.X > body.Joints[JointType.HipRight].Position.X || posStartHandRight.X > body.Joints[JointType.ElbowRight].Position.X;

            bool isLeftHandOpen = body.HandRightState == HandState.Open;
            bool isLeftHandAboveHip = posStartHandLeft.Y > posStartHipLeft.Y && posStartHandLeft.Y < body.Joints[JointType.Head].Position.Y;
            bool isLeftHandWidth = posStartHandLeft.X > body.Joints[JointType.HipLeft].Position.X || posStartHandLeft.X < body.Joints[JointType.ElbowLeft].Position.X;*/

            // Return true if all initial conditions are met.
            /*return isRightHandOpen && isRightHandAboveHip && isLeftHandOpen && isLeftHandAboveHip && isRightHandWidth && isLeftHandWidth;*/


            bool isRightAndLeftSameHeight = (Math.Abs(posStartHandLeft.Y - posStartHandRight.Y) <= 10);
            bool isRightAndLeftSpaced = (Math.Abs(posStartHandLeft.X - posStartHandRight.X) >= 100);
            Debug.WriteLine("Left:" + posStartHandLeft.ToString());
            Debug.WriteLine("Right:" + posStartHandRight.ToString());
            return isRightAndLeftSameHeight && isRightAndLeftSpaced;
        }

        // Overrides the TestPosture method from the base class to implement ClapHands-specific posture conditions.
        protected override bool TestPosture(Body body)
        {
            Debug.WriteLine("Right :" + body.Joints[JointType.HandRight].Position.ToString());
            Debug.WriteLine("Left :" + body.Joints[JointType.HandLeft].Position.ToString());
            if (!startPosture) 
            { 
                startPosture = TestInitialConditions(body);
            }
            if (startPosture) 
            {
                Debug.WriteLine("ClapHands : startposture is valid");
                oldPosture = TestRunningGesture(body); 
            }
            if (!oldPosture) 
            { 
                startPosture = false; 
            }
            if (startPosture && oldPosture) 
            { 
                return TestEndConditions(body); 
            }
            return false;
        }

        // Overrides the TestRunningGesture method from the base class to implement ClapHands-specific running gesture conditions.
        protected override bool TestRunningGesture(Body body)
        {
            // Return true if oldPosture is true and TestPosture conditions are met.
            // Check if hands are open, above hip, and within a specified width range.
            bool isRightAndLeftSameHeight = (Math.Abs(body.Joints[JointType.HandLeft].Position.Y - body.Joints[JointType.HandRight].Position.Y) <= 10);

            // Return false if posture conditions are not met.
            return isRightAndLeftSameHeight;
        }

        // Overrides the TestEndConditions method from the base class to implement ClapHands-specific end conditions.
        protected override bool TestEndConditions(Body body)
        {
            // End conditions: return false if running gesture conditions are met.
            bool isRightAndLeftClose = (Math.Abs(body.Joints[JointType.HandLeft].Position.X - body.Joints[JointType.HandRight].Position.X) <= 10);

            // Return true if end conditions are met.
            return isRightAndLeftClose;
        }
    }
}
