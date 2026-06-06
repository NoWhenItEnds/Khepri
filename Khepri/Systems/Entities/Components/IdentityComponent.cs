using System;
using System.Collections.Generic;
using Godot;
using Khepri.Descriptions;
using Khepri.Entities.Components.Parts;
using Khepri.Entities.Definitions;

namespace Khepri.Entities.Components
{
    /// <summary> The single owner of an entity's gestalt identity — holds the entity's parts and delegates naming entirely to them. </summary>
    [GlobalClass]
    public partial class IdentityComponent : Component
    {
        /// <summary> The entity's anatomical or structural parts, authored in the Inspector. Multiplicity is supported here — an entity may have four wheels and two arms without violating the one-component-per-type rule on the entity itself. </summary>
        [Export] public Godot.Collections.Array<PartComponent> Parts { get; set; } = new Godot.Collections.Array<PartComponent>();


        /// <inheritdoc/>
        public override void OnInstantiate(ISet<EntityPrefab> ancestry)
        {
            if (Parts.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Entity '{Owner.UId}' has an IdentityComponent with no parts. Every entity must have at least one part so it always resolves to a name.");
            }

            foreach (PartComponent part in Parts)
            {
                if (part.Kind is null)  // TODO - IS there a better way?
                {
                    throw new InvalidOperationException(
                        $"Entity '{Owner.UId}' has a part of type '{part.GetType().Name}' with a null Kind. Every part must carry an EntityKind so the entity resolves to a meaningful name.");
                }
            }
        }


        /// <inheritdoc/>
        public override void Contribute(NameBuilder builder)
        {
            foreach (PartComponent part in Parts)
            {
                part.Contribute(builder);
            }
        }


        /// <inheritdoc/>
        public override void Contribute(DescriptionBuilder builder)
        {
            foreach (PartComponent part in Parts)
            {
                part.Contribute(builder);
            }
        }
    }
}
