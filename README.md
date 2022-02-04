# YAM - Yet Another Mapper

YAM is a object to object mapper using Roslyn Incremental Generator.

This mapper generate the source code for mapping on demand and at compile-time, which makes it reflection-free. It also makes mapping errors being reported at compile-time instead of being thrown as exceptions at runtime.

## Goals
 - (Done) Usage of the new Incremental Generator API from Roslyn.
 - (Done) Source code generation from Compilation Unit.
 - (ToDo) Diagnostics.