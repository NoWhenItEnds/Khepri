using Godot;

namespace Khepri.Rooms.Features
{
    /// <summary> A single aspect or characteristic of a room. A room's properties are defined by the features attached to it. </summary>
    /// <remarks>
    /// A feature is a Godot <see cref="Resource"/>: the same class is the designer-authored template, the live runtime instance, and the save payload.
    /// <c>RoomPrefab</c> instantiates a room by <see cref="Resource.Duplicate(bool)"/>-ing each template feature, calling <see cref="Bind"/>, then <see cref="OnInstantiate"/>. A room holds at most one feature of each concrete type.
    /// </remarks>
    [GlobalClass]
    public abstract partial class Feature : Resource
    {
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
    }
}
