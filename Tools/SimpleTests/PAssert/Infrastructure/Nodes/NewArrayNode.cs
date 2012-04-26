#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Linq;

namespace Sample.PAssert.Infrastructure.Nodes
{
    class NewArrayNode : Node
    {
        [NotNull]
        public List<Node> Items { get; set; }

        [NotNull]
        public string Type { get; set; }

        internal override void Walk(NodeWalker walker, int depth)
        {
            walker("new " + Type + "[]{");
            foreach (var node in Items.Take(1))
            {
                node.Walk(walker, depth);
            }
            foreach (var node in Items.Skip(1))
            {
                walker(", ");
                node.Walk(walker, depth);
            }
            walker("}");
        }
    }
}