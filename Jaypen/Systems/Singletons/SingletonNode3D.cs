using System;
using Godot;

namespace Jaypen.Utilities.Singletons
{
    /// <summary> Singleton base class backed by <see cref="Node3D"/>. Use this when the singleton participates in 3D scene rendering or physics. </summary>
    /// <typeparam name="T">The concrete singleton type — must derive from this class (CRTP).</typeparam>
    public abstract partial class SingletonNode3D<T> : Node3D where T : SingletonNode3D<T>
    {
        /// <summary> The single live instance of <typeparamref name="T"/>, or <c>null</c> if no instance has entered the scene tree yet. </summary>
        public static T? Instance => SingletonBehaviour<T>.Instance;


        /// <inheritdoc/>
        public override void _EnterTree()
        {
            base._EnterTree();

            if (!Engine.IsEditorHint())
            {
                Boolean registered = SingletonBehaviour<T>.TryRegister((T)this);

                if (!registered)
                {
                    QueueFree();
                }
            }
        }


        /// <inheritdoc/>
        public override void _ExitTree()
        {
            SingletonBehaviour<T>.Unregister((T)this);
            base._ExitTree();
        }


        /// <inheritdoc/>
        public override void _Notification(int what)
        {
            if (what == NotificationWMCloseRequest && Instance == this as T)
            {
                SingletonBehaviour<T>.Unregister((T)this);
                QueueFree();
            }
        }
    }
}
