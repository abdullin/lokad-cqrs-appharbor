#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Sample.PAssert.Infrastructure.Nodes
{
    class ConstantNode : Node
    {
        [NotNull]
        public string Text { get; set; }

        [CanBeNull]
        public string Value { get; set; }

        internal override void Walk(NodeWalker walker, int depth)
        {
            walker(Text.CleanupCamelCasing(), Value, depth);
        }
    }
}