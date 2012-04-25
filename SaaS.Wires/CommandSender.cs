using Lokad.Cqrs;
using Sample;

namespace SaaS.Wires
{
    public sealed class CommandSender : ICommandSender
    {
        readonly SimpleMessageSender _sender;

        public CommandSender(SimpleMessageSender sender)
        {
            _sender = sender;
        }


        public void SendCommandsAsBatch(ISampleCommand[] commands)
        {
            _sender.SendBatch(commands);
        }
    }
}