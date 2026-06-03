using System;

namespace Khepri.Prefabs
{
    /// <summary> An object that can accept and hold parts of type <typeparamref name="TPart"/>. Implement this on the owner type so that <see cref="PrefabBuilder{TOwner,TPart}"/> can attach parts without knowing the concrete type. </summary>
    /// <typeparam name="TPart"> The kind of part this container accepts. </typeparam>
    public interface IPartContainer<TPart>
    {
        /// <summary> Adds <paramref name="part"/> to this container. </summary>
        /// <param name="part"> The part to add. </param>
        /// <returns> <c>true</c> if the part was added; <c>false</c> when an equivalent part already exists. </returns>
        Boolean Add(TPart part);
    }
}
