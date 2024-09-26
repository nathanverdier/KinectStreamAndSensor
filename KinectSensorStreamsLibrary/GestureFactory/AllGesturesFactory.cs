using KinectSensorStreamsLibrary.Gestures;
using KinectSensorStreamsLibrary.Postures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectSensorStreamsLibrary.GestureFactory
{
    // Represents a factory class responsible for creating instances of gestures and postures.
    public class AllGesturesFactory : IGestureFactory
    {
        // Implements the CreateGestures method from the IGestureFactory interface.
        public IEnumerable<BaseGesture> CreateGestures()
        {
            // Create a list to store gestures and postures.
            List<BaseGesture> list = new List<BaseGesture>();

            // Create instances of postures and add them to the list.
            list.Add(new RightHandUp());
            list.Add(new TwoHandsUp());
            list.Add(new TwoHandsBottom());

            // Create instances of gestures (commented out for illustration purposes, as it's not currently in use).
            //list.Add(new SwipeRightHand());
            list.Add(new ClapHands());

            // Return the complete list of gestures and postures.
            return list;
        }
    }
}
