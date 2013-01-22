using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace DioramaExhibitionSupportSystem.GestureRecognition
{
    /// <summary>
    /// The gesture status arguments
    /// </summary>
    public class GestureStatus
    {
        /// <summary>
        /// Initializes a new Gesture Status.
        /// </summary>
        public GestureStatus()
        {
            this.isMakingAFast = false;
            this.startPoint = new SkeletonPoint();
            this.endPoint = new SkeletonPoint();
            this.headPoint = new SkeletonPoint();
        }

        /// <summary>
        /// Gets or sets the isMakingAFast.
        /// </summary>
        /// <value>
        /// Is making a fast.
        /// </value>
        public bool isMakingAFast
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the starting / end point.
        /// </summary>
        /// <value>
        /// The starting / ending point.
        /// </value>
        public SkeletonPoint startPoint;
        public SkeletonPoint endPoint;
        public SkeletonPoint headPoint;

        /// <summary>
        /// Reflush Gesture Status.
        /// </summary>
        public void GestureStatusFlush()
        {
            this.isMakingAFast = false;
        }
    }
}
