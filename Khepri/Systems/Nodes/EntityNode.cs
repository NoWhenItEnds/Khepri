using System;
using Godot;
using Khepri.Data.Entities;

namespace Khepri.Nodes
{
    /// <summary> The physic-simulated object representing an entity within the player's view. </summary>
    public partial class EntityNode : Node2D
    {
        /// <summary> The area shape used to represent the entity's physical body. </summary>
        [ExportGroup("Nodes")]
        [Export] private Area2D _bodyArea = null!;

        /// <summary> The sprite representing the entity. </summary>
        [Export] private Sprite2D _sprite = null!;


        /// <summary> The entity this node currently represents within the game world. A null indicates that it is stashed in the pool waiting for assignment. </summary>
        private Entity? _entity = null;


        /// <summary> Initialise the node by passing it a reference to the entity it will represent. </summary>
        /// <param name="entity"> The entity this node represents within the game world. </param>
        public void Build(Entity entity)
        {
            _entity = entity;
            GlobalPosition = entity.GlobalPosition;
            Visible = true;
        }


        /// <summary> Clean the node up and return it to the node pool for recycling. </summary>
        public void Cleanup()
        {
            Visible = false;
            GlobalPosition = Vector2.Zero;
            _entity = null;
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            if (_entity != null)
            {
                GlobalPosition = _entity.GlobalPosition;
            }
        }
    }
}
