using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Khepri.Entities.Prefabs;
using Khepri.Prefabs;

namespace Khepri.Entities.Components
{
    /// <summary> A component allowing an entity to hold other entities. </summary>
    /// <remarks>
    /// Implements <see cref="IPopulatableContainer"/> to record prefab-authored pending contents at spawn time.
    /// Implements <see cref="IStatefulPart"/> to own the serialisation and reconstruction of its contained entities:
    /// <list type="bullet">
    ///   <item><description>Spawn path: the single <see cref="Create"/> factory reads the optional <c>"contents"</c> string array from prefab data and stores the names as pending contents for <see cref="EntityPopulator"/>.</description></item>
    ///   <item><description>Save path: <see cref="WriteState"/> writes the contained entities as a <c>"entities"</c> JSON array, using the serialise callback threaded in via <see cref="StateWriter.Context"/>.</description></item>
    ///   <item><description>Restore path: the same <see cref="Create"/> factory reads the <c>"entities"</c> JSON array from save data and reconstructs each contained entity using the callback threaded in via <see cref="PrefabData.Context"/>.</description></item>
    /// </list>
    /// The <see cref="EntitySerialiser"/> and <see cref="EntityReconstructor"/> carry no hardcoded knowledge of a reserved child-entity key — all awareness of <c>"entities"</c> lives here in the component.
    /// </remarks>
    public class InventoryComponent : Component, IPopulatableContainer, IStatefulPart
    {
        /// <summary> The entities currently stored in this inventory. </summary>
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();

        /// <summary> The prefab names this inventory was authored to be populated with; empty after a restore path. </summary>
        private readonly IReadOnlyList<String> _pendingContents;


        /// <summary> Initialises a new instance of the <see cref="InventoryComponent"/> class with an explicit pending-contents list. </summary>
        /// <param name="entity"> The entity that has the ability to hold other entities. </param>
        /// <param name="pendingContents"> The ordered list of prefab names to be instantiated and inserted by <see cref="EntityPopulator"/>; pass an empty list when restoring from a save. </param>
        public InventoryComponent(Entity entity, IReadOnlyList<String> pendingContents) : base(entity)
        {
            _pendingContents = pendingContents;
        }


        /// <summary> Creates or restores an <see cref="InventoryComponent"/> from the supplied data, discriminating between spawn and restore paths by data shape. </summary>
        /// <remarks>
        /// Restore path — save data contains an <c>"entities"</c> JSON array:
        /// reconstructs each contained entity via the <c>Func&lt;JsonElement, Entity&gt;</c> reconstruct callback threaded in via <see cref="PrefabData.Context"/>.
        /// <see cref="PrefabData.Context"/> must be that callback when <c>"entities"</c> is present; its absence is an authoring error and throws immediately.
        /// No pending contents are recorded.
        ///
        /// Spawn path — prefab data contains no <c>"entities"</c> array:
        /// reads the optional <c>"contents"</c> string array and stores the names as pending contents for later population by <see cref="EntityPopulator"/>.
        ///
        /// Discriminating on data shape (key presence) rather than callback presence mirrors <see cref="HealthComponent.Create"/> and keeps the mode signal in the data, not in the infrastructure.
        /// </remarks>
        /// <param name="entity"> The entity the component will be attached to. </param>
        /// <param name="data"> The component's parsed data; presence of an <c>"entities"</c> array selects the restore path; its absence selects the spawn path. </param>
        /// <returns> A fully constructed <see cref="InventoryComponent"/> with either pending contents (spawn) or reconstructed child entities (restore) attached. </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <c>"entities"</c> is present but <see cref="PrefabData.Context"/> is not the expected <c>Func&lt;JsonElement, Entity&gt;</c> reconstruct callback — this indicates a wiring error in the reconstruction caller.
        /// Also thrown when <see cref="AddEntity"/> rejects a reconstructed child, indicating corrupt save data (duplicate entity uid or a containment cycle).
        /// </exception>
        [ComponentFactory]
        private static InventoryComponent Create(Entity entity, PrefabData data)
        {
            Boolean                        isRestore      = data.TryGetJsonArray("entities", out IReadOnlyList<JsonElement>? entityElements);
            InventoryComponent             component;

            if (isRestore)
            {
                Boolean callbackPresent = data.Context is Func<JsonElement, Entity>;

                if (!callbackPresent)
                {
                    throw new InvalidOperationException(
                        $"{nameof(InventoryComponent)}: save data contains an 'entities' array but no reconstruct callback was found in " +
                        $"{nameof(PrefabData)}.{nameof(PrefabData.Context)}. Ensure {nameof(EntityReconstructor)} sets the callback before invoking component factories.");
                }

                Func<JsonElement, Entity> reconstruct    = (Func<JsonElement, Entity>)data.Context!;
                InventoryComponent        emptyComponent = new InventoryComponent(entity, new List<String>());

                foreach (JsonElement entityElement in entityElements!)
                {
                    Entity  nested = reconstruct(entityElement);
                    Boolean added  = emptyComponent.AddEntity(nested);

                    if (!added)
                    {
                        throw new InvalidOperationException(
                            $"Entity save data: could not add nested entity '{nested.UId}' to " +
                            $"{nameof(InventoryComponent)} on entity '{entity.UId}'. " +
                            $"The save data may contain a duplicate entity uid or a containment cycle.");
                    }
                }

                component = emptyComponent;
            }
            else
            {
                Boolean               hasPending = data.TryGetStringArray("contents", out IReadOnlyList<String>? pending);
                IReadOnlyList<String> contents   = hasPending ? pending! : new List<String>();
                component = new InventoryComponent(entity, contents);
            }

            return component;
        }


        /// <inheritdoc/>
        public Boolean AddEntity(Entity entity)
        {
            Boolean wouldCycle = entity.Equals(ParentEntity)
                || entity.GetContainers().Any(c => c.Contains(ParentEntity));
            Boolean result = !wouldCycle && _entities.Add(entity);
            return result;
        }


        /// <inheritdoc/>
        public Boolean RemoveEntity(Entity entity) => _entities.Remove(entity);


        /// <inheritdoc/>
        public IReadOnlyCollection<Entity> GetEntities() => _entities.ToArray();


        /// <inheritdoc/>
        public IReadOnlyCollection<String> GetPendingContents() => _pendingContents.ToArray();


        /// <summary> Writes the set of contained entities as a <c>"entities"</c> JSON array using the serialise callback provided via <see cref="StateWriter.Context"/>. </summary>
        /// <remarks>
        /// <see cref="StateWriter.Context"/> must be a <c>Func&lt;Entity, JsonObject&gt;</c> serialise callback whenever the inventory is non-empty.
        /// An empty inventory writes an empty array without needing the callback.
        /// A populated inventory with no callback throws immediately — silently omitting held entities would produce corrupt save data.
        /// </remarks>
        /// <param name="writer"> The state writer to receive the <c>"entities"</c> array. </param>
        /// <exception cref="InvalidOperationException"> Thrown when the inventory holds one or more entities but <see cref="StateWriter.Context"/> is not the expected <c>Func&lt;Entity, JsonObject&gt;</c> serialise callback — this indicates a wiring error in the serialisation caller. </exception>
        public void WriteState(StateWriter writer)
        {
            JsonArray array          = new JsonArray();
            Boolean   hasEntities    = _entities.Count > 0;
            Boolean   hasSerialiser  = writer.Context is Func<Entity, JsonObject>;

            if (hasEntities && !hasSerialiser)
            {
                throw new InvalidOperationException(
                    $"{nameof(InventoryComponent)}: this inventory contains {_entities.Count} entity/entities but no serialise callback was found in " +
                    $"{nameof(StateWriter)}.{nameof(StateWriter.Context)}. Ensure {nameof(EntitySerialiser)} sets the callback before invoking {nameof(WriteState)}.");
            }

            if (hasSerialiser)
            {
                Func<Entity, JsonObject> serialise = (Func<Entity, JsonObject>)writer.Context!;

                foreach (Entity held in _entities)
                {
                    array.Add(serialise(held));
                }
            }

            writer.SetJsonArray("entities", array);
        }
    }
}
