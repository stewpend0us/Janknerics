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
    
    /*
    private bool? recurse(ClassDeclarationSyntax cls)
    {
        foreach (var list in cls.AttributeLists)
        {
            foreach (var attr in list.Attributes)
            {
                
            }
        }
    }
    */
    private static bool viable(MemberDeclarationSyntax member)
    {
        bool viable;
        if (!ContainsJanknericAttribute(member))
            viable = false;
        else
            viable = member is ClassDeclarationSyntax cls && cls.Members.Any(ContainsJanknericAttribute);
        
        return viable;
    }
    
    private static CSharpCompilation CreateCompilation(string source)
        => CSharpCompilation.Create("compilation",
            [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
}