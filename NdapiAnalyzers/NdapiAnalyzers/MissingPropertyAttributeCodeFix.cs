using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace NdapiAnalyzers
{
    [ExportCodeFixProvider("MissingPropertyAttributeCodeFix", LanguageNames.CSharp), Shared]
    public class MissingPropertyAttributeCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> GetFixableDiagnosticIds() => ImmutableArray.Create(MissingPropertyAttributeAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task ComputeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

            var constant = MissingPropertyAttributeAnalyzer.FindConstant(declaration);

            context.RegisterFix(
                CodeAction.Create("Add Property attribute", c => AddAttribute(context.Document, declaration, constant, c)),
                diagnostic);
        }

        private async Task<Document> AddAttribute(Document document, PropertyDeclarationSyntax property, ExpressionSyntax constant, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync();

            var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Property"))
                .WithArgumentList(
                    SyntaxFactory.AttributeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AttributeArgument(constant))));
            
            var newProperty = property.WithoutLeadingTrivia()
                .AddAttributeLists(
                    SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                        .WithLeadingTrivia(property.GetLeadingTrivia()));

            newProperty = newProperty
                .WithAdditionalAnnotations(Formatter.Annotation);

            root = root.ReplaceNode(property, newProperty);
            return document.WithSyntaxRoot(root);
        }
    }
}