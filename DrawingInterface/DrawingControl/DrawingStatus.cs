using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DrawingInterface._3DSFileControl;

namespace DrawingInterface.DrawingControl
{
    public class DrawingStatus
    {
        /// <summary>
        /// The list of all gestures we are currently looking for
        /// </summary>
        private Vertex _eye, _projector, _kinect;

        /// <summary>
        /// Get and set eye location in Kinect Coordinate.
        /// </summary>
        public Vertex eye
        {
            set { _eye.x = value.x; _eye.y = value.y; _eye.z = value.z; }
            get { return _eye; }
        }

        /// <summary>
        /// Get and set projector location in world coordinate.
        /// </summary>
        public Vertex projector
        {
            set { _projector.x = value.x; _projector.y = value.y; _projector.z = value.z; }
            get { return _projector; }
        }

        /// <summary>
        /// Get and set kinect location at world coordinate.
        /// </summary>
        public Vertex kinect
        {
            set { _kinect.x = value.x; _kinect.y = value.y; _kinect.z = value.z; }
            get { return _kinect; }
        }

        public DrawingStatus()
        {
            _eye = new Vertex();
            _projector = new Vertex();
            _kinect = new Vertex();
        }
    }
}
