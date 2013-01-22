using System;
using System.Collections.Generic;
using DioramaExhibitionSupportSystem.GestureRecognition.Segments;
using Microsoft.Kinect;
using Emgu.CV;
using Emgu.CV.Structure;

namespace DioramaExhibitionSupportSystem.GestureRecognition
{
    public class GestureController
    {
        /// <summary>
        /// The list of all gestures we are currently looking for
        /// </summary>
        private List<Gesture> gestures = new List<Gesture>();

        /// <summary>
        /// The status of gestures
        /// </summary>
        public GestureStatus gestureStatus = new GestureStatus();

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureController"/> class.
        /// </summary>
        public GestureController()
        {
            // Define the gestures

            IRelativeGestureSegment moveDown = new MoveDown();
            AddGesture(GestureType.MoveDown, moveDown);

            IRelativeGestureSegment moveIn = new MoveIn();
            AddGesture(GestureType.MoveIn, moveIn);

            IRelativeGestureSegment moveLeft = new MoveLeft();
            AddGesture(GestureType.MoveLeft, moveLeft);

            IRelativeGestureSegment moveRight = new MoveRight();
            AddGesture(GestureType.MoveRight, moveRight);

            IRelativeGestureSegment moveOut = new MoveOut();
            AddGesture(GestureType.MoveOut, moveOut);

            IRelativeGestureSegment moveUp = new MoveUp();
            AddGesture(GestureType.MoveUp, moveUp);

            IRelativeGestureSegment handCloseOrOpen = new HandCloseOrOpen();
            AddGesture(GestureType.HandCloseOrOpen, handCloseOrOpen);

            IRelativeGestureSegment headMoved = new HeadMoved();
            AddGesture(GestureType.HeadMoved, headMoved);

            //CvInvoke.cvNamedWindow("Source");
        }

        /// <summary>
        /// Occurs when [gesture recognised].
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        /// <summary>
        /// Updates all gestures.
        /// </summary>
        /// <param name="data">The skeleton data.</param>
        public void UpdateAllGestures(Skeleton data, AllFramesReadyEventArgs e)
        {
            foreach (Gesture gesture in this.gestures)
            {
                gesture.UpdateGesture(data, e, gestureStatus);
            }
        }

        /// <summary>
        /// Adds the gesture.
        /// </summary>
        /// <param name="type">The gesture type.</param>
        /// <param name="gestureDefinition">The gesture definition.</param>
        public void AddGesture(GestureType type, IRelativeGestureSegment gestureDefinition)
        {
            Gesture gesture = new Gesture(type, gestureDefinition);
            //gesture.GestureRecognized += new EventHandler<GestureEventArgs>(this.Gesture_GestureRecognized);
            gesture.GestureRecognized += OnGestureRecognized;
            this.gestures.Add(gesture);
        }

        /// <summary>
        /// Handles the GestureRecognized event of the g control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KinectSkeltonTracker.GestureEventArgs"/> instance containing the event data.</param>
        //private void Gesture_GestureRecognized(object sender, GestureEventArgs e)
        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            if (this.GestureRecognized != null)
            {
                this.GestureRecognized(this, e);
            }
        }
    }
}
