using Godot;

namespace Khepri.Entities.Items.Components
{
    /// <summary> A type of entity representing something that can be grabbed and placed in an inventory. </summary>
    public class ItemDataComponent
    {
        /// <summary> Relative points representing the grid cells the item occupies in an inventory. </summary>
        public Vector2I[] Points { get; private set; }


        public ItemDataComponent(Vector2I[] points)
        {
            Points = points;
        }
    }
}
