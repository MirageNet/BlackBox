using System;
using Mirage;

namespace BlackBox
{
    public class BlackBoxEncryption : Encryption
    {
        public BlackBoxEncryption(MessageHandler messageHandler) : base(messageHandler)
        {
        }

        protected override ArraySegment<byte> DecryptMessage(ArraySegment<byte> payload)
        {
            throw new NotImplementedException();
        }

        protected override ArraySegment<byte> EncryptMessage(ArraySegment<byte> payload)
        {
            throw new NotImplementedException();
        }
    }
}
