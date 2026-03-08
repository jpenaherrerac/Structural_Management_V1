using System;
using System.Collections.Generic;
using App.Domain.Entities.Sap;

namespace App.Application.Interfaces
{
    /// <summary>
    /// Manages discovery, attachment, creation, and switching of SAP2000 COM sessions.
    /// Implemented in App.SAP2000; consumed by use-cases and UI.
    /// </summary>
    public interface ISapConnectionManager
    {
        /// <summary>Current active session, or null when disconnected.</summary>
        SapSession CurrentSession { get; }

        /// <summary>True when a SAP2000 COM connection is active.</summary>
        bool IsConnected { get; }

        /// <summary>
        /// Discovers all running SAP2000 processes with a valid main window.
        /// </summary>
        IReadOnlyList<SapInstanceInfo> DiscoverInstances();

        /// <summary>
        /// Attaches to an existing SAP2000 instance identified by its process id.
        /// </summary>
        SapSession AttachToInstance(SapInstanceInfo instance);

        /// <summary>
        /// Launches a brand-new SAP2000 process and connects to it.
        /// </summary>
        SapSession CreateNewInstance();

        /// <summary>
        /// Disconnects from the current session without killing SAP2000.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Disconnects current session and attaches to a different instance.
        /// </summary>
        SapSession SwitchToInstance(SapInstanceInfo instance);

        /// <summary>Fired whenever the connection state changes.</summary>
        event EventHandler<bool> ConnectionStateChanged;
    }
}
