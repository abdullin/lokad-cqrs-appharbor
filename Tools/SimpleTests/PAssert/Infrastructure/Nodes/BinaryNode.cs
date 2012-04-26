#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Sample.PAssert.Infrastructure.Nodes
{
    class BinaryNode : Node
    {
        [NotNull]
        public Node Left { get; set; }

        [NotNull]
        public Node Right { get; set; }

        [NotNull]
        public string Operator { get; set; }

        [CanBeNull]
        public string Value { get; set; }

        internal override void Walk(NodeWalker walker, int depth)
        {
            Left.Walk(walker, depth + 1);
            walker(" ");
            walker(Operator, Value, depth);
            walker(" ");
            Right.Walk(walker, depth + 1);
        }
    }
}