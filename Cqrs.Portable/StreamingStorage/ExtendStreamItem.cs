#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Text;

namespace Lokad.Cqrs.StreamingStorage
{
    /// <summary>
    /// Helper extensions for the <see cref="IStreamItem"/>
    /// </summary>
    public static class ExtendStreamItem
    {
        public static void WriteText(this IStreamItem item, string text)
        {
            item.Write(s =>
                {
                    using (var writer = new StreamWriter(s))
                    {
                        writer.Write(text);
                    }
                });
        }

        public static void WriteText(this IStreamItem item, string text, Encoding encoding)
        {
            item.Write(s =>
                {
                    using (var writer = new StreamWriter(s, encoding))
                    {
                        writer.Write(text);
                    }
                });
        }

        public static string ReadText(this IStreamItem item)
        {
            return item.ReadInto(stream =>
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                });
        }

        /// <summary>
        /// Performs the write operation, ensuring that the condition is met.
        /// </summary>
        /// <param name="stream"> </param>
        /// <param name="writer">The writer.</param>
        /// <returns>number of bytes written</returns>
        public static void Write(this IStreamItem stream, Action<Stream> writer)
        {
            using (var file = stream.OpenWrite())
            {
                writer(file);
            }
        }

        /// <summary>
        /// Attempts to read the storage item.
        /// </summary>
        /// <param name="stream"> </param>
        /// <param name="reader">The reader.</param>
        /// <exception cref="StreamItemNotFoundException">if the item does not exist.</exception>
        /// <exception cref="StreamContainerNotFoundException">if the container for the item does not exist</exception>
        public static T ReadInto<T>(this IStreamItem stream, Func<Stream, T> reader)
        {
            using (var read = stream.OpenRead())
            {
                return reader(read);
            }
        }

        public static void ReadInto(this IStreamItem stream, Action<Stream> reader)
        {
            using (var read = stream.OpenRead())
            {
                reader(read);
            }
        }
    }
}