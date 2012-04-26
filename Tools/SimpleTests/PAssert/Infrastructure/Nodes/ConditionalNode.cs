#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Sample.PAssert.Infrastructure.Nodes
{
    class ConditionalNode : Node
    {
        [NotNull]
        public Node Condition { get; set; }

        [NotNull]
        public Node TrueValue { get; set; }

        [NotNull]
        public Node FalseValue { get; set; }

        internal override void Walk(NodeWalker walker, int depth)
        {
            walker("(");
            Condition.Walk(walker, depth + 1);
            walker(" ? ");
            TrueValue.Walk(walker, depth + 1);
            walker(" : ");
            FalseValue.Walk(walker, depth + 1);
            walker(")");
        }
    }
}