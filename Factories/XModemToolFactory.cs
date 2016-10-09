namespace XModemProtocol.Factories {
    using Tools;
    public class XModemToolFactory : IToolFactory {
        /// <summary>
        /// Get the tools needed for a mode.
        /// </summary>
        /// <param name="mode">The mode for which tools are requested.</param>
        /// <returns>The tools.</returns>
        public IXModemTools GetToolsFor(XModemMode mode) {
            switch(mode) {
                case XModemMode.Checksum:
                    return new XModem128Tools();
                case XModemMode.CRC:
                    return new XModemCRCTools();
                case XModemMode.OneK:
                    return new XModemOneKTools();
                default:
                    return null;
            }
        }
    }
}