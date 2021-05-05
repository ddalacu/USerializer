# USerializer

[![.NET](https://github.com/ddalacu/USerializer/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/ddalacu/USerializer/actions/workflows/dotnet.yml)

USerializer is a version tolerant binary serializer.

This serializer was made for Unity3D but will run on net core, net framework and mono (at least it should :)).

It uses no code generation so it works AOT and it supports versioning.

Uses same principles as Unity's serialization pipeline to make serialization as fast as possible, so you don't get support for polimorphism but 
as a difference to unity serialization there is null serialization support

This project is best used in cases where no code generation is allowed and you don't want to run any AOT generation

![Performance image](../gh-pages/output.png)

[Performance](../gh-pages/performance.md)

Performance comes close to MessagePackSerializer even if no code is generated!
Ceras is slower in perf tests because version tolerance is set to VersionToleranceMode.Standard also ceras offers way more features than this library does


Only serializes fields but you can implement add custom serializers and serialize properties too,
for examples on how to do this check ExampleClassSerializer inside tests project

For examples on how to use this please check the BinaryUtility.cs inside tests project