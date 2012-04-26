#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample
{
    public sealed class AggregateAssertion<T> : Assertion<IEvent<T>[]> where T : IIdentity
    {
        readonly List<IEvent<T>> _expected;

        public AggregateAssertion(List<IEvent<T>> expected)
        {
            _expected = expected;
        }

        public IEnumerable<ExpectationResult> Assert(object fromWhen)
        {
            var actual = ((IEvent<T>[]) fromWhen);
            // structurally equal comparison


            for (int i = 0; i < _expected.Count; i++)
            {
                var expectedHumanReadable = Describe.Object(_expected[i]);
                if (actual.Length > i)
                {
                    var diffs = CompareObjects.FindDifferences(_expected[i], actual[i]);
                    if (string.IsNullOrEmpty(diffs))
                    {
                        yield return new ExpectationResult
                            {
                                Passed = true,
                                Text = expectedHumanReadable
                            };
                    }
                    else
                    {
                        var actualHumanReadable = Describe.Object(actual[i]);

                        if (actualHumanReadable != expectedHumanReadable)
                        {
                            var msg = PrintEvil.GetAdjusted("Was: ", actualHumanReadable);
                            // there is a difference in textual representations
                            yield return new ExpectationResult
                                {
                                    Passed = false,
                                    Text = expectedHumanReadable,
                                    Exception = new InvalidOperationException(msg)
                                };
                        }
                        else
                        {
                            yield return new ExpectationResult
                                {
                                    Passed = false,
                                    Text = expectedHumanReadable,
                                    Exception = new InvalidOperationException(diffs)
                                };
                        }
                    }
                }
                else
                {
                    yield return new ExpectationResult
                        {
                            Passed = false,
                            Text = expectedHumanReadable,
                            Exception = new InvalidOperationException("  Message is missing")
                        };
                }
            }

            for (int i = _expected.Count; i < actual.Count(); i++)
            {
                var msg = PrintEvil.GetAdjusted("Was: ", Describe.Object(actual[i]));

                yield return new ExpectationResult
                    {
                        Passed = false,
                        Text = "Unexpected message",
                        Exception = new InvalidOperationException(msg)
                    };
            }
        }
    }
}