using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace DioramaExhibitionSupportSystem.GestureRecognition.Segments
{
    class HandCloseOrOpen : IRelativeGestureSegment
    {
        /// <summary>
        /// For map skeleton point to depth image point.
        /// </summary>
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton, AllFramesReadyEventArgs e, GestureStatus gestureStatus)
        {
            // Right in front of Shoulders
            if (skeleton.Joints[JointType.HandRight].Position.Z > skeleton.Joints[JointType.ShoulderRight].Position.Z - 0.15)
            {
                return GesturePartResult.Pausing;
            }

            DepthImageFrame depthFrame = e.OpenDepthImageFrame();
            byte[] handBytes = null;

            if (depthFrame == null)
            {
                return GesturePartResult.Fail;
            }

            try
            {
                if (skeleton.Joints[JointType.HandRight].Position.X == 0 || skeleton.Joints[JointType.HandRight].Position.Y == 0)
                {
                    return GesturePartResult.Fail;
                }

                DepthImagePoint rightHandPoint = depthFrame.MapFromSkeletonPoint(skeleton.Joints[JointType.HandRight].Position);

                int intRightX = (int)rightHandPoint.X - (int)50;
                int intRightY = (int)rightHandPoint.Y - (int)60;

                Int32Rect rectRightHand = new Int32Rect(
                              (intRightX < 0) ? 0 : (intRightX + 100 >= depthFrame.Width) ? depthFrame.Width - 100 : intRightX,
                              (intRightY < 0) ? 0 : (intRightY + 100 >= depthFrame.Height) ? depthFrame.Height - 100 : intRightY,
                               100,
                               100
                             );

                handBytes = GenerateColoredBytes(depthFrame, rectRightHand, rightHandPoint.Depth);

                // Judge if making a fist and saving the starting / ending point
                // if 'no'->'yes' record the starting point
                // if 'yes'->'no' record the ending point

                // Initialize unmanged memory to hold the array. 
                try
                {
                    // Create image and show it
                    MIplImage source = new MIplImage();
                    IntPtr sourcePtr = CvInvoke.cvCreateImage(new System.Drawing.Size(rectRightHand.Width, rectRightHand.Height),
                        Emgu.CV.CvEnum.IPL_DEPTH.IPL_DEPTH_8U, 3);
                    source = (MIplImage)Marshal.PtrToStructure(sourcePtr, typeof(MIplImage));

                    // Copy the array to unmanaged memory.
                    Marshal.Copy(handBytes, 0, source.imageData, rectRightHand.Width * rectRightHand.Height * 3);
                    CvInvoke.cvShowImage("Source", sourcePtr);
                }
                finally
                {
                    // Free the unmanaged memory.
                } 

                if (isMakingAFist(handBytes, rectRightHand))
                {
                    if (!gestureStatus.isMakingAFast)
                    {
                        gestureStatus.startPoint.X = skeleton.Joints[JointType.HandRight].Position.X;
                        gestureStatus.startPoint.Y = skeleton.Joints[JointType.HandRight].Position.Y;
                        gestureStatus.startPoint.Z = skeleton.Joints[JointType.HandRight].Position.Z;
                    }
                    gestureStatus.isMakingAFast = true;
                }
                else
                {
                    if (gestureStatus.isMakingAFast)
                    {
                        gestureStatus.startPoint.X = skeleton.Joints[JointType.HandRight].Position.X;
                        gestureStatus.startPoint.Y = skeleton.Joints[JointType.HandRight].Position.Y;
                        gestureStatus.startPoint.Z = skeleton.Joints[JointType.HandRight].Position.Z;
                    }
                    gestureStatus.isMakingAFast = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("AfterConvert : " + ex.Message);
                depthFrame.Dispose();
                return GesturePartResult.Fail;
            }
            depthFrame.Dispose();
            //TODO
            return GesturePartResult.Suceed;
        }

        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame, Int32Rect rectRightHand, int DistanceToHand)
        {
            //get the raw data from kinect with the depth for every pixel
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            //use depthFrame to create the image to display on-screen
            //depthFrame contains color information for all pixels in image
            //Height * Width * 4(Red, Green, Blue, empth byte)
            byte[] pixels = new byte[rectRightHand.Height * rectRightHand.Width * 3];

            //Bgr32  - Blue, Green, Red, empth byte
            //Bgra32 - Blue, Green, Red, transparency
            //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparency

            //hardcoded locations to Blue, Green, Red(RGB) index positions

            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            int handDistanceMax = DistanceToHand + 40;
            int handDistancemin = DistanceToHand - 100;
            //loop through all distances
            //pick a RGB color based on distance
            for (int depthImageX = rectRightHand.X, depthImageY = rectRightHand.Y, colorIndex = 0;
                depthImageY < rectRightHand.Y + rectRightHand.Height
                && colorIndex < pixels.Length;
                colorIndex += 3, depthImageX++)
            {
                if (depthImageX == rectRightHand.X + rectRightHand.Width)
                {
                    if (depthImageY + 1 < rectRightHand.Y + rectRightHand.Height)
                    {
                        depthImageX = rectRightHand.X;
                        depthImageY++;
                    }
                    else
                    {
                        //loop end
                        break;
                    }
                }
                //get the player(requires skeleton tracking enabled for values)
                int player = rawDepthData[depthImageY * depthFrame.Width + depthImageX] & DepthImageFrame.PlayerIndexBitmask;

                //gets the depth value
                int depth = rawDepthData[depthImageY * depthFrame.Width + depthImageX] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (depth < handDistanceMax && depth > 0)
                {
                    pixels[colorIndex + BlueIndex] = 255;
                    pixels[colorIndex + GreenIndex] = 255;
                    pixels[colorIndex + RedIndex] = 255;

                }
                else
                {
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 0;
                }
            }

            return pixels;
        }

        /// <summary>
        /// Hey this is just the most important part that detects whether the hand is making a fist.
        /// </summary>
        /// <param name="imgHand">Hand image</param>
        /// <param name="rect">Hand rect</param>
        /// <returns>Is making a fist??</returns>
        bool isMakingAFist(byte[] imgHand, Int32Rect rect)
        {
            bool calcFingure = false;
            int BlackWidth = 0;
            int BlackTimes = 0;

            for (int yy = 0; yy < rect.Height; yy += 10)
            {
                for (int xx = 0; xx </*MaxX*/ rect.Width; xx += 1)
                {
                    if (imgHand[(yy * rect.Width + xx) * 3] == 255 &&
                        imgHand[(yy * rect.Width + xx) * 3 + 1] == 255 &&
                        imgHand[(yy * rect.Width + xx) * 3 + 2] == 255)
                    {
                        if (!calcFingure)
                        {
                            if (BlackWidth > 10 && BlackWidth < 15)
                            {
                                BlackTimes++;
                            }
                            BlackWidth = 0;
                        }
                        else
                        {
                            BlackWidth++;
                        }

                        calcFingure = true;
                    }
                    else
                    {
                        calcFingure = false;
                    }

                }
                if (BlackTimes > 3) { return false; }
            }
            //this.Title = BlackTime.ToString();
            return true;
        }
    }
}
