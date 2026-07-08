using Jaypen.Logging;
using Jaypen.Singletons;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The central manager for all UI elements displayed during the game. </summary>
    public partial class UIManager : SingletonControl<UIManager>
    {
        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<UIManager>();
    }
}
