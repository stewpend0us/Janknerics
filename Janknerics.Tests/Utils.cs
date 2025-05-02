using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Janknerics.Tests;

[TestClass]
public partial class JanknericsTests
{
    private static bool ContainsJanknericAttribute(MemberDeclarationSyntax member) => member.AttributeLists
        .Any(list => list.Attributes
            .Any(attr => attr.Name.ToString() == "Jankneric"));
    
    private static CSharpCompilation CreateCompilation(string source)
        => CSharpCompilation.Create("compilation",
            [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
}