using System;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Socks5S.Plugin.Default
{
    internal class AssemblyPluginLoader : IPluginLoader
    {

        /// <summary>
        /// Load all IPlugin implementations from executing .net assembly
        /// </summary>
        /// <returns>A collection of IPlugin implementations</returns>
        IEnumerable<IPlugin> IPluginLoader.LoadPlugins(Configuration.Configuration config)
        {
            Type IPluginType = typeof(IPlugin);
            Assembly pluginAssembly = Assembly.GetExecutingAssembly();

            IEnumerable<IPlugin> criteria =
                (from Type t in pluginAssembly.GetTypes()
                    where t.GetInterfaces().Contains(IPluginType)
                    let plugin = (IPlugin)pluginAssembly.CreateInstance(t.FullName, false)
                    select plugin);

            foreach (IPlugin foundPlugin in criteria)
                yield return foundPlugin;
        }

        /// <summary>
        /// Load all IRawHandler implementations from executing .net assembly
        /// </summary>
        /// <returns>A collection of IRawHandler implementations</returns>
        IEnumerable<IRawHandler> IPluginLoader.LoadRawHandler(Configuration.Configuration config)
        {
            Type IPluginType = typeof(IRawHandler);
            Assembly pluginAssembly = Assembly.GetExecutingAssembly();

            IEnumerable<IRawHandler> criteria =
                (from Type t in pluginAssembly.GetTypes()
                    where t.GetInterfaces().Contains(IPluginType)
                    let plugin = (IRawHandler)pluginAssembly.CreateInstance(t.FullName, false)
                    select plugin);

            foreach (IRawHandler foundPlugin in criteria)
                yield return foundPlugin;
        }

        /// <summary>
        /// Load all IStateDependentHandler implementations from executing .net assembly
        /// </summary>
        /// <returns>A collection of IStateDependentHandler implementations</returns>
        IEnumerable<IStateDependentHandler> IPluginLoader.LoadStateDependentHandler(Configuration.Configuration config)
        {
            Type IPluginType = typeof(IStateDependentHandler);
            Assembly pluginAssembly = Assembly.GetExecutingAssembly();

            IEnumerable<IStateDependentHandler> criteria =
                (from Type t in pluginAssembly.GetTypes()
                    where t.GetInterfaces().Contains(IPluginType)
                    let plugin = (IStateDependentHandler)pluginAssembly.CreateInstance(t.FullName, false)
                    select plugin);

            foreach (IStateDependentHandler foundPlugin in criteria)
                yield return foundPlugin;
        }

    }
}
