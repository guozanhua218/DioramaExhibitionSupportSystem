using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrawingInterface.DrawingControl
{
    class Projector
    {
        public float[] modelviewMatrix
        { set; get; }
        public float[] projMatrix
        { set; get; }

        public Projector()
        {
            modelviewMatrix = new float[16];
            projMatrix = new float[16];
        }
    }
}
