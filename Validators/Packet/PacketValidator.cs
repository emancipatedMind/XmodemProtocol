using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Validators.Packet {
    using Checksum;
    using Options;
    public class PacketValidator : IPacketValidator {

        private IValidateChecksum _validator;
        private IXModemProtocolOptions _options;
        private byte _packetNumberExpected;
        private List<byte> _data;

        protected PacketValidator(
            IValidateChecksum validator, IXModemProtocolOptions options) {
            Reset();
            _validator = validator;
            _options = options;
        } 

        public void Reset() {
            _packetNumberExpected = 1;
        }

        public bool ValidatePacket(IEnumerable<byte> input) {
            _data = input.ToList();
            if (PacketExpectedIsIncorrect()) {
                if (PacketIsDuplicate())
                    return true;
                return false;
            }
            if (OnesComplementIsIncorrect()) return false;
            if (ChecksumIsIncorrect()) return false;
            _packetNumberExpected++;
            return true;
        }

        private bool PacketExpectedIsIncorrect() => _data[1] != _packetNumberExpected;

        private bool PacketIsDuplicate() {
            if (_packetNumberExpected == 1) return false; 
            byte previousPacketNumber = (byte) ( _packetNumberExpected - 1);
            return _data[1] == previousPacketNumber;
        }

        private bool OnesComplementIsIncorrect() {
            byte onesComplement = (byte)(0xFF - _packetNumberExpected);
            return _data[2] != onesComplement;
        }

        private bool ChecksumIsIncorrect() {
            List<byte> payload = _data.GetRange(3, _data.Count - 3);
            return _validator.ValidateChecksum(payload); 
        }
    }
}