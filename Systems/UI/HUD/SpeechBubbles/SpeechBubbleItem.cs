using Godot;
using Khepri.Nodes;
using System;

namespace Khepri.UI.HUD.SpeechBubbles
{
    /// <summary> A single speech bubble item representing a single thought / line. </summary>
    public partial class SpeechBubbleItem : PanelContainer
    {
        /// <summary> The label used to display the text. </summary>
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _label;

        /// <summary> The maximum width, in pixels, the bubble can grow to. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _maxWidth = 256f;

        /// <summary> The font to use in the popup. </summary>
        [Export] private Font _font = ThemeDB.FallbackFont;

        /// <summary> How quickly the bubble moves up across the screen. </summary>
        [Export] private Single _moveSpeed = 20f;


        /// <summary> A reference to the node that has 'spoken'. </summary>
        public Node3D? SpeakingNode { get; private set; } = null;


        /// <summary> The amount to offset the item by if there is already another speech bubble active for the node. </summary>
        private Single _offset = 0f;

        /// <summary> The amount of time that has progressed since the item was initialised. </summary>
        private Single _currentDelta = 0f;

        /// <summary> A reference to the game world camera. </summary>
        private WorldCamera _worldCamera;

        /// <summary> The colour to use for transparency. </summary>
        private readonly Color TRANSPARENT = new Color(1f, 1f, 1f, 0f);


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldCamera = WorldCamera.Instance;
            Size = Vector2.Zero;
        }


        /// <summary> Initialise the item for new text. Handles repositioning itself. </summary>
        /// <param name="text"> The text to display. </param>
        /// <param name="speakingNode"> A reference to the node that has 'spoken'. </param>
        /// <param name="offset"> The amount to offset the item by if there is already another speech bubble active for the node. </param>
        public void Initialise(String text, Node3D speakingNode, Single offset)
        {
            SpeakingNode = speakingNode;
            _label.Text = text;
            _offset = offset;
            _currentDelta = 0f;

            SetSize(text);

            Modulate = TRANSPARENT;
            TransitionItem(true);
        }


        /// <summary> Get the offset of the item's bottom corner from its tracked node. </summary>
        public Single GetBottomOffset()
        {
            if (SpeakingNode == null) { return 0f; }
            return GlobalPosition.Y - _worldCamera.GetCamera().UnprojectPosition(SpeakingNode.GlobalPosition).Y + Size.Y;
        }


        /// <summary> Sets the bubble's size depending upon the text. </summary>
        /// <param name="text"> The string that is going to be displayed. </param>
        private void SetSize(String text)
        {
            Vector2 textSize = _font.GetStringSize(text);
            Single width = textSize.X;
            Single height = textSize.Y;

            if (textSize.X > _maxWidth)
            {
                width = _maxWidth;
                height = textSize.Y * (Int32)(textSize.X / _maxWidth);
            }

            Size = new Vector2(width, height);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if(SpeakingNode != null)
            {
                Vector2 screenPosition = _worldCamera.GetCamera().UnprojectPosition(SpeakingNode.GlobalPosition) + new Vector2(0f, _offset - _currentDelta * _moveSpeed);
                GlobalPosition = screenPosition;

                _currentDelta += (Single)delta;
            }
        }


        /// <summary> Trigger the item's cleanup routine. </summary>
        public void CleanUp()
        {
            SpeakingNode = null;
            TransitionItem(false);
        }


        /// <summary> Transition the item in or out. </summary>
        /// <param name="toVisible"> Whether the transition is to visible or not. </param>
        private void TransitionItem(Boolean toVisible)
        {
            Tween tween = CreateTween();
            tween.TweenProperty(this, "modulate", toVisible ? Colors.White : TRANSPARENT, 1f);
        }
    }
}
