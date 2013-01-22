using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace DioramaExhibitionSupportSystem.GestureRecognition
{
    class Gesture
    {
        public Microsoft.Kinect.SkeletonPoint startPoint = new SkeletonPoint();

        /// <summary>
        /// The parts that make up this gesture
        /// </summary>
        private IRelativeGestureSegment gestureParts;
        private UInt32 _interval;
        private UInt32 _count;

        /// <summary>
        /// Get and set the interval of every gesture.
        /// </summary>
        public UInt32 Interval
        {
            set { _interval = value; }
            get { return _interval; }
        }

        /// <summary>
        /// Check if gesture recognition should be run.
        /// </summary>
        public bool ShouldExecut()
        {
            if (++_count % _interval == 0)
                return true;
            return false;
        }

        /// <summary>
        /// The type of gesture that this is
        /// </summary>
        private GestureType type;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gesture"/> class.
        /// </summary>
        /// <param name="type">The type of gesture.</param>
        /// <param name="gestureParts">The gesture parts.</param>
        public Gesture(GestureType type, IRelativeGestureSegment gestureParts)
        {
            switch (type) 
            {
                case GestureType.HandCloseOrOpen:
                    this.Interval = 1;
                    break;
                case GestureType.HeadMoved:
                    this.Interval = 1;
                    break;
                default:
                    this.Interval = 5;
                    break;
            }
            this.gestureParts = gestureParts;
            this.type = type;
            this._count = 0;
        }

        /// <summary>
        /// Occurs when [gesture recognised].
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        /// <summary>
        /// Write Log.
        /// </summary>
        public static String Logpath = "log.txt";

        /// <summary>
        /// Updates the gesture.
        /// </summary>
        /// <param name="data">The skeleton data.</param>
        public void UpdateGesture(Skeleton data, AllFramesReadyEventArgs e, GestureStatus gestureStatus)
        {
            if (!this.ShouldExecut())
                return;

            GesturePartResult result = this.gestureParts.CheckGesture(data, e, gestureStatus);

            if (result == GesturePartResult.Suceed)
            {
                if (this.GestureRecognized != null)
                {
                    // Success & send event
                    this.GestureRecognized(this, new GestureEventArgs(this.type, gestureStatus, data.TrackingId));

                    // Flush gesture status if a gesture recognized
                    if(this.type != GestureType.HandCloseOrOpen)
                        gestureStatus.GestureStatusFlush();
                }
            }
        }

        /// <summary>
        /// Logging something.
        /// </summary>
        public static void Log(string logMessage)
        {
            using (StreamWriter w = File.AppendText(Logpath))
            {
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  :");
                w.WriteLine("  :{0}\n", logMessage);
            }
        }
    }
}
