using System;
using Godot;

namespace Khepri.Entities.Components.Attributes
{
    /// <summary> A single attribute known by an entity — one entity's proficiency in a shared <see cref="AttributeKind"/>. </summary>
    [GlobalClass]
    public partial class EntityAttribute : Resource
    {
        /// <summary> The highest level any attribute can reach; whole numbers map to the dots shown in the UI. </summary>
        public const Single MaxLevel = 5f;

        /// <summary> The shared definition of which attribute this is. </summary>
        [Export] public AttributeKind Kind { get; set; } = null!;

        /// <summary> The entity's current level in this attribute. </summary>
        [Export] public Single Level { get; set; } = 0f;


        /// <summary> The whole-dot rating (0–5) of <see cref="Level"/>, for display. </summary>
        public Int32 LevelDots => (Int32)Math.Floor(Level);
    }
}
