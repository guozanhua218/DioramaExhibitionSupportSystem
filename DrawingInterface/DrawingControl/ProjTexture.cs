using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGL;
using SharpGL.SceneGraph.Assets;

namespace DrawingInterface.DrawingControl
{
    /********************************************************
    Usage Instruction:
           //in init()
           glGenTextures(1, &id);
           glBindTexture(GL_TEXTURE_2D, id);
           glTexImage2D(GL_TEXTURE_2D, ......, texImage);
 
           ProjectiveTexture lightmap;
           lightmap.SetupTexture(id);
           lightmap.SetupMatrix(Camera* lightCam);
 
           //in the render pipe loop...
           lightmap.SetupMatrix(lightCam);
           lightmap.BeginRender();
           draw scene...
           lightmap.EndRender();
 
    ********************************************************/
    class ProjectiveTexture
    {
        private uint texture;
        private float  []matrix;
        private OpenGL gl;

        public ProjectiveTexture(OpenGL gl)
        {
            // TODO: Complete member initialization
            this.gl = gl;
            matrix = new float[16];
        }

        public void SetupTexture(uint []texture_name)
        {
            texture = texture_name[0];

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, texture_name[0]);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_R, OpenGL.GL_CLAMP);
            //gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            //gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            //gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            //glTexEnvi(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_MODULATE);
        }
 
        //lightCam是我们定义的一个Camera类，此处代表Projector
        public void SetupMatrix(Projector proj)
        {
            gl.MatrixMode(OpenGL.GL_TEXTURE_MATRIX);
            gl.LoadIdentity();

            gl.PushMatrix();
            float[] biasMatrix = { 
                0.5f, 0.0f, 0.0f, 0.0f,
                0.0f, 0.5f, 0.0f, 0.0f,
                0.0f, 0.0f, 0.5f, 0.0f,
                0.5f, 0.5f, 0.5f, 1.0f 
            };
            //获得Projector的模型视图矩阵，用于把world space的顶点转换到projector space
            //获得Projector的投影矩阵，用于把projector space的顶点转换到projector clip space
  
            gl.LoadMatrixf(biasMatrix);

            gl.MultMatrix(proj.projMatrix);
            gl.MultMatrix(proj.modelviewMatrix);
 
            gl.GetFloat(OpenGL.GL_CURRENT_MATRIX_ARB, matrix);          //获得纹理矩阵
  
            gl.PopMatrix();
        }

        public void BeginRender()
        {
            float []planeS = { 1.0f, 0.0f, 0.0f, 0.0f };
            float []planeT = { 0.0f, 1.0f, 0.0f, 0.0f };
            float []planeR = { 0.0f, 0.0f, 1.0f, 0.0f };
            float []planeQ = { 0.0f, 0.0f, 0.0f, 1.0f };

            gl.TexGen(OpenGL.GL_S, OpenGL.GL_TEXTURE_GEN_MODE, OpenGL.GL_EYE_LINEAR);
            gl.TexGen(OpenGL.GL_T, OpenGL.GL_TEXTURE_GEN_MODE, OpenGL.GL_EYE_LINEAR);
            gl.TexGen(OpenGL.GL_R, OpenGL.GL_TEXTURE_GEN_MODE, OpenGL.GL_EYE_LINEAR);
            gl.TexGen(OpenGL.GL_Q, OpenGL.GL_TEXTURE_GEN_MODE, OpenGL.GL_EYE_LINEAR);

            gl.TexGen(OpenGL.GL_S, OpenGL.GL_EYE_PLANE, planeS);
            gl.TexGen(OpenGL.GL_T, OpenGL.GL_EYE_PLANE, planeT);
            gl.TexGen(OpenGL.GL_R, OpenGL.GL_EYE_PLANE, planeR);
            gl.TexGen(OpenGL.GL_Q, OpenGL.GL_EYE_PLANE, planeQ);

            gl.Enable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_T);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_R);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_Q);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.LoadMatrixf(matrix);

            //渲染管线就像流水线，顶点是我们的操作对象，何时把相关的操作传入渲染管线，
            //何时把不必要的操作卸下是我们该考虑的。物体顶点坐标应该是在模型视图矩阵
            //（GL_MODELVIEW）转换到世界坐标，然后进入纹理矩阵模式下求出纹理坐标
                     
        }

        public void EndRender()
        {
            gl.Disable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Disable(OpenGL.GL_TEXTURE_GEN_T);
            gl.Disable(OpenGL.GL_TEXTURE_GEN_R);
            gl.Disable(OpenGL.GL_TEXTURE_GEN_Q);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
        }
    }
}
