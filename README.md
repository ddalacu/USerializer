# USerializer

[![.NET](https://github.com/ddalacu/USerializer/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/ddalacu/USerializer/actions/workflows/dotnet.yml)

USerializer is a version-tolerant binary serializer designed for high performance and compatibility across .NET Core, .NET Framework, and Mono.

It uses no code generation, making it ideal for AOT (Ahead-Of-Time) environments like Unity. It supports versioning (without field type changes) and uses reflection only during the first encounter of a type to cache serialization metadata.

### Key Features
- **High Performance:** Uses object pinning and direct memory access for speed, comparable to MessagePackSerializer.
- **Version Tolerant:** Supports adding or removing fields without breaking compatibility.
- **AOT Friendly:** No runtime or build-time code generation required.
- **Extensive Type Support:**
  - Primitive types
  - Enums
  - Custom classes and structs
  - Lists and Dictionaries
  - One-dimensional arrays
  - Custom serialization via `ISerializationPolicy` and custom serializers

### Usage Example

```csharp
// Setup the serializer with desired providers
var consoleLogger = new MyLogger();
ISerializationProvider[] providers =
{
    new PrimitivesSerializerProvider(),
    new CustomSerializerProvider(consoleLogger),
    new EnumSerializer(),
    new ArraySerializer(),
    new ListSerializer(),
    new DictionarySerializerProvider(),
    new ClassSerializationProvider(),
    new StructSerializationProvider(),
};

var serializer = new USerializer(new UnitySerializationPolicy(), providers, new DataTypesDatabase(), consoleLogger);

// Serialization
using var output = new SerializerOutput(8192, ArrayPool<byte>.Shared);
if (serializer.TryGetDataSerializer(myObject.GetType(), out var data))
{
    data.Write(SpanUtils.GetByteSpan(ref myObject), output, context);
    output.Flush(myStream);
}

// Deserialization
using var input = new SerializerInput(512, myStream, ArrayPool<byte>.Shared);
if (serializer.TryGetDataSerializer(typeof(MyClass), out var data))
{
    MyClass result = null;
    data.Read(SpanUtils.GetByteSpan(ref result), input, context);
    input.FinishRead();
}
```

### Limitations
- **No Polymorphism:** Only the exact type is serialized; derived types are not handled automatically.
- **Circular References:** Throws an exception if a circular reference is detected.
- **No Reference Tracking:** If the same object is referenced multiple times, it is serialized as a separate copy each time.
- **Properties:** Properties are not serialized automatically. Use custom serializers or back them with fields.
---
![Performance image](../gh-pages/output.png)

Detailed benchmarks can be found in [Performance.md](../gh-pages/performance.md).
