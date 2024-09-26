using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectSensorStreamsLibrary
{
    // Interface for a gesture factory responsible for creating instances of BaseGesture.
    public interface IGestureFactory
    {
        // Defines a method to create and return a collection of BaseGesture instances.
        public IEnumerable<BaseGesture> CreateGestures();
    }
}
