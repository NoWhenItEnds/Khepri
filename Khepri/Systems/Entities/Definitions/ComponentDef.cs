using System.Collections.Generic;
using Godot;
using Khepri.Entities.Components;

namespace Khepri.Entities.Definitions
{
    /// <summary> The authored, designer-editable definition of a single component, used by <see cref="EntityPrefab"/> to construct a live <see cref="Component"/> for a freshly spawned entity. </summary>
    /// <remarks>
    /// This is the Godot-side data layer for components: concrete subclasses expose their authoring fields as <c>[Export]</c> properties and are tagged <c>[GlobalClass]</c> so they appear in the editor's "New Resource" menu and can be edited in the Inspector.
    /// The runtime <see cref="Component"/> types remain plain POCOs with no Godot dependency; a <see cref="ComponentDef"/> is the only bridge between the two.
    /// </remarks>
    [GlobalClass]
    public abstract partial class ComponentDef : Resource
    {
        /// <summary> Constructs a live component for <paramref name="owner"/> from this definition's authored values. </summary>
        /// <param name="owner"> The entity the produced component will belong to. </param>
        /// <param name="ancestry"> The set of <see cref="EntityPrefab"/>s currently being instantiated on the recursion stack; container definitions forward it to <see cref="EntityPrefab.Instantiate(ISet{EntityPrefab})"/> so a prefab that transitively contains itself is rejected rather than recursing forever. Non-container definitions ignore it. </param>
        /// <returns> A fully constructed, non-null <see cref="Component"/>. </returns>
        public abstract Component Create(Entity owner, ISet<EntityPrefab> ancestry);
    }
}
