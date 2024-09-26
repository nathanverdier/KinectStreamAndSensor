using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectSensorStreamsLibrary
{
    // Abstract class representing a posture, which is a type of gesture, inheriting from BaseGesture.
    public abstract class Posture : BaseGesture
    {
        // Overrides the TestGesture method from the base class to implement posture-specific testing logic.
        public override bool TestGesture(Body body)
        {
            // Test whether the posture is recognized for the given body.
            bool isPostureRecognized = TestPosture(body);

            // If the posture is recognized, trigger the OnGestureRecognized event.
            if (isPostureRecognized)
            {
                OnGestureRecognized(body);
            }

            // Return whether the posture is recognized.
            return isPostureRecognized;
        }

        // Abstract method to be implemented by derived classes to test whether a specific posture is recognized for the given body.
        protected abstract bool TestPosture(Body body);
    }
}
