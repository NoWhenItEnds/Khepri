using Godot;
using Khepri.Entities.Interfaces;
using System;
using System.Collections.Generic;

namespace Khepri.Entities.Actors.Components.Sensors
{
    /// <summary> A sensor used to find visual information about the surrounding environment. </summary>
    public partial class SightSensor : Node3D
    {
        /// <summary> A reference to the unit sensor parent. Contains the unit's persistent memory. </summary>
        [ExportGroup("Nodes")]
        [Export] private SensorComponent _sensors;

        /// <summary> The area representing the observer's field of view. Items in here are potentially visible. </summary>
        [Export] private Area3D _fieldOfViewArea;

        /// <summary> A ray-cast to check if an objective is visible or being occluded. </summary>
        [Export] private RayCast3D _lineOfSightRayCast;


        /// <summary> Whether the debug tools should be shown. </summary>
        [ExportGroup("Debug")]
        [Export] private Boolean _showDebug = false;


        /// <summary> The array of entities that are potentially visible. </summary>
        private HashSet<IEntity> _nearbyEntities = new HashSet<IEntity>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            _fieldOfViewArea.BodyEntered += OnBodyEntered;
            _fieldOfViewArea.BodyExited += OnBodyExited;
        }


        /// <summary> Triggered when a new object enters the sensor's field of view. These are objects that a potentially visible. </summary>
        /// <param name="body"> A reference to the new body. </param>
        private void OnBodyEntered(Node3D body)
        {
            if (body is IEntity entity && entity != Owner)
            {
                _nearbyEntities.Add(entity);
            }
        }


        /// <summary> Triggered when a new object leaves the sensor's field of view. These are objects that are no longer visible. </summary>
        /// <param name="body"> A reference to the new body. </param>
        private void OnBodyExited(Node3D body)
        {
            if (body is IEntity entity)
            {
                // Remove the entity from visibility.
                KnownEntity? trackedObject = _sensors.KnowsEntity(entity);
                if (trackedObject != null) { trackedObject.SetIsVisible(false); }

                _nearbyEntities.Remove(entity);
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            foreach (IEntity current in _nearbyEntities)
            {
                Vector3 targetPosition = ToLocal(current.CollisionShape.GlobalPosition);
                _lineOfSightRayCast.TargetPosition = targetPosition;
                _lineOfSightRayCast.ForceRaycastUpdate();


                if (_lineOfSightRayCast.IsColliding())
                {
                    KnownEntity? trackedObject = _sensors.KnowsEntity(current);
                    if (_lineOfSightRayCast.GetCollider() is IEntity entity && entity == current)  // Is the entity directly visible.
                    {
                        if (trackedObject == null)  // If the entity isn't already known, add it.
                        {
                            trackedObject = _sensors.RememberEntity(current);
                        }

                        trackedObject?.SetIsVisible(true);
                        trackedObject?.UpdateLastKnownPosition();
                    }
                    else    // If the entity is hidden.
                    {
                        trackedObject?.SetIsVisible(false);
                    }

                    // Render the debug helpers.
                    if (_showDebug)
                    {
                        DebugDraw3D.DrawLineHit(_lineOfSightRayCast.GlobalPosition, ToGlobal(_lineOfSightRayCast.TargetPosition), _lineOfSightRayCast.GetCollisionPoint(), true);
                    }
                }
            }
        }
    }
}
