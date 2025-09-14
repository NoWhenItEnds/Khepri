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
                    return new Aabb(collisionShape.GlobalPosition - (box.Size * 0.5f), box.Size);
                case CylinderShape3D cylinder:
                    Vector3 cylinderSize = new Vector3(cylinder.Radius * 2f, cylinder.Height, cylinder.Radius * 2f);
                    return new Aabb(collisionShape.GlobalPosition - (cylinderSize * 0.5f), cylinderSize);
                case CapsuleShape3D capsule:
                    Vector3 capsuleSize = new Vector3(capsule.Radius * 2f, capsule.Height, capsule.Radius * 2f);
                    return new Aabb(collisionShape.GlobalPosition - (capsuleSize * 0.5f), capsuleSize);
                case ConcavePolygonShape3D polygon:
                    Aabb result = new Aabb();
                    foreach (Vector3 face in polygon.GetFaces()) { result = result.Expand(face); }
                    return result;
                default:
                    throw new NotImplementedException($"The given collision shape, {collisionShape.Shape.GetType()}, hasn't been implemented.");
            }
        }
    }
}
