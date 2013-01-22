using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using lib3ds.Net;
using System.Collections;

namespace DrawingInterface._3DSFileControl
{
    /// <summary>
    /// Draw type
    /// </summary>
    public enum DrawType
    {
        Full,
        Face,
        WireFrame,
        Translucent
    }

    public class _3DSDrawerByLib3DS
    {
        /// <summary>
        /// Draw texture or not
        /// </summary>
        public bool IsDrawTexture
        {
            set;
            get;
        }
        /// <summary>
        /// To create a new instance
        /// </summary>
        public _3DSDrawerByLib3DS()
        {
            IsDrawTexture = true;
        }
        /// <summary>
        /// Draw a BuildingObjectLib3DS object.
        /// </summary>
        /// <param name="gl">Opengl handler.</param>
        /// <param name="buildingObj">The BuildingObjectLib3DS object.</param>
        public void DrawBuildingPart(OpenGL gl, BuildingObjectLib3DS buildingObj, DrawType type)
        {
            // Return when the input object is null
            if (buildingObj == null)
                return;

            // Draw all the children when type is Building, Floor or room. Or draw a Object.
            switch (buildingObj.GetBuildingType())
            {
                case BuildingObjectType.Building:
                case BuildingObjectType.Floor:
                case BuildingObjectType.Room:
                    foreach (BuildingObjectLib3DS child in buildingObj.GetChilds().Values)
                    {
                        DrawBuildingPart(gl, child, type);
                    }
                    break;
                case BuildingObjectType.Outside:
                case BuildingObjectType.Object:
                    DrawObject(gl, buildingObj, type);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Get a mesh object by mesh name.
        /// </summary>
        /// <param name="name">Mesh name.</param>
        /// <param name="meshes">List of all meshes.</param>
        private Lib3dsMesh FindLib3dsMeshByName(string name, List<Lib3dsMesh> meshes)
        {
            foreach (Lib3dsMesh mesh in meshes)
            {
                if (mesh.name.Equals(name))
                    return mesh;
            }

            return null;
        }
        /// <summary>
        /// Draw a object.
        /// </summary>
        /// <param name="gl">OpenGL handler.</param>
        /// <param name="buildingObj">The object.</param>
        private void DrawObject(OpenGL gl, BuildingObjectLib3DS buildingObj, DrawType type)
        {
            Lib3dsMeshInstanceNode thisMeshInst = buildingObj.Object;

            if (thisMeshInst.childs.Count == 0)
            {
                // Maybe this object only have one mesh.
                Lib3dsMesh thisMesh = FindLib3dsMeshByName(thisMeshInst.name, buildingObj.Model.meshes);
                DrawMesh(gl, thisMesh, buildingObj.Textures, buildingObj.Model.materials, type);
            }
            else
            {
                // Draw all the meshes in this object.
                foreach (Lib3dsNode node in thisMeshInst.childs)
                {
                    Lib3dsMesh thisMesh = FindLib3dsMeshByName(node.name, buildingObj.Model.meshes);
                    DrawMesh(gl, thisMesh, buildingObj.Textures, buildingObj.Model.materials, type);
                }
            }
        }
        /// <summary>
        /// Draw a mesh.
        /// </summary>
        /// <param name="gl">OpenGL handler.</param>
        /// <param name="buildingObj">The mesh.</param>BuildingObjectLib3DS _object,
        private void DrawMesh(OpenGL gl,  Lib3dsMesh thisMesh, Hashtable textures, List<Lib3dsMaterial> matrials, DrawType type)
        {
            if (thisMesh == null || thisMesh.nfaces == 0)
                return;

            // Draw all the faces in this mesh.
            for (int j = 0; j < thisMesh.faces.Count; j++)
            {
                Lib3dsFace thisFace = thisMesh.faces[j];
                float transparency = matrials[thisFace.material].transparency;
                //float[] fogColor = new float[4] { 0.5f, 0.5f, 0.5f, 1.0f };
                //float[] LightAmbient = new float[4] { 0.5f, 0.5f, 0.5f, 1.0f };
                //float[] LightDiffuse = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };
                //float[] LightPosition = new float[4] { 0.0f, 0.0f, 2.0f, 1.0f };

                switch (type)
                {
                    case DrawType.WireFrame:
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                        IsDrawTexture = false;
                        gl.Color(0.5f, 0.5f, 0.5f, 0.5f);
                        gl.LineWidth(1.5f);
                        break;
                    case DrawType.Full:
                        IsDrawTexture = BindTexture(gl, textures, matrials[thisFace.material].name);
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                        break;
                    case DrawType.Face:
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                        IsDrawTexture = false;
                        break;
                    case DrawType.Translucent:
                        IsDrawTexture = BindTexture(gl, textures, matrials[thisFace.material].name);
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                        gl.Enable(OpenGL.GL_BLEND);
                        gl.BlendFunc(OpenGL.GL_SRC_ALPHA,OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                        
                        //gl.Enable(OpenGL.GL_TEXTURE_2D);							// Enable Texture Mapping
                        //gl.ShadeModel(OpenGL.GL_SMOOTH);							// Enable Smooth Shading
                        //gl.ClearColor(0.5f,0.5f,0.5f,1.0f);					// We'll Clear To The Color Of The Fog
                        //gl.ClearDepth(1.0f);									// Depth Buffer Setup
                        //gl.Enable(OpenGL.GL_DEPTH_TEST);							// Enables Depth Testing
                        //gl.DepthFunc(OpenGL.GL_LEQUAL);								// The Type Of Depth Testing To Do
                        //gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);	// Really Nice Perspective Calculations

                        //gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, LightAmbient);		// Setup The Ambient Light
                        //gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, LightDiffuse);		// Setup The Diffuse Light
                        //gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION,LightPosition);	// Position The Light
                        //gl.Enable(OpenGL.GL_LIGHT1);	

                        //gl.Fog(OpenGL.GL_FOG_COLOR, fogColor);//设置雾颜色，f是一个指定颜色的数组float f[4]
                        //gl.Fog(OpenGL.GL_FOG_DENSITY, 0.85f); // 设置雾的密度
                        //gl.Hint(OpenGL.GL_FOG_HINT, OpenGL.GL_DONT_CARE); // 设置系统如何计算雾气
                        //gl.Fog(OpenGL.GL_FOG_START, 0.01f);//设置雾从多远开始
                        //gl.Fog(OpenGL.GL_FOG_END, 100.0f);//设置雾从多远结束
                        //gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_LINEAR);//设置使用哪种雾，共有三中雾化模式
                        //gl.Enable(OpenGL.GL_FOG);//打开雾效果

                        transparency = 0.2f;
                        break;
                    default:
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                        IsDrawTexture = false;
                        break;
                }
                if (type != DrawType.WireFrame)
                {
                    gl.Color(matrials[thisFace.material].diffuse[0], matrials[thisFace.material].diffuse[1],
                        matrials[thisFace.material].diffuse[2], matrials[thisFace.material].transparency);
                }

                gl.Begin(OpenGL.GL_TRIANGLES);

                for (int k = 0; k != 3; ++k)
                {
                    int index = thisFace.index[k];

                    if (IsDrawTexture)
                        gl.TexCoord(thisMesh.texcos[index].s, thisMesh.texcos[index].t);
                    gl.Vertex(thisMesh.vertices[index].x / 20, thisMesh.vertices[index].z / 20, -thisMesh.vertices[index].y / 20);
                }

                gl.End();
                if(type == DrawType.Translucent)
                   gl.Disable(OpenGL.GL_BLEND);
            }
        }
        /// <summary>
        /// Bind a texture
        /// </summary>
        /// <param name="gl">OpenGL control</param>
        /// <param name="textures">List of all textures</param>
        /// <param name="textureName">Texture name will be bind</param>
        private bool BindTexture(OpenGL gl, Hashtable textures, string textureName)
        {
            Texture thisTexture = null;

            if (textures.ContainsKey(textureName))
            {
                thisTexture = (Texture)textures[textureName];

                gl.MatrixMode(OpenGL.GL_TEXTURE);
                gl.LoadIdentity();

                thisTexture.Bind(gl);

                return true;
            }

            return false;
        }
    }
}
