using CommunityToolkit.Mvvm.ComponentModel;
using KinectSensorStreamsLibrary;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;

namespace Model
{
    public partial class BodyBasics : KinectStream
    {
        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double HighConfidenceHandSize = 40;

        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double LowConfidenceHandSize = 20;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 8.0;

        /// <summary>
        /// Thickness of seen bone lines
        /// </summary>
        private const double TrackedBoneThickness = 4.0;

        /// <summary>
        /// Thickness of inferred joint lines
        /// </summary>
        private const double InferredBoneThickness = 1.0;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 5;

        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        public BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        public Body[] GetBodies() { return bodies; }

        /// <summary>
        /// Main Canvas that contains all visual objects for all bodies and clipped edges
        /// </summary>
        [ObservableProperty]
        private Canvas drawingCanvas;

        /// <summary>
        /// List of BodyInfo objects for each potential body
        /// </summary>
        private BodyInfo[] BodyInfos;

        /// <summary>
        /// List of colors for each body tracked
        /// </summary>
        private List<Color> BodyColors;

        /// <summary>
        /// Clipped edges rectangles
        /// </summary>
        private Rectangle LeftClipEdge;
        private Rectangle RightClipEdge;
        private Rectangle TopClipEdge;
        private Rectangle BottomClipEdge;

        private const int Width = 512;
        private const int Height = 414;

        private int BodyCount
        {
            set
            {
                if (value == 0)
                {
                    this.BodyInfos = null;
                    return;
                }

                // creates instances of BodyInfo objects for potential number of bodies
                if (this.BodyInfos == null || this.BodyInfos.Length != value)
                {
                    this.BodyInfos = new BodyInfo[value];

                    for (int bodyIndex = 0; bodyIndex < this.bodies.Length; bodyIndex++)
                    {
                        this.BodyInfos[bodyIndex] = new BodyInfo(this.BodyColors[bodyIndex]);
                    }
                }
            }

            get { return this.BodyInfos == null ? 0 : this.BodyInfos.Length; }
        }

        private float JointSpaceWidth { get; set; }

        private float JointSpaceHeight { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// </summary>
        public BodyBasics(KinectManager kinectManager) :base(kinectManager)
        {
            // populate body colors, one for each BodyIndex
            this.BodyColors = new List<Color>
            {
                Colors.Red,
                Colors.Orange,
                Colors.Green,
                Colors.Blue,
                Colors.Indigo,
                Colors.Violet
            };
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            bool hasTrackedBody = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                this.BeginBodiesUpdate();

                // iterate through each body
                for (int bodyIndex = 0; bodyIndex < this.bodies.Length; bodyIndex++)
                {
                    Body body = this.bodies[bodyIndex];

                    if (body.IsTracked)
                    {
                        // check if this body clips an edge
                        this.UpdateClippedEdges(body, hasTrackedBody);

                        this.UpdateBody(body, bodyIndex);

                        hasTrackedBody = true;
                    }
                    else
                    {
                        // collapse this body from canvas as it goes out of view
                        this.ClearBody(bodyIndex);
                    }
                }

                if (!hasTrackedBody)
                {
                    // clear clipped edges if no bodies are tracked
                    this.ClearClippedEdges();
                }
            }

            this.Bitmap = SaveAsWriteableBitmap(this.DrawingCanvas);
        }

        /// <summary>
        /// Clear update status of all bodies
        /// </summary>
        internal void BeginBodiesUpdate()
        {
            if (this.BodyInfos != null)
            {
                foreach (var bodyInfo in this.BodyInfos)
                {
                    bodyInfo.Updated = false;
                }
            }
        }

        /// <summary>
        /// Update body data for each body that is tracked.
        /// </summary>
        /// <param name="body">body for getting joint info</param>
        /// <param name="bodyIndex">index for body we are currently updating</param>
        internal void UpdateBody(Body body, int bodyIndex)
        {
            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
            var jointPointsInDepthSpace = new Dictionary<JointType, Point>();

            var bodyInfo = this.BodyInfos[bodyIndex];

            this.coordinateMapper = this.kinectManager.kinectSensor.CoordinateMapper;

            // update all joints
            foreach (var jointType in body.Joints.Keys)
            {
                // sometimes the depth(Z) of an inferred joint may show as negative
                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                CameraSpacePoint position = body.Joints[jointType].Position;
                if (position.Z < 0)
                {
                    position.Z = InferredZPositionClamp;
                }

                // map joint position to depth space
                DepthSpacePoint depthSpacePoint = coordinateMapper.MapCameraPointToDepthSpace(position);
                jointPointsInDepthSpace[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);

                // modify the joint's visibility and location
                this.UpdateJoint(bodyInfo.JointPoints[jointType], joints[jointType], jointPointsInDepthSpace[jointType]);

                // modify hand ellipse colors based on hand states
                // modity hand ellipse sizes based on tracking confidences
                if (jointType == JointType.HandRight)
                {
                    this.UpdateHand(bodyInfo.HandRightEllipse, body.HandRightState, body.HandRightConfidence, jointPointsInDepthSpace[jointType]);
                }

                if (jointType == JointType.HandLeft)
                {
                    this.UpdateHand(bodyInfo.HandLeftEllipse, body.HandLeftState, body.HandLeftConfidence, jointPointsInDepthSpace[jointType]);
                }
            }

            // update all bones
            foreach (var bone in bodyInfo.Bones)
            {
                this.UpdateBone(bodyInfo.BoneLines[bone], joints[bone.Item1], joints[bone.Item2],
                                jointPointsInDepthSpace[bone.Item1],
                                jointPointsInDepthSpace[bone.Item2]);
            }
        }

        /// <summary>
        /// Collapse the body from the canvas.
        /// </summary>
        /// <param name="bodyIndex"></param>
        private void ClearBody(int bodyIndex)
        {
            var bodyInfo = this.BodyInfos[bodyIndex];

            // collapse all joint ellipses
            foreach (var joint in bodyInfo.JointPoints)
            {
                joint.Value.Visibility = Visibility.Collapsed;
            }

            // collapse all bone lines
            foreach (var bone in bodyInfo.Bones)
            {
                bodyInfo.BoneLines[bone].Visibility = Visibility.Collapsed;
            }

            // collapse handstate ellipses
            bodyInfo.HandLeftEllipse.Visibility = Visibility.Collapsed;

            bodyInfo.HandRightEllipse.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Updates hand state ellipses depending on tracking state and it's confidence.
        /// </summary>
        /// <param name="ellipse">ellipse representing handstate</param>
        /// <param name="handState">open, closed, or lasso</param>
        /// <param name="trackingConfidence">confidence of handstate</param>
        /// <param name="point">location of handjoint</param>
        private void UpdateHand(Ellipse ellipse, HandState handState, TrackingConfidence trackingConfidence, Point point)
        {
            ellipse.Fill = new SolidColorBrush(this.HandStateToColor(handState));

            // draw handstate ellipse based on tracking confidence
            ellipse.Width = ellipse.Height = (trackingConfidence == TrackingConfidence.Low) ? LowConfidenceHandSize : HighConfidenceHandSize;

            ellipse.Visibility = Visibility.Visible;

            // don't draw handstate if hand joints are not tracked
            if (!Double.IsInfinity(point.X) && !Double.IsInfinity(point.Y))
            {
                Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, point.Y - ellipse.Width / 2);
            }
        }

        /// <summary>
        /// Update a joint.
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="joint"></param>
        /// <param name="point"></param>
        private void UpdateJoint(Ellipse ellipse, Joint joint, Point point)
        {
            TrackingState trackingState = joint.TrackingState;

            // only draw if joint is tracked or inferred
            if (trackingState != TrackingState.NotTracked)
            {
                if (trackingState == TrackingState.Tracked)
                {
                    ellipse.Fill = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    // inferred joints are yellow
                    ellipse.Fill = new SolidColorBrush(Colors.Yellow);
                }

                Canvas.SetLeft(ellipse, point.X - JointThickness / 2);
                Canvas.SetTop(ellipse, point.Y - JointThickness / 2);

                ellipse.Visibility = Visibility.Visible;
            }
            else
            {
                ellipse.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Update a bone line.
        /// </summary>
        /// <param name="line">line representing a bone line</param>
        /// <param name="startJoint">start joint of bone line</param>
        /// <param name="endJoint">end joint of bone line</param>
        /// <param name="startPoint">location of start joint</param>
        /// <param name="endPoint">location of end joint</param>
        private void UpdateBone(Line line, Joint startJoint, Joint endJoint, Point startPoint, Point endPoint)
        {
            // don't draw if neither joints are tracked
            if (startJoint.TrackingState == TrackingState.NotTracked || endJoint.TrackingState == TrackingState.NotTracked)
            {
                line.Visibility = Visibility.Collapsed;
                return;
            }

            // all lines are inferred thickness unless both joints are tracked
            line.StrokeThickness = InferredBoneThickness;

            if (startJoint.TrackingState == TrackingState.Tracked &&
                endJoint.TrackingState == TrackingState.Tracked)
            {
                line.StrokeThickness = TrackedBoneThickness;
            }

            line.Visibility = Visibility.Visible;

            line.X1 = startPoint.X;
            line.Y1 = startPoint.Y;
            line.X2 = endPoint.X;
            line.Y2 = endPoint.Y;
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data.
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="hasTrackedBody">bool to determine if another body is triggering a clipped edge</param>
        private void UpdateClippedEdges(Body body, bool hasTrackedBody)
        {
            // BUG (waiting for confirmation): 
            // Clip dectection works differently for top and right edges compared to left and bottom edges
            // due to the current joint confidence model. This is an ST issue.
            // Joints become inferred immediately as they touch the left/bottom edges and clip detection triggers.
            // Joints squish on the right/top edges and clip detection doesn't trigger until more joints of 
            // the body goes out of view (e.g all hand joints vs only handtip).

            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                this.LeftClipEdge.Visibility = Visibility.Visible;
            }
            else if (!hasTrackedBody)
            {
                // don't clear this edge if another body is triggering clipped edge
                this.LeftClipEdge.Visibility = Visibility.Collapsed;
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                this.RightClipEdge.Visibility = Visibility.Visible;
            }
            else if (!hasTrackedBody)
            {
                this.RightClipEdge.Visibility = Visibility.Collapsed;
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                this.TopClipEdge.Visibility = Visibility.Visible;
            }
            else if (!hasTrackedBody)
            {
                this.TopClipEdge.Visibility = Visibility.Collapsed;
            }

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                this.BottomClipEdge.Visibility = Visibility.Visible;
            }
            else if (!hasTrackedBody)
            {
                this.BottomClipEdge.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Clear all clipped edges.
        /// </summary>
        private void ClearClippedEdges()
        {
            this.LeftClipEdge.Visibility = Visibility.Collapsed;

            this.RightClipEdge.Visibility = Visibility.Collapsed;

            this.TopClipEdge.Visibility = Visibility.Collapsed;

            this.BottomClipEdge.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Select color of hand state
        /// </summary>
        /// <param name="handState"></param>
        /// <returns></returns>
        private Color HandStateToColor(HandState handState)
        {
            switch (handState)
            {
                case HandState.Open:
                    return Colors.Green;

                case HandState.Closed:
                    return Colors.Red;

                case HandState.Lasso:
                    return Colors.Blue;
            }

            return Colors.Transparent;
        }

        /// <summary>
        /// Instantiate new objects for joints, bone lines, and clipped edge rectangles
        /// </summary>
        private void PopulateVisualObjects()
        {
            // create clipped edges and set to collapsed initially
            this.LeftClipEdge = new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Red),
                Width = ClipBoundsThickness,
                Height = Height,
                Visibility = Visibility.Collapsed
            };

            this.RightClipEdge = new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Red),
                Width = ClipBoundsThickness,
                Height = Height,
                Visibility = Visibility.Collapsed
            };

            this.TopClipEdge = new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Red),
                Width = Width,
                Height = ClipBoundsThickness,
                Visibility = Visibility.Collapsed
            };

            this.BottomClipEdge = new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Red),
                Width = Width,
                Height = ClipBoundsThickness,
                Visibility = Visibility.Collapsed
            };

            foreach (var bodyInfo in this.BodyInfos)
            {
                // add left and right hand ellipses of all bodies to canvas
                this.drawingCanvas.Children.Add(bodyInfo.HandLeftEllipse);
                this.drawingCanvas.Children.Add(bodyInfo.HandRightEllipse);

                // add joint ellipses of all bodies to canvas
                foreach (var joint in bodyInfo.JointPoints)
                {
                    this.drawingCanvas.Children.Add(joint.Value);
                }

                // add bone lines of all bodies to canvas
                foreach (var bone in bodyInfo.Bones)
                {
                    this.drawingCanvas.Children.Add(bodyInfo.BoneLines[bone]);
                }
            }

            // add clipped edges rectanges to main canvas
            this.drawingCanvas.Children.Add(this.LeftClipEdge);
            this.drawingCanvas.Children.Add(this.RightClipEdge);
            this.drawingCanvas.Children.Add(this.TopClipEdge);
            this.drawingCanvas.Children.Add(this.BottomClipEdge);

            // position the clipped edges
            Canvas.SetLeft(this.LeftClipEdge, 0);
            Canvas.SetTop(this.LeftClipEdge, 0);
            Canvas.SetLeft(this.RightClipEdge, Width - ClipBoundsThickness);
            Canvas.SetTop(this.RightClipEdge, 0);
            Canvas.SetLeft(this.TopClipEdge, 0);
            Canvas.SetTop(this.TopClipEdge, 0);
            Canvas.SetLeft(this.BottomClipEdge, 0);
            Canvas.SetTop(this.BottomClipEdge, Height - ClipBoundsThickness);
        }

        /// <summary>
        /// BodyInfo class that contains joint ellipses, handstate ellipses, lines for bones between two joints.
        /// </summary>
        private class BodyInfo
        {
            public bool Updated { get; set; }

            public Color BodyColor { get; set; }

            // ellipse representing left handstate
            public Ellipse HandLeftEllipse { get; set; }

            // ellipse representing right handstate
            public Ellipse HandRightEllipse { get; set; }

            // dictionary of all joints in a body
            public Dictionary<JointType, Ellipse> JointPoints { get; private set; }

            // definition of bones
            public TupleList<JointType, JointType> Bones { get; private set; }

            // collection of bones associated with the line object
            public Dictionary<Tuple<JointType, JointType>, Line> BoneLines { get; private set; }

            public BodyInfo(Color bodyColor)
            {
                this.BodyColor = bodyColor;

                // create hand state ellipses
                this.HandLeftEllipse = new Ellipse()
                {
                    Visibility = Visibility.Collapsed
                };

                this.HandRightEllipse = new Ellipse()
                {
                    Visibility = Visibility.Collapsed
                };

                // a joint defined as a jointType with a point location in XY space represented by an ellipse
                this.JointPoints = new Dictionary<JointType, Ellipse>();

                // pre-populate list of joints and set to non-visible initially
                foreach (JointType jointType in Enum.GetValues(typeof(JointType)))
                {
                    this.JointPoints.Add(jointType, new Ellipse()
                    {
                        Visibility = Visibility.Collapsed,
                        Fill = new SolidColorBrush(BodyColor),
                        Width = JointThickness,
                        Height = JointThickness
                    });
                }

                // collection of bones
                this.BoneLines = new Dictionary<Tuple<JointType, JointType>, Line>();

                // a bone defined as a line between two joints
                this.Bones = new TupleList<JointType, JointType>
            {
                // Torso
                { JointType.Head, JointType.Neck },
                { JointType.Neck, JointType.SpineShoulder },
                { JointType.SpineShoulder, JointType.SpineMid },
                { JointType.SpineMid, JointType.SpineBase },
                { JointType.SpineShoulder, JointType.ShoulderRight },
                { JointType.SpineShoulder, JointType.ShoulderLeft },
                { JointType.SpineBase, JointType.HipRight },
                { JointType.SpineBase, JointType.HipLeft },

                // Right Arm
                { JointType.ShoulderRight, JointType.ElbowRight },
                { JointType.ElbowRight, JointType.WristRight },
                { JointType.WristRight, JointType.HandRight },
                { JointType.HandRight, JointType.HandTipRight },
                { JointType.WristRight, JointType.ThumbRight },

                // Left Arm
                { JointType.ShoulderLeft, JointType.ElbowLeft },
                { JointType.ElbowLeft, JointType.WristLeft },
                { JointType.WristLeft, JointType.HandLeft },
                { JointType.HandLeft, JointType.HandTipLeft },
                { JointType.WristLeft, JointType.ThumbLeft },

                // Right Leg
                { JointType.HipRight, JointType.KneeRight },
                { JointType.KneeRight, JointType.AnkleRight },
                { JointType.AnkleRight, JointType.FootRight },
                
                // Left Leg
                { JointType.HipLeft, JointType.KneeLeft },
                { JointType.KneeLeft, JointType.AnkleLeft },
                { JointType.AnkleLeft, JointType.FootLeft },
            };

                // pre-populate list of bones that are non-visible initially
                foreach (var bone in this.Bones)
                {
                    this.BoneLines.Add(bone, new Line()
                    {
                        Stroke = new SolidColorBrush(BodyColor),
                        Visibility = Visibility.Collapsed
                    });
                }
            }
        }

        private class TupleList<T1, T2> : List<Tuple<T1, T2>>
        {
            public void Add(T1 item, T2 item2)
            {
                this.Add(new Tuple<T1, T2>(item, item2));
            }
        }

        public override void Start()
        {
            if (bodyFrameReader != null) { return; }

            // start the sensor
            this.kinectManager.StartSensor();

            // get the coordinate mapper
            this.coordinateMapper = this.kinectManager.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectManager.kinectSensor.DepthFrameSource.FrameDescription;

            // get size of joint space
            this.JointSpaceWidth = frameDescription.Width;
            this.JointSpaceHeight = frameDescription.Height;

            // get total number of bodies from BodyFrameSource
            this.bodies = new Body[this.kinectManager.kinectSensor.BodyFrameSource.BodyCount];

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectManager.kinectSensor.BodyFrameSource.OpenReader();

            // wire handler for frame arrival
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            // sets total number of possible tracked bodies
            // create ellipses and lines for drawing bodies
            this.BodyCount = this.kinectManager.kinectSensor.BodyFrameSource.BodyCount;

            // Instantiate a new Canvas
            this.drawingCanvas = new Canvas();

            // set the clip rectangle to prevent rendering outside the canvas
            this.drawingCanvas.Clip = new RectangleGeometry(new Rect(0.0, 0.0, Width, Height));

            // create visual objects for drawing joints, bone lines, and clipped edges
            this.PopulateVisualObjects();
        }

        public override void Stop()
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived -= this.Reader_BodyFrameArrived;
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }
        }

        /// <summary>
        /// Use to convert a canvas to bitmap
        /// </summary>
        /// <param name="surface">the canvas to convert</param>
        /// <returns></returns>
        public WriteableBitmap SaveAsWriteableBitmap(Canvas surface)
        {
            if (surface == null) return null;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(512, 414);

            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);


            //Restore previously saved layout
            surface.LayoutTransform = transform;

            //create and return a new WriteableBitmap using the RenderTargetBitmap
            return new WriteableBitmap(renderBitmap);
        }
    }
}