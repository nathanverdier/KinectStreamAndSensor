using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace KinectSensorStreamsLibrary.KinectSensorStream
{
    public class InfraredImageStream : KinectStream
    {
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;

        private const float InfraredOutputValueMinimum = 0.01f;

        private const float InfraredOutputValueMaximum = 1.0f;

        private const float InfraredSceneValueAverage = 0.08f;

        private const float InfraredSceneStandardDeviations = 3.0f;

        private const int BytesPerPixel = 4;

        private InfraredFrameReader infraredFrameReader = null;

        private ushort[] infraredFrameData = null;

        private byte[] infraredPixels = null;

        public InfraredImageStream(KinectManager kinectManager) : base(kinectManager) {}

        public override void Start()
        {
            kinectManager.StartSensor();

            if (this.infraredFrameReader == null)
            {
                this.infraredFrameReader = this.kinectManager.kinectSensor.InfraredFrameSource.OpenReader();
            }

            this.infraredFrameReader.FrameArrived += this.Reader_InfraredFrameArrived;

            FrameDescription infraredFrameDescription = this.kinectManager.kinectSensor.InfraredFrameSource.FrameDescription;

            this.infraredFrameData = new ushort[infraredFrameDescription.Width * infraredFrameDescription.Height];
            this.infraredPixels = new byte[infraredFrameDescription.Width * infraredFrameDescription.Height * BytesPerPixel];

            this.Bitmap = new WriteableBitmap(infraredFrameDescription.Width, infraredFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
        }

        public override void Stop()
        {
            if (this.infraredFrameReader != null)
            {
                this.infraredFrameReader.FrameArrived -= this.Reader_InfraredFrameArrived;
                this.infraredFrameReader.Dispose();
                this.infraredFrameReader = null;
            }
        }

        private void Reader_InfraredFrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            bool infraredFrameProcessed = false;

            // InfraredFrame is IDisposable
            using (InfraredFrame infraredFrame = e.FrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    FrameDescription infraredFrameDescription = infraredFrame.FrameDescription;

                    // verify data and write the new infrared frame data to the display bitmap
                    if (((infraredFrameDescription.Width * infraredFrameDescription.Height) == this.infraredFrameData.Length) &&
                        (infraredFrameDescription.Width == this.Bitmap.PixelWidth) && (infraredFrameDescription.Height == this.Bitmap.PixelHeight))
                    {
                        // Copy the pixel data from the image to a temporary array
                        infraredFrame.CopyFrameDataToArray(this.infraredFrameData);

                        infraredFrameProcessed = true;
                    }
                }
            }

            if (infraredFrameProcessed)
            {
                this.ConvertInfraredData();
                this.RenderInfraredPixels(this.infraredPixels);
            }
        }

        private void ConvertInfraredData()
        {
            // Convert the infrared to RGB
            int colorPixelIndex = 0;
            for (int i = 0; i < this.infraredFrameData.Length; ++i)
            {
                // normalize the incoming infrared data (ushort) to a float ranging from 
                // [InfraredOutputValueMinimum, InfraredOutputValueMaximum] by
                // 1. dividing the incoming value by the source maximum value
                float intensityRatio = (float)this.infraredFrameData[i] / InfraredSourceValueMaximum;

                // 2. dividing by the (average scene value * standard deviations)
                intensityRatio /= InfraredSceneValueAverage * InfraredSceneStandardDeviations;

                // 3. limiting the value to InfraredOutputValueMaximum
                intensityRatio = Math.Min(InfraredOutputValueMaximum, intensityRatio);

                // 4. limiting the lower value InfraredOutputValueMinimym
                intensityRatio = Math.Max(InfraredOutputValueMinimum, intensityRatio);

                // 5. converting the normalized value to a byte and using the result
                // as the RGB components required by the image
                byte intensity = (byte)(intensityRatio * 255.0f);
                this.infraredPixels[colorPixelIndex++] = intensity;
                this.infraredPixels[colorPixelIndex++] = intensity;
                this.infraredPixels[colorPixelIndex++] = intensity;
                this.infraredPixels[colorPixelIndex++] = 255;
            }
        }

        private void RenderInfraredPixels(byte[] pixels)
        {
            this.Bitmap.Lock();
            bool isBitmapLocked = true;

            try
            {
                // Get the address of the pixel buffer
                IntPtr pixelBuffer = this.Bitmap.BackBuffer;

                // Copy the pixel data to the bitmap
                Marshal.Copy(pixels, 0, pixelBuffer, pixels.Length);
            }
            finally
            {
                // Unlock the bitmap
                this.Bitmap.Unlock();
                isBitmapLocked = false;
            }

            if (!isBitmapLocked)
            {
                // Invalidate the bitmap
                this.Bitmap.Lock();
                this.Bitmap.AddDirtyRect(new Int32Rect(0, 0, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight));
                this.Bitmap.Unlock();
            }
        }
    }
}
