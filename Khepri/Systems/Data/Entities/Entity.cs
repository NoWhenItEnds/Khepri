using Godot;

namespace Khepri.Data.Entities
{
    /// <summary> An entity that exists within the game world. </summary>
    /// <remarks> The abstract data model, rather than the actual node, to allow for simulation even if they're not being rendered. </remarks>
    public abstract class Entity
    {
        /// <summary> The Godot-position of the entity. Used to position the node correctly in the world. </summary>
        public Vector2 GlobalPosition => Vector2.Zero;


        /// <summary> The system the entity is currently within. A null indicates that it's lost in null-space. The void between the stars. </summary>
        //private SolarSystem? _solarSystem = null;
    }
}
