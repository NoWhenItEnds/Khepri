using Godot;
using Jaypen.Utilities.Pooling;
using Khepri.Rooms;
using System;
using System.Collections.Generic;

namespace Khepri.UI.Map
{
    /// <summary> Panel displaying the game world as a node-link graph centred on the current room. </summary>
    /// <remarks>
    /// Rooms are nodes and connections are edges. The world is non-Euclidean (rooms have no global coordinates),
    /// but every connection attaches at a <see cref="RoomPosition"/> that encodes the direction travelled to reach
    /// a neighbour. Starting from the current room at the origin, each reachable room is placed by accumulating the
    /// unit direction of the connection that first reached it. Neighbours that share an exit direction are fanned
    /// out angularly so their markers do not stack. Because a looping, non-Euclidean world cannot always be honoured
    /// on a flat plane, two rooms may still occasionally land near the same point — an accepted limitation.
    /// <para/>
    /// The graph renders at a fixed scale with the current room centred; the player can drag to pan and use the wheel
    /// to zoom. Room markers are <see cref="RoomNode"/> prefab instances and edges are <see cref="Line2D"/> nodes,
    /// both recycled through a <see cref="NodePool{T}"/> so a rebuild reuses nodes instead of re-instancing them.
    /// Edges sit on a layer behind the markers.
    /// <code>
    /// OverworldPanel (this script, a plain Control used as the drawing surface)
    /// </code>
    /// The edge and room layers, and the markers and edges within them, are created programmatically; no editor-side
    /// children are required beyond assigning the room-node prefab.
    /// </remarks>
    public partial class OverworldPanel : Control
    {
        /// <summary> The prefab instantiated once per room marker. Must resolve to a <see cref="RoomNode"/> at its root. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _roomNodeScene = null!;


        /// <summary> Logical distance between a room and a directly-connected neighbour, before scaling to the panel. </summary>
        private const Single StepLength = 1f;

        /// <summary> Pixels one <see cref="StepLength"/> spans at unit zoom. </summary>
        private const Single PixelsPerStep = 120f;

        /// <summary> Total angular spread, in radians, across which neighbours sharing one exit direction are fanned. </summary>
        private const Single FanSpread = Mathf.Pi / 2f;

        /// <summary> Smallest permitted zoom factor. </summary>
        private const Single MinZoom = 0.35f;

        /// <summary> Largest permitted zoom factor. </summary>
        private const Single MaxZoom = 3f;

        /// <summary> Multiplicative zoom change applied per wheel notch. </summary>
        private const Single ZoomStep = 1.1f;

        /// <summary> Width of the drawn connection lines, in pixels. </summary>
        private const Single EdgeWidth = 3f;

        /// <summary> The colour of the drawn connection lines. </summary>
        private static readonly Color EdgeColor = new Color(1f, 1f, 1f, 0.35f);


        /// <summary> The layer holding edge lines; added before any markers so it renders behind them. </summary>
        private Control _edgeLayer = null!;

        /// <summary> Pool of reusable room markers, parented directly to this panel (above <see cref="_edgeLayer"/>). </summary>
        private NodePool<RoomNode> _roomPool = null!;

        /// <summary> Pool of reusable edge lines, parented to <see cref="_edgeLayer"/>. </summary>
        private NodePool<Line2D> _edgePool = null!;

        /// <summary> Player-controlled pan, in pixels, added to the current room's centred position. Persists across rebuilds. </summary>
        private Vector2 _panOffset = Vector2.Zero;

        /// <summary> Player-controlled zoom factor multiplying <see cref="PixelsPerStep"/>. </summary>
        private Single _zoom = 1f;

        /// <summary> Whether a pan-drag is currently in progress. </summary>
        private Boolean _isPanning;

        /// <summary> The room the graph was last built for; used to skip rebuilding when nothing has changed. Null until the first build. </summary>
        private Room? _builtRoom;

        /// <summary> Logical (pre-scaling) positions of the rooms in the current build, reused by <see cref="Reproject"/>. </summary>
        private Dictionary<Room, Vector2> _logical = new Dictionary<Room, Vector2>();

        /// <summary> The active markers and the room each represents, so they can be repositioned without rebuilding. </summary>
        private readonly List<(RoomNode Node, Room Room)> _markers = new List<(RoomNode, Room)>();

        /// <summary> The active edge lines and the room pair each spans, so they can be repositioned without rebuilding. </summary>
        private readonly List<(Line2D Line, Room A, Room B)> _edges = new List<(Line2D, Room, Room)>();


        /// <summary> Raised when the player clicks a room marker, carrying the room they chose. </summary>
        public event Action<Room>? RoomSelected;


        /// <inheritdoc/>
        public override void _Ready()
        {
            // Mask the graph to the panel's rect so markers and edges panned out of view are clipped, not drawn over the rest of the UI.
            ClipContents = true;

            _edgeLayer = CreateFullRectLayer();

            // Markers are parented directly to the panel, so they are added after _edgeLayer and render in front of
            // the edges. Their Buttons stay clickable while a drag begun on empty panel space still pans (Godot keeps
            // feeding motion to the panel that captured the press). Each marker is subscribed once, at creation, so
            // reuse across rebuilds does not stack duplicate handlers.
            _roomPool = new NodePool<RoomNode>(this, CreateMarker);
            _edgePool = new NodePool<Line2D>(_edgeLayer, CreateEdge);
        }


        /// <inheritdoc/>
        public override void _GuiInput(InputEvent @event)
        {
            switch (@event)
            {
                case InputEventMouseButton { ButtonIndex: MouseButton.WheelUp, Pressed: true } wheelUp:
                    ZoomAt(wheelUp.Position, ZoomStep);
                    AcceptEvent();
                    break;

                case InputEventMouseButton { ButtonIndex: MouseButton.WheelDown, Pressed: true } wheelDown:
                    ZoomAt(wheelDown.Position, 1f / ZoomStep);
                    AcceptEvent();
                    break;

                case InputEventMouseButton { ButtonIndex: MouseButton.Left } leftButton:
                    _isPanning = leftButton.Pressed;
                    AcceptEvent();
                    break;

                case InputEventMouseMotion motion when _isPanning:
                    _panOffset += motion.Relative;
                    AcceptEvent();
                    break;
            }
        }


        /// <summary> Reflects the world reachable from <paramref name="currentRoom"/>, rebuilding the graph only when the room has changed and otherwise just repositioning the existing markers and edges. </summary>
        /// <remarks> Markers are rebuilt (and so briefly hidden) only on a room change; within a room they are merely repositioned, which keeps their Buttons interactive across frames. </remarks>
        /// <param name="currentRoom"> The player's current room; placed at the centre of the graph. Must not be null. </param>
        public void ForceUpdate(Room currentRoom)
        {
            if (!currentRoom.Equals(_builtRoom))
            {
                Rebuild(currentRoom);
            }

            Reproject();
        }


        /// <summary> Rebuilds the marker and edge nodes for <paramref name="currentRoom"/>, recycling them through the pools. </summary>
        /// <param name="currentRoom"> The room to centre the graph on. </param>
        private void Rebuild(Room currentRoom)
        {
            _logical = BuildLayout(currentRoom, out IReadOnlyList<(Room A, Room B)> edges);

            _edgePool.ReleaseAll();
            _edges.Clear();
            foreach ((Room a, Room b) in edges)
            {
                Line2D line = _edgePool.Acquire();
                _edges.Add((line, a, b));
            }

            _roomPool.ReleaseAll();
            _markers.Clear();
            foreach (KeyValuePair<Room, Vector2> entry in _logical)
            {
                RoomNode node = _roomPool.Acquire();
                node.SetRoom(entry.Key, entry.Key.Equals(currentRoom));
                _markers.Add((node, entry.Key));
            }

            _builtRoom = currentRoom;
        }


        /// <summary> Repositions the existing markers and edges for the current pan, zoom, and panel size, without touching the pools. </summary>
        private void Reproject()
        {
            Single scale   = PixelsPerStep * _zoom;
            Vector2 origin = (Size / 2f) + _panOffset;

            foreach ((RoomNode node, Room room) in _markers)
            {
                node.CenterOn(origin + (_logical[room] * scale));
            }

            foreach ((Line2D line, Room a, Room b) in _edges)
            {
                line.Points = new Vector2[] { origin + (_logical[a] * scale), origin + (_logical[b] * scale) };
            }
        }


        /// <summary>
        /// Traverses the world graph via BFS from <paramref name="root"/>, assigning each reachable room a logical
        /// position by accumulating the unit direction of the connection that first reached it. Neighbours of a room
        /// that share an exit direction are fanned out so their markers do not coincide.
        /// </summary>
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

                // Group this room's not-yet-placed neighbours by the exit direction reaching them, so several
                // neighbours sharing a direction can be fanned out instead of stacking on one point.
                Dictionary<RoomPosition, List<Room>> pending = new Dictionary<RoomPosition, List<Room>>();
                HashSet<Room> scheduled                      = new HashSet<Room>();

                foreach (Connection connection in current.GetConnections())
                {
                    foreach (Room neighbour in connection.GetRooms())
                    {
                        if (neighbour.Equals(current))
                        {
                            continue;   // Self-connections carry no inter-room direction; skip them for layout.
                        }

                        if (seenEdges.Add(OrderEdgeKey(current.UId, neighbour.UId)))
                        {
                            edgeList.Add((current, neighbour));
                        }

                        if (!positions.ContainsKey(neighbour) && scheduled.Add(neighbour))
                        {
                            RoomPosition direction = FirstPosition(connection, current);
                            if (!pending.TryGetValue(direction, out List<Room>? group))
                            {
                                group              = new List<Room>();
                                pending[direction] = group;
                            }
                            group.Add(neighbour);
                        }
                    }
                }

                foreach (KeyValuePair<RoomPosition, List<Room>> group in pending)
                {
                    IReadOnlyList<Vector2> offsets = FanOffsets(group.Key, group.Value.Count);
                    for (Int32 i = 0; i < group.Value.Count; i++)
                    {
                        Room neighbour       = group.Value[i];
                        positions[neighbour] = positions[current] + offsets[i];
                        frontier.Enqueue(neighbour);
                    }
                }
            }

            edges = edgeList;
            return positions;
        }


        /// <summary>
        /// Produces <paramref name="count"/> offset vectors of length <see cref="StepLength"/>, fanned across
        /// <see cref="FanSpread"/> radians centred on <paramref name="direction"/> so co-directional neighbours
        /// splay apart. A single neighbour gets the exact direction; <see cref="RoomPosition.Center"/> (which has
        /// no direction) is spread evenly around a full circle.
        /// </summary>
        /// <param name="direction"> The shared exit direction the neighbours were reached through. </param>
        /// <param name="count"> The number of neighbours to lay out; must be at least one. </param>
        /// <returns> One offset per neighbour, in the same order they should be assigned. </returns>
        private static IReadOnlyList<Vector2> FanOffsets(RoomPosition direction, Int32 count)
        {
            Vector2 baseDirection  = DirectionToVector(direction);
            List<Vector2> offsets  = new List<Vector2>(count);

            if (count == 1)
            {
                offsets.Add(baseDirection * StepLength);
                return offsets;
            }

            Boolean isCentre = baseDirection == Vector2.Zero;
            Single baseAngle = isCentre ? 0f : Mathf.Atan2(baseDirection.Y, baseDirection.X);

            for (Int32 i = 0; i < count; i++)
            {
                Single fraction = (Single)i / (count - 1);
                Single angle    = isCentre
                    ? Mathf.Tau * i / count
                    : baseAngle + Mathf.Lerp(-FanSpread / 2f, FanSpread / 2f, fraction);

                offsets.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * StepLength);
            }

            return offsets;
        }


        /// <summary> Adjusts the zoom by <paramref name="factor"/> while keeping the logical point under <paramref name="anchor"/> fixed on screen. </summary>
        /// <param name="anchor"> The screen point (panel-local) to zoom about, typically the cursor. </param>
        /// <param name="factor"> Multiplier applied to the current zoom before clamping. </param>
        private void ZoomAt(Vector2 anchor, Single factor)
        {
            Single oldScale = PixelsPerStep * _zoom;
            _zoom           = Mathf.Clamp(_zoom * factor, MinZoom, MaxZoom);
            Single newScale = PixelsPerStep * _zoom;

            Vector2 origin  = (Size / 2f) + _panOffset;
            Vector2 logical = (anchor - origin) / oldScale;
            _panOffset      = anchor - (logical * newScale) - (Size / 2f);
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


        /// <summary> Instantiates a room marker and subscribes to its selection event; used as the room pool's factory. </summary>
        /// <returns> A new marker wired to relay clicks through <see cref="RoomSelected"/>. </returns>
        private RoomNode CreateMarker()
        {
            RoomNode marker = _roomNodeScene.Instantiate<RoomNode>();
            marker.Selected += OnMarkerSelected;
            return marker;
        }


        /// <summary> Relays a marker's selection up through this panel's <see cref="RoomSelected"/> event. </summary>
        /// <param name="room"> The room whose marker was clicked. </param>
        private void OnMarkerSelected(Room room) => RoomSelected?.Invoke(room);


        /// <summary> Creates a fresh, antialiased <see cref="Line2D"/> for use as an edge. </summary>
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
