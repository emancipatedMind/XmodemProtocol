namespace XModemProtocol.Operations {
    public class ReceiveOperation : Operation {

        public ReceiveOperation() {
            _initializer = new Initialize.InitializeReceive();
            _invoker = new Invoke.InvokeReceive();
            _invoker.PacketReceived += FirePacketReceivedEvent;
            _finalizer = new Finalize.FinalizeReceive();
        }

        protected override void TransitionToInvoke() {
            _requirements = new Options.SendReceiveRequirements {
                Communicator = _requirements.Communicator,
                Context = _requirements.Context,
                Detector = _tools.Detector,
                Options = _requirements.Options,
                Validator = _tools.Validator,
            };
        }
    }
}