using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using lib3ds.Net;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System.Collections;

namespace DrawingInterface._3DSFileControl
{
    /// <summary>
    /// Not use
    /// </summary>
    #region 3DS classes
    public class Vertex
    {
	    public float   x;
	    public float   y;
	    public float   z;
        public Vertex()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
        }
    };

    public class Face
    {
	    public ushort  []Index;
	    public ushort  MaterialPos;
	    public Vertex  Normal;
        public Face()
        {
            this.Index = new ushort[3];
            this.MaterialPos = 0;
            this.Normal = new Vertex();
        }
    };

    public class Chunk
    {
	    public ushort  ID;
	    public uint    Len;
        public Chunk ()
        {
            this.ID = 0;
            this.Len = 0;
        }
    };

    public class Material
    {
	    public String      name;
	    public float[]     ambientColor;
	    public float[]     diffuseColor;
	    public float[]     specularColor;
	    public float[]     emissiveColor;
	    public float       shininess;
	    public float       transparency;
        public Material()
        {
            this.name = "";
            this.ambientColor = new float[3];
            this. diffuseColor = new float[3];
	        this.specularColor = new float[3];
	        this.emissiveColor = new float[3];
	        this.shininess = 0;
	        this.transparency = 0;
        }
    };

    public class Object 
    {
	    public String          Name;
        public string[]        Location; 
	    public List<Vertex>    Vertexs;
	    public List<Face>      Faces;
        public Object()
        {
            this.Name = "";
            this.Vertexs = new List<Vertex>();
            this.Faces = new List<Face>();
        }
        public bool GetObjectLocation()
        {
            if (this.Name == null)
                return false;
            Location = this.Name.Split('_');
            if (Location == null || Location.Length < 3)
                return false;
            
            return true;
        }
    };

    public class Model 
    {
        public List<Object> MyObject;
        public List<Material> MyMaterial;
        public Model()
        {
            this.MyObject = new List<Object>();
            this.MyMaterial = new List<Material>();
        }
    };

    #endregion 3DS structs
    /// <summary>
    /// Not use
    /// </summary>
    class _3DSLoader
    {
        #region 3DS ID
        public const ushort PRIMARY = 0x4D4D;
        public const ushort MAINOBJECT = 0x3D3D;
        public const ushort EDITKEYFRAME = 0xB000;
        public const ushort MATERIAL = 0xAFFF;
        public const ushort OBJECT = 0x4000;
        public const ushort MATNAME = 0xA000;
        public const ushort OBJECT_MESH = 0x4100;
        public const ushort OBJECT_VERTICES = 0x4110;
        public const ushort OBJECT_FACES = 0x4120;
        public const ushort OBJECT_MATERIAL = 0x4130;
        public const ushort MAT_AMBIENT = 0xa010;
        public const ushort MAT_DIFFUSE = 0xa020;
        public const ushort MAT_SPECULAR = 0xa030;
        public const ushort MAT_SHININESS = 0xa040;
        public const ushort MAT_TRANSPARENCY = 0xa050;
        public const ushort INT_PERCENTAGE = 0x0030;
        public const ushort FLOAT_PERCENTAGE = 0x0031;
        public const ushort COLOR_F = 0x0010;
        public const ushort COLOR_24 = 0x0011;
        #endregion 3DS ID

	    private UInt32 NowPos;
	    private UInt32 FileLength;
	    private UInt32 Version;
	    private Model MyModel;
	    private FileStream FileReader;
        private StreamReader StreamReader;
        private byte[] buf;

        public _3DSLoader()
        {
            this.NowPos = 0;
            this.FileLength = 0;
            this.Version = 0;
            this.MyModel = new Model();
        }
        // Function		: open 3DS file.
        // Description	: use locale::global to make sure we can read file from the path.
        // Input		: 3DS file's path.
        // Output		: nothing.
        public void OpenFile(string FileRoad )
        {
            try 
            {
	            FileReader = new FileStream(FileRoad,FileMode.Open);
                StreamReader = new StreamReader(FileReader);
            }
	        catch (IOException)  
            {  
                Console.WriteLine("Read 3DS file error!");
                return;  
            } 
        }
        // Function		: close file reader.
        // Description	: close file reader.
        // Input		: nothing.
        // Output		: nothing.
        public void CloseFile()
        {
	        FileReader.Dispose();
        }
        // Function		: read 3DS file.
        // Description	: read information from 3DS file, all other functions work for it.
        // Input		: nothing.
        // Output		: nothing.
        public void LoadFile()
        {
            buf = new byte[FileReader.Length];
            if (FileReader.Read(buf, 0, (int)FileReader.Length) <= 0)
                throw new Exception("File read error.");

            Chunk MyChunk = new Chunk();
	        ReadChunk(ref MyChunk);
	        if(PRIMARY != MyChunk.ID)
		        throw new Exception("File format error.");
	        FileLength = MyChunk.Len;

	        ReadChunk(ref MyChunk);
	        ReadGLuint(ref Version);
	        while(NowPos < FileLength)
	        {
                ReadChunk(ref MyChunk);	
		        if(MAINOBJECT == MyChunk.ID)
                    LoadModel(ref MyChunk);
		        else
			        SkipNByte(MyChunk.Len - 6);
	        }

	        ComputeNormals();
        }
        // Function		: loading model.
        // Description	: loading information of a model.
        // Input		: chunk info of model
        // Output		: nothing
        public void LoadModel(ref Chunk MyChunk)
        {
	        UInt32 BeginPos = NowPos - 6;
	        Chunk TempChunk = new Chunk();
	        while(NowPos - BeginPos != MyChunk.Len)
	        {
		        ReadChunk(ref TempChunk);
		        switch(TempChunk.ID)
		        {
		        case OBJECT:
			        LoadObject(ref TempChunk);
			        break;

		        case MATERIAL:
			        LoadMaterial(ref TempChunk);
			        break;

		        default:
			        SkipNByte(TempChunk.Len - 6);
                    break;
		        }
	        }
        }
        // Function		: loading objct.
        // Description	: loading objct.
        // Input		: chunk info of the object.
        // Output		: nothing.
        private void LoadObject(ref Chunk MyChunk)
        {
	        Object _object = new Object();
	        _object.Name = ReadString();
            MyModel.MyObject.Add(_object);

	        Chunk ThisChunk = new Chunk();
	        UInt32 BeginPos = NowPos - 7 - (uint)_object.Name.Length;
	        while(NowPos - BeginPos != MyChunk.Len)
	        {
		        ReadChunk(ref ThisChunk);
		        if(OBJECT_MESH == ThisChunk.ID)
			        LoadMesh(ref ThisChunk);
		        else
			        SkipNByte(ThisChunk.Len - 6);
	        }
        }
        // Function		: loading mesh information.
        // Description	: loading mesh information.
        // Input		: chunk info of the mesh.
        // Output		: nothing.
        private void LoadMesh(ref Chunk MyChunk)
        {
	        Object _object = MyModel.MyObject[MyModel.MyObject.Count - 1];

	        uint BeginPos = NowPos - 6;
	        Chunk ThisChunk = new Chunk();
	        while(NowPos - BeginPos != MyChunk.Len)
	        {
		        ReadChunk(ref ThisChunk);
		        switch(ThisChunk.ID)
		        {
		        case OBJECT_VERTICES:  //vertice
			        LoadVertex(ref _object );
			        break;

		        case OBJECT_FACES:     //face
			        LoadFaces(ref _object );
			        break;

		        case OBJECT_MATERIAL:  //material
			        LoadObjectMaterial(ref _object );
			        break;

		        default:              //something we do not need
			        SkipNByte( ThisChunk.Len - 6 );
                    break;
		        }
	        }
        }
        // Function		: loading material information of object.
        // Description	: loading material information of object.
        // Input		: pointer of the object.
        // Output		: nothing.
        private void LoadObjectMaterial(ref Object _object)
        {
	        string Name = ReadString();
	        int Pos = -1;
	        for(int i = 0; i != MyModel.MyMaterial.Count; ++i)
	        {
		        if((MyModel.MyMaterial[i]).name == Name)
			        Pos = i;
	        }

	        if( Pos == -1 )
		        throw new Exception( "Search material failed." );

	        ushort Sum = 0; 
            ushort FacePos = 0;
	        ReadGLushort(ref Sum);
	        for(int i = 0; i < Sum; ++ i)
	        {
		        ReadGLushort(ref FacePos);
                (_object.Faces)[0].MaterialPos = 0;
		        Face A = _object.Faces[FacePos];
                A.MaterialPos = (ushort)Pos;
	        }
        }
        // Function		: loading material information.
        // Description	: loading material information.
        // Input		: the chunk of material.
        // Output		: nothing.
        private void LoadMaterial( ref Chunk MyChunk )
        {
	        Chunk TempChunk = new Chunk();
	        Material material = new Material();
	        uint BeginPos = NowPos - 6;

	        while(NowPos - BeginPos < MyChunk.Len)
	        {
		        ReadChunk(ref TempChunk);
		        switch(TempChunk.ID)
		        {
		        case MATNAME:                      //material name
			        material.name = ReadString();
			        break;
		        case MAT_AMBIENT:                  //material Ambient
			        LoadColor(ref material.ambientColor);
			        break;
		        case MAT_DIFFUSE:                  //material Diffuse
			        LoadColor(ref material.diffuseColor);
			        break;
		        case MAT_SPECULAR:                 //material Specular
			        LoadColor(ref material.specularColor);
			        break;
		        case MAT_SHININESS:                //material Shininess
			        LoadPercent(ref material.shininess);
			        break;
		        case MAT_TRANSPARENCY:             //material Transparency
			        LoadPercent(ref material.transparency);
			        break;
		        default:
			        SkipNByte(TempChunk.Len - 6);
                    break;
		        }
	        }
	        MyModel.MyMaterial.Add(material);
        }
        // Function		: loading color information.
        // Description	: loading color information.
        // Input		: the color pointer we should save to.
        // Output		: nothing.
        private void LoadColor(ref float[] color)
        {
	        Chunk TempChunk = new Chunk();
	        ReadChunk(ref TempChunk);
	        switch( TempChunk.ID )
	        {
	        case COLOR_F:
		        ReadGLfloat(ref color[ 0 ]);
		        ReadGLfloat(ref color[ 1 ]);
		        ReadGLfloat(ref color[ 2 ]);
		        break;
	        case COLOR_24:
		        byte Byte = 0;
		        for(int i = 0; i != 3; ++i)
		        {
			        ReadGLubyte(ref Byte);
			        color[ i ] = Byte / 255.0f;
		        }
		        break;
	        default:
		        SkipNByte(TempChunk.Len - 6);
                break;
	        }
        }
        // Function		: loading Percentage.
        // Description	: Shininess and transparency of the material is a percentage, 
        //				  so we need a function to load them.
        // Input		: where the percentage should be saved.
        // Output		: nothing.
        private void LoadPercent(ref float Temp)
        {
	        Chunk TempChunk = new Chunk();
	        ReadChunk(ref TempChunk);
	        switch(TempChunk.ID)
	        {
	        case INT_PERCENTAGE:    //Int Percentage
		        ushort Percent = 0;
		        ReadGLushort(ref Percent);
		        Temp = Percent / 100.0f;
		        break;
	        case FLOAT_PERCENTAGE:  //Float Percentage
		        ReadGLfloat(ref Temp);
		        break;
	        default:
		        SkipNByte(TempChunk.Len - 6);
                break;
	        }
        }
        // Function		: loading vector.
        // Description	: loading vector.
        // Input		: two vertices of a vector.
        // Output		: vector.
        private Vertex Vectors(Vertex lPoint, Vertex rPoint)
        {
	        Vertex Point = new Vertex();
	        Point.x = lPoint.x - rPoint.x;
	        Point.y = lPoint.y - rPoint.y;
	        Point.z = lPoint.z - rPoint.z;
	        return Point;
        }
        // Function		: calculate the cross product of two vectors
        // Description	: calculate the cross product of two vectors
        // Input		: two vectors.
        // Output		: vector.
        private Vertex Cross( Vertex lPoint, Vertex rPoint )
        {
	        Vertex Point = new Vertex();
	        Point.x = lPoint.y * rPoint.z - lPoint.z * rPoint.y;
	        Point.y = lPoint.z * rPoint.x - lPoint.x * rPoint.z;
	        Point.z = lPoint.x * rPoint.y - lPoint.y * rPoint.x;
	        return Point;
        }
        // Function		: unitise vector
        // Description	: unitise vector
        // Input		: vector
        // Output		: unitised vector
        private void Normalize( ref Vertex point )
        {
	        float Magnitude = (float)Math.Sqrt(point.x * point.x + point.y * point.y + point.z * point.z);
	        if(0 == Magnitude)
		        Magnitude = 1;
	        point.x /= Magnitude;				
	        point.y /= Magnitude;				
	        point.z /= Magnitude;											
        }
        // Function		: calculate normal vector for all planes.
        // Description	: calculate normal vector for all planes.
        // Input		: nothing.
        // Output		: nothing.
        private void ComputeNormals()
        {
	        for(int i = 0; i != MyModel.MyObject.Count; ++ i)
	        {
		        Object _object = MyModel.MyObject[i];
		        for(int j = 0; j != MyModel.MyObject[i].Faces.Count; ++j)
		        {
			        Face  face = _object.Faces[j];
			        Vertex Point1 = _object.Vertexs[face.Index[0]];
			        Vertex Point2 = _object.Vertexs[face.Index[1]];
			        Vertex Point3 = _object.Vertexs[face.Index[2]];

			        face.Normal = Cross(Vectors(Point1, Point3), Vectors(Point3, Point2));
			        Normalize(ref face.Normal);
		        }
	        }
        }
        // Function		: get the model we read.
        // Description	: get the model we read.
        // Input		: nothing.
        // Output		: the model.
        public Model GetModel()
        {
	        return MyModel;
        }

        // Function		: load all faces.
        // Description	: 3ds max face graphics are stored as triangular.
        // Input		: 
        // Output		:
        private void LoadFaces(ref Object ThisObject)
        {
	        ushort Sum = 0;
	        ReadGLushort(ref Sum);
            ushort Temp = 0;
	        for(int i = 0; i != Sum; ++i)
	        {
                Face face = new Face();
		        for(int j = 0; j != 4; ++j)
		        {
			        ReadGLushort(ref Temp);
			        if(j < 3)
				        face.Index[j] = Temp;
		        }
		        ThisObject.Faces.Add(face);
	        }
        }
        // Function		: load Vertex.
        // Description	: load Vertex.
        // Input		: 
        // Output		:
        private void LoadVertex(ref Object ThisObject )
        {
	        ushort Sum = 0;
	        ReadGLushort( ref Sum );
	        for( int i = 0; i != Sum; ++ i )
	        {
                Vertex Point = new Vertex();
		        ReadGLfloat( ref Point.x );
		        ReadGLfloat( ref Point.z );
		        ReadGLfloat( ref Point.y );
		        Point.z *= -1;
		        ThisObject.Vertexs.Add( Point );
	        }
            return;
        }

        private void ReadChunk(ref Chunk MyChunk )
        {
	        ReadGLushort( ref MyChunk.ID );
	        ReadGLuint( ref MyChunk.Len );
        }

        private void ReadGLubyte( ref byte Ubyte )
        {
            Ubyte = buf[NowPos];
	        NowPos += sizeof( byte );
            //Console.WriteLine("ReadGLubyte[" + sizeof(byte) + "]. now position:" + NowPos);
        }

        private void ReadGLushort( ref ushort Ushort )
        {
            Ushort = BitConverter.ToUInt16(buf, (int)NowPos);
            NowPos += sizeof(ushort);
            //Console.WriteLine("ReadGLushort[" + sizeof(ushort) + "]. now position:" + NowPos);
        }

        private void ReadGLuint(ref uint Uint)
        {
            Uint = BitConverter.ToUInt32(buf, (int)NowPos);
            NowPos += sizeof(uint);
            //Console.WriteLine("ReadGLuint[" +Uint + "]. now position:" + NowPos);
        }

        private void ReadGLfloat( ref float Float )
        {
            Float = BitConverter.ToSingle(buf, (int)NowPos);
            NowPos += sizeof(float);
            //Console.WriteLine("ReadGLfloat[" + Float + "]. now position:" + NowPos);
        }

        private string ReadString()
        {
            char alpha;
            string TempWord = "";
            int i = 0;
            while ((alpha = (char)buf[i + NowPos]) != '\0')
            {
                TempWord += alpha;
                i++;
            }
            NowPos += (uint)(TempWord.Length) + 1;
            Console.WriteLine("ReadString[" + TempWord + "]. now position:" + NowPos);
	        return TempWord;
        }

        private void SkipNByte( UInt32 Num )
        {
            FileReader.Seek(Num, SeekOrigin.Current);
	        NowPos += Num;
        }
    }
    /// <summary>
    /// 3DS file Loader
    /// </summary>
    public class _3DSLoaderByLib3DS
    {
        private Lib3dsFile MyModel;
        public class MyObject
        {
            public string[] Location;
            public Lib3dsVertex[] Normalizes;
            public bool Flag;

            public Lib3dsMesh node
            {
                set;
                get;
            }
            public Lib3dsFile model
            {
                set;
                get;
            }

            public MyObject(Lib3dsMesh _node, Lib3dsFile _model)
            {
                this.node = _node;
                this.model = _model;

                if (_node.faces == null || _node.faces.Count == 0)
                {
                    this.Flag = false;
                }
                else
                {
                    this.Flag = true;
                    this.Normalizes = new Lib3dsVertex[_node.faces.Count];

                    for (int i = 0; i < _node.faces.Count; i++)
                    {
                        Lib3dsFace face = _node.faces[i];
                        Lib3dsVertex Point1 = _node.vertices[face.index[0]];
                        Lib3dsVertex Point2 = _node.vertices[face.index[1]];
                        Lib3dsVertex Point3 = _node.vertices[face.index[2]];

                        this.Normalizes[i] = CreateNormalize(Point1, Point2, Point3);
                    }
                }
            }

            private Lib3dsVertex CreateNormalize(Lib3dsVertex p1, Lib3dsVertex p2, Lib3dsVertex p3)
            {
                Lib3dsVertex v1 = new Lib3dsVertex();
                Lib3dsVertex v2 = new Lib3dsVertex();
                Lib3dsVertex rnt = new Lib3dsVertex();

                v1.x = (p1.x - p3.x);
                v1.y = (p1.y - p3.y);
                v1.z = (p1.z - p3.z);

                v2.x = (p3.x - p2.x);
                v2.y = (p3.y - p2.y);
                v2.z = (p3.z - p2.z);

                rnt.x = v1.y * v2.z - v1.z * v2.y;
	            rnt.y = v1.z * v2.x - v1.x * v2.z;
	            rnt.z = v1.x * v2.y - v1.y * v2.x;

                return rnt;
            }

            public bool GetObjectLocation()
            {
                if (this.node.name == null)
                    return false;
                Location = this.node.name.Split('_');
                if (Location == null || Location.Length < 3)
                    return false;

                return true;
            }
        }

        // Function		: open 3DS file.
        // Description	: use locale::global to make sure we can read file from the path.
        // Input		: 3DS file's path.
        // Output		: nothing.
        public void OpenFile(string FileRoad)
        {
            OpenGL gl = new OpenGL();
            MyModel = LIB3DS.lib3ds_file_open(FileRoad);//モデルデータ読み込み
            if (MyModel == null)
            {
                Console.WriteLine("Read 3DS file error!");
                return; 
            }
        }

        public BuildingObjectLib3DS CreateBuildingModel()
        {
            BuildingObjectLib3DS Building;
            Hashtable Textures = new Hashtable();
            Lib3dsMesh test = new Lib3dsMesh();
            OpenGLControl openglCtr = new SharpGL.OpenGLControl();

            foreach (Lib3dsMaterial material in MyModel.materials)
            {
                Texture texture = new Texture();
                try
                {
                    texture.Create(openglCtr.OpenGL, "..\\Model\\" + material.name + ".jpg");
                    Textures.Add(material.name, texture);
                }
                catch
                {
                    // Do not find the texture file
                }
            }

            Building = new BuildingObjectLib3DS(null, BuildingObjectType.Building, 0,
                MyModel, Textures);

            foreach (Lib3dsMeshInstanceNode node in MyModel.nodes)
            {
                if (node.type != Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE || node.childs == null)
                    continue;
                Building.AddNewChild(node);
            }

            return Building;
        }
    }
}
