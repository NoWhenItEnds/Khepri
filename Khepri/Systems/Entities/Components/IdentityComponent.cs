using System;
using System.Collections.Generic;
using System.Linq;
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


        /// <summary> Resolves the entity's noun by letting each part stake its claim. </summary>
        /// <returns> The resolved noun, or a fallback label when no part claims one. </returns>
        public String GetNoun()
        {
            // Take the most popular claim as the winner.
            Dictionary<String, Int32> claims = new Dictionary<String, Int32>(); // TODO - Kind should use a map or something. Something slightly more complected.
            Parts.GroupBy(part => part.Kind.Noun).ToList().ForEach(group =>
            {
                String claim = group.Key;
                Int32 count = group.Count();
                claims[claim] = count;
            });

            return claims.Keys.Count > 0 ? claims.OrderBy(x => x.Value).Last().Key : "something";
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
