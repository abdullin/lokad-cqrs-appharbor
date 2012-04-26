#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq.Expressions;
using Sample.PAssert.Infrastructure;
using Sample.PAssert.Infrastructure.Nodes;

namespace Sample.PAssert
{
    public static class PAssert
    {
        public static TException Throws<TException>(Action a) where TException : Exception
        {
            try
            {
                a();
            }
            catch (TException exception)
            {
                return exception;
            }

            throw new Exception("An exception of type " + typeof(TException).Name +
                " was expected, but no exception occured");
        }

        public static void IsTrue(Expression<Func<bool>> expression)
        {
            Func<bool> func = expression.Compile();
            if (!func())
            {
                throw CreateException(expression, "Assertion failed");
            }
        }

        public static string CreateSimpleFormatFor(Expression<Func<bool>> expression)
        {
            Node constantNode = NaturalExpressionParser.Parse(expression.Body);
            return NodeFormatter.SimpleFormat(constantNode);
        }

        public static string CreateSimpleFormatFor(Expression<Action> expression)
        {
            Node constantNode = NaturalExpressionParser.Parse(expression.Body);
            return NodeFormatter.SimpleFormat(constantNode);
        }

        static Exception CreateException(Expression<Func<bool>> expression, string message)
        {
            Node constantNode = NaturalExpressionParser.Parse(expression.Body);
            string[] lines = NodeFormatter.Format(constantNode);
            string nl = Environment.NewLine;
            return new Exception(message + ", expression was:" + nl + nl + String.Join(nl, lines));
        }
    }
}