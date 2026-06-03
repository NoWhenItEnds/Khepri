using System;
using Godot;

namespace Khepri.Rooms.Features
{
    /// <summary> A feature that supplies a fixed textual description for a room, backing <see cref="Room.BuildDescription"/>. </summary>
    [GlobalClass]
    public partial class DescriptionFeature : Feature
    {
        /// <summary> The description text returned to callers of <see cref="Room.BuildDescription"/>. Must not be blank. </summary>
        [Export(PropertyHint.MultilineText)] public String Text { get; set; } = String.Empty;


        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException"> Thrown when <see cref="Text"/> was left blank in the authored resource. </exception>
        public override void OnInstantiate()
        {
            if (String.IsNullOrWhiteSpace(Text))
            {
                throw new InvalidOperationException($"{nameof(DescriptionFeature)} on room '{Owner.UId}' has blank Text.");
            }
        }
    }
}
