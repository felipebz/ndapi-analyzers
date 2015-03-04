using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace NdapiAnalyzers.Test
{
    public class MissingPropertyAttributeTests : CodeFixVerifier
    {
        [Fact]
        public void PropertyWithAttributeShouldNotTriggerDiagnostic()
        {
            const string test = @"
using Ndapi;

class TypeName : NdapiObject
{
    [Property(NdapiConstants.D2FP_FONT_SIZ)]
    public int Id
    {
        get { return GetNumberProperty(NdapiConstants.D2FP_FONT_SIZ); }
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
}";
            VerifyCSharpHasNoDiagnostics(test);
        }

        [Fact]
        public void PropertyWithGetAcessorWithoutAttributeShouldTriggerDiagnostic()
        {
            const string test = @"
using Ndapi;

class TypeName : NdapiObject
{
    public int Id
    {
        get { return GetNumberProperty(NdapiConstants.D2FP_FONT_SIZ); }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = MissingPropertyAttributeAnalyzer.DiagnosticId,
                Message = string.Format("Property '{0}' doesn't have a Property attribute", "Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 5) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void ExpressionBodiedPropertyWithoutAttributeShouldTriggerDiagnostic()
        {
            const string test = @"
using Ndapi;

class TypeName : NdapiObject
{
    public int Id => GetNumberProperty(NdapiConstants.D2FP_FONT_SIZ);
}";

            var expected = new DiagnosticResult
            {
                Id = MissingPropertyAttributeAnalyzer.DiagnosticId,
                Message = string.Format("Property '{0}' doesn't have a Property attribute", "Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 5) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void FixPropertyWithGetAcessorWithoutAttribute()
        {
            const string test = @"
using Ndapi.Core;

class TypeName : NdapiObject
{
    public int Id
    {
        get { return GetNumberProperty(NdapiConstants.D2FP_FONT_SIZ); }
    }
}";

            const string fixtest = @"
using Ndapi.Core;

class TypeName : NdapiObject
{
    [Property(NdapiConstants.D2FP_FONT_SIZ)]
    public int Id
    {
        get { return GetNumberProperty(NdapiConstants.D2FP_FONT_SIZ); }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }


        [Fact]
        public void FixPropertyWithGetAcessorAndCommentsWithoutAttribute()
        {
            const string test = @"
using Ndapi.Core;

class TypeName : NdapiObject
{
    // comment
    public int Id
    {
        get { return GetNumberProperty(NdapiConstants.D2FP_FONT_SIZ); }
    }
}";

            const string fixtest = @"
using Ndapi.Core;

class TypeName : NdapiObject
{
    // comment
    [Property(NdapiConstants.D2FP_FONT_SIZ)]
    public int Id
    {
        get { return GetNumberProperty(NdapiConstants.D2FP_FONT_SIZ); }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }


        [Fact]
        public void FixExpressionBodiedPropertyWithoutAttribute()
        {
            const string test = @"
using Ndapi.Core;

class TypeName : NdapiObject
{
    public int Id => GetNumberProperty(NdapiConstants.D2FP_FONT_SIZ);
}";

            const string fixtest = @"
using Ndapi.Core;

class TypeName : NdapiObject
{
    [Property(NdapiConstants.D2FP_FONT_SIZ)]
    public int Id => GetNumberProperty(NdapiConstants.D2FP_FONT_SIZ);
}";
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