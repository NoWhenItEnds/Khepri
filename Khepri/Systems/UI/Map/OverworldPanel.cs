using Godot;
using Khepri.Rooms;
using System;
using System.Collections.Generic;

namespace Khepri.UI.Map
{
    /// <summary> Panel displaying the game world as a node-link graph centred on the current room. </summary>
    public partial class OverworldPanel : Control
    {
        /// <summary> The prefab instantiated once per room marker. Must resolve to a <see cref="RoomNode"/> at its root. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _roomNodeScene = null!;


        /// <summary> Logical distance between a room and a directly-connected neighbour, before scaling to the panel. </summary>
        private const Single StepLength = 1f;

        /// <summary> Largest pixel distance allotted to one <see cref="StepLength"/>, so small graphs are not blown up to fill the panel. </summary>
        private const Single MaxPixelsPerStep = 130f;

        /// <summary> Pixel gap kept between the graph's outermost markers and the panel edge. </summary>
        private const Single Margin = 60f;

        /// <summary> Width of the drawn connection lines, in pixels. </summary>
        private const Single EdgeWidth = 3f;

        /// <summary> The colour of the drawn connection lines. </summary>
        private static readonly Color EdgeColor = new Color(1f, 1f, 1f, 0.35f);


        /// <summary> The layer holding edge lines; added first so it renders behind the room markers. </summary>
        private Control _edgeLayer = null!;

        /// <summary> The layer holding room markers; added second so it renders in front of the edges. </summary>
        private Control _roomLayer = null!;

        /// <summary> Pool of reusable room markers, parented to <see cref="_roomLayer"/>. </summary>
        private ScenePool<RoomNode> _roomPool = null!;

        /// <summary> Pool of reusable edge lines, parented to <see cref="_edgeLayer"/>. </summary>
        private ScenePool<Line2D> _edgePool = null!;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _edgeLayer = CreateFullRectLayer();
            _roomLayer = CreateFullRectLayer();

            _roomPool = new ScenePool<RoomNode>(_roomLayer, () => _roomNodeScene.Instantiate<RoomNode>());
            _edgePool = new ScenePool<Line2D>(_edgeLayer, CreateEdge);
        }


        /// <summary> Rebuilds the graph to reflect the world reachable from <paramref name="currentRoom"/>. </summary>
        /// <param name="currentRoom"> The player's current room; placed at the centre of the graph. Must not be null. </param>
        public void ForceUpdate(Room currentRoom)
        {
            Dictionary<Room, Vector2> layout = BuildLayout(currentRoom, out IReadOnlyList<(Room A, Room B)> edges);
            Dictionary<Room, Vector2> pixels = ProjectToPanel(layout);

            _edgePool.Begin();
            foreach ((Room a, Room b) in edges)
            {
                Line2D line = _edgePool.Acquire();
                line.Points = new Vector2[] { pixels[a], pixels[b] };
            }
            _edgePool.End();

            _roomPool.Begin();
            foreach (KeyValuePair<Room, Vector2> entry in pixels)
            {
                RoomNode node = _roomPool.Acquire();
                node.SetRoom(entry.Key, entry.Key.Equals(currentRoom));
                node.CenterOn(entry.Value);
            }
            _roomPool.End();
        }


        /// <summary> Traverses the world graph via BFS from <paramref name="root"/>, assigning each reachable room a logical position by accumulating the unit direction of the connection that first reached it. </summary>
        /// <param name="root"> The room placed at the origin; the graph's centre. </param>
        /// <param name="edges"> Receives the unique list of connections between reachable rooms, for edge rendering. </param>
        /// <returns> A map from each reachable room to its logical (pre-scaling) position; always contains <paramref name="root"/> at the origin. </returns>
        private static Dictionary<Room, Vector2> BuildLayout(Room root, out IReadOnlyList<(Room, Room)> edges)
        {
            Dictionary<Room, Vector2> positions = new Dictionary<Room, Vector2>();
            HashSet<(Guid, Guid)> seenEdges     = new HashSet<(Guid, Guid)>();
            List<(Room, Room)> edgeList         = new List<(Room, Room)>();
            Queue<Room> frontier                = new Queue<Room>();

            positions[root] = Vector2.Zero;
            frontier.Enqueue(root);

            while (frontier.Count > 0)
            {
                Room current = frontier.Dequeue();

                foreach (Connection connection in current.GetConnections())
                {
                    foreach (Room neighbour in connection.GetRooms())
                    {
                        if (neighbour.Equals(current))
                        {
                            continue;   // Self-connections carry no inter-room direction; skip them for layout.
                        }

                        (Guid, Guid) edgeKey = OrderEdgeKey(current.UId, neighbour.UId);
                        if (seenEdges.Add(edgeKey))
                        {
                            edgeList.Add((current, neighbour));
                        }

                        if (!positions.ContainsKey(neighbour))
                        {
                            RoomPosition direction = FirstPosition(connection, current);
                            positions[neighbour]   = positions[current] + (DirectionToVector(direction) * StepLength);
                            frontier.Enqueue(neighbour);
                        }
                    }
                }
            }

            edges = edgeList;
            return positions;
        }


        /// <summary> Maps each room's logical position into panel pixel coordinates, centred and uniformly scaled to fit within <see cref="Margin"/> of the edges. </summary>
        /// <param name="layout"> The logical positions produced by <see cref="BuildLayout"/>. </param>
        /// <returns> A map from each room to its pixel-space centre point within this panel. </returns>
        private Dictionary<Room, Vector2> ProjectToPanel(Dictionary<Room, Vector2> layout)
        {
            Vector2 min = new Vector2(Single.MaxValue, Single.MaxValue);
            Vector2 max = new Vector2(Single.MinValue, Single.MinValue);

            foreach (Vector2 position in layout.Values)
            {
                min = new Vector2(Mathf.Min(min.X, position.X), Mathf.Min(min.Y, position.Y));
                max = new Vector2(Mathf.Max(max.X, position.X), Mathf.Max(max.Y, position.Y));
            }

            Vector2 logicalCentre = (min + max) / 2f;
            Vector2 span          = max - min;

            // Pick the largest uniform scale that keeps every marker inside the margin on both axes, capped so a
            // tiny graph is not magnified to fill the panel. An axis with no spread imposes no limit.
            Single scaleX = span.X > Mathf.Epsilon ? (Size.X - (2f * Margin)) / span.X : Single.MaxValue;
            Single scaleY = span.Y > Mathf.Epsilon ? (Size.Y - (2f * Margin)) / span.Y : Single.MaxValue;
            Single scale  = Mathf.Min(Mathf.Min(scaleX, scaleY), MaxPixelsPerStep);

            Vector2 panelCentre = Size / 2f;

            Dictionary<Room, Vector2> pixels = new Dictionary<Room, Vector2>(layout.Count);
            foreach (KeyValuePair<Room, Vector2> entry in layout)
            {
                pixels[entry.Key] = panelCentre + ((entry.Value - logicalCentre) * scale);
            }

            return pixels;
        }


        /// <summary> Returns the position at which <paramref name="connection"/> attaches within <paramref name="room"/>, defaulting to <see cref="RoomPosition.Center"/> if none is found. </summary>
        /// <param name="connection"> The connection whose attachment direction is wanted. </param>
        /// <param name="room"> The room from whose perspective the direction is measured. </param>
        /// <returns> The first attachment position within <paramref name="room"/>. </returns>
        private static RoomPosition FirstPosition(Connection connection, Room room)
        {
            foreach (RoomPosition position in connection.GetPositions(room))
            {
                return position;
            }

            return RoomPosition.Center;
        }


        /// <summary> Maps a <see cref="RoomPosition"/> to a unit direction in panel space (north is up / negative Y). </summary>
        /// <param name="position"> The room position to convert. </param>
        /// <returns> A unit-length offset, or the zero vector for <see cref="RoomPosition.Center"/>. </returns>
        private static Vector2 DirectionToVector(RoomPosition position) => position switch
        {
            RoomPosition.NorthWest => new Vector2(-1f, -1f).Normalized(),
            RoomPosition.North     => new Vector2(0f, -1f),
            RoomPosition.NorthEast => new Vector2(1f, -1f).Normalized(),
            RoomPosition.West      => new Vector2(-1f, 0f),
            RoomPosition.Center    => Vector2.Zero,
            RoomPosition.East      => new Vector2(1f, 0f),
            RoomPosition.SouthWest => new Vector2(-1f, 1f).Normalized(),
            RoomPosition.South     => new Vector2(0f, 1f),
            RoomPosition.SouthEast => new Vector2(1f, 1f).Normalized(),
            _                      => Vector2.Zero,
        };


        /// <summary> Builds an order-independent key for a connection between two rooms, so each edge is recorded once. </summary>
        /// <param name="a"> One endpoint's identifier. </param>
        /// <param name="b"> The other endpoint's identifier. </param>
        /// <returns> A tuple of the two identifiers in ascending order. </returns>
        private static (Guid, Guid) OrderEdgeKey(Guid a, Guid b) => a.CompareTo(b) <= 0 ? (a, b) : (b, a);


        /// <summary> Creates a fresh, mouse-transparent <see cref="Line2D"/> for use as an edge. </summary>
        /// <returns> A configured line ready to have its two endpoints assigned. </returns>
        private static Line2D CreateEdge() => new Line2D
        {
            Width        = EdgeWidth,
            DefaultColor = EdgeColor,
            Antialiased  = true,
        };


        /// <summary> Creates a full-rect, mouse-transparent child <see cref="Control"/> to host one pool's nodes. </summary>
        /// <returns> The created layer, already added as a child of this panel. </returns>
        private Control CreateFullRectLayer()
        {
            Control layer = new Control();
            layer.SetAnchorsPreset(LayoutPreset.FullRect);
            layer.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(layer);
            return layer;
        }
    }
}
