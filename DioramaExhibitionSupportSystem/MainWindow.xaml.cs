using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

// Create image for projector
//using DrawingInterface;
//using DrawingInterface._3DSFileControl;

// For Kinect
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;

// For Gesture Recognition
using DioramaExhibitionSupportSystem.GestureRecognition;

// For Show the Result
using DrawingInterface.DrawingControl;

namespace DioramaExhibitionSupportSystem
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private GestureController gestureController;
        private DrawingController drawingController;
        private DrawingStatus drawingStatus;
        private Skeleton[] skeletons = new Skeleton[0];
        //private DrawingTest dt;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            this.Show();
            this.Topmost = true;

            drawingController = new DrawingController();
            drawingStatus = new DrawingStatus();

            drawingStatus.eye.x = 0.0f;
            drawingStatus.eye.y = 5.0f;
            drawingStatus.eye.z = 25.0f;

            drawingStatus.kinect.x = 0.0f;
            drawingStatus.kinect.y = 0.34f;
            drawingStatus.kinect.z = 0.0f;

            drawingController.ChangeStatus(drawingStatus);

            // initialize kinect
            InitKinect();
        }

        private void InitKinect()
        {
            // initialize the Kinect sensor manager
            KinectSensorManager = new KinectSensorManager();
            KinectSensorManager.KinectSensorChanged += this.KinectSensorChanged;

            // locate an available sensor
            sensorChooser.Start();

            // bind chooser's sensor value to the local sensor manager
            var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.KinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);
        }

        #region Kinect Discovery & Setup

        private void KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null != args.OldValue)
                UninitializeKinectServices(args.OldValue);

            if (null != args.NewValue)
                InitializeKinectServices(KinectSensorManager, args.NewValue);
        }

        /// <summary>
        /// Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        /// </summary>
        /// <param name="sensor"></param>
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            // Todo
        }

        /// <summary>
        /// Kinect enabled apps should customize which Kinect services it initializes here.
        /// </summary>
        /// <param name="kinectSensorManager"></param>
        /// <param name="sensor"></param>
        private void InitializeKinectServices(KinectSensorManager kinectSensorManager, KinectSensor sensor)
        {
            // configure the color stream
            //kinectSensorManager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            //kinectSensorManager.ColorStreamEnabled = true;

            // configure the depth stream
            kinectSensorManager.DepthStreamEnabled = true;
            kinectSensorManager.TransformSmoothParameters =
                new TransformSmoothParameters
                {
                    Smoothing = 0.5f,
                    Correction = 0.5f,
                    Prediction = 0.5f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.04f
                };

            // configure the skeleton stream
            // sensor.SkeletonFrameReady += OnSkeletonFrameReady;
            kinectSensorManager.SkeletonStreamEnabled = true;

            // configure the depth stream
            // sensor.DepthFrameReady += OnDepthFrameReady;
            sensor.AllFramesReady += OnAllFrameReady;

            // initialize the gesture recognizer
            gestureController = new GestureController();
            gestureController.GestureRecognized += OnGestureRecognized;

            kinectSensorManager.KinectSensorEnabled = true;

            if (!kinectSensorManager.KinectSensorAppConflict)
            {
                // addition configuration, as needed
            }
        }
        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.OpenSkeletonFrame();
            if (skeletonFrame == null)
                return;

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                // resize the skeletons array if needed
                if (skeletons.Length != frame.SkeletonArrayLength)
                    skeletons = new Skeleton[frame.SkeletonArrayLength];

                // get the skeleton data
                frame.CopySkeletonDataTo(skeletons);

                foreach (var skeleton in skeletons)
                {
                    // skip the skeleton if it is not being tracked
                    if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                        continue;

                    // update the gesture controller
                    gestureController.UpdateAllGestures(skeleton, e);
                }

                if (gestureController.gestureStatus.isMakingAFast)
                {
                    Start.Text = "Is Making a fast";
                }
                else 
                {
                    Start.Text = "Is not Making a fast";
                }
            }
        }

        #endregion Kinect Discovery & Setup

        #region Properties

        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(MainWindow),
                new PropertyMetadata(null));

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }

        /// <summary>
        /// Gets or sets the last recognized gesture.
        /// </summary>
        private string _gesture;
        public String Gesture
        {
            get { return _gesture; }

            private set
            {
                if (_gesture == value)
                    return;

                _gesture = value;

                System.Diagnostics.Debug.WriteLine("Gesture = " + _gesture);

                if (this.PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Gesture"));
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Event implementing INotifyPropertyChanged interface.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Event Handlers

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Gesture event arguments.</param>
        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            Debug.WriteLine(e.GestureType);

            switch (e.GestureType)
            {
                case GestureType.HandCloseOrOpen:
                    //Gesture = "HandCloseOrOpen";
                    break;
                case GestureType.MenuIn:
                    drawingController.SendSingle(DrawingEnumTypes.Movement.MenuIn);
                    Gesture = "MenuIn";
                    break;
                case GestureType.MenuOut:
                    drawingController.SendSingle(DrawingEnumTypes.Movement.MenuOut);
                    Gesture = "MenuOut";
                    break;
                case GestureType.MoveDown:
                    drawingController.SendSingle(DrawingEnumTypes.Movement.MoveDown);
                    Gesture = "MoveDown";
                    break;
                case GestureType.MoveIn:
                    drawingController.SendSingle(DrawingEnumTypes.Movement.MoveIn);
                    Gesture = "MoveIn";
                    break;
                case GestureType.MoveLeft:
                    drawingController.SendSingle(DrawingEnumTypes.Movement.MoveLeft);
                    Gesture = "MoveLeft";
                    break;
                case GestureType.MoveOut:
                    drawingController.SendSingle(DrawingEnumTypes.Movement.MoveOut);
                    Gesture = "MoveOut";
                    break;
                case GestureType.MoveRight:
                    drawingController.SendSingle(DrawingEnumTypes.Movement.MoveRight);
                    Gesture = "MoveRight";
                    break;
                case GestureType.MoveUp:
                    drawingController.SendSingle(DrawingEnumTypes.Movement.MoveUp);
                    Gesture = "MoveUp";
                    break;
                case GestureType.HeadMoved:
                    Gesture = "HeadMoved: new position(" + e.GestureStatus.headPoint.X + "," + (e.GestureStatus.headPoint.Y + 0.2) + "," + e.GestureStatus.headPoint.Z + ")";
                    drawingStatus.eye.x = - e.GestureStatus.headPoint.Z;
                    drawingStatus.eye.y = e.GestureStatus.headPoint.Y;
                    drawingStatus.eye.z = e.GestureStatus.headPoint.X;

                    drawingController.ChangeStatus(drawingStatus);

                    this.txtEyex.Text = e.GestureStatus.headPoint.X + "";
                    this.txtEyey.Text = e.GestureStatus.headPoint.Y + "";
                    this.txtEyez.Text = e.GestureStatus.headPoint.Z + "";
                    this.txtKinectx.Text = "0";
                    this.txtKinecty.Text = "0.2";
                    this.txtKinectz.Text = "0";
                    //dt.SetLookat(e.GestureStatus.headPoint.X, e.GestureStatus.headPoint.Y+2, e.GestureStatus.headPoint.Z);
                    break;
                default:
                    break;
            }

            End.Text = Gesture;
        }

        #endregion Event Handlers

        #region OpenGL Control Functions
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            drawingController.isStart = false;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            drawingStatus.eye.x = (float)Convert.ToSingle(txtEyex.Text);
            drawingStatus.eye.y = (float)Convert.ToSingle(txtEyey.Text);
            drawingStatus.eye.z = (float)Convert.ToSingle(txtEyez.Text);
            drawingStatus.projector.x = (float)Convert.ToSingle(txtProjectorx.Text);
            drawingStatus.projector.y = (float)Convert.ToSingle(txtProjectory.Text);
            drawingStatus.projector.z = (float)Convert.ToSingle(txtProjectorz.Text);
            drawingStatus.kinect.x = (float)Convert.ToSingle(txtKinectx.Text);
            drawingStatus.kinect.y = (float)Convert.ToSingle(txtKinecty.Text);
            drawingStatus.kinect.z = (float)Convert.ToSingle(txtKinectz.Text);

            drawingController.ChangeStatus(drawingStatus);
            drawingController.isStart = true;
        }

        private void btnup_Click(object sender, RoutedEventArgs e)
        {
            drawingController.SendSingle(DrawingEnumTypes.Movement.MoveUp);
        }

        private void btnleft_Click(object sender, RoutedEventArgs e)
        {
            drawingController.SendSingle(DrawingEnumTypes.Movement.MoveLeft);
        }

        private void btnplus_Click(object sender, RoutedEventArgs e)
        {
            drawingController.SendSingle(DrawingEnumTypes.Movement.MoveIn);
        }

        private void btnminus_Click(object sender, RoutedEventArgs e)
        {
            drawingController.SendSingle(DrawingEnumTypes.Movement.MoveOut);
        }

        private void btnright_Click(object sender, RoutedEventArgs e)
        {
            drawingController.SendSingle(DrawingEnumTypes.Movement.MoveRight);
        }

        private void btndown_Click(object sender, RoutedEventArgs e)
        {
            drawingController.SendSingle(DrawingEnumTypes.Movement.MoveDown);
        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            drawingController.SendSingle(DrawingEnumTypes.Movement.MenuIn);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            drawingController.SendSingle(DrawingEnumTypes.Movement.MenuOut);
        }

        #endregion OpenGL Control Functions
    }
}
