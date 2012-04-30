using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using Mono.Cecil;
using SaaS.AtomicStorage;
using SaaS.Client;
using SaaS.TapeStorage;
using SaaS.Wires;

namespace SaaS.Engine
{
    public static class StartupProjectionRebuilder
    {
        public static void Rebuild(IDocumentStore targetContainer, ITapeStream stream)
        {
            var strategy = targetContainer.Strategy;
            var memory = new MemoryStorageConfig();

            var memoryContainer = memory.CreateNuclear(strategy).Container;
            var tracked = new ProjectionInspectingContainer(memoryContainer);

            var projections = new List<object>();
            projections.AddRange(DomainBoundedContext.Projections(tracked));
            projections.AddRange(ClientBoundedContext.Projections(tracked));
            //projections.AddRange(ApiOpsBoundedContext.Projections(tracked));

            if (tracked.Buckets.Count != projections.Count())
                throw new InvalidOperationException("Count mismatch");

            var storage = new NuclearStorage(targetContainer);
            var hashes = storage.GetSingletonOrNew<ProjectionHash>().Entries;

            var memoryProjections = projections.Select((projection, i) =>
                {
                    var bucketName = tracked.Buckets[i];
                    var viewType = tracked.Views[i];

                    var projectionHash = GetClassHash(projection.GetType()) + "\r\n" + GetClassHash(viewType);

                    bool needsRebuild = !hashes.ContainsKey(bucketName) || hashes[bucketName] != projectionHash;
                    return new
                        {
                            bucketName,
                            projection,
                            hash = projectionHash,
                            needsRebuild
                        };


                }).ToArray();

            foreach (var memoryProjection in memoryProjections)
            {
                if (memoryProjection.needsRebuild)
                {
                    SystemObserver.Notify("[warn] {0} needs rebuild", memoryProjection.bucketName);
                }
                else
                {
                    SystemObserver.Notify("[good] {0} is up-to-date", memoryProjection.bucketName);
                }
            }


            var needRebuild = memoryProjections.Where(x => x.needsRebuild).ToArray();

            if (needRebuild.Length == 0)
            {
                return;
            }



            var watch = Stopwatch.StartNew();

            var wire = new RedirectToDynamicEvent();
            needRebuild.ForEach(x => wire.WireToWhen(x.projection));


            var handlersWatch = Stopwatch.StartNew();

            Observe(stream, wire);
            var timeTotal = watch.Elapsed.TotalSeconds;
            var handlerTicks = handlersWatch.ElapsedTicks;
            var timeInHandlers = Math.Round(TimeSpan.FromTicks(handlerTicks).TotalSeconds, 1);
            Console.WriteLine("Total Elapsed: {0}sec ({1}sec in handlers)", Math.Round(timeTotal, 0), timeInHandlers);


            // delete projections that were rebuilt
            var bucketNames = needRebuild.Select(x => x.bucketName).ToArray();

            foreach (var name in bucketNames)
            {
                targetContainer.Reset(name);

                var contents = memoryContainer.EnumerateContents(name);
                targetContainer.WriteContents(name, contents);
            }

            var allBuckets = new HashSet<string>(memoryProjections.Select(p => p.bucketName));
            var obsolete = hashes.Keys.Where(s => !allBuckets.Contains(s)).ToArray();
            foreach (var name in obsolete)
            {
                SystemObserver.Notify("[warn] {0} is obsolete", name);
                targetContainer.Reset(name);
            }
            storage.UpdateSingletonEnforcingNew<ProjectionHash>(x =>
                {
                    x.Entries.Clear();

                    foreach (var prj in memoryProjections)
                    {
                        x.Entries[prj.bucketName] = prj.hash;
                    }
                });
        }


        sealed class ProjectionInspectingContainer : IDocumentStore
        {
            readonly IDocumentStore _real;

            public ProjectionInspectingContainer(IDocumentStore real)
            {
                _real = real;
            }

            public readonly List<string> Buckets = new List<string>();
            public readonly List<Type> Views = new List<Type>();

            public IDocumentWriter<TKey, TEntity> GetWriter<TKey, TEntity>()
            {
                Buckets.Add(_real.Strategy.GetEntityBucket<TEntity>());
                Views.Add(typeof(TEntity));
                return _real.GetWriter<TKey, TEntity>();
            }

            public IDocumentReader<TKey, TEntity> GetReader<TKey, TEntity>()
            {
                return _real.GetReader<TKey, TEntity>();
            }

            public IDocumentStrategy Strategy
            {
                get { return _real.Strategy; }
            }

            public IEnumerable<DocumentRecord> EnumerateContents(string bucket)
            {
                return _real.EnumerateContents(bucket);
            }

            public void WriteContents(string bucket, IEnumerable<DocumentRecord> records)
            {
                _real.WriteContents(bucket, records);
            }

            public void Reset(string bucket)
            {
                _real.Reset(bucket);
            }
        }

        static readonly IEnvelopeStreamer Streamer = Contracts.CreateStreamer();

        static string GetClassHash(Type type1)
        {
            var location = type1.Assembly.Location;
            var mod = ModuleDefinition.ReadModule(location);
            var builder = new StringBuilder();
            var type = type1;


            var typeDefinition = mod.GetType(type.FullName);
            builder.AppendLine(typeDefinition.Name);
            ProcessMembers(builder, typeDefinition);

            // we include nested types
            foreach (var nested in typeDefinition.NestedTypes)
            {
                ProcessMembers(builder, nested);
            }

            return builder.ToString();
        }

        static void ProcessMembers(StringBuilder builder, TypeDefinition typeDefinition)
        {
            foreach (var md in typeDefinition.Methods.OrderBy(m => m.ToString()))
            {
                builder.AppendLine("  " + md);

                foreach (var instruction in md.Body.Instructions)
                {
                    // we don't care about offsets
                    instruction.Offset = 0;
                    builder.AppendLine("    " + instruction);
                }
            }
            foreach (var field in typeDefinition.Fields.OrderBy(f => f.ToString()))
            {
                builder.AppendLine("  " + field);
            }
        }


        static void Observe(ITapeStream tapes, RedirectToDynamicEvent wire)
        {
            var date = DateTime.MinValue;
            var watch = Stopwatch.StartNew();
            foreach (var record in tapes.ReadRecords(0, int.MaxValue))
            {
                var env = Streamer.ReadAsEnvelopeData(record.Data);
                if (date.Month != env.CreatedOnUtc.Month)
                {
                    date = env.CreatedOnUtc;
                    SystemObserver.Notify("Observing {0:yyyy-MM-dd} {1}", date, Math.Round(watch.Elapsed.TotalSeconds, 2));
                    watch.Restart();
                }
                foreach (var item in env.Items)
                {
                    var e = item.Content as ISampleEvent;
                    if (e != null)
                    {
                        wire.InvokeEvent(e);
                    }
                }
            }
        }
    }

    [DataContract]
    public sealed class ProjectionHash
    {
        [DataMember(Order = 1)]
        public IDictionary<string, string> Entries { get; set; }

        public ProjectionHash()
        {
            Entries = new Dictionary<string, string>();
        }
    }


}