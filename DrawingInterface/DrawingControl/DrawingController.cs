using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGL;
using DrawingInterface._3DSFileControl;
using System.Windows.Forms;
using DrawingInterface.DrawingControl;

namespace DrawingInterface.DrawingControl
{
    public class DrawingController
    {
        /// <summary>
        /// The list of all gestures we are currently looking for
        /// </summary>
        private bool _isStart = false;
        public ProjectorWin projectorWin;
        private _3DSLoaderByLib3DS loader;
        private _3DSLoaderByLib3DS loaderForFun;
        private _3DSDrawerByLib3DS drawer;
        private BuildingObjectLib3DS buildingModel;
        private BuildingObjectLib3DS modelForFun;
        /// <summary>
        /// Get and set is start rendering.
        /// </summary>
        public bool isStart
        {
            set { _isStart = value; }
            get { return _isStart; }
        }

        public DrawingController()
        {
            projectorWin = new ProjectorWin();
            projectorWin.status = new DrawingStatus();

            loader = new _3DSLoaderByLib3DS();
            loaderForFun = new _3DSLoaderByLib3DS();
            loader.OpenFile("..\\Model\\Untitled.3ds");
            loaderForFun.OpenFile("..\\Model\\miku.3ds");

            buildingModel = loader.CreateBuildingModel();
            buildingModel.CalculateLocation();
            modelForFun = loaderForFun.CreateBuildingModel();
            modelForFun.CalculateLocation();

            drawer = new _3DSDrawerByLib3DS();

            projectorWin.buildingModelCursor = buildingModel;
            projectorWin.buildingOutsideModel = buildingModel.GetChilds()["0"] as BuildingObjectLib3DS;
            projectorWin.modelForFun = modelForFun;
            projectorWin.drawer = drawer;
            projectorWin.Show();
        }

        public void SendSingle(DrawingEnumTypes.Movement cmd)
        {
            projectorWin.buildingModelCursor = projectorWin.buildingModelCursor.Move(cmd, projectorWin.status);
        }

        public void ChangeStatus(DrawingStatus new_status)
        {
            projectorWin.status = new_status;
        }
    }
}
