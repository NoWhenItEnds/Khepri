using System.Collections.Generic;
using Godot;

namespace Khepri.Entities.Components
{
    /// <summary> A single aspect of an entity. An entity's capabilities are defined by the components that construct it. </summary>
    /// <remarks>
    /// A component is a Godot <see cref="Resource"/>: the same class serves as the designer-authored template (edited in the Inspector via its <c>[Export]</c> fields), the live runtime instance, and — in due course — the save payload, because <c>ResourceSaver</c> can persist it directly.
    /// <see cref="EntityPrefab"/> instantiates an entity by <see cref="Resource.Duplicate(bool)"/>-ing each template component, calling <see cref="Bind"/> to attach the owner, then <see cref="OnInstantiate"/> to resolve any spawn-time state (rolled ranges, container contents).
    /// </remarks>
    [GlobalClass]
    public abstract partial class Component : Resource
    {
        /// <summary> The entity this component is attached to. Wired by <see cref="Bind"/> at instantiation; deliberately not exported, so it is neither authored nor serialised. </summary>
        public Entity Owner { get; private set; } = null!;


        /// <summary> Attaches this component to its owning entity. Called once by <see cref="EntityPrefab"/> immediately after duplication and before <see cref="OnInstantiate"/>. </summary>
        /// <param name="owner"> The entity that owns this component. </param>
        internal void Bind(Entity owner)
        {
            Owner = owner;
        }


        /// <summary> Resolves any spawn-time state once the component has been duplicated onto a fresh entity — for example rolling a range to a concrete value, or instantiating authored container contents. </summary>
        /// <remarks> The default is a no-op; components whose authored values are already their runtime values need not override it. Not called on the restore path, so rolled values stay fixed across a save/load. </remarks>
        /// <param name="ancestry"> The <see cref="EntityPrefab"/>s currently open on the instantiation stack; container components forward it to <see cref="EntityPrefab.Instantiate(ISet{EntityPrefab})"/> to reject a prefab that transitively contains itself. </param>
        public virtual void OnInstantiate(ISet<EntityPrefab> ancestry)
        {
        }


        /// <summary> Verifies this component's authored data once at load, before any entity is built from its prefab. The default is a no-op, for components with no authoring invariants to enforce. </summary>
        /// <remarks> Distinct from <see cref="OnInstantiate"/>: this asserts fixed facts about the authored template (a required reference was filled), whereas <see cref="OnInstantiate"/> resolves per-spawn state. Running it once at load surfaces authoring mistakes at boot rather than lazily when an entity of that prefab first spawns. </remarks>
        /// <param name="prefab"> The prefab this template belongs to, named in any error raised. </param>
        public virtual void Validate(EntityPrefab prefab)
        {
        }
    }
}
