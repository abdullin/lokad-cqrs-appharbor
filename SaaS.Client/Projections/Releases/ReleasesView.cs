using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SaaS.Client.Projections.Releases
{
    [DataContract]
    public sealed class ReleasesView
    {
        [DataMember(Order = 1)]
        public List<string> List { get; set; }

        public ReleasesView()
        {
            List = new List<string>();
        }
    }
}
