using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using KinectSensorStreamsLibrary.KinectSensorStream;

namespace KinectSensorStreamsLibrary
{
    // Partial class representing the KinectManager, responsible for managing Kinect sensor functionality.
    public partial class KinectManager : ObservableObject
    {
        // Observable property for the Kinect sensor status.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EllipseColor))]
        [NotifyPropertyChangedFor(nameof(StatusText))]
        private bool status = false;

        // Property that returns a string representing the status of the Kinect sensor.
        public string StatusText { get { return Status ? "Connected" : "Not connected"; } }

        // Property that returns a string representing the color of an ellipse based on the Kinect sensor status.
        public string EllipseColor { get { return Status ? "Green" : "Red"; } }

        // Nullable instance of KinectSensor representing the connected Kinect sensor.
        public KinectSensor? kinectSensor = null;

        // Default constructor for KinectManager.
        public KinectManager()
        {
        }

        // Method to start the Kinect sensor.
        public void StartSensor()
        {
            // Check if the Kinect sensor is not already initialized.
            if (this.kinectSensor == null)
            {
                // Initialize the Kinect sensor, open it, and register the IsAvailableChanged event.
                this.kinectSensor = KinectSensor.GetDefault();
                this.kinectSensor.Open();
                this.kinectSensor.IsAvailableChanged += this.KinectSensor_IsAvailableChanged;
            }
        }

        // Method to stop the Kinect sensor.
        public void StopSensor()
        {
            // Check if the Kinect sensor is initialized.
            if (this.kinectSensor != null)
            {
                // Close the Kinect sensor and set the reference to null.
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        // Event handler for the IsAvailableChanged event of the Kinect sensor.
        private void KinectSensor_IsAvailableChanged(Object sender, IsAvailableChangedEventArgs args)
        {
            // Update the status property based on the IsAvailable property of the Kinect sensor.
            if (this.kinectSensor != null)
            {
                this.Status = this.kinectSensor.IsAvailable;
            }
        }
    }
}
