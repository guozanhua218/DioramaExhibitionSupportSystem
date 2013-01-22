using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrawingInterface._3DSFileControl
{

    /// <summary>
    /// the type of building
    /// </summary>
    public enum BuildingObjectType
    {
        Building,
        Outside,
        Floor,
        Room,
        Object
    }

    class BuildingObject
    {
        /// <summary>
        /// The father node.
        /// </summary>
        private BuildingObject Father;
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
        private Vertex Location;
        /// <summary>
        /// Id , to distingnish from other child.
        /// </summary>
        private uint Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingObject"/> class.
        /// </summary>
        /// <param name="father">The father node.</param>
        /// <param name="type">The type of this node.</param>
        public BuildingObject(BuildingObject father, BuildingObjectType type, uint id)
        {
            this.Father = father;
            this.Type = type;
            this.Childs = new Hashtable();
            this.Id = id;
        }

        /// <summary>
        /// Return all the child of this object.
        /// </summary>
        /// <returns>List of child</returns>
        public Hashtable GetChilds()
        {
            return this.Childs;
        }

        /// <summary>
        /// Get the type of this object.
        /// </summary>
        /// <returns>Type of object</returns>
        public BuildingObjectType GetBuildingType()
        {
            return this.Type;
        }

        /// <summary>
        /// Insert a new child into child list.[Not use] 
        /// </summary>
        /// <param name="child">New child</param>
        public void AddNewChild(Object child)
        {
            switch (this.Type)
            { 
                case BuildingObjectType.Outside:
                    this._object = child;
                    break;
                case BuildingObjectType.Building:
                    if (this.Childs.Contains(child.Location[0]) == false)
                    {
                        BuildingObjectType childType;
                        if(child.Location[0] == "0") 
                        {
                            childType = BuildingObjectType.Outside;
                        }
                        else
                        {
                            childType = BuildingObjectType.Floor;
                        }

                        BuildingObject bObj = new BuildingObject(this, childType, 
                            (uint)System.Convert.ToInt32(child.Location[0]));
                        this.Childs.Add(child.Location[0], bObj);
                        bObj.AddNewChild(child);
                    }
                    else
                    {
                        (this.Childs[(child.Location[0])] as BuildingObject).AddNewChild(child);
                    }
                    break;
                case BuildingObjectType.Floor:
                    if (this.Childs.Contains(child.Location[1]) == false)
                    {
                        BuildingObject bObj = new BuildingObject(this, BuildingObjectType.Room,
                            (uint)System.Convert.ToInt32(child.Location[1]));
                        this.Childs.Add(child.Location[1], bObj);
                        bObj.AddNewChild(child);
                    }
                    else
                    {
                        (this.Childs[(child.Location[1])] as BuildingObject).AddNewChild(child);
                    }
                    break;
                case BuildingObjectType.Room:
                    if (this.Childs.Contains(child.Location[2]) == false)
                    {
                        BuildingObject bObj = new BuildingObject(this, BuildingObjectType.Object,
                            (uint)System.Convert.ToInt32(child.Location[2]));
                        this.Childs.Add(child.Location[2], bObj);
                        bObj.AddNewChild(child);
                    }
                    else
                    {
                        // Same Name... ???
                    }
                    break;
                case BuildingObjectType.Object:
                    this._object = child;
                    break;
                default:
                    break;
            }
        }

        public void CalculateLocation()
        {
            if (this.Type != BuildingObjectType.Outside)
            {
                this.Location = new Vertex();
                foreach (BuildingObject child in Childs.Values)
                {
                    child.CalculateLocation();
                    this.Location.x += child.GetLocation().x;
                    this.Location.y += child.GetLocation().y;
                    this.Location.z += child.GetLocation().z;
                }
                this.Location.x /= Childs.Count;
                this.Location.y /= Childs.Count;
                this.Location.z /= Childs.Count;
            }
            else
            {
                int allCount = 0;
                this.Location = new Vertex();
                foreach (Vertex vertex in this._object.Vertexs)
                {
                    this.Location.x += vertex.x;
                    this.Location.y += vertex.y;
                    this.Location.z += vertex.z;
                    allCount++;
                }
                this.Location.x /= allCount;
                this.Location.y /= allCount;
                this.Location.z /= allCount;
            }
        }

        public Vertex GetLocation()
        {
            return this.Location;
        }
        
        public Object _object
        {
            get;
            set;
        }
    
    }
}
