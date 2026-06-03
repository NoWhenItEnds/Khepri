namespace Khepri.Prefabs
{
    /// <summary> Implemented by parts that carry runtime state worth persisting across save/load cycles. </summary>
    /// <remarks>
    /// Only implement this interface on a part whose runtime state differs from what the prefab already declares.
    /// Parts with no rolling or dynamic state — such as marker components whose identity is fully captured by their type-key — need not implement this interface; the serialiser writes their type-key alone.
    /// The complementary restore path is handled by the same <c>[ComponentFactory]</c>-decorated method, which discriminates between spawn and restore data by inspecting which keys are present.
    /// </remarks>
    public interface IStatefulPart
    {
        /// <summary> Writes this part's runtime state into <paramref name="writer"/> so that a restore factory can reconstruct an identical instance without consulting the original prefab. </summary>
        /// <param name="writer"> The writer to receive the state properties. Each property key must be unique within this call; duplicates cause <see cref="StateWriter"/> to throw. </param>
        void WriteState(StateWriter writer);
    }
}
