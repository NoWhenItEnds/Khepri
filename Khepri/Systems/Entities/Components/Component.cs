using System;

namespace Khepri.Entities.Components
{
    /// <summary> A single aspect of an entity. An entity's capabilities are defined by the components the construct it. </summary>
    public abstract class Component : IEquatable<Component>
    {
        /// <summary> The entity that this component is attached to. </summary>
        protected readonly Entity _entity;

        /// <summary> Initialises a new instance of the <see cref="Component"/> class. </summary>
        /// <param name="entity"> The entity that this component is attached to. </param>
        public Component(Entity entity)
        {
            _entity = entity;
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => GetType().GetHashCode();


        /// <inheritdoc/>
        public override Boolean Equals(object? obj) => obj is Component other && Equals(other);


        /// <inheritdoc/>
        public Boolean Equals(Component? other) => other is not null && GetType().Equals(other.GetType());
    }
}
