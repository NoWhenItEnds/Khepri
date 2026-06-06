namespace Khepri.Entities.Actions
{
    /// <summary> The outcome of attempting an <see cref="EntityAction"/>. </summary>
    public enum ActionResult
    {
        /// <summary> No action was attempted (the brain produced nothing this turn). </summary>
        None = 0,

        /// <summary> The action took effect. </summary>
        Succeeded = 1,

        /// <summary> The action was attempted but rejected by the world (e.g. an illegal move). </summary>
        Failed = 2,
    }
}
