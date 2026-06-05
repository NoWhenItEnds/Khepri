using Godot;

namespace Khepri.Entities.Components
{
    /// <summary> Decorates an entity's name with its visible condition, for example <c>"battered"</c>. Condition is an opinion adjective, so it sits furthest from the noun. </summary>
    [GlobalClass]
    public partial class ConditionComponent : AdjectiveComponent
    {
    }
}
