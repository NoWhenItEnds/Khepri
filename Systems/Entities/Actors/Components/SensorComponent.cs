using Godot;
using Khepri.Controllers;
using Khepri.Entities.Interfaces;
using Khepri.Entities.Items;
using Khepri.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Entities.Actors.Components
{
    /// <summary> The controller or root node of a unit's sensors. </summary>
    public partial class SensorComponent : Node3D
    {
        /// <summary> A reference to the brain's unit. </summary>
        [ExportGroup("Nodes")]
        [Export] private Unit _unit;

        /// <summary> Represents the unit's rendered line of sight. </summary>
        /// <remarks> This should only be used by a unit controlled by the player. </remarks>
        [Export] private Node3D _lights;


        /// <summary> How long, in hours, a unit should remember an entity before forgetting it if it hasn't been seen (in that period of time). </summary>
        [ExportGroup("Settings")]
        [Export] private Single _memoryLength = 6f;


        /// <summary> Whether the debug tools should be shown. </summary>
        [ExportGroup("Debug")]
        [Export] private Boolean _showDebug = false;


        /// <summary> The array of entities currently 'known' by the sensors. </summary>
        private HashSet<KnownEntity> _knownEntities = new HashSet<KnownEntity>();

        /// <summary> An array of positions recently visited by the unit. </summary>
        private HashSet<KnownPosition> _knownLocations = new HashSet<KnownPosition>();


        /// <summary> A reference to the global world controller. </summary>
        private WorldController _worldController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldController = WorldController.Instance;

            // Only render the lights if the unit is controlled by the player
            ToggleLights(PlayerController.Instance.PlayerUnit == _unit);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(double delta)
        {
            // Add the current position to the known positions.
            RememberPosition(GlobalPosition);

            // Remove entities that the unit hasn't seen for a while.
            _knownEntities.RemoveWhere(x => x.LastSeenTimestamp < _worldController.CurrentTime - TimeSpan.FromHours(_memoryLength));
            _knownLocations.RemoveWhere(x => x.LastSeenTimestamp < _worldController.CurrentTime - TimeSpan.FromHours(_memoryLength));

            // Render debug gizmos.
            if (_showDebug)
            {
                foreach (KnownEntity entity in _knownEntities)
                {
                    DebugDraw3D.DrawAabb(entity.Entity.CollisionShape.GetAabb(),
                        entity.IsVisible ? Colors.DarkViolet : Colors.MediumPurple);
                }
            }

            //  Update unit lights if they are shown.
            if (_lights.Visible)
            {
                _lights.GlobalRotationDegrees = new Vector3(0f, _unit.Direction * -1f - 90f, 0f);
            }
        }


        /// <summary> Adds an entity to the ones being tracked by the unit. Represents its memory. </summary>
        /// <param name="entity"> The object to begin tracking. </param>
        /// <returns> A reference to the newly remembered know entity. </returns>
        public KnownEntity RememberEntity(IEntity entity)
        {
            KnownEntity newEntity = new KnownEntity(entity);
            _knownEntities.Add(newEntity);
            return newEntity;
        }


        /// <summary> Stops tracking the given entity. </summary>
        /// <param name="entity"> A reference to the object to stop tracking. </param>
        /// <returns> Whether the object was removed. </returns>
        public Boolean ForgetEntity(IEntity entity)
        {
            return _knownEntities.RemoveWhere(x => x.Entity == entity) > 0;
        }


        /// <summary> Searches the tracked entity's for a particular entity. </summary>
        /// <param name="entity"> The entity to search for. </param>
        /// <returns> A reference to the tracked entity. A null means that one wasn't found. </returns>
        public KnownEntity? TryGetEntity(IEntity entity)
        {
            return _knownEntities.FirstOrDefault(x => x.Entity == entity);
        }


        /// <summary> Attempts to get a kind of item from the unit's memory. </summary>
        /// <param name="itemName"> The item's unique name or key. </param>
        /// <returns> An array of all instances of the item the unit is aware of. </returns>
        public KnownEntity[] TryGetItem(String itemName)
        {
            return _knownEntities.Where(x => x.Entity is Item item && item.Data.Name == itemName).ToArray();
        }


        /// <summary> Attempts to get a specific item using its unique id. </summary>
        /// <param name="uid"> The item's unique identifier. </param>
        /// <returns> A reference to the tracked entity. A null means that one wasn't found. </returns>
        public KnownEntity? TryGetItem(Guid uid)
        {
            return _knownEntities.Where(x => x.Entity is Item item && item.Data.UId == uid).FirstOrDefault();
        }


        /// <summary> Attempts to register the position as one it has visited recently. </summary>
        /// <param name="position"> The position in world space to register. </param>
        /// <returns> The known position data class representing this memory. </returns>
        public KnownPosition RememberPosition(Vector3 position)
        {
            Vector3 modifiedPosition = new Vector3((Single)Math.Round(position.X), (Single)Math.Round(position.Y), (Single)Math.Round(position.Z));
            KnownPosition? entity = _knownLocations.FirstOrDefault(x => x.Position == modifiedPosition);
            if (entity == null)
            {
                entity = new KnownPosition(modifiedPosition);
                Boolean isAdded = _knownLocations.Add(entity);
                if (isAdded)    // Reward the unit by discovering new areas by increasing their entertainment.
                {
                    _unit.Needs.UpdateEntertainment(0.1f);
                }
            }
            return entity;
        }


        /// <summary> Removes a given location from the unit's memory. </summary>
        /// <param name="position"> The position to remove. </param>
        /// <returns> Whether the position was removed or not. </returns>
        public Boolean ForgetPosition(Vector3 position)
        {
            Vector3 modifiedPosition = new Vector3((Single)Math.Round(position.X), (Single)Math.Round(position.Y), (Single)Math.Round(position.Z));
            return _knownLocations.RemoveWhere(x => x.Position == modifiedPosition) > 0;
        }


        /// <summary> Searches the tracked entity's for a particular location. </summary>
        /// <param name="position"> The location to search for. </param>
        /// <returns> A reference to the tracked location. A null means that one wasn't found. </returns>
        public KnownPosition? TryGetPosition(Vector3 position)
        {
            Vector3 modifiedPosition = new Vector3((Single)Math.Round(position.X), (Single)Math.Round(position.Y), (Single)Math.Round(position.Z));
            return _knownLocations.FirstOrDefault(x => x.Position == position);
        }


        /// <summary> Returns an array of positions that the unit knows. </summary>
        /// <returns> An array of positions in the game world that the unit is current aware of. </returns>
        public Vector3[] GetKnownPositions()
        {
            return _knownLocations.Select(x => x.Position).ToArray();
        }


        /// <summary> Returns an array of positions nearby the given position. </summary>
        /// <param name="position"> The position to search around. </param>
        /// <param name="radius"> The acceptable radius to search within. </param>
        /// <returns> An array of positions in the game world that the unit is current aware of. </returns>
        public Vector3[] GetKnownPositions(Vector3 position, Single radius)
        {
            return _knownLocations.Select(x => x.Position).Where(x => x.DistanceTo(position) <= radius).ToArray();
        }


        /// <summary> Toggle whether the lights are active. </summary>
        /// <param name="isActive"> Whether the visibility lights should be active or not. </param>
        public void ToggleLights(Boolean isActive) => _lights.Visible = isActive;
    }


    /// <summary> A data object that is 'known' by an agent. </summary>
    public class KnownEntity : IEquatable<KnownEntity>
    {
        /// <summary> A reference to the known entity. </summary>
        public IEntity Entity { get; init; }

        /// <summary> Whether the entity is visible. </summary>
        public Boolean IsVisible { get; private set; } = false;

        /// <summary> The last known position of the entity. </summary>
        public Vector3 LastKnownPosition { get; private set; }

        /// <summary> When the object was last seen. </summary>
        public DateTimeOffset LastSeenTimestamp { get; private set; }


        /// <summary> A reference to the global world controller. </summary>
        private WorldController _worldController;


        /// <summary> A data object that is 'known' by an agent. </summary>
        /// <param name="entity"> A reference to the known entity. </param>
        public KnownEntity(IEntity entity)
        {
            _worldController = WorldController.Instance;

            Entity = entity;
            LastKnownPosition = entity.WorldPosition;
            LastSeenTimestamp = _worldController.CurrentTime;
        }


        /// <summary> Set whether the entity is currently visible to the tracker. </summary>
        /// <param name="isVisible"> Whether the entity is directly visible. </param>
        public void SetIsVisible(Boolean isVisible)
        {
            IsVisible = isVisible;
        }


        /// <summary> Updates the last known position of the entity. </summary>
        /// <param name="position"> An optional position. A null will use the entity's actual position, a value will override it. </param>
        public void UpdateLastKnownPosition(Vector3? position = null)
        {
            LastSeenTimestamp = _worldController.CurrentTime;
            LastKnownPosition = position == null ? Entity.CollisionShape.GlobalPosition : position.Value;
        }

        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Entity);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            KnownEntity? other = obj as KnownEntity;
            return other != null ? Entity.Equals(other.Entity) : false;
        }


        /// <inheritdoc/>
        public Boolean Equals(KnownEntity other) => Entity.Equals(other.Entity);
    }


    /// <summary> A position that the unit knows about and has visited recently. </summary>
    public class KnownPosition : IEquatable<KnownPosition>
    {
        /// <summary> The position in godot's world space. </summary>
        public Vector3 Position { get; private set; }

        /// <summary> When the position was last visited. </summary>
        public DateTimeOffset LastSeenTimestamp { get; private set; }


        /// <summary> A reference to the global world controller. </summary>
        private WorldController _worldController;


        /// <summary> A position that the unit knows about and has visited recently. </summary>
        /// <param name="position"> The position in godot's world space. </param>
        public KnownPosition(Vector3 position)
        {
            _worldController = WorldController.Instance;

            Position = position;
            LastSeenTimestamp = _worldController.CurrentTime;
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Position);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            KnownPosition? other = obj as KnownPosition;
            return other != null ? Position.Equals(other.Position) : false;
        }


        /// <inheritdoc/>
        public bool Equals(KnownPosition other) => Position.Equals(other.Position);
    }
}
