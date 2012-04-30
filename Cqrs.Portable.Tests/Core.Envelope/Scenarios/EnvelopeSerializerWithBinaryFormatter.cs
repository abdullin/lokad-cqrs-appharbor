using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lokad.Cqrs.Envelope;

namespace Lokad.Cqrs.Core.Envelope.Scenarios
{
    class EnvelopeSerializerWithBinaryFormatter : IEnvelopeSerializer
    {
        public void SerializeEnvelope(Stream stream, EnvelopeContract c)
        {
            Formatter.Serialize(stream, c);
        }


        public EnvelopeContract DeserializeEnvelope(Stream stream)
        {
            return (EnvelopeContract)Formatter.Deserialize(stream);
        }

        static readonly BinaryFormatter Formatter = new BinaryFormatter();
    }
}