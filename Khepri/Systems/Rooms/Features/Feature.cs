using System;
using Godot;
using Jaypen.Utilities.ECS;
using Khepri.Descriptions;

namespace Khepri.Rooms.Features
{
    /// <summary> A single aspect or characteristic of a room. A room's properties are defined by the features attached to it. </summary>
    /// <remarks>
    /// A feature is a Godot <see cref="Resource"/>: the same class is the designer-authored template, the live runtime instance, and the save payload.
    /// <c>RoomPrefab</c> instantiates a room by <see cref="Resource.Duplicate(bool)"/>-ing each template feature, calling <see cref="Initialise"/>, then <see cref="OnInstantiate"/>. A room holds at most one feature of each concrete type.
    /// </remarks>
    [GlobalClass]
    public abstract partial class Feature : Resource, INoteSource, IComponent
    {
        /// <summary> This feature's descriptive text. Read as prose in the room's description by the default <see cref="Contribute"/>; a feature that instead reads as a hoverable note (overriding <see cref="Contribute"/>) shows this as the note's tooltip via the default <see cref="BuildDescription"/>. </summary>
        [Export(PropertyHint.MultilineText)] public String Description { get; set; } = String.Empty;

        /// <summary> Orders this feature's prose within the room's description; lower values appear first. Features sharing an order fall back to an unspecified but stable order. </summary>
        [Export] public Int32 Order { get; set; } = 0;


        /// <summary>
        /// The room this feature is attached to. <c>null</c> before <see cref="Initialise"/> is called and again after
        /// <see cref="Detach"/> completes. Not exported — neither authored nor serialised.
        /// </summary>
        public Room? Owner { get; private set; }


        /// <summary>
        /// Sets <see cref="Owner"/> to <paramref name="owner"/>. Safe to call when <see cref="Owner"/> is already
        /// <paramref name="owner"/> (no-op). Throws if <see cref="Owner"/> is already bound to a <em>different</em>
        /// room, because a feature instance belongs to exactly one room.
        /// </summary>
        /// <param name="owner"> The room to bind this feature to. </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this feature is already bound to a different room — rebinding across rooms is not permitted.
        /// </exception>
        internal void Initialise(Room owner)
        {
            if (Owner is not null && !ReferenceEquals(Owner, owner))
            {
                throw new InvalidOperationException(
                    $"Feature '{GetType().Name}' is already bound to a different room and cannot be rebound.");
            }

            Boolean newlyBound = Owner is null;
            Owner = owner;
            if (newlyBound)
            {
                OnAttached();
            }
        }


        /// <summary> Clears <see cref="Owner"/>, leaving it <c>null</c>. Called by <see cref="Room"/> on a successful remove. </summary>
        internal void Detach()
        {
            if (Owner is not null)
            {
                OnDetached();
            }
            Owner = null;
        }


        /// <summary> Called once when this feature is bound to a room, after <see cref="Owner"/> is set. The default is a no-op. </summary>
        /// <remarks> Override to react to attachment — for example subscribing to the owner's events. Runs before the room raises its <c>ComponentAdded</c> event. </remarks>
        protected virtual void OnAttached()
        {
        }


        /// <summary> Called once when this feature is removed from its room, while <see cref="Owner"/> is still set. The default is a no-op. </summary>
        /// <remarks> Override to undo whatever <see cref="OnAttached"/> established — <see cref="Owner"/> remains available here so subscriptions can be released, and is cleared immediately afterwards. Runs before the room raises its <c>ComponentRemoved</c> event. </remarks>
        protected virtual void OnDetached()
        {
        }


        /// <summary> Resolves any spawn-time state once the feature has been duplicated onto a fresh room. The default is a no-op. </summary>
        public virtual void OnInstantiate()
        {
        }


        /// <summary> Appends this feature's contribution to its room's description. By default, contributes <see cref="Description"/> as plain prose; a feature that reads as a hoverable item of note overrides this to weave a <see cref="DescriptionBuilder.Note"/> into the prose instead. </summary>
        /// <param name="builder"> The builder assembling the owning room's description. </param>
        public virtual void Contribute(DescriptionBuilder builder)
        {
            if (!String.IsNullOrEmpty(Description))
            {
                builder.Text(Description);
            }
        }


        /// <summary> Builds the description shown when one of this feature's notes is hovered — by default, the feature's own <see cref="Description"/> text. </summary>
        /// <returns> The tooltip body for this feature. </returns>
        public virtual Description BuildDescription() => new DescriptionBuilder().Text(Description).Build();
    }
}
