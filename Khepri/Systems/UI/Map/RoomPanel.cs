using Godot;
using Khepri.Rooms;
using System;
using System.Collections.Generic;

namespace Khepri.UI.Map
{
    /// <summary> Panel displaying a 3×3 spatial grid of the current room, one cell per <see cref="RoomPosition"/>. </summary>
    /// <remarks>
    /// Expected scene tree:
    /// <code>
    /// RoomPanel (this script)
    /// └── Grid : GridContainer   ← assign to _grid; set Columns = 3 in the editor
    /// </code>
    /// </remarks>
    public partial class RoomPanel : Control
    {
        /// <summary> The 3×3 container whose children are rebuilt on each <see cref="ForceUpdate"/>. </summary>
        /// <remarks> Set Columns = 3 in the Godot editor to produce the correct layout. </remarks>
        [ExportGroup("Nodes")]
        [Export] private GridContainer _grid = null!;


        /// <summary> Number of columns in the room grid — mirrors the <see cref="RoomPosition"/> enum's row-major layout. </summary>
        private const Int32 GridColumns = 3;


        /// <summary> Forces the panel to rebuild its 3×3 grid to reflect the current state of <paramref name="room"/>. </summary>
        /// <param name="room"> The room whose spatial layout and connections are to be rendered; must not be null. </param>
        public void ForceUpdate(Room room)
        {
            IReadOnlyCollection<Connection> connections = room.GetConnections();
            HashSet<RoomPosition> exitPositions = BuildExitPositionSet(connections, room);

            ClearGeneratedChildren(_grid);
            _grid.Columns = GridColumns;

            foreach (RoomPosition position in Enum.GetValues<RoomPosition>())
            {
                Int32 entityCount = room.GetEntities(position).Count;
                Boolean hasExit = exitPositions.Contains(position);

                Label cell = BuildCellLabel(position, entityCount, hasExit);
                _grid.AddChild(cell);
            }
        }


        /// <summary> Collects every <see cref="RoomPosition"/> within <paramref name="room"/> at which at least one connection attaches. </summary>
        /// <param name="connections"> All connections registered on the room. </param>
        /// <param name="room"> The room whose attachment positions are being resolved. </param>
        /// <returns> A set of positions that have at least one exit; never null, but may be empty. </returns>
        private static HashSet<RoomPosition> BuildExitPositionSet(IReadOnlyCollection<Connection> connections, Room room)
        {
            HashSet<RoomPosition> result = new HashSet<RoomPosition>();

            foreach (Connection connection in connections)
            {
                foreach (RoomPosition attachPosition in connection.GetPositions(room))
                {
                    result.Add(attachPosition);
                }
            }

            return result;
        }


        /// <summary> Constructs a <see cref="Label"/> that summarises one grid cell's contents. </summary>
        /// <param name="position"> The room position this cell represents. </param>
        /// <param name="entityCount"> Number of entities currently at <paramref name="position"/>. </param>
        /// <param name="hasExit"> Whether at least one connection attaches at <paramref name="position"/>. </param>
        /// <returns> A configured label ready to be added to the grid container. </returns>
        private static Label BuildCellLabel(RoomPosition position, Int32 entityCount, Boolean hasExit)
        {
            // TODO: Highlight the cell when the player entity occupies this position.
            //       Entities currently carry no distinguishing flag — add a player-marker
            //       component or compare against GameManager.Instance!.PlayerEntity once
            //       GetEntities(position) is exposed via RoomPanel's ForceUpdate signature.

            String exitMarker = hasExit ? "[exit]" : String.Empty;
            String entityMarker = entityCount > 0 ? $"×{entityCount}" : String.Empty;
            String separator = entityCount > 0 && hasExit ? " " : String.Empty;

            Label label = new Label();
            label.Text = $"{position}{System.Environment.NewLine}{entityMarker}{separator}{exitMarker}".TrimEnd();
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            label.CustomMinimumSize = new Godot.Vector2(60f, 60f);

            return label;
        }


        /// <summary>
        /// Detaches and frees all children that were added programmatically by a previous <see cref="ForceUpdate"/> call.
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
