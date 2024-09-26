using Microsoft.Kinect;
using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KinectSensorStreamsLibrary
{
    // Static class responsible for managing gestures and Kinect sensor frames.
    public static class GestureManager
    {
        // List to store known gestures.
        public static List<BaseGesture> KnownGestures = new List<BaseGesture>();

        // Event triggered when a gesture is recognized.
        public static EventHandler<GestureRecognizedEventArgs> GestureRecognized;

        // Reference to the KinectStream instance responsible for handling Kinect sensors.
        public static BodyBasics KinectStream { get; private set; }

        // Adds gestures to the KnownGestures list using an IGestureFactory.
        public static void AddGestures(IGestureFactory gestureFactory)
        {
            if (gestureFactory != null)
            {
                // Create gestures using the provided factory and add them to the KnownGestures list.
                IEnumerable<BaseGesture> gesturesToAdd = gestureFactory.CreateGestures();
                foreach (BaseGesture gesture in gesturesToAdd)
                {
                    KnownGestures.Add(gesture);
                    gesture.GestureRecognized += OnGestureRecognized;
                }
            }
        }

        // Adds gestures directly to the KnownGestures list.
        public static void AddGestures(params BaseGesture[] gesturesToAdd)
        {
            foreach (var gesture in gesturesToAdd)
            {
                KnownGestures.Add(gesture);
            }
        }

        // Removes a specified gesture from the KnownGestures list.
        public static void RemoveGesture(BaseGesture gestureToRemove)
        {
            KnownGestures.Remove(gestureToRemove);
        }

        // Starts acquiring frames from the Kinect sensor using the specified KinectManager.
        public static void StartAcquiringFrames(BodyBasics bodyStream)
        {
            // Set the KinectManager reference and start the sensor.
            KinectStream = bodyStream;
            KinectStream.Start();

            KinectStream.bodyFrameReader.FrameArrived += Reader_BodyFrameArrived;
        }

        // Stops acquiring frames from the Kinect sensor.
        public static void StopAcquiringFrame()
        {
            // Stop the Kinect sensor.
            KinectStream.bodyFrameReader.FrameArrived -= Reader_BodyFrameArrived;
        }

        private static void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            foreach (Body body in KinectStream.GetBodies())
            {
                foreach (BaseGesture gesture in KnownGestures)
                {
                    gesture.TestGesture(body);
                }
            }
        }

        private static void OnGestureRecognized(object sender, GestureRecognizedEventArgs e)
        {
            Debug.WriteLine("Gesture Recognized ! name:" + e.GestureName);
        }
    }
}
