using System;
using AsyncTCPLib;

namespace Socks5S.Plugin
{
    
    public interface IPlugin
    {

        #region General

        /// <summary>
        /// Name of the plugin
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Plugin processing order
        /// </summary>
        ushort Ranking { get; }

        /// <summary>
        /// When the plugin instance was created, Init must be called to ensure everything's hooked
        /// </summary>
        /// <param name="instance">Singleton instace of type Program</param>
        /// <returns>True if plugin was loaded successfully, false if not (program will exit then)</returns>
        bool Init(Program instance);

        #endregion

        #region Plugin

        IRawHandler RawHandler { get; }

        IStateDependentHandler StateDependentHandler { get; }

        #endregion

    }

}
