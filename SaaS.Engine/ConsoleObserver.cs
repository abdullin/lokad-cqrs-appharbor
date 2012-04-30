using System;
using System.Diagnostics;
using System.Text;
using Lokad.Cqrs;
using SaaS.Envelope.Events;
using ServiceStack.Text;

namespace SaaS.Engine
{
    sealed class ConsoleObserver : IObserver<ISystemEvent>
    {
        readonly Stopwatch _watch = Stopwatch.StartNew();


        public void OnNext(ISystemEvent value)
        {
            RedirectToWhen.InvokeEventOptional(this, value);
        }

        void When(EnvelopeDispatched ed)
        {
            if (ed.Dispatcher == "router")
            {
                foreach (var item in ed.Envelope.Items)
                {
                    var prefix = "";
                    if (item.Content is ICommand<IIdentity>)
                    {
                        prefix = ((ICommand<IIdentity>)(item.Content)).Id + " ";
                    }
                    else if (item.Content is IEvent<IIdentity>)
                    {
                        prefix = ((IEvent<IIdentity>)(item.Content)).Id + " ";
                    }
                    WriteLine(prefix + Describe.Object(item.Content));
                }
            }
        }
        void When(SystemObserver.MessageEvent e)
        {
            WriteLine(e.Message);
        }

        void When(EnvelopeQuarantined e)
        {
            WriteLine(e.LastException.ToString());
        }

        void WriteLine(string line)
        {
            var color = Console.ForegroundColor;
            var newCol = color;
            if (line.StartsWithIgnoreCase("[warn"))
            {
                newCol = ConsoleColor.DarkYellow;
            }
            else if (line.StartsWithIgnoreCase("[good"))
            {
                newCol = ConsoleColor.DarkGreen;
            }

            if (newCol == color)
            {
                var prefix = String.Format("[{0:0000000}]: ", _watch.ElapsedMilliseconds);
                Console.WriteLine(GetAdjusted(prefix, line));
            }
            else
            {
                Console.ForegroundColor = newCol;

                var prefix = String.Format("[{0:0000000}]: ", _watch.ElapsedMilliseconds);
                Console.WriteLine(GetAdjusted(prefix, line));
                Console.ForegroundColor = color;
            }
        }

        static string GetAdjusted(string adj, string text)
        {
            bool first = true;
            var builder = new StringBuilder();
            foreach (var s in text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                builder.Append(first ? adj : new string(' ', adj.Length));
                builder.AppendLine(s);
                first = false;
            }
            return builder.ToString().TrimEnd();
        }


        public void OnError(Exception error) { }

        public void OnCompleted() { }
    }
}