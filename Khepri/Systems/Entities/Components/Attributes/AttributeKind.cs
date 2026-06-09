using System;
using Godot;

namespace Khepri.Entities.Components.Attributes
{
    /// <summary> An authored definition of a single attribute an entity can have — shared by every entity that possesses it. </summary>
    [GlobalClass]
    public partial class AttributeKind : Resource
    {
        /// <summary> The attribute's name, for example <c>"Strength"</c> or <c>"Intelligence"</c>. </summary>
        [Export] public String Noun { get; set; } = String.Empty;

        /// <summary> The broad discipline this attribute belongs to. </summary>
        [Export] public AttributeCategory Category { get; set; } = AttributeCategory.Physical;

        /// <summary> A short description of what the attribute covers, shown to the player. </summary>
        [Export(PropertyHint.MultilineText)] public String Summary { get; set; } = String.Empty;
    }


    /// <summary> The broad disciplines attributes are grouped under. </summary>
    public enum AttributeCategory
    {
        Physical,
        Mental,
        Social
    }
}
