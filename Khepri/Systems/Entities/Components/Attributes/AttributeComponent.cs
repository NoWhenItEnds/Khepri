using System;
using System.Linq;
using Godot;

namespace Khepri.Entities.Components.Attributes
{
    /// <summary> A component allowing an entity to have attributes. </summary>
    [GlobalClass]
    public partial class AttributeComponent : Component
    {
        /// <summary> The attributes this entity has, authored in the Inspector. Each entry pairs a shared <see cref="AttributeKind"/> with this entity's level in it. </summary>
        [Export] public Godot.Collections.Array<EntityAttribute> Attributes { get; set; } = new Godot.Collections.Array<EntityAttribute>();


        /// <summary> Finds this entity's level in a given attribute. </summary>
        /// <param name="kind"> The attribute to look up. </param>
        /// <returns> The matching <see cref="EntityAttribute"/>, or <c>null</c> if this entity has never learned that attribute. </returns>
        public EntityAttribute GetAttribute(AttributeKind kind) => Attributes.FirstOrDefault(attr => attr.Kind == kind);  // TODO - What if you don't have the attribute?


        /// <inheritdoc/>
        public override void Validate(EntityPrefab prefab)
        {
            foreach (EntityAttribute attribute in Attributes)
            {
                if (attribute.Kind is null)
                {
                    throw new InvalidOperationException(
                        $"Entity prefab '{prefab.Name}' has a AttributeComponent entry with no AttributeKind assigned. Every authored attribute must reference a kind.");
                }
            }
        }
    }
}
