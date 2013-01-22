using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace DioramaExhibitionSupportSystem.GestureRecognition.Segments
{
    class MoveIn : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton, AllFramesReadyEventArgs e, GestureStatus gestureStatus)
        {
            // Don't check if user is not making a fast
            if (!gestureStatus.isMakingAFast)
            {
                return GesturePartResult.Fail;
            }

            // Right in front of Shoulders
            if (skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z)
            {
                // Hands are higher then start point
                if (skeleton.Joints[JointType.HandRight].Position.Z < gestureStatus.startPoint.Z - 0.1)
                {
                    return GesturePartResult.Suceed;
                }

                return GesturePartResult.Fail;
            }

            return GesturePartResult.Fail;
        }
    }
}
