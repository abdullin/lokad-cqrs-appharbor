using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;

namespace SaaS.Client.Projections.Releases
{
    public sealed class ReleasesProjection
    {
        readonly IDocumentWriter<unit, ReleasesView> _writer;

        public ReleasesProjection(IDocumentWriter<unit, ReleasesView> writer)
        {
            _writer = writer;
        }

        public void When(InstanceStarted e)
        {
            _writer.UpdateEnforcingNew(v => v.List.Add(e.CodeVersion));
        }
    }
}