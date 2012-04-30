#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SaaS;

namespace Sample
{
    /// <summary>
    /// Just for grabbing the information about specifications
    /// </summary>
    public interface IAggregateSpecification
    {
        IEnumerable<IEvent<IIdentity>> GetExpect();
        IEnumerable<IEvent<IIdentity>> GetGiven();
        ICommand<IIdentity> GetWhen();
        Type Identity { get; }
    }


    public class AggregateFailSpecification<T, TException> :
        TypedSpecification<TException>, IAggregateSpecification
        where T : IIdentity
        where TException : Exception
    {
        public List<Expression<Action>> Before = new List<Expression<Action>>();
        public List<Expression<Func<TException, bool>>> Expect = new List<Expression<Func<TException, bool>>>();
        public Action Finally;
        public List<IEvent<T>> Given = new List<IEvent<T>>();
        public ICommand<T> When;
        public string Name;
        public InMemoryStore Factory = new InMemoryStore();
        public Func<IApplicationService> Handler;
        public List<Func<object>> Docs = new List<Func<object>>();


        public string GetName()
        {
            return Name;
        }

        public void Document(RunResult result)
        {
            var env = Before.Select(PAssert.PAssert.CreateSimpleFormatFor).Concat(Docs.Select(e => e().ToString()));

            PrintEvil.Document(result, env.ToList(), Given.ToArray(), When, _text.ToString());
        }

        public Action GetBefore()
        {
            return () =>
                {
                    foreach (var expression in Before)
                    {
                        expression.Compile()();
                    }
                };
        }


        public Delegate GetOn()
        {
            return new Func<IApplicationService>(() =>
                {
                    var list = new List<IEvent<T>>();


                    return Handler();
                });
        }

        readonly StringBuilder _text = new StringBuilder();

        IEnumerable<IEvent<IIdentity>> IAggregateSpecification.GetExpect()
        {
            return new IEvent<IIdentity>[0];
        }

        IEnumerable<IEvent<IIdentity>> IAggregateSpecification.GetGiven()
        {
            return Given.Cast<IEvent<IIdentity>>();
        }

        ICommand<IIdentity> IAggregateSpecification.GetWhen()
        {
            return (ICommand<IIdentity>) When;
        }

        public Type Identity
        {
            get { return typeof(T); }
        }

        public Delegate GetWhen()
        {
            return new Func<IApplicationService, TException>(feed =>
                {
                    foreach (var @event in Given)
                    {
                        Factory.Store.Add((IEvent<IIdentity>) @event);
                    }
                    using (var capt = Context.CaptureForThread())
                    {
                        try
                        {
                            feed.Execute(When);
                            return null;
                        }
                        catch (TException ex)
                        {
                            Context.Explain(ex.Message);
                            return ex;
                        }
                        finally
                        {
                            _text.Append(capt.Log);
                        }
                    }
                });
        }

        public IEnumerable<Assertion<TException>> GetAssertions()
        {
            return Expect.Select(x => new PAssertion<TException>(x));
        }

        public Action GetFinally()
        {
            return this.Finally;
        }
    }


    public class AggregateSpecification<T> : TypedSpecification<IEvent<T>[]>, IAggregateSpecification
        where T : IIdentity
    {
        public List<Expression<Action>> Before = new List<Expression<Action>>();
        public List<IEvent<T>> Expect = new List<IEvent<T>>();
        public Action Finally;
        public List<IEvent<T>> Given = new List<IEvent<T>>();
        public ICommand<T> When;
        public string Name;
        public List<Func<object>> Docs = new List<Func<object>>();


        public InMemoryStore Factory = new InMemoryStore();
        public Func<IApplicationService> Handler;

        public string GetName()
        {
            return Name;
        }

        public Action GetBefore()
        {
            return () =>
                {
                    foreach (var expression in Before)
                    {
                        expression.Compile()();
                    }
                };
        }

        public Delegate GetOn()
        {
            return new Func<IApplicationService>(() =>
                {
                    foreach (var @event in Given)
                    {
                        Factory.Store.Add((IEvent<IIdentity>) @event);
                    }
                    return Handler();
                });
        }


        readonly StringBuilder _text = new StringBuilder();

        public Delegate GetWhen()
        {
            return new Func<IApplicationService, IEvent<T>[]>(feed =>
                {
                    using (Context.CaptureForThread(s => _text.AppendLine(s)))
                    {
                        var before = Factory.Store.Count;
                        feed.Execute(When);
                        return Factory.Store.Skip(before).Cast<IEvent<T>>().ToArray();
                    }
                });
        }

        public Type Identity
        {
            get { return typeof(T); }
        }

        public IEnumerable<IEvent<IIdentity>> GetExpect()
        {
            return Expect.Cast<IEvent<IIdentity>>();
        }

        public IEnumerable<IEvent<IIdentity>> GetGiven()
        {
            return Given.Cast<IEvent<IIdentity>>();
        }

        ICommand<IIdentity> IAggregateSpecification.GetWhen()
        {
            return (ICommand<IIdentity>) When;
        }

        public IEnumerable<Assertion<IEvent<T>[]>> GetAssertions()
        {
            yield return new AggregateAssertion<T>(Expect);
        }

        public Action GetFinally()
        {
            return Finally;
        }


        public void Document(RunResult result)
        {
            var env = Before.Select(PAssert.PAssert.CreateSimpleFormatFor).Concat(Docs.Select(e => e().ToString()));

            PrintEvil.Document(result, env.ToList(), Given.ToArray(), When, _text.ToString());
        }
    }

    static class PrintEvil
    {
        public static void PrintAdjusted(string adj, string text)
        {
            bool first = true;
            foreach (var s in text.Split(new[] {Environment.NewLine}, StringSplitOptions.None))
            {
                Console.Write(first ? adj : new string(' ', adj.Length));
                Console.WriteLine(s);
                first = false;
            }
        }

        public static string GetAdjusted(string adj, string text)
        {
            bool first = true;
            var builder = new StringBuilder();
            foreach (var s in text.Split(new[] {Environment.NewLine}, StringSplitOptions.None))
            {
                builder.Append(first ? adj : new string(' ', adj.Length));
                builder.AppendLine(s);
                first = false;
            }
            return builder.ToString();
        }

        public static void Document<T>(
            RunResult result,
            List<string> before,
            IEvent<T>[] given,
            ICommand<T> when,
            string decisions) where T : IIdentity
        {
            var passed = result.Passed ? "[Passed]" : "[Failed]";
            var cleanupName = result.FoundOnMemberInfo.DeclaringType.Name.CleanupName();
            Console.WriteLine("{2} Use case '{0} - {1}'.", cleanupName, result.Name.CleanupName(), passed);

            var cleared = before
                .Select(b => (b ?? "").TrimEnd())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
            if (cleared.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Environment: ");
                foreach (var e in cleared)
                {
                    PrintAdjusted("  ", e);
                }
            }

            if (given.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Given:");

                for (int i = 0; i < given.Length; i++)
                {
                    PrintAdjusted(" " + (i + 1) + ". ", Describe.Object(given[i]).Trim());
                }
            }


            if (when != null)
            {
                Console.WriteLine();
                Console.WriteLine("When:");
                PrintAdjusted("  ", Describe.Object(when).Trim());
            }

            Console.WriteLine();
            Console.WriteLine("Expectations:");
            foreach (var expecation in result.Expectations)
            {
                var s = expecation.Passed ? "[ok]" : "[NO]";
                PrintAdjusted("  " + s + " ", expecation.Text.Trim());
                if (!expecation.Passed && expecation.Exception != null)
                {
                    PrintAdjusted("  ", expecation.Exception.Message);
                }
            }


            if (result.Thrown != null)
            {
                Console.WriteLine("Specification failed: " + result.Message.Trim());
                Console.WriteLine();
                Console.WriteLine(result.Thrown);
            }

            if (decisions.Length > 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Decisions made:");
                PrintAdjusted("  ", decisions);
            }

            Console.WriteLine(new string('-', 80));
            Console.WriteLine();
        }
    }

    public sealed class InMemoryStore : IEventStore
    {
        public List<IEvent<IIdentity>> Store = new List<IEvent<IIdentity>>();


        public EventStream LoadEventStream(IIdentity id)
        {
            var stream = Store
                .Where(i => id.Equals(i.Id))
                .ToList();
            return new EventStream
                {
                    Events = stream,
                    Version = stream.Count
                };
        }

        public void AppendToStream(IIdentity id, long originalVersion, ICollection<IEvent<IIdentity>> events,
            string explanation)
        {
            foreach (var @event in events)
            {
                Store.Add(@event);
            }
        }
    }
}