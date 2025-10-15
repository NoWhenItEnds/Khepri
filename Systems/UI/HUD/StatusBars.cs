using Godot;
using Khepri.Controllers;
using Khepri.Types.Extensions;
using System;
using System.Linq;

namespace Khepri.UI.HUD
{
    /// <summary> The status bars representing the player's needs. </summary>
    public partial class StatusBars : Control
    {
        /// <summary> The container holding the player's needs. </summary>
        [ExportGroup("Nodes")]
        [Export] private VBoxContainer _needsContainer;

        /// <summary> The bar for the player's stamina. </summary>
        [Export] private TextureProgressBar _staminaBar;

        /// <summary> The bar for the player's health. </summary>
        [Export] private TextureProgressBar _healthBar;

        /// <summary> The bar for the player's hunger. </summary>
        [Export] private TextureProgressBar _hungerBar;

        /// <summary> The bar for the player's fatigue. </summary>
        [Export] private TextureProgressBar _fatigueBar;

        /// <summary> The bar for the player's entertainment. </summary>
        [Export] private TextureProgressBar _entertainmentBar;

        /// <summary> The slider to indicate the affect of the lowest need upon the player's max stamina. </summary>
        [Export] private TextureRect _statusBarSlider;


        /// <summary> A reference to the player controller. </summary>
        private PlayerController _playerController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = PlayerController.Instance;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            _staminaBar.Value = _playerController.PlayerBeing.Needs.CurrentStamina;
            _healthBar.Value = _playerController.PlayerBeing.Needs.CurrentHealth;
            _hungerBar.Value = _playerController.PlayerBeing.Needs.CurrentHunger;
            _fatigueBar.Value = _playerController.PlayerBeing.Needs.CurrentFatigue;
            _entertainmentBar.Value = _playerController.PlayerBeing.Needs.CurrentEntertainment;

            TextureProgressBar highestNeed = OrderNeedsBars();
            Single sliderPosition = (Single)Mathf.Lerp(0f, 255f, highestNeed.Value / 100f) - 2f;    // Small offset to bring it inline.
            _statusBarSlider.SetPosition(new Vector2(sliderPosition, 0f));
        }


        /// <summary> Order the needs bars in descending order. </summary>
        /// <returns> The bar representing the highest need. </returns>
        private TextureProgressBar OrderNeedsBars()
        {
            TextureProgressBar[] childrenBars = _needsContainer.GetChildrenOfType<TextureProgressBar>();
            Boolean isSorted = IsSorted(childrenBars);
            if (!isSorted)
            {
                IOrderedEnumerable<TextureProgressBar> sortedBars = childrenBars.OrderBy(x => x.Value);
                foreach (TextureProgressBar bar in sortedBars)
                {
                    _needsContainer.RemoveChild(bar);
                    _needsContainer.AddChild(bar);
                }
            }

            if (_needsContainer.GetChild(0) is TextureProgressBar child)
            {
                return child;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Unable to find a texture progress bar in the needs container.");
            }
        }


        /// <summary> Determine whether the needs are sorted. </summary>
        /// <param name="array"> The array of bars. </param>
        /// <returns> Whether the array needs to be sorted. </returns>
        public Boolean IsSorted(TextureProgressBar[] array)
        {
            Boolean result = true;
            for (Int32 i = 1; i < array.Length; i++)
            {
                if (array[i - 1].Value < array[i].Value)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
    }
}
