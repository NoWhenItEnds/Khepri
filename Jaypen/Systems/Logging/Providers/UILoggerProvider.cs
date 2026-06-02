using System;
using Godot;
using Microsoft.Extensions.Logging;

namespace Jaypen.Utilities.Logging.Providers
{
    /// <summary> MEL provider that creates <see cref="UILogger"/> instances, routing log output to a <see cref="RichTextLabel"/> in the game's UI. </summary>
    /// <remarks> he target label may be set after construction via <see cref="SetTarget"/> . </remarks>
    public sealed class UILoggerProvider : ILoggerProvider
    {
        /// <summary> Default maximum log line count used when no explicit limit is supplied. </summary>
        private const Int32 DefaultMaxLines = 200;

        /// <summary> The label node that receives all UI log output; null until <see cref="SetTarget"/> is called. </summary>
        private RichTextLabel? _target;

        /// <summary> Maximum number of log paragraphs retained before the oldest is discarded. </summary>
        private readonly Int32 _maxLines;


        /// <summary> Initialises the provider without a target; call <see cref="SetTarget"/> once the scene node is ready. </summary>
        /// <param name="maxLines"> Maximum number of log paragraphs to retain in the label before the oldest is discarded. </param>
        public UILoggerProvider(Int32 maxLines = DefaultMaxLines)
        {
            _maxLines = maxLines;
        }


        /// <summary> The <see cref="RichTextLabel"/> that all loggers created by this provider write to; null when no target has been set yet. </summary>
        internal RichTextLabel? Target => _target;

        /// <summary> The maximum number of retained log paragraphs configured for this provider. </summary>
        internal Int32 MaxLines => _maxLines;


        /// <summary> Assigns the <see cref="RichTextLabel"/> that log output will be appended to. </summary>
        /// <param name="target"> The label node that receives all UI log output; must have BBCode enabled. </param>
        public void SetTarget(RichTextLabel target)
        {
            if (_target != null)
            {
                GD.PushWarning("[UILoggerProvider] UI log target replaced; prior RichTextLabel will no longer receive log output.");
            }

            _target = target;
        }


        /// <inheritdoc/>
        public ILogger CreateLogger(String categoryName) => new UILogger(categoryName, this);


        /// <inheritdoc/>
        public void Dispose() { }
    }
}
