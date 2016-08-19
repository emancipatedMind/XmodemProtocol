using System;
namespace XModemProtocol {
    /// <summary>
    /// The event data for the RoleChanged event.
    /// </summary>
    public class RoleChangedEventArgs : EventArgs {
        /// <summary>
        /// The new role of the XModemCommunicator instance.
        /// </summary>
        public XModemRole Role { get; private set; }
        /// <summary>
        /// The old role of the XModemCommunicator instance.
        /// </summary>
        public XModemRole OldRole { get; private set; }

        /// <summary>
        /// Constructor used to set properties needed by RoleChanged event.
        /// </summary>
        /// <param name="role">The current role of the instance.</param>
        /// <param name="oldRole">The old role of the instance.</param>
        internal RoleChangedEventArgs(XModemRole role, XModemRole oldRole) {
            Role = role;
            OldRole = oldRole;
        }
    }
}