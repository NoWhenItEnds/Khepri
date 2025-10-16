using System;
using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors;

namespace Khepri.Resources
{
    /// <summary> The data component of a game entity. </summary>
    public abstract partial class EntityResource : Resource
    {
        /// <summary> The resource's identifying kind. </summary>
        [ExportGroup("General")]
        [Export] public String Id { get; set; }

        /// <summary> An array of descriptions for the given resource. </summary>
        [Export] public String[] Descriptions { get; set; }


        /// <summary> An instance of random to use. </summary>
        private static Random RANDOM = new Random();


        /// <summary> The data component of a game entity. </summary>
        public EntityResource() { }


        /// <summary> The internal logic to use when the entity is examined. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public void Examine(Being activatingEntity)
        {
            if (Descriptions.Length > 0)
            {
                Int32 index = RANDOM.Next(0, Descriptions.Length);
                String description = Descriptions[index];
                UIController.Instance.SpawnSpeechBubble(description, activatingEntity);
            }
        }
    }
}
