using System;
using Godot;
using Khepri.Nodes.Extensions;
using Khepri.Resources.Celestial;
using Khepri.Types;

namespace Khepri.UI.Windows.Components
{
    /// <summary> An item representing an object in the telescope's view. </summary>
    public partial class TelescopeItem : Control, IPoolable<StarResource>
    {
        [Export] private Label _label;

        [Export] private ColorRect _rect;

        /// <inheritdoc/>
        public StarResource Resource { get; set; }


        /// <summary> A reference to the window this item is parented to. </summary>
        private TelescopeWindow _window;

        public Vector2 TestPosition;


        /// <summary> Create the telescope item by settings its internal variables. </summary>
        /// <param name="window"> A reference to the window this item is parented to. </param>
        /// <param name="resource"> The raw data used to build the item. </param>
        /// <param name="position"> The star's position on the screen. </param>
        public void Initialise(TelescopeWindow window, StarResource resource, Vector2 position)
        {
            _window = window;
            GlobalPosition = position;
            TestPosition = position;
            if (this is IPoolable<StarResource> poolable)
            {
                poolable.Initialise(resource);
                _label.Text = resource.ProperName;
                _rect.Color = resource.CalculateColour();
            }
        }


        /// <inheritdoc/>
        public void FreeObject() => _window.StarPool.FreeObject(this);
    }
}
