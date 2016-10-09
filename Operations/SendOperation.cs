namespace XModemProtocol.Operations {
    public class SendOperation : Operation {

        public SendOperation() {
            _initializer = new Initialize.InitializeSend();
            _invoker = new Invoke.InvokeSend();
            _invoker.PacketToSend += FirePacketToSendEvent;
            _finalizer = new Finalize.FinalizeSend();
        }

        protected override void TransitionToInvoke() {
            _requirements.Context.Packets = _tools.Builder.GetPackets(_requirements.Context.Data, _requirements.Options);
        }
    }
}