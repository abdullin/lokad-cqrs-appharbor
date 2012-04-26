#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Linq;

namespace Sample.PAssert.Infrastructure.Nodes
{
    class MethodCallNode : MemberAccessNode
    {
        internal MethodCallNode()
        {
            Parameters = new List<Node>();
        }

        [NotNull]
        public List<Node> Parameters { get; set; }

        internal override void Walk(NodeWalker walker, int depth)
        {
            base.Walk(walker, depth);
            walker("(");
            foreach (var parameter in Parameters.Take(1))
            {
                parameter.Walk(walker, depth);
            }
            foreach (var parameter in Parameters.Skip(1))
            {
                walker(", ");
                parameter.Walk(walker, depth);
            }
            walker(")");
        }
    }
}