namespace Khepri.Entities
{
    /// <summary> Tags the entity as one that can be directly controlled. </summary>
    public interface IControllable
    {
        /// <summary> Handle the input sent to the entity by it's controller. </summary>
        /// <param name="input"> The input data class to interpret. </param>
        public void HandleInput(IInput input);
    }
}
