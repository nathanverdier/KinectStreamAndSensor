using Microsoft.Kinect;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectSensorStreamsLibrary.KinectSensorStream
{
    // Class representing a color image stream from a Kinect sensor
    public class ColorImageStream : KinectStream
    {
        // Color frame reader to capture color frames
        private ColorFrameReader? colorFrameReader = null;

        // Constructor that takes a KinectManager and initializes the base class
        public ColorImageStream(KinectManager kinectManager) : base(kinectManager) { }

        // Start the color image stream
        public override void Start()
        {
            // Start the sensor
            kinectManager.StartSensor();

            // Initialize color frame reader if not already initialized
            if (this.colorFrameReader == null)
            {
                this.colorFrameReader = this.kinectManager.kinectSensor.ColorFrameSource.OpenReader();
            }

            // Register event handler for color frame arrived event
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            // Create a WriteableBitmap for color frame visualization
            FrameDescription colorFrameDescription = this.kinectManager.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            this.Bitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }

        // Stop the color image stream
        public override void Stop()
        {
            // Unregister event handler and dispose of color frame reader
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.FrameArrived -= this.Reader_ColorFrameArrived;
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }
        }

        // Event handler for color frame arrived event
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // Acquire the color frame and process it
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    ProcessColorFrame(colorFrame);
                }
            }
        }

        // Process the color frame and update the WriteableBitmap
        private void ProcessColorFrame(ColorFrame colorFrame)
        {
            // Get color frame description
            FrameDescription colorFrameDescription = colorFrame.FrameDescription;

            // Lock the WriteableBitmap for writing
            using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
            {
                this.Bitmap.Lock();

                // Copy color frame data to WriteableBitmap
                if ((colorFrameDescription.Width == this.Bitmap.PixelWidth) && (colorFrameDescription.Height == this.Bitmap.PixelHeight))
                {
                    colorFrame.CopyConvertedFrameDataToIntPtr(this.Bitmap.BackBuffer,
                        (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                        ColorImageFormat.Bgra);

                    // Add dirty rectangle to indicate the region that changed
                    this.Bitmap.AddDirtyRect(new Int32Rect(0, 0, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight));
                }

                // Unlock the WriteableBitmap
                this.Bitmap.Unlock();
            }
        }
    }
}
