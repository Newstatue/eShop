using System;
using System.Linq;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

void DumpMethods(Type type)
{
	foreach (var method in type.GetMethods().OrderBy(m => m.Name))
	{
		var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
		Console.WriteLine($"{method.Name}({parameters})");
	}
}

DumpMethods(typeof(InMemoryVectorStore));

Console.WriteLine("---");

var vectorAssembly = typeof(VectorStoreCollectionDefinition).Assembly;
foreach (var type in vectorAssembly.GetTypes().OrderBy(t => t.FullName))
{
	Console.WriteLine(type.FullName);
}

Console.WriteLine("--- Collection Methods ---");

var vectorStoreCollectionType = vectorAssembly.GetType("Microsoft.Extensions.VectorData.VectorStoreCollection`2")!
	.MakeGenericType(typeof(int), typeof(object));

DumpMethods(vectorStoreCollectionType);
