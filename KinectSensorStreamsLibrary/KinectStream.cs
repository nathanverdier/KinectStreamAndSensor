using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KinectSensorStreamsLibrary
{
    // Abstract partial class representing a KinectStream, providing a base for different stream types.
    public abstract partial class KinectStream : ObservableObject
    {
        // Protected property for the associated KinectManager instance.
        protected KinectManager kinectManager { get; set; }

        // Observable property representing the WriteableBitmap used for the stream.
        [ObservableProperty]
        public WriteableBitmap? bitmap;

        // Constructor for KinectStream, initializing the associated KinectManager.
        public KinectStream(KinectManager kinectManager)
        {
            this.kinectManager = kinectManager;
        }

        // Abstract method to start the Kinect stream.
        public abstract void Start();

        // Abstract method to stop the Kinect stream.
        public abstract void Stop();
    }
}
