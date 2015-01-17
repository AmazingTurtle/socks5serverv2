using System;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Socks5S.Plugin.Default
{
    internal class FilePluginLoader : IPluginLoader
    {

        /// <summary>
        /// Load all IPlugin implementations from .net assemblies in configured plugin directory
        /// </summary>
        /// <returns>A collection of IPlugin implementations</returns>
        IEnumerable<IPlugin> IPluginLoader.LoadPlugins(Configuration.Configuration config)
        {
            Type IPluginType = typeof(IPlugin);
            foreach (string file in Directory.GetFiles(CompileVars.PluginDirectory, "*.dll"))
            {
                Assembly pluginAssembly = Assembly.LoadFile(file);

                IEnumerable<IPlugin> criteria =
                    (from Type t in pluginAssembly.GetTypes()
                     where t.GetInterfaces().Contains(IPluginType)
                     let plugin = (IPlugin)pluginAssembly.CreateInstance(t.FullName, false)
                     select plugin);

                foreach(IPlugin foundPlugin in criteria)
                    yield return foundPlugin;
            }
        }

        /// <summary>
        /// Load all IRawHandler implementations from .net assemblies in configured plugin directory
        /// </summary>
        /// <returns>A collection of IRawHandler implementations</returns>
        IEnumerable<IRawHandler> IPluginLoader.LoadRawHandler(Configuration.Configuration config)
        {
            Type IPluginType = typeof(IRawHandler);
            foreach (string file in Directory.GetFiles(CompileVars.PluginDirectory, "*.dll"))
            {
                Assembly pluginAssembly = Assembly.LoadFile(file);

                IEnumerable<IRawHandler> criteria =
                    (from Type t in pluginAssembly.GetTypes()
                     where t.GetInterfaces().Contains(IPluginType)
                     let plugin = (IRawHandler)pluginAssembly.CreateInstance(t.FullName, false)
                     select plugin);

                foreach (IRawHandler foundPlugin in criteria)
                    yield return foundPlugin;
            }
        }

        /// <summary>
        /// Load all IStateDependentHandler implementations from .net assemblies in configured plugin directory
        /// </summary>
        /// <returns>A collection of IStateDependentHandler implementations</returns>
        IEnumerable<IStateDependentHandler> IPluginLoader.LoadStateDependentHandler(Configuration.Configuration config)
        {
            Type IPluginType = typeof(IStateDependentHandler);
            foreach (string file in Directory.GetFiles(CompileVars.PluginDirectory, "*.dll"))
            {
                Assembly pluginAssembly = Assembly.LoadFile(file);

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
}
