using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace KinectSensorStreamsLibrary
{
    // Represents the arguments for the GestureRecognized event.
    public class GestureRecognizedEventArgs : EventArgs
    {
        // Gets the Body associated with the recognized gesture.
        public Body Body { get; private set; }

        // Gets the name of the recognized gesture.
        public String GestureName { get; private set; }

        // Constructor for GestureRecognizedEventArgs.
        public GestureRecognizedEventArgs(Body body, string gestureName)
        {
            Body = body;
            GestureName = gestureName;
        }
    }

    // Represents the base class for every gesture.
    public abstract class BaseGesture
    {
        // Event triggered when a gesture is recognized.
        public event EventHandler<GestureRecognizedEventArgs> GestureRecognized;

        // Gets the name of the gesture.
        public string GestureName { get; protected set; }

        // Abstract method to test whether a gesture is recognized for the given body.
        public abstract bool TestGesture(Body body);

        // Invokes the GestureRecognized event with information about the recognized gesture.
        protected void OnGestureRecognized(Body body)
        {
            GestureRecognized?.Invoke(this, new GestureRecognizedEventArgs(body, GestureName));
        }
    }
}
