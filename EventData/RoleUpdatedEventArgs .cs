using System;

namespace XModemProtocol.EventData {
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.RoleChanged event.
    /// </summary>
    public class RoleUpdatedEventArgs : EventArgs {
        /// <summary>
        /// The new role of the XModemCommunicator instance.
        /// </summary>
        public XModemRole Role { get; private set; }
        /// <summary>
        /// The old role of the XModemCommunicator instance.
        /// </summary>
        public XModemRole OldRole { get; private set; }
        internal RoleUpdatedEventArgs(XModemRole role, XModemRole oldRole) {
            Role = role;
            OldRole = oldRole;
        }
    }
}