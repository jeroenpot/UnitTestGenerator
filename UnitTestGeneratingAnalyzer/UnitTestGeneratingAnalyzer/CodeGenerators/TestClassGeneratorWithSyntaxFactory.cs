using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestGeneratingAnalyzer.CodeGenerators
{
    class TestClassGeneratorWithSyntaxFactory
    {
        public SyntaxNode Get()
        {
            //            LocalDeclarationStatement(
            //    VariableDeclaration(
            //        IdentifierName("B"))
            //    .WithVariables(
            //        SingletonSeparatedList<VariableDeclaratorSyntax>(
            //            VariableDeclarator(
            //                Identifier("_target")))))
            //.NormalizeWhitespace();


            //new   FieldDeclaration(
            //     VariableDeclaration(
            //         IdentifierName("B"))
            //     .WithVariables(
            //         SingletonSeparatedList<VariableDeclaratorSyntax>(
            //             VariableDeclarator(
            //                 Identifier("_target")))))))

            return null;
        }
    }
}
