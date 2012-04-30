using System;
using System.IO;
using Lokad.Cqrs.Envelope;
using ServiceStack.Text;

namespace Lokad.Cqrs.Core.Envelope.Scenarios
{
    class EnvelopeSerializerWithServiceStackJSON : IEnvelopeSerializer
    {
        public void SerializeEnvelope(Stream stream, EnvelopeContract c)
        {
            // we don't support
            c = new EnvelopeContract(c.EnvelopeId, c.EnvelopeAttributes, c.Messages,
                RestoreKindToUtc(c.DeliverOnUtc, DateTimeKind.Unspecified),
                RestoreKindToUtc(c.CreatedOnUtc, DateTimeKind.Unspecified));
            JsonSerializer.SerializeToStream(c, stream);
        }

        public EnvelopeContract DeserializeEnvelope(Stream stream)
        {
            var c = JsonSerializer.DeserializeFromStream<EnvelopeContract>(stream);
            // JSON looses UTC bit. We need to restore it.

            
            return new EnvelopeContract(c.EnvelopeId, c.EnvelopeAttributes, c.Messages, 
                RestoreKindToUtc(c.DeliverOnUtc, DateTimeKind.Utc),
                RestoreKindToUtc(c.CreatedOnUtc, DateTimeKind.Utc));
        }

        static DateTime RestoreKindToUtc(DateTime date, DateTimeKind kind)
        {
            return new DateTime(date.Ticks, kind);
        }
    }
}