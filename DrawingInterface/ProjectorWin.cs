using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGL;
using DrawingInterface._3DSFileControl;
using DrawingInterface.DrawingControl;
using SharpGL.SceneGraph.Assets;

namespace DrawingInterface
{
    /// <summary>
    /// The draw form class.
    /// </summary>
    public partial class ProjectorWin : Form
    {
        /// <summary>
        /// The object for draw.
        /// </summary>
        public _3DSDrawerByLib3DS drawer;
        /// <summary>
        /// BuildingObjectLib3DS objects
        /// buildingModelCursor : the current object.
        /// rootBuildingModel   : all objects.
        /// </summary>
        public BuildingObjectLib3DS buildingModelCursor;
        public BuildingObjectLib3DS buildingOutsideModel;
        public BuildingObjectLib3DS modelForFun;
        /// <summary>
        /// Drawing status, Controlled by Main.
        /// </summary>
        public DrawingStatus status;
        /// <summary>
        /// buffer id for 双対レンダリング.
        /// </summary>
        private uint[] texture_name;
        private uint[] renderbuffer_name;
        private uint[] framebuffer_name;
        /// <summary>
        /// Texture size. Use screen size...
        /// </summary>
        private int TEXTURE_WIDTH, TEXTURE_HEIGHT;
        private float fovy;
        private int fps;
        /// <summary>
        /// ProjectiveTexture is used to projecte something.
        /// Projector contain modelviewMatrix and projMatrix
        /// </summary>
        ProjectiveTexture projTexture;
        Projector projector;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectorWin"/> class.
        /// </summary>
        public ProjectorWin()
        {
            // Initial something.
            this.drawer = null;
            this.buildingModelCursor = null;
            texture_name = new uint[1];
            renderbuffer_name = new uint[1];
            framebuffer_name = new uint[1];

            // Initial texture size.
            if (Screen.AllScreens.Count() >= 2)
            {
                this.TEXTURE_WIDTH = Screen.AllScreens[1].Bounds.Width;
                this.TEXTURE_HEIGHT = Screen.AllScreens[1].Bounds.Height;
            }
            else
            {
                this.TEXTURE_WIDTH = Screen.AllScreens[0].Bounds.Width;
                this.TEXTURE_HEIGHT = Screen.AllScreens[0].Bounds.Height;            
            }
            fovy = 45.0f;

            System.Timers.Timer t = new System.Timers.Timer(10000);
            t.Elapsed +=
                new System.Timers.ElapsedEventHandler(this.CalculateFPS);

            t.AutoReset = true;
            t.Enabled = true;
            fps = 0;

            // Initial form components.
            InitializeComponent();
        }
        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, PaintEventArgs e)
        {            
            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            status.eye.x = -0.589f;
            status.eye.y = 0.1325f;
            status.eye.z = 0.589f;
            fps++;

            // Calculate fovy
            fovy = CalculateFovy(status.eye.x + status.kinect.x, status.eye.y + status.kinect.y, status.eye.z + status.kinect.z);
            if (Single.IsNaN(fovy) || fovy >= 90.0f || fovy <= 0)
                fovy = 45.0f;

            // RenderTexWithFBOandDraw
            RenderTexWithFBOandDraw();

            // Clear the color and depth buffer.
            gl.ClearColor(0, 0, 0, 0);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);

            // Texture projector
            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(fovy, (double)TEXTURE_WIDTH / (double)TEXTURE_HEIGHT, 0.01f, 100.0);
            gl.LookAt(status.eye.x + status.kinect.x, status.eye.y + status.kinect.y, status.eye.z + status.kinect.z, buildingModelCursor.GetCoordinate().x,
                buildingModelCursor.GetCoordinate().y, buildingModelCursor.GetCoordinate().z, 0, 1, 0);
            //　ライトのビュー行列
            gl.GetFloat(OpenGL.GL_MODELVIEW_MATRIX, projector.modelviewMatrix);
            //　ライトの射影行列
            gl.GetFloat(OpenGL.GL_PROJECTION_MATRIX, projector.projMatrix);
            gl.PopMatrix();

            //　変換行列を送る＆テクスチャ設定
            projTexture.SetupMatrix(projector);
            projTexture.SetupTexture(texture_name);
            projTexture.BeginRender();

            //　視点位置を設定
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(22.5f, (double)Width / (double)Height, 0.01, 100.0);
            gl.LookAt(-0.589, 0.1325, 0.589, 0, 0.13275, 0, 0, 1, 0);

            //　建物モデルにテクスチャを投影
            drawer.IsDrawTexture = false;
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            drawer.DrawBuildingPart(gl, buildingOutsideModel, DrawType.Face);
            drawer.IsDrawTexture = true;

            projTexture.EndRender();

            gl.Flush();
        }
        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            // Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            // Initial texture projector.
            projTexture = new ProjectiveTexture(gl);
            projector = new Projector();

            // Initial something for creating texture.
            gl.GenTextures(1, texture_name);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, texture_name[0]);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_GENERATE_MIPMAP_HINT_SGIS, OpenGL.GL_TRUE); // automatic mipmap generation included in OpenGL v1.4
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D,
                0, (int)OpenGL.GL_RGBA8, TEXTURE_WIDTH, TEXTURE_HEIGHT, 0, OpenGL.GL_RGBA, OpenGL.GL_UNSIGNED_BYTE, null);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
            
            // InitFBO
            // create a framebuffer object, you need to delete them when program exits.
            gl.GenFramebuffersEXT(1, framebuffer_name);
            gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, framebuffer_name[0]);

            // create a renderbuffer object to store depth info
            // NOTE: A depth renderable image should be attached the FBO for depth test.
            // If we don't attach a depth renderable image to the FBO, then
            // the rendering output will be corrupted because of missing depth test.
            // If you also need stencil test for your rendering, then you must
            // attach additional image to the stencil attachement point, too.
            gl.GenRenderbuffersEXT(1, renderbuffer_name);
            gl.BindRenderbufferEXT(OpenGL.GL_RENDERBUFFER_EXT, renderbuffer_name[0]);
            gl.RenderbufferStorageEXT(OpenGL.GL_RENDERBUFFER_EXT, OpenGL.GL_DEPTH_COMPONENT, TEXTURE_WIDTH, TEXTURE_HEIGHT);
            gl.BindRenderbufferEXT(OpenGL.GL_RENDERBUFFER_EXT, 0);

            // attach a texture to FBO color attachement point
            gl.FramebufferTexture2DEXT(OpenGL.GL_FRAMEBUFFER_EXT,
                OpenGL.GL_COLOR_ATTACHMENT0_EXT, OpenGL.GL_TEXTURE_2D, texture_name[0], 0);

            // attach a renderbuffer to depth attachment point
            gl.FramebufferRenderbufferEXT(OpenGL.GL_FRAMEBUFFER_EXT,
                OpenGL.GL_DEPTH_ATTACHMENT_EXT, OpenGL.GL_RENDERBUFFER_EXT, renderbuffer_name[0]);

            gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, 0);

            // initial projection matrix
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            //  Create a perspective transformation.
            gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);

            //  Use the 'look at' helper function to position and aim the camera.
            gl.LookAt(0, 0, 10, 0, 0, 0, 0, 1, 0);

            //  Set the clear color.
            gl.ClearColor(0, 0, 0, 0);
        }
        /// <summary>
        /// Handles the Resized event of the openGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, EventArgs e)
        {
            //  TODO: Set the projection matrix here.

            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //  Set the projection matrix.
            gl.MatrixMode(OpenGL.GL_PROJECTION);

            //  Load the identity.
            gl.LoadIdentity();

            //gl.Ortho(0.0f, 250.0f * (double)Width / (double)Height, 0.0f, 250.0f, 1.0, -1.0);
            gl.Perspective(22.5f, (double)Width / (double)Height, 0.01, 100.0);

            //  Use the 'look at' helper function to position and aim the camera.
            gl.LookAt(-0.589, 0.1325, 0.589, 0, 0.13275, 0, 0, 1, 0);

            //  Set the modelview matrix.
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openGLControl_Load(object sender, EventArgs e)
        {
            if (Screen.AllScreens.Count() >= 2)
            {
                // Render in the second screen.
                this.Left = Screen.AllScreens[1].Bounds.Width + 1;
                this.Top = Screen.AllScreens[1].Bounds.Height;
                this.StartPosition = FormStartPosition.Manual;
                this.Location = Screen.AllScreens[1].Bounds.Location;
                Point p = new Point(Screen.AllScreens[1].Bounds.Location.X + 1, Screen.AllScreens[1].Bounds.Location.Y);
                this.Location = p;
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.None;
                this.TopMost = true;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                // Render int the first screen.
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }

            this.Show();
        }
        /// <summary>
        /// Render texture with FBO and draw it to memory.
        /// </summary>
        private void RenderTexWithFBOandDraw()
        {
            OpenGL gl = openGLControl.OpenGL;
            // render to texture 
            // adjust viewport and projection matrix to texture dimension
            gl.Viewport(0, 0, TEXTURE_WIDTH, TEXTURE_HEIGHT);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(fovy, (double)Width / (double)Height, 0.01f, 100.0);
            gl.LookAt(
                status.eye.x + status.kinect.x, 
                status.eye.y + status.kinect.y, 
                status.eye.z + status.kinect.z, 
                buildingModelCursor.GetCoordinate().x,
                buildingModelCursor.GetCoordinate().y, 
                buildingModelCursor.GetCoordinate().z, 
                0, 1, 0
                );

            // with FBO
            // set the rendering destination to FBO
            gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, framebuffer_name[0]);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            // clear buffer
            gl.ClearColor(0, 0, 0, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            // start to draw
            gl.PushMatrix();

            //  Load the identity matrix.
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            drawer.DrawBuildingPart(gl, buildingModelCursor, DrawType.Full);
            if (status != null && buildingModelCursor != null)
            {
                switch (buildingModelCursor.GetBuildingType())
                {
                    case BuildingObjectType.Floor:
                        drawer.DrawBuildingPart(gl, buildingOutsideModel, DrawType.WireFrame);
                        drawer.DrawBuildingPart(gl, buildingModelCursor, DrawType.Full);
                        break;
                    case BuildingObjectType.Object:
                        System.Collections.Hashtable childs = buildingModelCursor.Father.GetChilds();
                        foreach (System.Collections.DictionaryEntry childEntry in buildingModelCursor.Father.GetChilds())
                        {
                            BuildingObjectLib3DS child = childEntry.Value as BuildingObjectLib3DS;
                            if (child != buildingModelCursor)
                            {
                                drawer.DrawBuildingPart(gl,
                                    child,
                                    DrawType.WireFrame);
                            }
                            else
                            {
                                drawer.DrawBuildingPart(gl,
                                    child,
                                    DrawType.Full);
                            }
                        }
                        break;
                    case BuildingObjectType.Room:
                    // TODO
                    case BuildingObjectType.Outside:
                    case BuildingObjectType.Building:
                        drawer.DrawBuildingPart(gl, buildingModelCursor, DrawType.Full);
                        break;
                }
            }
            //drawer.DrawBuildingPart(gl, modelForFun, DrawType.Full);
            gl.PopMatrix();

            // back to normal window-system-provided framebuffer
            gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, 0); // unbind

            // trigger mipmaps generation explicitly
            // NOTE: If GL_GENERATE_MIPMAP is set to GL_TRUE, then glCopyTexSubImage2D()
            // triggers mipmap generation automatically. However, the texture attached
            // onto a FBO should generate mipmaps manually via glGenerateMipmapEXT().
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, texture_name[0]);
            gl.GenerateMipmapEXT(OpenGL.GL_TEXTURE_2D);
            //gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);

            // TODO: without FBO, maybe need
        }
        /// <summary>
        /// For test...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openGLControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'i':
                case 'I':
                    status.eye.z += 5.0f;
                    break;
                case 'o':
                case 'O':
                    status.eye.z -= 5.0f;
                    break;
                case 'a':
                case 'A':
                    status.eye.x += 5.0f;
                    break;
                case 'D':
                case 'd':
                    status.eye.x -= 5.0f;
                    break;
                case 'W':
                case 'w':
                    status.eye.y += 5.0f;
                    break;
                case 's':
                case 'S':
                    status.eye.y -= 5.0f;
                    break;
                case 'K':
                case 'k':
                    this.buildingModelCursor = buildingModelCursor.Move(DrawingEnumTypes.Movement.MenuOut, this.status);
                    break;
                case 'L':
                case 'l':
                    this.buildingModelCursor = buildingModelCursor.Move(DrawingEnumTypes.Movement.MenuIn, this.status);
                    break;
                case 'Y':
                case 'y':
                    this.buildingModelCursor = buildingModelCursor.Move(DrawingEnumTypes.Movement.MoveIn, this.status);
                    break;
                case 'H':
                case 'h':
                    this.buildingModelCursor = buildingModelCursor.Move(DrawingEnumTypes.Movement.MoveOut, this.status);
                    break;
                case 'G':
                case 'g':
                    this.buildingModelCursor = buildingModelCursor.Move(DrawingEnumTypes.Movement.MoveLeft, this.status);
                    break;
                case 'J':
                case 'j':
                    this.buildingModelCursor = buildingModelCursor.Move(DrawingEnumTypes.Movement.MoveRight, this.status);
                    break;
            }
        }
        private float CalculateFovy(float x, float y, float z)
        {
            float a = 0.75f;
            float b = (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z + 0.07, 2));
            float c = (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z - 0.45, 2));

            return (float)Math.Acos((Math.Pow(b, 2) + Math.Pow(c, 2) - Math.Pow(a, 2)) / (2 * c * b)) / (float)(Math.PI / 180);
        }

        private void CalculateFPS(object source, System.Timers.ElapsedEventArgs e)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText("I:\\Log.txt"))
            {
                w.WriteLine("{0}\t{1}", DateTime.Now.ToLongTimeString(),
                    fps);
                fps = 0;
            }
        }
    }
}
