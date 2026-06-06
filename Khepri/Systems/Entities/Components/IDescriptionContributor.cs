using Khepri.Descriptions;

namespace Khepri.Entities.Components
{
    /// <summary> Implemented by the components that have prose to add to their entity's description. </summary>
    public interface IDescriptionContributor
    {
        /// <summary> Appends this component's contribution to its entity's description. </summary>
        /// <param name="builder"> The builder assembling the owning entity's description. </param>
        public void Contribute(DescriptionBuilder builder);
    }
}
