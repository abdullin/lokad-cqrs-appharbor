#region Copyright (c) 2006-2011 LOKAD SAS. All rights reserved

// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed

#endregion

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Media;
using Lokad.CodeDsl;

namespace Hub.Dsl
{
    class Program
    {
        static ConcurrentDictionary<string, string> _states = new ConcurrentDictionary<string, string>();

        static void Main(string[] args)
        {
            var info = new DirectoryInfo("..\\..\\..\\..\\SaaS.Contracts");

            var files = info.GetFiles("*.tt", SearchOption.AllDirectories);

            foreach (var fileInfo in files)
            {
                var text = File.ReadAllText(fileInfo.FullName);
                Changed(fileInfo.FullName, text);
                Rebuild(text, fileInfo.FullName);
            }

            var notifier = new FileSystemWatcher(info.FullName, "*.tt");
            notifier.Changed += NotifierOnChanged;

            notifier.EnableRaisingEvents = true;


            Console.ReadLine();
        }

        static void NotifierOnChanged(object sender, FileSystemEventArgs args)
        {
            if (!File.Exists(args.FullPath)) return;

            try
            {
                var text = File.ReadAllText(args.FullPath);

                if (!Changed(args.FullPath, text))
                    return;


                Console.WriteLine("{1}-{0}", args.Name, args.ChangeType);
                Rebuild(text, args.FullPath);
                SystemSounds.Beep.Play();
            }
            catch (IOException) {}
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                SystemSounds.Exclamation.Play();
            }
        }

        static bool Changed(string path, string value)
        {
            var changed = false;
            _states.AddOrUpdate(path, key =>
                {
                    changed = true;
                    return value;
                }, (s, s1) =>
                    {
                        changed = s1 != value;
                        return value;
                    });
            return changed;
        }

        static void Rebuild(string text, string fullPath)
        {
            var dsl = text;
            var generator = new TemplatedGenerator()
                {
                    Namespace = "Sample",
                    GenerateInterfaceForEntityWithModifiers = "?",
                    TemplateForInterfaceName = "public interface I{0}Aggregate",
                    TemplateForInterfaceMember = "void When({0} c);",
                    ClassNameTemplate = @"
    

[DataContract(Namespace = ""Sample"")]
public partial class {0}",
                    MemberTemplate = "[DataMember(Order = {0})] public {1} {2} {{ get; private set; }}",
                    
                };

            var prefix = @"
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

";
            File.WriteAllText(Path.ChangeExtension(fullPath, "cs"), prefix + GeneratorUtil.Build(dsl, generator));
        }
    }
}