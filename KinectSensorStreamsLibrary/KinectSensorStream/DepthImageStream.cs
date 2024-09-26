using Microsoft.Kinect;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace KinectSensorStreamsLibrary.KinectSensorStream
{
    // Class representing a depth image stream from a Kinect sensor
    public class DepthImageStream : KinectStream
    {
        // Constant used for mapping depth to byte
        private const int MapDepthToByte = 8000 / 256;

        // Depth frame reader to capture depth frames
        private DepthFrameReader? depthFrameReader = null;

        // Arrays to store depth frame data and converted depth pixels
        private ushort[] depthFrameData;
        private byte[] depthPixels;

        // Bytes per pixel in the image
        private readonly int cbytesPerPixel = 4;

        // Constructor that takes a KinectManager and initializes the base class
        public DepthImageStream(KinectManager kinectManager) : base(kinectManager) { }

        // Start the depth image stream
        public override void Start()
        {
            // Start the sensor
            kinectManager.StartSensor();

            // Initialize depth frame reader if not already initialized
            if (this.depthFrameReader == null)
            {
                this.depthFrameReader = this.kinectManager.kinectSensor.DepthFrameSource.OpenReader();
            }

            // Register event handler for depth frame arrived event
            this.depthFrameReader.FrameArrived += this.Reader_DepthFrameArrived;

            // Create arrays and a WriteableBitmap for depth frame visualization
            FrameDescription depthFrameDescription = this.kinectManager.kinectSensor.DepthFrameSource.FrameDescription;
            this.depthFrameData = new ushort[depthFrameDescription.Width * depthFrameDescription.Height];
            this.depthPixels = new byte[depthFrameDescription.Width * depthFrameDescription.Height * this.cbytesPerPixel];
            this.Bitmap = new WriteableBitmap(depthFrameDescription.Width, depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }

        // Stop the depth image stream
        public override void Stop()
        {
            // Unregister event handler and dispose of depth frame reader
            if (this.depthFrameReader != null)
            {
                this.depthFrameReader.FrameArrived -= this.Reader_DepthFrameArrived;
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }
        }

        // Event handler for depth frame arrived event
        private void Reader_DepthFrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            // Variables to store min and max depth values
            ushort minDepth = 0;
            ushort maxDepth = 0;

            // Flag to indicate whether the depth frame has been processed
            bool depthFrameProcessed = false;

            // Acquire the depth frame and process it
            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    (depthFrameProcessed, minDepth, maxDepth) = ProcessDepthFrame(depthFrame);
                }
            }

            // If the depth frame has been processed, convert and render depth pixels
            if (depthFrameProcessed)
            {
                ConvertDepthData(minDepth, maxDepth);
                RenderDepthPixels(this.depthPixels);
            }
        }

        // Process the depth frame and return processed data along with min and max depth values
        private (bool, ushort, ushort) ProcessDepthFrame(DepthFrame depthFrame)
        {
            ushort minDepth = 0;
            ushort maxDepth = 0;
            bool depthFrameProcessed = false;
            FrameDescription depthFrameDescription = depthFrame.FrameDescription;

            // Verify data and write the new depth frame data to the display bitmap
            if (((depthFrameDescription.Width * depthFrameDescription.Height) == this.depthFrameData.Length) &&
                (depthFrameDescription.Width == this.Bitmap.PixelWidth) && (depthFrameDescription.Height == this.Bitmap.PixelHeight))
            {
                // Copy the pixel data from the image to a temporary array
                depthFrame.CopyFrameDataToArray(this.depthFrameData);

                // Set minDepth to the minimum reliable distance and maxDepth to the maximum possible depth
                minDepth = depthFrame.DepthMinReliableDistance;
                maxDepth = ushort.MaxValue;

                // If you wish to filter by reliable depth distance, uncomment the following line:
                // maxDepth = depthFrame.DepthMaxReliableDistance

                depthFrameProcessed = true;
            }

            return (depthFrameProcessed, minDepth, maxDepth);
        }

        // Convert depth data to color pixels
        private void ConvertDepthData(ushort minDepth, ushort maxDepth)
        {
            int colorPixelIndex = 0;

            for (int i = 0; i < this.depthFrameData.Length; ++i)
            {
                // Get the depth for this pixel
                ushort depth = this.depthFrameData[i];

                // To convert to a byte, map the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);

                // Write out blue, green, red, and alpha bytes
                this.depthPixels[colorPixelIndex++] = intensity;
                this.depthPixels[colorPixelIndex++] = intensity;
                this.depthPixels[colorPixelIndex++] = intensity;
                this.depthPixels[colorPixelIndex++] = 255; // Alpha value
            }
        }

        // Render depth pixels to the WriteableBitmap
        private void RenderDepthPixels(byte[] pixels)
        {
            // Lock the WriteableBitmap for writing
            this.Bitmap.Lock();
            bool isBitmapLocked = true;

            try
            {
                // Get the address of the pixel buffer and copy the pixel data
                IntPtr pixelBuffer = this.Bitmap.BackBuffer;
                Marshal.Copy(pixels, 0, pixelBuffer, pixels.Length);
            }
            finally
            {
                // Unlock the bitmap
                this.Bitmap.Unlock();
                isBitmapLocked = false;
            }

            // If the bitmap was not locked during the try block, invalidate the bitmap
            if (!isBitmapLocked)
            {
                this.Bitmap.Lock();
                this.Bitmap.AddDirtyRect(new Int32Rect(0, 0, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight));
                this.Bitmap.Unlock();
            }
        }
    }
}
