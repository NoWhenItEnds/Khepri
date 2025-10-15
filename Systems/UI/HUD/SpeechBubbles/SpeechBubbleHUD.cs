using Godot;
using Khepri.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.UI.HUD.SpeechBubbles
{
    /// <summary> The HUD window to render speech bubbles onto. </summary>
    public partial class SpeechBubbleHUD : Control
    {
        /// <summary> The prefab to use when spawning bubble items. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _speechBubblePrefab;


        /// <summary> How many nodes the pool should contain. </summary>
        [ExportGroup("Settings")]
        [Export] private Int32 _poolSize = 50;

        /// <summary> How long each item should be visible for, in seconds. </summary>
        [Export] private Single _itemLifetime = 10f;


        /// <summary> The pool containing all the items and how much time they have left before fading. Values of -10f indicate that they are unused. </summary>
        private Dictionary<SpeechBubbleItem, Single> _itemPool = new Dictionary<SpeechBubbleItem, Single>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            //  Populate the item pool.
            for (Int32 i = 0; i < _poolSize; i++)
            {
                SpeechBubbleItem item = _speechBubblePrefab.Instantiate<SpeechBubbleItem>();
                AddChild(item);
                item.GlobalPosition = new Vector2(-100f, -100f);
                _itemPool.Add(item, -10f);
            }
        }


        /// <summary> Spawns a speech bubble on the HUD containing text. </summary>
        /// <param name="text"> The BBCode formatted text to display. </param>
        /// <param name="speakingNode"> A reference to the node that has 'spoken'. </param>
        public void SpawnText(String text, Node3D speakingNode)
        {
            // Build offset.
            IEnumerable<SpeechBubbleItem> existingBubbles = _itemPool.Where(x => x.Key.SpeakingNode == speakingNode).Select(x => x.Key);
            Single offset = 0f;
            foreach (SpeechBubbleItem existing in existingBubbles)
            {
                Single current = existing.GetBottomOffset();
                if (offset < current) { offset = current; }
            }

            SpeechBubbleItem? item = _itemPool.FirstOrDefault(x => x.Value == -10f).Key ?? null;
            if(item != null)
            {
                item.Initialise(text, speakingNode, offset);
                _itemPool[item] = _itemLifetime;
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            foreach (KeyValuePair<SpeechBubbleItem, Single> item in _itemPool)
            {
                if (item.Value >= 0f)    // The item is still active.
                {
                    _itemPool[item.Key] -= (Single)delta;
                }
                else if(item.Value != -10f)  // The item has died, but hasn't been cleaned.
                {
                    item.Key.CleanUp();
                    _itemPool[item.Key] = -10f;
                    item.Key.GlobalPosition = new Vector2(-100f, -100f);
                }
            }
        }
    }
}
