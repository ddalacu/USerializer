# USerializer
USerializer is a version tolerant binary serializer.

This serializer was made for Unity3D but will run on net core, net framework and mono (at least it should :)).

It uses no code generation so it works AOT and it supports versioning.

Uses same principles as Unity's serialization pipeline to make serialization as fast as possible


![Performance image](../gh-pages/output.png)

[Performance](../gh-pages/performance.md)

For examples on how to use this please check the BinaryUtility.cs inside tests project