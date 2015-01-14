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
using System;

namespace NdapiAnalyzers
{
    [ExportCodeFixProvider("MissingPropertyAttributeCodeFix", LanguageNames.CSharp), Shared]
    public class IncompleteDllImportCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> GetFixableDiagnosticIds() => ImmutableArray.Create(IncompleteDllImportAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task ComputeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var attribute = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<AttributeSyntax>().First();

            context.RegisterFix(
                CodeAction.Create("Add missing parameters", c => AddAttributeArgument(context.Document, attribute, c)),
                diagnostic);
        }

        private async Task<Document> AddAttributeArgument(Document document, AttributeSyntax attribute, CancellationToken c)
        {
            var newAttribute = attribute;
            foreach (var argument in IncompleteDllImportAnalyzer.GetMissingArguments(attribute))
            {
                newAttribute = newAttribute.AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals(argument.Key), null, SyntaxFactory.ParseExpression(argument.Value)));
            }

            newAttribute = newAttribute
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync();
            root = root.ReplaceNode(attribute, newAttribute);
            return document.WithSyntaxRoot(root);
        }
    }
}
