using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Plugin
{
    public interface IPluginLoader
    {

        /// <summary>
        /// Load all IPlugin implementations
        /// </summary>
        /// <returns>A collection of IPlugin implementations</returns>
        IEnumerable<IPlugin> LoadPlugins(Configuration.Configuration config);

        /// <summary>
        /// Load all IRawHandler implementations
        /// </summary>
        /// <returns>A collection of IRawHandler implementations</returns>
        IEnumerable<IRawHandler> LoadRawHandler(Configuration.Configuration config);

        /// <summary>
        /// Load all IStateDependentHandler implementations
        /// </summary>
        /// <returns>A collection of IStateDependentHandler implementations</returns>
        IEnumerable<IStateDependentHandler> LoadStateDependentHandler(Configuration.Configuration config);

    }
}
