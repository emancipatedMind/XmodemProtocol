namespace XModemProtocol.Operations {
    public class ReceiveOperation : Operation {
        public ReceiveOperation() {
            _initializer = new Initialize.InitializeReceive();
            _invoker = new Invoke.InvokeReceive();
            _invoker.PacketReceived += FirePacketReceivedEvent;
            _finalizer = new Finalize.FinalizeReceive();
        }
    }
}