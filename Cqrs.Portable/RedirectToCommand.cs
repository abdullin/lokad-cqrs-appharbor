#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Lokad.Cqrs
{
    public sealed class RedirectToCommand : HideObjectMembersFromIntelliSense
    {
        public readonly IDictionary<Type, Action<object>> Dict = new Dictionary<Type, Action<object>>();


        static readonly MethodInfo InternalPreserveStackTraceMethod =
            typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);


        public void WireToWhen(object o)
        {
            WireToMethod(o, "When");
        }

        public void WireToMethod(object o, string methodName)
        {
            var infos = o.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == methodName)
                .Where(m => m.GetParameters().Length == 1);

            foreach (var methodInfo in infos)
            {
                var type = methodInfo.GetParameters().First().ParameterType;

                var info = methodInfo;
                Dict.Add(type, message => info.Invoke(o, new[] {message}));
            }
        }

        public void WireToLambda<T>(Action<T> handler)
        {
            Dict.Add(typeof(T), o => handler((T) o));
        }

        public void InvokeMany(IEnumerable<object> messages, Action<object> onNull = null)
        {
            foreach (var message in messages)
            {
                Invoke(message, onNull);
            }
        }

        [DebuggerNonUserCode]
        public void Invoke(object message, Action<object> onNull = null)
        {
            Action<object> handler;
            var type = message.GetType();
            if (!Dict.TryGetValue(type, out handler))
            {
                handler = onNull ??
                    (o => { throw new InvalidOperationException("Failed to locate command handler for " + type); });
                //Trace.WriteLine(string.Format("Discarding {0} - failed to locate event handler", type.Name));
            }
            try
            {
                handler(message);
            }
            catch (TargetInvocationException ex)
            {
                if (null != InternalPreserveStackTraceMethod)
                    InternalPreserveStackTraceMethod.Invoke(ex.InnerException, new object[0]);
                throw ex.InnerException;
            }
        }
    }

    /// <summary>
    /// Creates convention-based routing rules
    /// </summary>
    public sealed class RedirectToDynamicEvent
    {
        public readonly IDictionary<Type, List<Wire>> Dict = new Dictionary<Type, List<Wire>>();

        public sealed class Wire
        {
            public MethodInfo Method;
            public object Instance;
        }

        static readonly MethodInfo InternalPreserveStackTraceMethod =
            typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);


        public void WireToWhen(object o)
        {
            var infos = o.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "When")
                .Where(m => m.GetParameters().Length == 1);

            foreach (var methodInfo in infos)
            {
                var type = methodInfo.GetParameters().First().ParameterType;

                List<Wire> list;
                if (!Dict.TryGetValue(type, out list))
                {
                    list = new List<Wire>();
                    Dict.Add(type, list);
                }
                list.Add(new Wire
                {
                    Instance = o,
                    Method = methodInfo
                });
            }
        }

        [DebuggerNonUserCode]
        public void InvokeEvent(object @event)
        {
            List<Wire> info;
            var type = @event.GetType();
            if (!Dict.TryGetValue(type, out info))
            {
                //Trace.WriteLine(string.Format("Discarding {0} - failed to locate event handler", type.Name));
                return;
            }
            try
            {
                foreach (var wire in info)
                {
                    wire.Method.Invoke(wire.Instance, new[] { @event });
                }
            }
            catch (TargetInvocationException ex)
            {
                if (null != InternalPreserveStackTraceMethod)
                    InternalPreserveStackTraceMethod.Invoke(ex.InnerException, new object[0]);
                throw ex.InnerException;
            }
        }
    }

}