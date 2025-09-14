using Godot;
using System;
using System.Collections.Generic;

namespace Khepri.Navigation
{
    /// <summary> A point in our navigation graph. Represents an octant in the octree. </summary>
    public class Node
    {
        /// <summary> The internal id of the next node generated. </summary>
        private static Int32 _nextId;

        /// <summary> The unique identifier of this node. </summary>
        public readonly Int32 Id;

        /// <summary> All the edges this node is connected to. </summary>
        public List<Edge> Edges { get; private set; } = new List<Edge>();

        /// <summary> The actual cost from the start to the current node. Also known as 'G'. </summary>
        public Single ActualPathCost = 0f;

        /// <summary> The actual cost from the start to the current node. Also known as 'H'. </summary>
        public Single EstimatedPathRemainingCost = 0f;

        /// <summary> The estimated cost from the start node to the goal node. Also known as 'F'. </summary>
        public Single EstimatedPathTotalCost = 0f;

        /// <summary> The previous node we travelled from to reach this one. </summary>
        public Node PreviousNode;

        /// <summary> A reference to the octant this node represents in the graph. </summary>
        public Octant Octant;


        /// <summary> A point in our navigation graph. Represents an octant in the octree. </summary>
        /// <param name="octant"> A reference to the octant this node represents in the graph. </param>
        public Node(Octant octant)
        {
            Id = _nextId++;
            Octant = octant;
        }


        /// <inheritdoc/>
        public override Boolean Equals(Object obj) => obj is Node other && Id == other.Id;


        /// <inheritdoc/>
        public override Int32 GetHashCode() => Id.GetHashCode();
    }


    /// <summary> Connects one node to another. </summary>
    public class Edge
    {
        /// <summary> The first node this edge connects. </summary>
        public readonly Node NodeA;

        /// <summary> The second node this edge connects. </summary>
        public readonly Node NodeB;


        /// <summary> Connects one node to another. </summary>
        /// <param name="nodeA"> The first node this edge connects. </param>
        /// <param name="nodeB"> The second node this edge connects. </param>
        public Edge(Node nodeA, Node nodeB)
        {
            NodeA = nodeA;
            NodeB = nodeB;
        }


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            // It doesn't matter the order as long as an edge connects the same nodes.
            return obj is Edge other
                && ((NodeA == other.NodeA && NodeB == other.NodeB)
                    || (NodeA == other.NodeB && NodeB == other.NodeA));
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => NodeA.GetHashCode() ^ NodeB.GetHashCode();
    }

    public class Graph
    {
        /// <summary> All the nodes in the path keyed against the octant they represent. </summary>
        public readonly Dictionary<Octant, Node> Nodes = new Dictionary<Octant, Node>();

        /// <summary> All the edges in the graph. </summary>
        public readonly HashSet<Edge> Edges = new HashSet<Edge>();


        /// <summary> The final path through the nodes resulting from our calculations. </summary>
        private List<Node> _finalPath = new List<Node>();


        /// <summary> Add a node to the graph by way of its referenced octant. </summary>
        /// <param name="octant"> The octant to add to the graph. </param>
        public void AddNode(Octant octant)
        {
            if (!Nodes.ContainsKey(octant))
            {
                Nodes.Add(octant, new Node(octant));
            }
        }


        /// <summary> Connects two octants by forming an edge. </summary>
        /// <param name="octantA"> The first node to connect. </param>
        /// <param name="octantB"> The second node to connect. </param>
        public void AddEdge(Octant octantA, Octant octantB)
        {
            Node nodeA = FindNode(octantA);
            Node nodeB = FindNode(octantB);

            // Make sure that we find a node.
            if (nodeA != null && nodeB != null)
            {
                Edge edge = new Edge(nodeA, nodeB);
                if (Edges.Add(edge))
                {
                    nodeA.Edges.Add(edge);
                    nodeB.Edges.Add(edge);
                }
            }
        }


        /// <summary> Draw the navigation graph using debug tools. </summary>
        public void DrawGraph()
        {
            foreach (Edge edge in Edges)
            {
                DebugDraw3D.DrawLine(edge.NodeA.Octant.Bounds.GetCenter(), edge.NodeB.Octant.Bounds.GetCenter(), Colors.Maroon);
            }

            foreach (Node node in Nodes.Values)
            {
                DebugDraw3D.DrawSphere(node.Octant.Bounds.GetCenter(), 0.1f, Colors.DeepPink);
            }
        }


        /// <summary> Find the node associated with the given octant. </summary>
        /// <param name="octant"> The octant to search for. </param>
        /// <returns> The graph node keyed against the octant. A null means that one wasn't found. </returns>
        private Node? FindNode(Octant octant)
        {
            Nodes.TryGetValue(octant, out Node node);
            return node;
        }
    }
}
