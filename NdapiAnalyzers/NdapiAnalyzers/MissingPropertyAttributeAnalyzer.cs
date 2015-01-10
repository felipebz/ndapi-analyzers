using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NdapiAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingPropertyAttributeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NDAPI-1";
        internal const string Title = "Add Property attribute";
        internal const string MessageFormat = "Property '{0}' don't have a Property attribute";
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var property = (PropertyDeclarationSyntax)context.Node;
            if (property.AttributeLists.Any()) return;

            var constant = FindConstant(property);
            if (constant == null) return;

            var argumentSymbol = context.SemanticModel.GetConstantValue(constant, context.CancellationToken);
            if (!argumentSymbol.HasValue) return;

            var diagnostic = Diagnostic.Create(Rule, property.GetLocation(), property.Identifier);

            context.ReportDiagnostic(diagnostic);
        }

        public static ExpressionSyntax FindConstant(PropertyDeclarationSyntax property)
        {
            InvocationExpressionSyntax functionCall;

            if (property.AccessorList == null)
            {
                functionCall = property.ExpressionBody.Expression as InvocationExpressionSyntax;
            }
            else
            {
                var getterAccessor = property.AccessorList.Accessors.Single(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                if (getterAccessor.Body == null) return null;
                var getterStatements = getterAccessor.Body.Statements;

                if (getterStatements.Count != 1) return null;

                var statement = getterStatements.Single() as ReturnStatementSyntax;
                if (statement == null) return null;

                functionCall = statement.Expression as InvocationExpressionSyntax;
            }

            if (functionCall == null) return null;

            return functionCall.ArgumentList.Arguments.SingleOrDefault()?.Expression;
        }
    }
}
