using Godot;
using Godot.Collections;
using Khepri.Controllers;
using Khepri.Entities.Interfaces;
using Khepri.Entities.UnitComponents;
using Khepri.Entities.UnitComponents.States;
using Khepri.Models.Input;
using Khepri.Models.Persistent;
using Khepri.Nodes;
using System;

namespace Khepri.Entities
{
    /// <summary> An active entity controlled by something. </summary>
    public partial class Unit : CharacterBody3D, ISmartEntity
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }

        /// <summary> A reference to the unit's navigation agent. </summary>
        [Export] public NavigationAgent3D NavigationAgent { get; private set; }

        /// <summary> The position to position a camera when it's following this unit. </summary>
        [Export] public Marker3D CameraPosition { get; private set; }

        /// <summary> A reference to the unit's sprite. </summary>
        [Export] public UnitSprite AnimatedSprite { get; private set; }

        /// <summary> A reference to the unit's long term memory and senses. </summary>
        [Export] public UnitBrain Brain { get; private set; }


        /// <summary> Modifies the amount of hunger the unit looses each tick. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _hungerModifier = 0.001f;

        /// <summary> Modifies the amount of fatigue the unit looses each tick. </summary>
        [Export] private Single _fatigueModifier = 0.001f;

        /// <summary> Modifies the amount of entertainment the unit looses each tick. </summary>
        [Export] private Single _entertainmentModifier = 0.001f;

        /// <summary> The amount of stamina the unit recovers per tick. </summary>
        [Export] private Single _staminaModifier = 1f;


        /// <summary> The animation sheets to use for the unit's animations. </summary>
        [ExportGroup("Resources")]
        [Export] private Dictionary<UnitSpriteLayer, SpriteFrames> _spriteFrames;


        /// <inheritdoc/>
        public Guid UId => Data.UId;

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <summary> The stats used by the unit to set its state. </summary>
        public readonly UnitData Data = new UnitData();


        /// <summary> A reference to the world controller. </summary>
        private WorldController _worldController;


        /// <summary> The current state of the unit. </summary>
        private UnitState _currentState;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldController = WorldController.Instance;

            _currentState = new IdleState(this);

            // Setup the sprite animations.
            foreach (var frames in _spriteFrames)
            {
                AnimatedSprite.SetSpriteLayer(frames.Key, frames.Value);
            }
            AnimatedSprite.Play();
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            _currentState.Update(delta);

            // Update stats.
            Single gameTimeDelta = (Single)_worldController.GameTimeDelta;
            Data.UpdateHunger(-gameTimeDelta * _hungerModifier);
            Data.UpdateFatigue(-gameTimeDelta * _fatigueModifier);
            Data.UpdateEntertainment(-gameTimeDelta * _entertainmentModifier);
            Data.UpdateStamina(gameTimeDelta * _staminaModifier);
        }


        /// <summary> Attempts to set the unit's state. </summary>
        /// <param name="state"> The state to set the unit. </param>
        public void TrySetUnitState(Type state) => _currentState = _currentState.TryTransitionTo(state);


        /// <summary> Handle the input sent to the unit by it's controller. </summary>
        /// <param name="input"> The input data class to interpret. </param>
        public void HandleInput(IInput input)
        {
            _currentState.HandleInput(input);
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Data.UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            Unit? other = obj as Unit;
            return other != null ? Data.UId.Equals(other.Data.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(IEntity other) => UId.Equals(other.UId);
    }
}
