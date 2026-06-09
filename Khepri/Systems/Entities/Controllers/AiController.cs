using Jaypen.Utilities.Logging;
using Microsoft.Extensions.Logging;

namespace Khepri.Entities.Controllers
{
    /// <summary> A placeholder non-player brain. The scheduler asks it to act once per world step. </summary>
    public sealed class AiController : EntityController
    {
        /// <summary> The logger instance the controller uses. </summary>
        private static readonly ILogger Logger = Log.For<AiController>();


        /// <summary> Initialises a new AI controller for the given entity. </summary>
        /// <param name="owner"> The entity this brain drives. </param>
        public AiController(Entity owner) : base(owner)
        {
        }
    }
}
