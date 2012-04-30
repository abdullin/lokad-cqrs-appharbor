#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Lokad.Cqrs.TapeStorage
{
    public class TapeStreamSerializer
    {
        static readonly byte[] ReadableHeaderStart = Encoding.UTF8.GetBytes("/* header ");
        static readonly byte[] ReadableHeaderEnd = Encoding.UTF8.GetBytes(" */\r\n");
        static readonly byte[] ReadableFooterStart = Encoding.UTF8.GetBytes("\r\n/* footer ");
        static readonly byte[] ReadableFooterEnd = Encoding.UTF8.GetBytes(" */\r\n");

        public static bool SkipRecords(long count, Stream file)
        {
            for (var i = 0; i < count; i++)
            {
                if (file.Position == file.Length)
                    return false;

                file.Seek(ReadableHeaderStart.Length, SeekOrigin.Current);
                var dataLength = ReadReadableInt64(file);
                var skip = ReadableHeaderEnd.Length + dataLength +
                    ReadableFooterStart.Length + 16 + 16 + 28 + ReadableFooterEnd.Length;
                file.Seek(skip, SeekOrigin.Current);
            }

            return true;
        }

        public static void WriteRecord(Stream stream, byte[] data, long versionToWrite)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            using (var managed = new SHA1Managed())
            {
                writer.Write(ReadableHeaderStart);
                WriteReadableInt64(writer, data.Length);
                writer.Write(ReadableHeaderEnd);

                writer.Write(data);
                writer.Write(ReadableFooterStart);
                WriteReadableInt64(writer, data.Length);
                WriteReadableInt64(writer, versionToWrite);
                WriteReadableHash(writer, managed.ComputeHash(data));
                writer.Write(ReadableFooterEnd);

                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(stream);
            }
        }

        public static TapeRecord ReadRecord(Stream file)
        {
            ReadAndVerifySignature(file, ReadableHeaderStart, "Start");
            var dataLength = ReadReadableInt64(file);
            ReadAndVerifySignature(file, ReadableHeaderEnd, "Header-End");

            var data = new byte[dataLength];
            file.Read(data, 0, (int) dataLength);

            ReadAndVerifySignature(file, ReadableFooterStart, "Footer-Start");

            ReadReadableInt64(file); //length verified
            var recVersion = ReadReadableInt64(file);
            var hash = ReadReadableHash(file);
            using (var managed = new SHA1Managed())
            {
                var computed = managed.ComputeHash(data);

                if (!computed.SequenceEqual(hash))
                    throw new InvalidOperationException("Hash corrupted");
            }
            ReadAndVerifySignature(file, ReadableFooterEnd, "End");

            return new TapeRecord(recVersion, data);
        }

        public static long ReadVersionFromTheEnd(Stream stream)
        {
            if (stream.Position == 0)
                return 0;

            var seekBack = ReadableFooterEnd.Length + 28 + 16;
            stream.Seek(-seekBack, SeekOrigin.Current);

            var version = ReadReadableInt64(stream);

            stream.Seek(28, SeekOrigin.Current);
            ReadAndVerifySignature(stream, ReadableFooterEnd, "End");

            return version;
        }

        static long ReadReadableInt64(Stream stream)
        {
            var buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            var s = Encoding.UTF8.GetString(buffer);
            return Int64.Parse(s, NumberStyles.HexNumber);
        }

        static IEnumerable<byte> ReadReadableHash(Stream stream)
        {
            var buffer = new byte[28];
            stream.Read(buffer, 0, buffer.Length);
            var hash = Convert.FromBase64String(Encoding.UTF8.GetString(buffer));
            return hash;
        }

        static void ReadAndVerifySignature(Stream source, byte[] signature, string name)
        {
            for (var i = 0; i < signature.Length; i++)
            {
                var readByte = source.ReadByte();
                if (readByte == -1)
                    throw new InvalidOperationException(
                        String.Format("Expected byte[{0}] of signature '{1}', but found EOL", i, name));
                if (readByte != signature[i])
                {
                    throw new InvalidOperationException("Signature failed: " + name);
                }
            }
        }

        static void WriteReadableInt64(BinaryWriter writer, long value)
        {
            // long is 8 bytes ==> 16 bytes of readable Unicode.
            var buffer = Encoding.UTF8.GetBytes(value.ToString("x16"));

            writer.Write(buffer);
        }

        static void WriteReadableHash(BinaryWriter writer, byte[] hash)
        {
            // hash is 20 bytes, which is encoded into 28 bytes of readable Unicode
            var buffer = Encoding.UTF8.GetBytes(Convert.ToBase64String(hash));

            writer.Write(buffer);
        }
    }
}