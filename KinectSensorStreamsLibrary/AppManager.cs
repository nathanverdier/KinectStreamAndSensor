using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KinectSensorStreamsLibrary.KinectSensorStream;
using Model;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;
using KinectSensorStreamsLibrary.GestureFactory;
using Microsoft.Kinect;

namespace KinectSensorStreamsLibrary
{
    // This class manages the application's Kinect functionality and user interface.
    public partial class AppManager : ObservableObject
    {
        // The KinectManager instance responsible for managing Kinect sensors.
        public KinectManager KinectManager { get; private set; }

        // Observable property for the current Kinect stream.
        [ObservableProperty]
        public KinectStream kinectStream;

        // Observable property for the visibility of a grid in the UI.
        [ObservableProperty]
        public Visibility gridVisibility = Visibility.Collapsed;

        // Observable property for the visibility of an image in the UI.
        [ObservableProperty]
        public Visibility imageVisibility = Visibility.Visible;

        // Constructor for the AppManager class.
        public AppManager()
        {
            // Initialize KinectManager and start the sensor.
            KinectManager = new KinectManager();
            KinectManager.StartSensor();

            // Initialize KinectStream with ColorImageStream and start it.
            KinectStream = new ColorImageStream(KinectManager);
            KinectStream.Start();

            // Initialize commands for switching between different Kinect streams.
            SwitchToColorImageCommand = new RelayCommand(SwitchToColorImage);
            SwitchToDepthImageCommand = new RelayCommand(SwitchToDepthImage);
            SwitchToInfraredImageCommand = new RelayCommand(SwitchToInfraredImage);
            SwitchToBodyImageCommand = new RelayCommand(SwitchToBodyImage);
        }

        // Command for switching to the color image stream.
        public ICommand SwitchToColorImageCommand { get; }
        private void SwitchToColorImage()
        {
            // Update visibility properties and switch to ColorImageStream.
            ImageVisibility = Visibility.Visible;
            GridVisibility = Visibility.Collapsed;
            KinectStream.Stop();
            KinectStream = new ColorImageStream(KinectManager);
            KinectStream.Start();
        }

        // Command for switching to the depth image stream.
        public ICommand SwitchToDepthImageCommand { get; }
        private void SwitchToDepthImage()
        {
            // Update visibility properties and switch to DepthImageStream.
            ImageVisibility = Visibility.Visible;
            GridVisibility = Visibility.Collapsed;
            KinectStream.Stop();
            KinectStream = new DepthImageStream(KinectManager);
            KinectStream.Start();
            Debug.WriteLine("Switching to depth image");
        }

        // Command for switching to the infrared image stream.
        public ICommand SwitchToInfraredImageCommand { get; }
        private void SwitchToInfraredImage()
        {
            // Update visibility properties and switch to InfraredImageStream.
            ImageVisibility = Visibility.Visible;
            GridVisibility = Visibility.Collapsed;
            KinectStream.Stop();
            KinectStream = new InfraredImageStream(KinectManager);
            KinectStream.Start();
            Debug.WriteLine("Switching to infrared image");
        }

        // Command for switching to the body image stream.
        public ICommand SwitchToBodyImageCommand { get; }
        public void SwitchToBodyImage()
        {
            // Update visibility properties and switch to BodyBasics stream.
            ImageVisibility = Visibility.Collapsed;
            GridVisibility = Visibility.Visible;
            KinectStream.Stop();
            BodyBasics bodyStream = new BodyBasics(KinectManager);
            KinectStream = bodyStream;
            KinectStream.Start();

            IGestureFactory factory = new AllGesturesFactory();
            GestureManager.AddGestures(factory);
            GestureManager.StartAcquiringFrames(bodyStream);
        }
    }
}
