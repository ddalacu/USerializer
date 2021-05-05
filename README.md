# USerializer

[![.NET](https://github.com/ddalacu/USerializer/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/ddalacu/USerializer/actions/workflows/dotnet.yml)

USerializer is a version tolerant binary serializer.

Will run on net core, net framework and mono (at least it should :)).

It uses no code generation so it works AOT and it supports versioning(no support for changing field types),
 it does however use reflection when first encountering a type to extract type data so you will see a perf pennality first time serailizing a certain type
 
Uses same principles as Unity's serialization pipeline to make serialization as fast as possible.
As a difference to unity serialization there is null serialization support.

Features:
Serializes primitive data
Serializes enums
Serializes custom classes, structs
Serializes Lists
Serializes one dimmension arrays
Control on what fields/types get serialized is obtained by implementing ISerializationPolicy


Limitations and downsides:
No support for polimorphism.
No support for Dictionaries :( (can be implemented but would require a AOT generation step)
Throws exception in case of circular references
Properties won't serialize auttomaticaly (add custom serializers and serialize properties too,for examples on how to do this check ExampleClassSerializer inside tests project)

This project is best used in cases where no code generation is allowed and you don't want to run any AOT generation, i use this as a dirrect replacement for unity's JsonUtillity

![Performance image](../gh-pages/output.png)

[Performance](../gh-pages/performance.md)

Performance comes close to MessagePackSerializer even if no code is generated!
Ceras is slower in perf tests because version tolerance is set to VersionToleranceMode.Standard also ceras offers way more features than this library does

Only serializes fields but you can implement add custom serializers and serialize properties too,
for examples on how to do this check ExampleClassSerializer inside tests project

For examples on how to use this system, please check the BinaryUtility.cs inside tests project