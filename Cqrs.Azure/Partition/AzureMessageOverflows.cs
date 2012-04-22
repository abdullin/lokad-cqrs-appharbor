using System;
using System.Text;
using Lokad.Cqrs.Envelope;

namespace Lokad.Cqrs.Feature.AzurePartition
{
    public static class AzureMessageOverflows
    {
        const string ReferenceSignature = "[cqrs-ref-r1]";
        static readonly byte[] Reference = Encoding.Unicode.GetBytes(ReferenceSignature);

        // New azure limit is 64k after BASE 64 conversion. We Are adding 152 on top just to be safe
        public const int CloudQueueLimit = 49000;


        /// <summary>
        /// Saves reference to message envelope as array of bytes.
        /// </summary>
        /// <param name="reference">The reference to to message envelope.</param>
        /// <returns>byte array that could be used to rebuild reference</returns>

        public static byte[] SaveEnvelopeReference(EnvelopeReference reference)
        {
            // important to use \r\n
            var builder = new StringBuilder();
            builder
                .Append("[cqrs-ref-r1]\r\n")
                .Append(reference.StorageContainer).Append("\r\n")
                .Append(reference.StorageReference);

            return Encoding.Unicode.GetBytes(builder.ToString());
        }

        public static bool TryReadAsEnvelopeReference(byte[] buffer, out EnvelopeReference reference)
        {
            if (BytesStart(buffer, Reference))
            {
                var text = Encoding.Unicode.GetString(buffer);
                var args = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                reference = new EnvelopeReference(args[1], args[2]);
                return true;
            }
            reference = null;
            return false;
        }

        static bool BytesStart(byte[] buffer, byte[] signature)
        {
            if (buffer.Length < signature.Length)
                return false;

            for (int i = 0; i < signature.Length; i++)
            {
                if (buffer[i] != signature[i])
                    return false;
            }

            return true;
        }
     
    }
}