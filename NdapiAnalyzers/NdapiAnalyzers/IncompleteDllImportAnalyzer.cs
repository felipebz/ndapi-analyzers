using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NdapiAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IncompleteDllImportAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NDAPI-2";
        internal const string Title = "Incomplete DllImport";
        internal const string MessageFormat = "DllImport for method '{0}' is incomplete";
        internal const string Category = "Design";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.Attribute);
        }

        private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var attribute = (AttributeSyntax)context.Node;

            if (attribute.Name.ToString() != "DllImport") return;

            var methodDeclaration = attribute.Parent.Parent as MethodDeclarationSyntax;
            if (methodDeclaration == null) return;

            if (GetMissingArguments(attribute).Any())
            {
                var diagnostic = Diagnostic.Create(Rule, attribute.GetLocation(), methodDeclaration.Identifier);
                context.ReportDiagnostic(diagnostic);
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> GetMissingArguments(AttributeSyntax attribute)
        {
            var methodDeclaration = (MethodDeclarationSyntax)attribute.Parent.Parent;

            var methodHasAnOutStringParameter = methodDeclaration.ParameterList.Parameters
                .Any(parameter => parameter.Modifiers.Any(SyntaxKind.OutKeyword) && parameter.Type.ToString() == "string");

            var expectedArguments = ImmutableList.CreateBuilder<KeyValuePair<string, string>>();
            expectedArguments.Add(new KeyValuePair<string, string>("CallingConvention", "CallingConvention.Cdecl"));

            if (methodHasAnOutStringParameter)
            {
                expectedArguments.Add(new KeyValuePair<string, string>("CharSet", "CharSet.Ansi"));
                expectedArguments.Add(new KeyValuePair<string, string>("BestFitMapping", "false"));
                expectedArguments.Add(new KeyValuePair<string, string>("ThrowOnUnmappableChar", "true"));
            }

            var actualArguments = attribute.ArgumentList.Arguments
                .Where(argument => argument.NameEquals != null)
                .ToDictionary(argument => argument.NameEquals.Name.ToString(),
                                       argument => argument.Expression.ToString());

            return expectedArguments.Except(actualArguments);
        }
    }
}
