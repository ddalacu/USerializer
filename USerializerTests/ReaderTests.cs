using System;
using System.Buffers;
using System.IO;
using NUnit.Framework;
using USerialization;

namespace USerializerTests;

public class ReaderTests
{
    [Test]
    public void TestNotThrowing()
    {
        var stream = new MemoryStream();
        stream.Write(new byte[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        });
        stream.Position = 0;

        var span = new Span<byte>(stream.GetBuffer(), 0, (int)stream.Length);
        var input = new SerializerInput(span);

        input.Read<byte>();

        input.Read<int>();
        input.Read<int>();
        input.Read<byte>();
    }
}