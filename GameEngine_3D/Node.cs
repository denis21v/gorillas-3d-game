////////////////////////////////////////////////////////////////////////////////
//                                                                            //
//      Submitted for the BSc in Computer Science for Games Development       //
//      Project code: SG7 (3D Gorillas)                                       //
//      By Denis Volosin                                                      //
//                                                                            //
//      Node.cs                                                               //
//                                                                            //
//      Abstract 3D scene node object                                         //
//                                                                            //
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using OpenTK;

namespace GameEngine_3D
{
    ///////////////////////////////////////////////////////////////////////////
    // Node class

    public class Node
    {
        ///////////////////////////////////////////////////////////////////////
        // Private class data

        string mName;                        // Node name
        Node mParent;                        // Parent node
        Transform mTransform;                // Node transform
        Dictionary<string, Node> mChildren;  // Children nodes
        bool mVisible;                       // Visibility flag


        ///////////////////////////////////////////////////////////////////////
        // Constructors

        // Standard node constructor
        public Node(string name)
        {
            mName = name;                 // Save node name
            mParent = null;               // No parent
            mTransform = new Transform(); // Default transform
            mChildren = null;             // No children by default
            mVisible = true;              // Visible by default
        }


        ///////////////////////////////////////////////////////////////////////
        // Properties

        // Access node name
        public string Name
        {
            // Read-only since names are used as static keys into
            // mChildren dictionaries
            get { return mName; }
        }

        // Access parent node
        public Node Parent
        {
            get { return mParent; }
            set { mParent = value; }
        }

        // Access scene node (root node)
        public Scene Scene
        {
            // Read-only
            get
            {
                // Climb up the node graph tree. Scene is always at the very
                // top as it the root node.
                Node node = this;
                while (node.Parent != null)
                    node = node.Parent;
                return (Scene)node;
            }
        }

        // Access node transform
        public Transform Transform
        {
            get { return mTransform; }
            set { mTransform = value; }
        }

        // Access children nodes
        public Dictionary<string, Node> Children
        {
            get { return mChildren; }
            set { mChildren = value; }
        }

        // Check if node has children nodes
        public bool HasChildren
        {
            get { return (mChildren != null); }
        }

        // Children node count
        public int ChildrenCount
        {
            get { return (mChildren != null) ? mChildren.Count : 0; }
        }

        // Access visibility flag
        public bool Visible
        {
            get { return mVisible; }
            set { mVisible = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        // Public methods

        // Add child node
        public void AddChild(Node node)
        {
            // Create children dictionary if not already present
            if (mChildren == null)
                mChildren = new Dictionary<string, Node>();

            // Add to the dictionary using node's name as a key
            mChildren.Add(node.Name, node);

            // Set ourselves as a parent of new child node
            node.Parent = this;
        }

        // Lookup child by name
        public Node GetChild(string name)
        {
            Node node = null;
            if (mChildren != null)
                mChildren.TryGetValue(name, out node);
            return node;
        }

        // Remove child node
        public void RemoveChild(string name)
        {
            if (mChildren != null)
                mChildren.Remove(name);
        }

        // Remove all children nodes
        public void RemoveAllChildren()
        {
            // Lose reference to children dictionary and let GC
            // do the tidying up
            mChildren = null;
        }

        // Frame render handler
        public virtual void OnRender(Matrix4 parentModelMatrix)
        {
            // Only render if node is visible
            if (mVisible)
            {
                // Render children nodes if any
                if (mChildren != null)
                {
                    // Calculate combined model (world) matrix that takes into
                    // account both parent and local node transforms
                    Matrix4 combinedModelMatrix = Transform.ModelMatrix * parentModelMatrix;

                    // Walk through children list and render each children node
                    foreach (KeyValuePair<string, Node> pair in mChildren)
                        pair.Value.OnRender(combinedModelMatrix);
                }
            }
        }
    }
}
