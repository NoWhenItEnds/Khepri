using Godot;
using Khepri.Rooms;
using System;
using System.Collections.Generic;

namespace Khepri.UI.Map
{
    /// <summary> Panel displaying a breadth-first layout of the game world starting from the current room. </summary>
    /// <remarks> Because the world is non-Euclidean (rooms have no coordinates), BFS depth is used as a proxy for distance — each depth level becomes one vertical column of room markers. The current room (depth 0) is highlighted with a distinct colour. </remarks>
    /// <remarks>
    /// Expected scene tree:
    /// <code>
    /// OverworldPanel (this script)
    /// └── Columns : HBoxContainer   ← assign to _columns
    /// </code>
    /// Depth columns and room markers within them are built programmatically on each
    /// <see cref="ForceUpdate"/>; no further editor-side children are required.
    /// </remarks>
    public partial class OverworldPanel : Control
    {
        /// <summary> The outer horizontal container that holds one <see cref="VBoxContainer"/> per BFS depth level. All column nodes are generated programmatically and freed on each update. </summary>
        [ExportGroup("Nodes")]
        [Export] private HBoxContainer _columns = null!;


        /// <summary> Number of leading characters from a <see cref="Guid"/> shown as the room's short stable identifier. </summary>
        private const Int32 UIdPrefixLength = 8;

        /// <summary>
        /// The colour used to tint the current room's marker panel, distinguishing it at a glance.
        /// Expressed as a named colour constant so it can be updated without hunting for magic values.
        /// </summary>
        private static readonly Godot.Color CurrentRoomTint = new Godot.Color(0.2f, 0.6f, 1.0f, 1.0f);


        /// <summary> Forces the panel to rebuild the world layout via BFS from <paramref name="currentRoom"/>. Rooms with no connections are represented as a single column at depth 0. </summary>
        /// <param name="currentRoom"> The room the player is currently in; used as the BFS root. Must not be null. </param>
        public void ForceUpdate(Room currentRoom)
        {
            IReadOnlyList<IReadOnlyList<Room>> depthGroups = BuildBfsDepthGroups(currentRoom);

            ClearGeneratedChildren(_columns);

            for (Int32 depth = 0; depth < depthGroups.Count; depth++)
            {
                VBoxContainer column = BuildDepthColumn(depthGroups[depth], currentRoom, depth);
                _columns.AddChild(column);
            }
        }


        /// <summary>
        /// Traverses the world graph via BFS starting from <paramref name="root"/> and groups rooms by depth.
        /// The returned list index equals BFS depth: index 0 contains only <paramref name="root"/>,
        /// index 1 its immediate neighbours, and so on. Each room appears at most once.
        /// </summary>
        /// <param name="root"> The room at which traversal begins; placed at depth 0. </param>
        /// <returns>
        /// A list of depth groups, each group being an ordered list of rooms discovered at that depth.
        /// Never null; contains at least one group (the root itself).
        /// </returns>
        private static IReadOnlyList<IReadOnlyList<Room>> BuildBfsDepthGroups(Room root)
        {
            List<IReadOnlyList<Room>> result = new List<IReadOnlyList<Room>>();
            HashSet<Room> visited            = new HashSet<Room>();
            Queue<Room> frontier             = new Queue<Room>();

            visited.Add(root);
            frontier.Enqueue(root);

            while (frontier.Count > 0)
            {
                Int32 levelSize     = frontier.Count;
                List<Room> level    = new List<Room>();

                for (Int32 i = 0; i < levelSize; i++)
                {
                    Room current = frontier.Dequeue();
                    level.Add(current);

                    foreach (Connection connection in current.GetConnections())
                    {
                        foreach (Room neighbour in connection.GetRooms())
                        {
                            Boolean isNew = !visited.Contains(neighbour);
                            if (isNew)
                            {
                                visited.Add(neighbour);
                                frontier.Enqueue(neighbour);
                            }
                        }
                    }
                }

                result.Add(level.AsReadOnly());
            }

            return result.AsReadOnly();
        }


        /// <summary>
        /// Builds a vertical column node containing one marker panel per room in <paramref name="rooms"/>.
        /// </summary>
        /// <param name="rooms"> The rooms to render in this column, in BFS discovery order. </param>
        /// <param name="currentRoom"> The player's current room, used to apply the highlight tint. </param>
        /// <param name="depth"> BFS depth of this column, shown as a header label above the room markers. </param>
        /// <returns> A configured <see cref="VBoxContainer"/> ready to be added to <see cref="_columns"/>. </returns>
        private static VBoxContainer BuildDepthColumn(IReadOnlyList<Room> rooms, Room currentRoom, Int32 depth)
        {
            VBoxContainer column = new VBoxContainer();

            Label header = new Label();
            header.Text                = depth == 0 ? "Here" : $"+{depth}";
            header.HorizontalAlignment = HorizontalAlignment.Center;
            column.AddChild(header);

            foreach (Room room in rooms)
            {
                Panel marker = BuildRoomMarker(room, currentRoom);
                column.AddChild(marker);
            }

            return column;
        }


        /// <summary>
        /// Constructs a <see cref="Panel"/> node representing a single room on the overworld map.
        /// The panel contains a label showing the room's short UID prefix and connection count.
        /// The current room's panel is tinted with <see cref="CurrentRoomTint"/>.
        /// </summary>
        /// <param name="room"> The room to represent. </param>
        /// <param name="currentRoom"> The player's current room; equality determines whether the tint is applied. </param>
        /// <returns> A configured panel ready to be added to a depth column. </returns>
        private static Panel BuildRoomMarker(Room room, Room currentRoom)
        {
            // TODO: Replace the UID prefix with a human-readable display name once rooms store
            //       one at runtime. The prefab's ResourceName is the natural source — pass it
            //       through Room (e.g. via a NameFeature) when that data becomes available.

            Boolean isCurrent         = room.Equals(currentRoom);
            Int32 connectionCount     = room.GetConnections().Count;
            String shortId            = room.UId.ToString()[..UIdPrefixLength];

            Panel panel = new Panel();
            panel.CustomMinimumSize = new Godot.Vector2(90f, 50f);

            if (isCurrent)
            {
                panel.Modulate = CurrentRoomTint;
            }

            Label label = new Label();
            label.Text                = $"{shortId}{System.Environment.NewLine}exits: {connectionCount}";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.AutowrapMode        = TextServer.AutowrapMode.WordSmart;
            label.AnchorRight         = 1f;
            label.AnchorBottom        = 1f;

            panel.AddChild(label);

            return panel;
        }


        /// <summary>
        /// Detaches and frees all programmatically-generated children of <paramref name="container"/> to prevent
        /// stale or duplicate nodes accumulating across successive <see cref="ForceUpdate"/> calls.
        /// <see cref="Node.RemoveChild"/> is called before <see cref="Node.QueueFree"/> so the old nodes leave the
        /// scene tree in the same frame that new children are added, preventing transient double-occupancy and the
        /// Godot "node already has a sibling with this name" warning.
        /// </summary>
        /// <param name="container"> The container whose children are to be detached and freed. </param>
        private static void ClearGeneratedChildren(Node container)
        {
            foreach (Node child in container.GetChildren())
            {
                container.RemoveChild(child);
                child.QueueFree();
            }
        }
    }
}
