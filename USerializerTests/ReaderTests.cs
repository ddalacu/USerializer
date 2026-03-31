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

        using var input = new SerializerInput(5, stream, ArrayPool<byte>.Shared);

        input.Read<byte>();
        
        input.Read<int>();
        input.Read<int>();
        input.Read<byte>();
    }
    
    [Test]
    public void ReadData()
    {
        var stream = new MemoryStream();
        stream.Write(new byte[]
        {
            0, 
            1, 
            2, 
            3, 
            4, 
            5, 
            6, 
            7, 
            8, 
            9
        });
        stream.Position = 0;

        using var input = new SerializerInput(4, stream, ArrayPool<byte>.Shared);
        
        var readPtr = new Span<byte>(new byte[2]);
        input.ReadBytes(readPtr);
        foreach (var a in readPtr)
            Console.WriteLine(a);
        
        var readPtr2 = new Span<byte>(new byte[4]);
        input.ReadBytes(readPtr2);
        foreach (var a in readPtr2)
            Console.WriteLine(a);
    }
}