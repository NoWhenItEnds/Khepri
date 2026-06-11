using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Jaypen.Utilities.ECS;
using Jaypen.Utilities.Extensions;
using Khepri.Descriptions;
using Khepri.Entities.Components;

namespace Khepri.Entities
{
    /// <summary> A thing or object that can exist within a room. All things, including players and items, are entities. </summary>
    public class Entity : SingleComponentHolder<Component>, IEquatable<Entity>, INoteSource
    {
        /// <summary> The entity's unique identifier. Should be unique across all entities. </summary>
        public readonly Guid UId;


        /// <summary> Initialises a new instance of the <see cref="Entity"/> class. </summary>
        /// <param name="uid"> The unique identifier for the entity. </param>
        public Entity(Guid uid)
        {
            UId = uid;
        }


        /// <summary> Binds a freshly added component to this entity by setting its <see cref="Component.Owner"/>. </summary>
        /// <param name="component"> The component that was just added. </param>
        protected override void Bind(Component component) => component.Initialise(this);


        /// <summary> Unbinds a freshly removed component from this entity by clearing its <see cref="Component.Owner"/>. </summary>
        /// <param name="component"> The component that was just removed. </param>
        protected override void Unbind(Component component) => component.Detach();


        /// <summary> Returns only the components that implement <see cref="IEntityContainer"/>, allowing callers to walk the containment hierarchy without exposing the full component set. </summary>
        /// <returns> The subset of attached components that act as entity containers, in unspecified order. </returns>
        public IReadOnlyCollection<IEntityContainer> GetContainers() => GetComponents().OfType<IEntityContainer>().ToList();


        /// <summary> Builds a dynamic description of the entity's current state — its tooltip body when it appears as a note. </summary>
        /// <returns> The assembled description of the entity's current state. </returns>
        public Description BuildDescription()
        {
            DescriptionBuilder builder = new DescriptionBuilder();

            // Open by naming what the entity is, then let its components add detail, each fold separated from the last.
            builder.Text(GetName().ToCapitalised() + ".");

            foreach (IDescriptionContributor contributor in GetComponents().OfType<IDescriptionContributor>())
            {
                contributor.Contribute(builder);
            }

            return builder.Build();
        }


        /// <summary> Appends this entity to its container's description as a single hoverable note, labelled with its current name and pointing back at itself. </summary>
        /// <remarks> Called when the entity is listed inside something else — a room, or a container component such as an inventory. Hovering the note surfaces this entity's own <see cref="BuildDescription"/>. </remarks>
        /// <param name="builder"> The builder assembling the containing room's or entity's description. </param>
        public void Contribute(DescriptionBuilder builder)
        {
            builder.Note(GetName(), this);
        }


        /// <summary> Resolves the entity's current display name from its components. </summary>
        /// <remarks> The name emerges from whatever components are present — the most salient noun, decorated by the adjectives others contribute — so it tracks the entity through transformations rather than being fixed at spawn. </remarks>
        /// <returns> The composed name, or a fallback label when no component claims a noun. </returns>
        public String GetName()
        {
            // The noun comes solely from the identity component; absent one, the builder falls back.
            // Found by assignability rather than exact type, so a subclassed identity component still claims the noun.
            String noun = "something";
            IdentityComponent? identityComponent = GetComponents().OfType<IdentityComponent>().FirstOrDefault();
            if (identityComponent is not null)
            {
                noun = identityComponent.GetNoun();
            }

            NameBuilder builder = NameBuilder.Create(noun);

            foreach (AdjectiveComponent adjective in GetComponents().OfType<AdjectiveComponent>())
            {
                builder.WithAdjective(adjective.RoyalIndex,adjective.GetAdjective());
            }

            return builder.Build();
        }


        /// <summary> Attempt to get a texture representing the current entity. </summary>
        /// <returns> A texture that represents the current entity's state / situation. A null indicates that there isn't one. </returns>
        public Texture2D? GetTexture()
        {
            return null;                // TODO - Figure out how to do this!
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => UId.GetHashCode();


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => obj is Entity other && Equals(other);


        /// <inheritdoc/>
        public Boolean Equals(Entity? other) => other is not null && UId.Equals(other.UId);
    }
}
