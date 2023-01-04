using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Horizon.Generators.Kernel
{
    class SyscallSyntaxReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> SvcImplementations { get; }

        public SyscallSyntaxReceiver()
        {
            SvcImplementations = new List<MethodDeclarationSyntax>();
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.AttributeLists.Count != 0)
            {
                foreach (var attributeList in classDeclaration.AttributeLists)
                {
                    if (attributeList.Attributes.Any(x => x.Name.GetText().ToString() == "SvcImpl"))
                    {
                        foreach (var memberDeclaration in classDeclaration.Members)
                        {
                            if (memberDeclaration is MethodDeclarationSyntax methodDeclaration)
                            {
                                VisitMethod(methodDeclaration);
                            }
                        }

                        break;
                    }
                }
            }
        }

        private void VisitMethod(MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.AttributeLists.Count != 0)
            {
                foreach (var attributeList in methodDeclaration.AttributeLists)
                {
                    if (attributeList.Attributes.Any(x => x.Name.GetText().ToString() == "Svc"))
                    {
                        SvcImplementations.Add(methodDeclaration);
                        break;
                    }
                }
            }
        }
    }
}
