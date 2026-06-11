using System;
using Godot;
using Jaypen.Utilities.ECS;
using Khepri.Descriptions;

namespace Khepri.Entities.Components.Parts
{
    /// <summary> A single anatomical or structural part belonging to an entity — for example a head, an arm, or a door. </summary>
    /// <remarks> Parts are held only inside an <see cref="IdentityComponent"/>. Every part must carry a <see cref="Kind"/>; the non-nullable type declares that contract and <see cref="IdentityComponent.Validate"/> enforces it once at load, so the spawn and naming paths may treat it as guaranteed. </remarks>
    [GlobalClass]
    public abstract partial class PartComponent : Resource, IComponent
    {
        /// <summary> The entity kind this part belongs to — the data-bearing identity it claims for its entity, for example a goblin <see cref="EntityKind"/> on a core part. Required: the non-nullable type declares the contract, and <see cref="Validate"/> verifies the Inspector slot was filled once at load, so everything downstream may treat it as set. </summary>
        [Export] public EntityKind Kind { get; set; } = null!;

        /// <summary> The prose this part contributes to its entity's description, authored in the Inspector. May be left empty for a part with nothing to say. </summary>
        [Export(PropertyHint.MultilineText)] public String Prose { get; set; } = String.Empty;


        /// <summary> Verifies this part's authored data once at load, before any entity is built. </summary>
        /// <param name="prefab"> The prefab being validated, named in any error raised. </param>
        /// <exception cref="InvalidOperationException"> Thrown when <see cref="Kind"/> was left unset in the Inspector. </exception>
        public void Validate(EntityPrefab prefab)
        {
            if (Kind is null)
            {
                throw new InvalidOperationException(
                    $"Entity prefab '{prefab.Name}' has a part of type '{GetType().Name}' with no Kind. Every part must carry an EntityKind so the entity resolves to a meaningful name.");
            }
        }


        /// <summary> Appends this part's contribution to its entity's description: the authored <see cref="Prose"/>, when there is any. Override only when the prose is genuinely dynamic — authored text belongs in <see cref="Prose"/>, not code. </summary>
        /// <param name="builder"> The builder assembling the owning entity's description. </param>
        public virtual void Contribute(DescriptionBuilder builder)
        {
            if (!String.IsNullOrEmpty(Prose))
            {
                builder.Text(Prose);
            }
        }
    }
}
