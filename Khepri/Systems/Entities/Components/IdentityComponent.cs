using System;
using Godot;
using Khepri.Descriptions;
using Khepri.Entities.Components.Parts;

namespace Khepri.Entities.Components
{
    /// <summary> The single owner of an entity's gestalt identity — holds the entity's parts and delegates naming entirely to them. </summary>
    [GlobalClass]
    public partial class IdentityComponent : Component, IDescriptionContributor
    {
        /// <summary> The entity's anatomical or structural parts, authored in the Inspector. Multiplicity is supported here — an entity may have four wheels and two arms without violating the one-component-per-type rule on the entity itself. </summary>
        [Export] public Godot.Collections.Array<PartComponent> Parts { get; set; } = new Godot.Collections.Array<PartComponent>();


        /// <inheritdoc/>
        public override void Validate(EntityPrefab prefab)
        {
            if (Parts.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Entity prefab '{prefab.Name}' has an IdentityComponent with no parts. Every entity must have at least one part so it always resolves to a name.");
            }

            foreach (PartComponent part in Parts)
            {
                part.Validate(prefab);
            }
        }


        /// <summary> Resolves the entity's noun by letting each part stake its claim; the first non-empty noun wins, per <see cref="NameBuilder.Noun"/>. As the sole owner of the entity's identity, this is the only source of the noun. </summary>
        /// <param name="builder"> The builder assembling the owning entity's name. </param>
        public void ResolveNoun(NameBuilder builder)
        {
            foreach (PartComponent part in Parts)
            {
                part.Contribute(builder);
            }
        }


        /// <inheritdoc/>
        public void Contribute(DescriptionBuilder builder)
        {
            foreach (PartComponent part in Parts)
            {
                part.Contribute(builder);
            }
        }
    }
}
