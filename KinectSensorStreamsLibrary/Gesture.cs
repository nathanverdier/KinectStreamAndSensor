using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectSensorStreamsLibrary
{
    // Represents a specific gesture that inherits from the BaseGesture class.
    public class Gesture : BaseGesture
    {
        // Indicates whether the gesture recognition is currently in the testing phase.
        public bool IsTesting;

        // Minimum number of frames required for the gesture to be recognized.
        protected int MinNbOfFrames;

        // Maximum number of frames allowed for the gesture testing phase.
        protected int MaxNbOfFrames;

        // Counter for the current frame during gesture recognition.
        private int mCurrentFrameCount;

        // Flag indicating whether gesture recognition is currently running.
        bool IsRecognitionRunning;

        // Checks the initial conditions required for gesture testing.
        protected virtual bool TestInitialConditions(Body body)
        {
            // If the body is null, the initial conditions are not met.
            if (body == null)
                return false;
            return true;
        }

        // Checks the posture conditions required for gesture testing.
        protected virtual bool TestPosture(Body body)
        {
            return false;
        }

        // Checks the running gesture conditions during the testing phase.
        protected virtual bool TestRunningGesture(Body body)
        {
            return false;
        }

        // Checks the end conditions for gesture testing.
        protected virtual bool TestEndConditions(Body body)
        {
            // If the running gesture condition is met, end conditions are not met.
            if (TestRunningGesture(body))
                return false;
            return true;
        }

        // Overrides the TestGesture method from the base class to implement gesture-specific testing logic.
        public override bool TestGesture(Body body)
        {
            if (IsRecognitionRunning)
            {
                // If initial conditions are met, reset the frame count to 1.
                if (TestInitialConditions(body))
                {
                    mCurrentFrameCount = 1;
                }
                // If already in the testing phase, continue counting frames.
                else if (mCurrentFrameCount > 0)
                {
                    mCurrentFrameCount++;

                    // If the required minimum frames are reached and posture conditions are met, perform additional actions.
                    if (mCurrentFrameCount >= MinNbOfFrames && TestPosture(body))
                    {
                        // Additional actions for recognizing the gesture.
                    }

                    // If the maximum frames are reached or end conditions are met, reset the frame count.
                    if (mCurrentFrameCount >= MaxNbOfFrames || TestEndConditions(body))
                    {
                        mCurrentFrameCount = 0;
                    }
                }
            }
            return false;
        }
    }
}
