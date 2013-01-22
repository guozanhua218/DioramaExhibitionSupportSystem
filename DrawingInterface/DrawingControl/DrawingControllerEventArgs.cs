using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrawingInterface.DrawingControl
{
    /// <summary>
    /// The gesture event arguments
    /// </summary>
    public class DrawingControllerEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GestureEventArgs"/> class.
        /// </summary>
        /// <param name="type">The gesture type.</param>
        /// <param name="trackingID">The tracking ID.</param>
        /// <param name="userID">The user ID.</param>
        public DrawingControllerEventArgs(DrawingStatus status)
        {
            this.DrawingStatus = status;
        }

        /// <summary>
        /// Gets or sets the type of the gesture.
        /// </summary>
        /// <value>
        /// The type of the gesture.
        /// </value>
        public DrawingStatus DrawingStatus
        {
            get;
            set;
        }
    }
}
