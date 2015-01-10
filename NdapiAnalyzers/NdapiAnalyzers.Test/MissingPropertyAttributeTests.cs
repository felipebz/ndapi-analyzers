using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace NdapiAnalyzers.Test
{
    public class MissingPropertyAttributeTests : CodeFixVerifier
    {
        private const string _attributeClass = @"
sealed class PropertyAttribute : System.Attribute
{
    public PropertyAttribute(int id) { }
}";

        [Fact]
        public void PropertyWithAttributeShouldNotTriggerDiagnostic()
        {
            const string test = @"
class TypeName
{
    const int constant = 0;

    [Property(constant)]
    public int Id
    {
        get { return Test(constant); }
    }
}";
            VerifyCSharpHasNoDiagnostics(test);
        }

        [Fact]
        public void AutoImplementedPropertyShouldNotTriggerDiagnostic()
        {
            const string test = @"
class TypeName
{
    const int constant = 0;

    public int Id { get; set; }
}";
            VerifyCSharpHasNoDiagnostics(test);
        }

        [Fact]
        public void PropertyWithoutConstantShouldNotTriggerDiagnostic()
        {
            const string test = @"
class TypeName
{
    public int Id
    {
        get { return Test(); }
    }

    public int Test() { return 0; }
}";
            VerifyCSharpHasNoDiagnostics(test);
        }

        [Fact]
        public void PropertyWithTwoOrMoreExpressionsInGetterShouldNotTriggerDiagnostic()
        {
            const string test = @"
class TypeName
{
    public int Id
    {
        get
        {
            var i = 0;
            return i;
        }
    }

    public int Test() { return 0; }
}";
            VerifyCSharpHasNoDiagnostics(test);
        }

        [Fact]
        public void PropertyWithGetAcessorWithoutAttributeShouldTriggerDiagnostic()
        {
            const string test = @"
class TypeName
{
    const int constant = 0;

    public int Id
    {
        get { return Test(constant); }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MissingPropertyAttributeAnalyzer.DiagnosticId,
                Message = string.Format("Property '{0}' doesn't have a Property attribute", "Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void ExpressionBodiedPropertyWithoutAttributeShouldTriggerDiagnostic()
        {
            const string test = @"
class TypeName
{
    const int constant = 0;

    public int Id => Test(constant);
}";

            var expected = new DiagnosticResult
            {
                Id = MissingPropertyAttributeAnalyzer.DiagnosticId,
                Message = string.Format("Property '{0}' doesn't have a Property attribute", "Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void FixPropertyWithGetAcessorWithoutAttribute()
        {
            const string test = @"
class TypeName
{
    const int constant = 0;

    public int Id
    {
        get { return Test(constant); }
    }
}" + _attributeClass;

            const string fixtest = @"
class TypeName
{
    const int constant = 0;

    [Property(constant)]
    public int Id
    {
        get { return Test(constant); }
    }
}" + _attributeClass;
            VerifyCSharpFix(test, fixtest);
        }


        [Fact]
        public void FixPropertyWithGetAcessorAndCommentsWithoutAttribute()
        {
            const string test = @"
class TypeName
{
    const int constant = 0;

    // comment
    public int Id
    {
        get { return Test(constant); }
    }
}" + _attributeClass;

            const string fixtest = @"
class TypeName
{
    const int constant = 0;

    // comment
    [Property(constant)]
    public int Id
    {
        get { return Test(constant); }
    }
}" + _attributeClass;
            VerifyCSharpFix(test, fixtest);
        }


        [Fact]
        public void FixExpressionBodiedPropertyWithoutAttribute()
        {
            const string test = @"
class TypeName
{
    const int constant = 0;
            
    public int Id => Test(constant);
}" + _attributeClass;

            const string fixtest = @"
class TypeName
{
    const int constant = 0;

    [Property(constant)]
    public int Id => Test(constant);
}" + _attributeClass;
            VerifyCSharpFix(test, fixtest);
        }


        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MissingPropertyAttributeCodeFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MissingPropertyAttributeAnalyzer();
        }
    }
}