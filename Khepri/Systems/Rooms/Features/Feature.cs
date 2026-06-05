using System;
using Godot;
using Khepri.Descriptions;

namespace Khepri.Rooms.Features
{
    /// <summary> A single aspect or characteristic of a room. A room's properties are defined by the features attached to it. </summary>
    /// <remarks>
    /// A feature is a Godot <see cref="Resource"/>: the same class is the designer-authored template, the live runtime instance, and the save payload.
    /// <c>RoomPrefab</c> instantiates a room by <see cref="Resource.Duplicate(bool)"/>-ing each template feature, calling <see cref="Bind"/>, then <see cref="OnInstantiate"/>. A room holds at most one feature of each concrete type.
    /// </remarks>
    [GlobalClass]
    public abstract partial class Feature : Resource, INoteSource
    {
        /// <summary> This feature's descriptive text. Read as prose in the room's description by the default <see cref="Contribute"/>; a feature that instead reads as a hoverable note (overriding <see cref="Contribute"/>) shows this as the note's tooltip via the default <see cref="BuildDescription"/>. </summary>
        [Export(PropertyHint.MultilineText)] public String Description { get; set; } = String.Empty;

        /// <summary> Orders this feature's prose within the room's description; lower values appear first. Features sharing an order fall back to an unspecified but stable order. </summary>
        [Export] public Int32 Order { get; set; } = 0;


        /// <summary> The room this feature is attached to. Wired by <see cref="Bind"/> at instantiation; not exported, so neither authored nor serialised. </summary>
        public Room Owner { get; private set; } = null!;


        /// <summary> Attaches this feature to its owning room. Called once immediately after duplication and before <see cref="OnInstantiate"/>. </summary>
        /// <param name="owner"> The room that owns this feature. </param>
        internal void Bind(Room owner)
        {
            Owner = owner;
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
