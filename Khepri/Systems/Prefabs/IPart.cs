namespace Khepri.Prefabs
{
    /// <summary> A part that belongs to and references a specific owning object. This is the generic counterpart to the entity domain's <c>Component</c> base class. </summary>
    /// <typeparam name="TOwner"> The type of the object that owns this part. </typeparam>
    public interface IPart<TOwner>
    {
        /// <summary> The owning object that this part is attached to. </summary>
        TOwner Owner { get; }
    }
}
