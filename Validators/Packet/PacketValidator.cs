
namespace XModemProtocol.Validators.Packet {
    using Checksum;
    using Options;
    using System.Collections.Generic;
    using System.Linq;

    public class PacketValidator : IPacketValidator {

        private IValidateChecksum _validator;
        private IXModemProtocolOptions _options;
        private byte _packetNumberExpected;
        private List<byte> _data;

        public PacketValidator( IValidateChecksum validator) {
            Reset();
            _validator = validator;
        } 

        public void Reset() =>
            _packetNumberExpected = 1;

        public ValidationResult ValidatePacket(IEnumerable<byte> input, IXModemProtocolOptions options) {
            _data = input.ToList();
            _options = options;
            if (PacketExpectedIsIncorrect) 
                return PacketIsDuplicate ? ValidationResult.Duplicate : ValidationResult.Invalid;
            if (OnesComplementIsIncorrect) return ValidationResult.Invalid;
            if (ChecksumIsIncorrect) return ValidationResult.Invalid;
            _packetNumberExpected++;
            return ValidationResult.Valid;
        }

        private bool PacketExpectedIsIncorrect => _data[1] != _packetNumberExpected;

        private bool PacketIsDuplicate {
            get { 
                if (_packetNumberExpected == 1) return false; 
                byte previousPacketNumber = (byte) ( _packetNumberExpected - 1);
                return _data[1] == previousPacketNumber;
            }
        }

        private bool OnesComplementIsIncorrect {
            get { 
                byte onesComplement = (byte)(0xFF - _packetNumberExpected);
                return _data[2] != onesComplement;
            }
        }

        private bool ChecksumIsIncorrect {
            get {
                List<byte> payload = _data.GetRange(3, _data.Count - 3);
                return _validator.ValidateChecksum(payload) == false; 
            }
        }
    }
}