using Godot;
using System;

namespace Khepri.Types.Extensions
{
    /// <summary> Helpful methods for working with collision shapes. </summary>
    public static class CollisionShape3DExtensions
    {
        /// <summary> Gets a collision shape's bounding box. </summary>
        /// <param name="collisionShape"> The collision shape. </param>
        /// <returns> The bounding box the collision shape fits within. </returns>
        /// <exception cref="NotImplementedException"> The given collision shape hasn't been implemented. </exception>
        public static Aabb GetAabb(this CollisionShape3D collisionShape)
        {
            switch (collisionShape.Shape)
            {
                case BoxShape3D box:
                    return new Aabb(collisionShape.GlobalPosition, box.Size);
                case CylinderShape3D cylinder:
                    return new Aabb(collisionShape.GlobalPosition, new Vector3(cylinder.Radius * 2f, cylinder.Height, cylinder.Radius * 2f));
                case CapsuleShape3D capsule:
                    return new Aabb(collisionShape.GlobalPosition, new Vector3(capsule.Radius * 2f, capsule.Height, capsule.Radius * 2f));
                default:
                    throw new NotImplementedException($"The given collision shape, {collisionShape.Shape.GetType()}, hasn't been implemented.");
            }
        }
    }
}
