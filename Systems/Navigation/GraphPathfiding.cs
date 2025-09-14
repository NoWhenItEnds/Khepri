using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary> The estimated cost from the current node to the goal node. Also known as 'H'. </summary>
        public Single EstimatedPathRemainingCost = 0f;

        /// <summary> The estimated cost from the start node to the goal node. Also known as 'F'. </summary>
        public Single EstimatedPathTotalCost = 0f;

        /// <summary> The previous node we travelled from to reach this one. </summary>
        /// <remarks> A null means that this node is the root. </remarks>
        public Node? PreviousNode;

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
        /// <summary> How many nodes there are in the current path. </summary>
        public Int32 PathLength => _finalPath.Count;


        /// <summary> All the nodes in the path keyed against the octant they represent. </summary>
        public readonly Dictionary<Octant, Node> Nodes = new Dictionary<Octant, Node>();

        /// <summary> All the edges in the graph. </summary>
        public readonly HashSet<Edge> Edges = new HashSet<Edge>();


        /// <summary> How many iterations the navigation allows before canceling prematurely. </summary>
        private const Int32 MAX_ITERATIONS = 10000;


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
            Node? nodeA = FindNode(octantA);
            Node? nodeB = FindNode(octantB);

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


        /// <summary> Find a path through the graph. </summary>
        /// <param name="startOctant"> The beginning octant. </param>
        /// <param name="endOctant"> The destination octant. </param>
        /// <returns> Whether a path was found. </returns>
        /// <exception cref="ArgumentNullException"> The given nodes were not in the graph. </exception>
        /// <exception cref="OverflowException"> There were too many iterations without finding a path. </exception>
        public Boolean AStar(Octant startOctant, Octant endOctant)
        {
            _finalPath.Clear();
            Node? startNode = FindNode(startOctant);
            Node? endNode = FindNode(endOctant);

            if (startNode == null || endNode == null)
            {
                throw new ArgumentNullException("Unable to find either start or end node in the navigation graph!");
            }

            SortedSet<Node> openSet = new SortedSet<Node>(new NodeComparer());  // The nodes to investigate while building the route.
            HashSet<Node> closedSet = new HashSet<Node>();                      // The nodes that have already been visited.
            Int32 iterationCount = 0;

            startNode.ActualPathCost = 0f;
            startNode.EstimatedPathRemainingCost = CalculateHeuristic(startNode, endNode);
            startNode.EstimatedPathTotalCost = startNode.ActualPathCost + startNode.EstimatedPathRemainingCost;

            startNode.PreviousNode = null;
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                if (++iterationCount > MAX_ITERATIONS)
                {
                    throw new OverflowException("The navigation graph has performed too many iterations while building!");
                }

                // Remove the cheapest node from the set to investigate. This will be the first as it's sorted.
                Node current = openSet.First();
                openSet.Remove(current);

                if (current.Equals(endNode))    // If the next node is the end, then we're done!
                {
                    ReconstructPath(current);
                    return true;
                }

                closedSet.Add(current);
                foreach (Edge edge in current.Edges)
                {
                    Node neighbour = Equals(edge.NodeA, current) ? edge.NodeB : edge.NodeA; // Find the 'other' node this one connects to.

                    if (!closedSet.Contains(neighbour))
                    {
                        Single estimatedPathCost = current.ActualPathCost + CalculateHeuristic(current, neighbour);

                        if (estimatedPathCost < neighbour.ActualPathCost || !openSet.Contains(neighbour))
                        {
                            neighbour.ActualPathCost = estimatedPathCost;
                            neighbour.EstimatedPathRemainingCost = CalculateHeuristic(neighbour, endNode);
                            neighbour.EstimatedPathTotalCost = neighbour.ActualPathCost + neighbour.EstimatedPathRemainingCost;
                            neighbour.PreviousNode = current;
                            openSet.Add(neighbour);
                        }
                    }
                }
            }

            GD.Print("No path between the given nodes could be found.");
            return false;
        }


        /// <summary> Attempt to get the node at a given position along the path. </summary>
        /// <param name="index"> The index of the node to retrieve. </param>
        /// <returns> The octant associated with the index. </returns>
        public Octant GetPathNode(Int32 index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _finalPath.Count);
            return _finalPath[index].Octant;
        }


        /// <summary> Find the node associated with the given octant. </summary>
        /// <param name="octant"> The octant to search for. </param>
        /// <returns> The graph node keyed against the octant. A null means that one wasn't found. </returns>
        private Node? FindNode(Octant octant)
        {
            Nodes.TryGetValue(octant, out Node node);
            return node;
        }


        /// <summary> Build the final path through the graph. </summary>
        /// <param name="current"> The node to begin iterating back from. </param>
        private void ReconstructPath(Node current)
        {
            while (current != null)
            {
                _finalPath.Add(current);
                current = current.PreviousNode;
            }
            _finalPath.Reverse();
        }


        /// <summary> Calculate the heuristic between the two nodes. </summary>
        /// <param name="nodeA"> The first node. </param>
        /// <param name="nodeB"> The second node. </param>
        /// <returns> The heuristic. </returns>
        private Single CalculateHeuristic(Node nodeA, Node nodeB) => (nodeA.Octant.Bounds.GetCenter() - nodeB.Octant.Bounds.GetCenter()).LengthSquared();


        /// <summary> Allows us to order nodes by their total cost. </summary>
        private class NodeComparer : IComparer<Node>
        {
            /// <inheritdoc/>
            public Int32 Compare(Node x, Node y)
            {
                if (x == null || y == null) return 0;

                Int32 compare = x.EstimatedPathTotalCost.CompareTo(y.EstimatedPathTotalCost);
                if (compare == 0) { return x.Id.CompareTo(y.Id); }
                return compare;
            }
        }
    }
}
