using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DioramaExhibitionSupportSystem.GestureRecognition
{
    /// <summary>
    /// the gesture part result
    /// </summary>
    public enum GesturePartResult
    {
        /// <summary>
        /// Gesture part fail
        /// </summary>
        Fail,

        /// <summary>
        /// Gesture part suceed
        /// </summary>
        Suceed,

        /// <summary>
        /// Gesture part result undetermined
        /// </summary>
        Pausing
    }

    /// <summary>
    /// The gesture type
    /// </summary>
    public enum GestureType
    {
        None,
        MoveUp,
        MoveDown,
        MoveRight,
        MoveLeft,
        MoveIn,
        MoveOut,
        MenuIn,
        MenuOut,
        HandCloseOrOpen,
        HeadMoved
        //MakingAFast,
        //NotMakingAFast
    }
}
