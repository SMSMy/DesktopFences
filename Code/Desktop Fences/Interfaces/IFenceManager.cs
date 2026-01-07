using System.Collections.Generic;

namespace Desktop_Fences.Interfaces
{
    /// <summary>
    /// Interface for the main Fence Manager that handles fence operations.
    /// </summary>
    public interface IFenceManager
    {
        /// <summary>
        /// Reloads all fences from the configuration.
        /// </summary>
        void ReloadFences();

        /// <summary>
        /// Gets the fence data list.
        /// </summary>
        List<dynamic> GetFenceData();

        /// <summary>
        /// Creates a new fence at the specified location.
        /// </summary>
        void CreateNewFence(string name, string type, int x, int y);

        /// <summary>
        /// Gets the portal fences dictionary.
        /// </summary>
        Dictionary<dynamic, PortalFenceManager> GetPortalFences();
    }
}
