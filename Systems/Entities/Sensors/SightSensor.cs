using Godot;
using System;
using System.Collections.Generic;

namespace Khepri.Entities.Sensors
{
    /// <summary> A sensor used to find visual information about the surrounding environment. </summary>
    public partial class SightSensor : Node3D, ISensor
    {
        /// <summary> A reference to the unit sensor parent. Contains the unit's persistent memory. </summary>
        [ExportGroup("Nodes")]
        [Export] private UnitSensors _sensorsParent;

        /// <summary> The area representing the observer's field of view. Items in here are potentially visible. </summary>
        [Export] private Area3D _fieldOfViewArea;

        /// <summary> A ray-cast to check if an objective is visible or being occluded. </summary>
        [Export] private RayCast3D _lineOfSightRayCast;


        /// <summary> The array of entities that are potentially visible. </summary>
        private HashSet<ISmartEntity> _nearbyEntities = new HashSet<ISmartEntity>();


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
            if (body is ISmartEntity entity)
            {
                _nearbyEntities.Add(entity);
            }
        }


        /// <summary> Triggered when a new object leaves the sensor's field of view. These are objects that are no longer visible. </summary>
        /// <param name="body"> A reference to the new body. </param>
        private void OnBodyExited(Node3D body)
        {
            if (body is ISmartEntity entity)
            {
                _nearbyEntities.Remove(entity);
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            foreach (ISmartEntity current in _nearbyEntities)
            {
                Vector3 targetPosition = ToLocal(current.CollisionShape.GlobalPosition);
                _lineOfSightRayCast.TargetPosition = targetPosition;
                _lineOfSightRayCast.ForceRaycastUpdate();

                //  Update whether the object is visible.
                if (_lineOfSightRayCast.IsColliding() && _lineOfSightRayCast.GetCollider() is ISmartEntity entity)
                {
                    KnownEntity? trackedObject = _sensorsParent.FindEntity(current);
                    if (trackedObject != null)
                    {
                        trackedObject.SetIsVisible(entity == current);
                        trackedObject.UpdateLastKnownPosition();
                    }
                }
            }
        }
    }
}
