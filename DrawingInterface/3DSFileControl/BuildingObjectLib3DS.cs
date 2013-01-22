using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.Util;
using lib3ds.Net;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using DrawingInterface.DrawingControl;
using SharpGL.SceneGraph;
using System.Xml;

namespace DrawingInterface._3DSFileControl
{
    /// <summary>
    /// Building object types.
    /// </summary>
    public enum BuildingObjectType
    {
        Building,
        Outside,
        Floor,
        Room,
        Object
    }
    /// <summary>
    /// BuildingObjectLib3DS model
    /// </summary>
    public class BuildingObjectLib3DS
    {
        /// <summary>
        /// The father node.
        /// </summary>
        public BuildingObjectLib3DS Father;
        /// <summary>
        /// The type of this building object (Building, Floor, Room etc..).
        /// </summary>
        private BuildingObjectType Type;
        /// <summary>
        /// List of child nodes.
        /// </summary>
        private Hashtable Childs;
        /// <summary>
        /// The location of this building object.
        /// </summary>
        private Vertex Coordinate;
        /// <summary>
        /// If this building part is an object, it will be assigned an id [object id].
        /// </summary>
        private uint Id;
        /// <summary>
        /// Mesh instance pointer
        /// </summary>
        public Lib3dsMeshInstanceNode Object
        {
            get;
            set;
        }
        // For All node
        /// <summary>
        /// Model information.
        /// </summary>
        public Lib3dsFile Model;
        /// <summary>
        /// Textures infromation.
        /// </summary>
        public Hashtable Textures;
        /// <summary>
        /// Faces' normalize(not use).
        /// </summary>
        public Lib3dsVertex[][] Normalizes;
        /// <summary>
        /// Create a new BuildingObjectLib3DS object.
        /// </summary>
        /// <param name="father">Father node</param>
        /// <param name="type">Type of this object</param>
        /// <param name="id">Id of this object</param>
        /// <param name="model">All models information</param>
        /// <param name="textures">All textures information</param>
        public BuildingObjectLib3DS(BuildingObjectLib3DS father, BuildingObjectType type, uint id,
            Lib3dsFile model, Hashtable textures)
        {
            this.Father = father;
            this.Type = type;
            this.Id = id;
            this.Model = model;
            this.Textures = textures;

            // Initialization
            this.Childs = new Hashtable();
            this.Coordinate = new Vertex();
        }
        /// <summary>
        /// Get list of children.
        /// </summary>
        /// <returns>Child list</returns>
        public Hashtable GetChilds()
        {
            return this.Childs;
        }
        /// <summary>
        /// Get this object's type.
        /// </summary>
        /// <returns>Object type</returns>
        public BuildingObjectType GetBuildingType()
        {
            return this.Type;
        }
        /// <summary>
        /// Get mesh by mesh name.
        /// </summary>
        /// <param name="name">Mesh name</param>
        /// <param name="meshes">All the instance of meshes</param>
        /// <returns>Mesh</returns>
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
        /// Insert a new child into this object.
        /// </summary>
        /// <param name="newObj">New child</param>
        public void AddNewChild(Lib3dsMeshInstanceNode newObj)
        {
            string[] newObjectLoca = null;

            // Return if location info is illegal
            if ((newObjectLoca = GetLocation(newObj.name)) == null)
                return;

            switch (this.Type)
            {
                case BuildingObjectType.Building:
                    if (this.Childs.Contains(newObjectLoca[0]) == false)
                    {
                        BuildingObjectType childType;
                        if (newObjectLoca[0] == "0")
                        {
                            childType = BuildingObjectType.Outside;
                        }
                        else
                        {
                            childType = BuildingObjectType.Floor;
                        }

                        BuildingObjectLib3DS bObj = new BuildingObjectLib3DS(this, childType,
                            (uint)System.Convert.ToInt32(newObjectLoca[0]), Model, Textures);
                        this.Childs.Add(newObjectLoca[0], bObj);
                        bObj.AddNewChild(newObj);
                    }
                    else
                    {
                        (this.Childs[(newObjectLoca[0])] as BuildingObjectLib3DS).AddNewChild(newObj);
                    }
                    break;
                case BuildingObjectType.Floor:
                    if (this.Childs.Contains(newObjectLoca[1]) == false)
                    {
                        BuildingObjectLib3DS bObj = new BuildingObjectLib3DS(this, BuildingObjectType.Room,
                            (uint)System.Convert.ToInt32(newObjectLoca[1]), Model, Textures);
                        this.Childs.Add(newObjectLoca[1], bObj);
                        bObj.AddNewChild(newObj);
                    }
                    else
                    {
                        (this.Childs[(newObjectLoca[1])] as BuildingObjectLib3DS).AddNewChild(newObj);
                    }
                    break;
                case BuildingObjectType.Room:
                    if (this.Childs.Contains(newObjectLoca[2]) == false)
                    {
                        BuildingObjectLib3DS bObj = new BuildingObjectLib3DS(this, BuildingObjectType.Object,
                            (uint)System.Convert.ToInt32(newObjectLoca[2]), Model, Textures);
                        this.Childs.Add(newObjectLoca[2], bObj);
                        bObj.AddNewChild(newObj);
                    }
                    else
                    {
                        // Same Name... ??? Do not insert it...
                    }
                    break;
                case BuildingObjectType.Outside:
                case BuildingObjectType.Object:
                    this.Object = newObj;
                    //if (this.Object.childs.Count == 0)
                    //{
                    //    Lib3dsMesh ThisMesh = FindLib3dsMeshByName(this.Object.name, this.Model.meshes);
                    //    if (ThisMesh != null)
                    //    {
                    //        this.Normalizes = new Lib3dsVertex[1][];
                    //        this.Normalizes[0] = new Lib3dsVertex[ThisMesh.faces.Count];
                    //        for (int i = 0; i < ThisMesh.faces.Count; i++)
                    //        {
                    //            Lib3dsFace face = ThisMesh.faces[i];
                    //            Lib3dsVertex Point1 = ThisMesh.vertices[face.index[0]];
                    //            Lib3dsVertex Point2 = ThisMesh.vertices[face.index[1]];
                    //            Lib3dsVertex Point3 = ThisMesh.vertices[face.index[2]];
                    //            this.Normalizes[0][i] = CreateNormalize(Point1, Point2, Point3);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    this.Normalizes = new Lib3dsVertex[newObj.childs.Count][];
                    //    int cnt = 0;
                    //    foreach (Lib3dsNode node in newObj.childs)
                    //    {
                    //        Lib3dsMesh ThisMesh = FindLib3dsMeshByName(node.name, this.Model.meshes);
                    //        if (ThisMesh == null)
                    //        {
                    //            continue;
                    //        }

                    //        this.Normalizes[cnt] = new Lib3dsVertex[ThisMesh.faces.Count];

                    //        for (int i = 0; i < ThisMesh.faces.Count; i++)
                    //        {
                    //            Lib3dsFace face = ThisMesh.faces[i];
                    //            Lib3dsVertex Point1 = ThisMesh.vertices[face.index[0]];
                    //            Lib3dsVertex Point2 = ThisMesh.vertices[face.index[1]];
                    //            Lib3dsVertex Point3 = ThisMesh.vertices[face.index[2]];
                    //            this.Normalizes[cnt][i] = CreateNormalize(Point1, Point2, Point3);
                    //        }
                    //        cnt++;
                    //    }
                    //}
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Calculate this object 's Location
        /// </summary>
        public void CalculateLocation()
        {
            // If this object just contain one mesh
            if (this.Type != BuildingObjectType.Outside &&
                this.Type != BuildingObjectType.Object)
            {
                foreach (BuildingObjectLib3DS child in Childs.Values)
                {
                    child.CalculateLocation();
                    this.Coordinate.x += child.Coordinate.x;
                    this.Coordinate.y += child.Coordinate.y;
                    this.Coordinate.z += child.Coordinate.z;
                }
                this.Coordinate.x /= Childs.Count;
                this.Coordinate.y /= Childs.Count;
                this.Coordinate.z /= Childs.Count;
            }
            // Maybe not just one mesh
            else
            {
                int allCount = 0;
                // If have children then calculate childre.
                if (Object.childs.Count != 0)
                {
                    foreach (Lib3dsNode node in Object.childs)
                    {
                        Lib3dsMesh mesh = FindLib3dsMeshByName(node.name, Model.meshes);
                        if (mesh != null)
                        {
                            foreach (Lib3dsVertex vertex in mesh.vertices)
                            {
                                this.Coordinate.x += vertex.x;
                                this.Coordinate.y += vertex.y;
                                this.Coordinate.z += vertex.z;
                                allCount++;
                            }
                        }
                    }
                }
                // If do not have children, then calculate itself.
                else
                {
                    Lib3dsMesh mesh = FindLib3dsMeshByName(Object.name,
                        Model.meshes);

                    if (mesh != null)
                    {
                        foreach (Lib3dsVertex vertex in mesh.vertices)
                        {
                            this.Coordinate.x += vertex.x;
                            this.Coordinate.y += vertex.y;
                            this.Coordinate.z += vertex.z;
                            allCount++;
                        }
                    }
                }
                
                this.Coordinate.x /= allCount;
                this.Coordinate.y /= allCount;
                this.Coordinate.z /= allCount;
            }
            this.Coordinate.x /= 20;
            this.Coordinate.y /= 20;
            this.Coordinate.z /= 20;
        }
        /// <summary>
        /// Caltulate the normalize line of a face.
        /// </summary>
        /// <param name="p1">Vertex 1</param>
        /// <param name="p2">Vertex 2</param>
        /// <param name="p3">Vertex 3</param>
        /// <returns>Normalized Vertex</returns>
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
        /// <summary>
        /// Get the location of this object.
        /// </summary>
        /// <returns>Object location coordinate</returns>
        public Vertex GetCoordinate()
        {
            return this.Coordinate;
        }
        /// <summary>
        /// Convert object name to location info.
        /// </summary>
        /// <param name="name">Object name</param>
        /// <returns>Location info</returns>
        public string[] GetLocation(String name)
        {
            String[] location;

            if (name == null)
                return null;
            location = name.Split('_');
            if (location == null || location.Length != 3)
                return null;

            return location;
        }
        /// <summary>
        /// Move focused (this object) to others. 
        /// </summary>
        /// <param name="move">Which direction</param>
        /// <param name="status">Status of eye or kinect..etc..</param>
        /// <returns>New focused object</returns>
        public BuildingObjectLib3DS Move(DrawingEnumTypes.Movement move, DrawingStatus status)
        {
            switch (move)
            {
                case DrawingEnumTypes.Movement.MenuIn:
                    if (this.Type == BuildingObjectType.Outside)
                    {
                        if (Father.Childs == null || Father.Childs.Count == 0)
                            return this;

                        foreach (BuildingObjectLib3DS value in Father.Childs.Values)
                        {
                            if (value.GetBuildingType() == BuildingObjectType.Outside)
                                continue;

                            return value;
                        }
                    }
                    else if (this.Type == BuildingObjectType.Building)
                    {
                        if(this.Childs.Contains("0"))
                            return (this.Childs["0"] as BuildingObjectLib3DS);
                        return this;
                    }
                    else if (this.Type == BuildingObjectType.Object)
                    {
                        return this;
                    }
                    else
                    {
                        foreach (BuildingObjectLib3DS value in this.Childs.Values)
                        {
                            if (value != null)
                                return value;
                        }
                        return this;
                    }
                    break;
                case DrawingEnumTypes.Movement.MenuOut:
                    if (this.Type == BuildingObjectType.Floor)
                    {
                        return Father.Childs["0"] as BuildingObjectLib3DS;
                    }
                    else if (this.Type == BuildingObjectType.Building) 
                    {
                        return this;
                    }
                    else
                    {
                        return this.Father;
                    }
                case DrawingEnumTypes.Movement.MoveDown:
                    if (this.Type == BuildingObjectType.Floor)
                    {
                        if (Father.Childs.Contains((Id - 1).ToString()))
                        {
                            return Father.Childs[(Id - 1).ToString()] as BuildingObjectLib3DS;
                        }
                    }
                    return this;
                case DrawingEnumTypes.Movement.MoveUp:
                    if (this.Type == BuildingObjectType.Floor)
                    {
                        if (Father.Childs.Contains((Id + 1).ToString()))
                        {
                            return Father.Childs[(Id + 1).ToString()] as BuildingObjectLib3DS;
                        }
                    }
                    return this;
                case DrawingEnumTypes.Movement.MoveIn:
                case DrawingEnumTypes.Movement.MoveOut:
                case DrawingEnumTypes.Movement.MoveRight:
                case DrawingEnumTypes.Movement.MoveLeft:
                    if (this.Type == BuildingObjectType.Room || this.Type == BuildingObjectType.Object)
                    {
                        return GetObjectByParallelMovement(move, status);
                    }
                    return this;
                default:
                    break;
            }
            return this;
        }
        /// <summary>
        /// Parallel move from focused object to others.
        /// </summary>
        /// <param name="move">Which direction</param>
        /// <param name="status">Status of eye or kinect..etc..</param>
        /// <returns>New focused object</returns>
        private BuildingObjectLib3DS GetObjectByParallelMovement(DrawingEnumTypes.Movement move, DrawingStatus status)
        {
            XmlTextWriter writer = null; 
            writer = new XmlTextWriter (Console.Out); 

            if (this.Father.Childs.Count <= 1)
                return this;

            int count = Father.Childs.Count - 1;
            BuildingObjectLib3DS rnt = null;

            double shita = Math.Atan((status.eye.x - this.Coordinate.x) / (status.eye.z + this.Coordinate.y));
            Matrix<double> matrix = new Matrix<double>(1, 2);
            matrix.SetValue(0); 
            matrix.Data[0, 0] = this.Coordinate.x;
            matrix.Data[0, 1] = -this.Coordinate.y;
            Matrix<double> _matrix = new Matrix<double>(2, 2);
            _matrix.SetValue(0);
            _matrix.Data[0, 0] = Math.Cos(shita);
            _matrix.Data[0, 1] = -Math.Sin(shita);
            _matrix.Data[1, 0] = Math.Sin(shita);
            _matrix.Data[1, 1] = Math.Cos(shita);

            Matrix<double> otherChilds = new Matrix<double>(1, 2);
            otherChilds.SetValue(0); 
            double MinIn = 99999.0f;
            double MinOut = 99999.0f;
            double MinLeft = 99999.0f;
            double MinRight = 99999.0f;
            foreach(BuildingObjectLib3DS ThisObj in Father.Childs.Values)
            {
                if(ThisObj.Equals(this))
                    continue;

                otherChilds.Data[0, 0] = ThisObj.Coordinate.x;
                otherChilds.Data[0, 1] = -ThisObj.Coordinate.y;
                otherChilds = (otherChilds - matrix) * _matrix;
                switch (move) 
                {
                    case DrawingEnumTypes.Movement.MoveIn:
                        if(otherChilds.Data[0, 1] < 0 &&
                            Math.Abs(otherChilds.Data[0, 1]) < MinIn)
                        {
                            rnt = ThisObj;
                            MinIn = Math.Abs(otherChilds.Data[0, 1]);
                        }
                        break;
                    case DrawingEnumTypes.Movement.MoveOut:
                        if (otherChilds.Data[0, 1] > 0 &&
                            Math.Abs(otherChilds.Data[0, 1]) < MinOut)
                        {
                            rnt = ThisObj;
                            MinOut = Math.Abs(otherChilds.Data[0, 1]);
                        }
                        break;
                    case DrawingEnumTypes.Movement.MoveLeft:
                        if (otherChilds.Data[0, 0] < 0 &&
                            Math.Abs(otherChilds.Data[0, 0]) < MinLeft)
                        {
                            rnt = ThisObj;
                            MinLeft = Math.Abs(otherChilds.Data[0, 0]);
                        }
                        break;
                    case DrawingEnumTypes.Movement.MoveRight:
                        if (otherChilds.Data[0, 0] > 0 &&
                            Math.Abs(otherChilds.Data[0, 0]) < MinRight)
                        {
                            rnt = ThisObj;
                            MinRight = Math.Abs(otherChilds.Data[0, 0]);
                        }
                        break;
                    default:
                        return this;
                }
            }

            // If we find the object
            if (rnt != null)
            { 
                return rnt;
            }

            // Do not find....
            return this;
        }
    }
}
