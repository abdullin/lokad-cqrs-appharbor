#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace SaaS.TapeStorage
{
    /// <summary>
    /// Is thrown internally, when storage version does not match the condition specified in <see cref="TapeAppendCondition"/>
    /// </summary>
    [Serializable]
    public class TapeAppendConditionException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public TapeAppendConditionException() {}
        public TapeAppendConditionException(string message) : base(message) {}
        public TapeAppendConditionException(string message, Exception inner) : base(message, inner) {}

        protected TapeAppendConditionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }
}