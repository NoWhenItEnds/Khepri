using System;
using System.Collections.Generic;
using Khepri.Entities.Components;

namespace Khepri.Entities
{
    /// <summary> Recursively fills an entity graph by instantiating the pending prefab contents declared on each <see cref="IPopulatableContainer"/> component. </summary>
    /// <remarks>
    /// The populator is the second half of the post-build population seam: the factory layer records what should be spawned (names);
    /// the populator is given a live spawner delegate and does the actual instantiation.
    /// It operates depth-first so that a child's own containers are populated before the next sibling is started.
    ///
    /// Cycle detection operates on prefab names, not entity instances.  If the expansion path already contains a
    /// requested prefab name an <see cref="InvalidOperationException"/> is thrown naming the full cycle (e.g. <c>"treasure_chest -> bag -> treasure_chest"</c>).
    /// This prevents infinite recursion during construction and is distinct from the runtime containment-cycle guard in
    /// <see cref="InventoryComponent.AddEntity"/>, which prevents cycles in the live entity graph.
    /// </remarks>
    public sealed class EntityPopulator
    {
        /// <summary> The delegate that constructs a single, unpopulated entity from a prefab name; supplied by the Godot layer so this class stays Godot-free. </summary>
        private readonly Func<String, Entity> _bareSpawner;


        /// <summary> Initialises the populator with the bare-spawn delegate. </summary>
        /// <param name="bareSpawner"> A delegate that accepts an entity prefab name and returns a freshly built entity with its components attached but not yet populated.
        /// The Godot layer passes its prefab-to-entity construction here; the populator must not be given a delegate that itself calls <see cref="Populate"/> to avoid re-entrancy surprises. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="bareSpawner"/> is null. </exception>
        public EntityPopulator(Func<String, Entity> bareSpawner)
        {
            if (bareSpawner is null)
            {
                throw new ArgumentNullException(nameof(bareSpawner), "Bare-spawner delegate must not be null.");
            }

            _bareSpawner = bareSpawner;
        }


        /// <summary> Recursively populates <paramref name="root"/> and every entity spawned beneath it, inserting instantiated children into each <see cref="IPopulatableContainer"/> component found. </summary>
        /// <remarks>
        /// The method seeds the prefab-name path with <paramref name="rootPrefabName"/> before recursing, so cycles that return to the root are detected immediately.
        /// A child entity is spawned bare via the injected delegate, recursively populated, then inserted into the container that requested it.
        /// </remarks>
        /// <param name="root"> The entity whose component graph is to be populated; must not be null. </param>
        /// <param name="rootPrefabName"> The prefab name used to spawn <paramref name="root"/>; seeds the cycle-detection path. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="root"/> is null. </exception>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="rootPrefabName"/> is null or whitespace. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when a pending prefab name creates a cycle in the expansion path; the message names the full cycle path. </exception>
        public void Populate(Entity root, String rootPrefabName)
        {
            if (root is null)
            {
                throw new ArgumentNullException(nameof(root), "Root entity must not be null.");
            }

            if (String.IsNullOrWhiteSpace(rootPrefabName))
            {
                throw new ArgumentException("Root prefab name must not be null or whitespace.", nameof(rootPrefabName));
            }

            List<String> expansionPath = new List<String> { rootPrefabName };
            PopulateEntity(root, expansionPath);
        }


        /// <summary> Walks every <see cref="IPopulatableContainer"/> component on <paramref name="entity"/>, spawns and recursively populates each pending child, then inserts the child into the container. </summary>
        /// <param name="entity"> The entity to process. </param>
        /// <param name="expansionPath"> The ordered list of prefab names on the current recursive expansion path, used for cycle detection. The caller is responsible for seeding it with the entity's own prefab name before calling this method. </param>
        /// <exception cref="InvalidOperationException"> Thrown when a pending prefab name is already present in <paramref name="expansionPath"/>; the message names the full cycle path. </exception>
        private void PopulateEntity(Entity entity, List<String> expansionPath)
        {
            IReadOnlyCollection<Component> components = entity.GetComponents();

            foreach (Component component in components)
            {
                Boolean isPopulatable = component is IPopulatableContainer;

                if (isPopulatable)
                {
                    IPopulatableContainer container = (IPopulatableContainer)component;
                    PopulateContainer(container, expansionPath);
                }
            }
        }


        /// <summary> Spawns, recursively populates, and inserts each pending child declared on <paramref name="container"/>. </summary>
        /// <param name="container"> The container whose <see cref="IPopulatableContainer.GetPendingContents"/> drives the loop. </param>
        /// <param name="expansionPath"> The ordered list of prefab names on the current recursive expansion path; mutated temporarily for each child to track the depth-first path. </param>
        /// <exception cref="InvalidOperationException"> Thrown when a pending prefab name is already present in <paramref name="expansionPath"/>. </exception>
        private void PopulateContainer(IPopulatableContainer container, List<String> expansionPath)
        {
            IReadOnlyCollection<String> pending = container.GetPendingContents();

            foreach (String prefabName in pending)
            {
                GuardCycle(prefabName, expansionPath);

                // The Add/RemoveAt pair is exception-safe: the only call that can throw between them is the
                // recursive spawn, which unwinds the whole stack and abandons the partially-built graph —
                // so the path list is never left in an inconsistent state across sibling iterations.
                expansionPath.Add(prefabName);

                Entity child = _bareSpawner(prefabName);
                PopulateEntity(child, expansionPath);
                container.AddEntity(child);

                expansionPath.RemoveAt(expansionPath.Count - 1);
            }
        }


        /// <summary> Throws when <paramref name="prefabName"/> is already present anywhere in <paramref name="expansionPath"/>, providing a full cycle path in the message. </summary>
        /// <param name="prefabName"> The prefab name about to be added to the expansion path. </param>
        /// <param name="expansionPath"> The ordered list of prefab names currently on the recursive expansion path. </param>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="prefabName"/> is already in <paramref name="expansionPath"/>; the message shows the full cycle (e.g. <c>"treasure_chest -> bag -> treasure_chest"</c>). </exception>
        private static void GuardCycle(String prefabName, List<String> expansionPath)
        {
            Boolean hasCycle = expansionPath.Contains(prefabName);

            if (hasCycle)
            {
                String path = String.Join(" -> ", expansionPath) + " -> " + prefabName;
                throw new InvalidOperationException(
                    $"Prefab expansion cycle detected: {path}. A prefab may not (directly or transitively) contain itself.");
            }
        }
    }
}
