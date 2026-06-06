using Godot;
using Khepri.Descriptions;

namespace Khepri.Entities.Components.Parts
{
    /// <summary> A single anatomical or structural part belonging to an entity — for example a head, an arm, or a door. </summary>
    /// <remarks> Parts are held only inside an <see cref="IdentityComponent"/>. Every part must carry a non-null <see cref="Kind"/>; this is enforced by <see cref="IdentityComponent.OnInstantiate"/> at spawn time. </remarks>
    [GlobalClass]   // TODO - Is there a better way to enure non-null identity?
    public abstract partial class PartComponent : Resource
    {
        /// <summary> The entity kind this part belongs to — the data-bearing identity it claims for its entity, for example a goblin <see cref="EntityKind"/> on a core part. Required: a null value is rejected by <see cref="IdentityComponent.OnInstantiate"/> at spawn time. The null-guard in <see cref="Contribute(NameBuilder)"/> remains as defensive cover on the save/load restore path, on which <see cref="IdentityComponent.OnInstantiate"/> is not called. </summary>
        [Export] public EntityKind? Kind { get; set; } = null;


        /// <summary> Submits the kind's noun to the builder when <see cref="Kind"/> is set, then allows subclasses to add adjectives. </summary>
        /// <remarks>
        /// The null check is retained as defensive cover for the save/load restore path; on the spawn path <see cref="Kind"/> is guaranteed non-null by <see cref="IdentityComponent.OnInstantiate"/>. Subclasses that override this method should call <c>base.Contribute(builder)</c> to preserve the kind's noun claim unless they deliberately supersede it.
        /// </remarks>
        /// <param name="builder"> The builder assembling the owning entity's name. </param>
        public virtual void Contribute(NameBuilder builder)
        {
            if (Kind is not null)
            {
                builder.Noun(Kind.Noun);
            }
        }


        /// <summary> Appends this part's contribution to its entity's description. The default is a no-op, for parts that have no prose to add. </summary>
        /// <param name="builder"> The builder assembling the owning entity's description. </param>
        public virtual void Contribute(DescriptionBuilder builder)
        {
        }
    }
}
