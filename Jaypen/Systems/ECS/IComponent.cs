namespace Jaypen.Utilities.ECS
{
    /// <summary> Marker interface that every ECS component must implement. </summary>
    /// <remarks>
    /// Intentionally memberless — its sole purpose is to provide a meaningful compile-time bound for
    /// the generic parameter <c>T</c> on <see cref="IComponentHolder{T}"/> and its derivatives,
    /// preventing arbitrary types from satisfying the component slot.
    /// </remarks>
    public interface IComponent
    {
    }
}
