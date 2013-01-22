using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace DioramaExhibitionSupportSystem.GestureRecognition.Segments
{
    class HeadMoved : IRelativeGestureSegment
    {
        /// <summary>
        /// The accuracy for checking if head moved.
        /// </summary>
        const float accuracy = 0.0006f;
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton, AllFramesReadyEventArgs e, GestureStatus gestureStatus)
        {
            if ((skeleton.Joints[JointType.Head].Position.X - gestureStatus.headPoint.X) < accuracy &&
                (skeleton.Joints[JointType.Head].Position.X - gestureStatus.headPoint.X) > -accuracy &&
                (skeleton.Joints[JointType.Head].Position.Y - gestureStatus.headPoint.Y) < accuracy &&
                (skeleton.Joints[JointType.Head].Position.Y - gestureStatus.headPoint.Y) > -accuracy &&
                (skeleton.Joints[JointType.Head].Position.Z - gestureStatus.headPoint.Z) < accuracy &&
                (skeleton.Joints[JointType.Head].Position.Z - gestureStatus.headPoint.Z) > -accuracy)
            {
                // Head does not moved.
                return GesturePartResult.Fail;
            }
            else 
            {
                // Head moved, and new point will be update.
                gestureStatus.headPoint = skeleton.Joints[JointType.Head].Position;
                return GesturePartResult.Suceed;
            }
        }
    }
}