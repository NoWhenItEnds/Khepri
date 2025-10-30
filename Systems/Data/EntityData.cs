using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Khepri.Controllers;
using Khepri.Entities.Actors;

namespace Khepri.Data
{
    /// <summary> The data component of a game entity. </summary>
    public abstract class EntityData : IEquatable<EntityData>
    {
        /// <summary> The instance's unique identifier. </summary>
        [JsonPropertyName("uid")]
        public Guid UId { get; init; } = Guid.NewGuid();

        /// <summary> The common kind of entity this data represents. </summary>
        /// <remarks> This should match the template / prefab's filename. </remarks>
        [JsonPropertyName("kind"), Required]
        public required String Kind { get; init; }

        /// <summary> An array of descriptions describing the entity. </summary>
        [JsonPropertyName("descriptions"), Required]
        public required String[] Descriptions { get; init; }


        /// <summary> The data component of a game entity. </summary>
        public EntityData()
        {
            DataController.Instance.RegisterData(this);
        }


        /// <summary> The internal logic to use when the entity is examined. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public void Examine(ActorNode activatingEntity)
        {
            if (Descriptions.Length > 0)
            {
                Int32 index = new Random().Next(0, Descriptions.Length);
                String description = Descriptions[index];
                UIController.Instance.SpawnSpeechBubble(description, activatingEntity);
            }
        }


        /// <inheritdoc/>
        public Boolean Equals(EntityData? other) => UId == other?.UId;


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(UId);
    }
}
