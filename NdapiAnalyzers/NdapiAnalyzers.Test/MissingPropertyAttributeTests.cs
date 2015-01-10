using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace NdapiAnalyzers.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        private const string _attributeClass = @"
sealed class PropertyAttribute : System.Attribute
{
    public PropertyAttribute(int id) { }
}";

        [TestMethod]
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
}" + _attributeClass;
            VerifyCSharpHasNoDiagnostics(test);
        }

        [TestMethod]
        public void AutoImplementedPropertyShouldNotTriggerDiagnostic()
        {
            const string test = @"
class TypeName
{
    const int constant = 0;

    public int Id { get; set; }
}" + _attributeClass;
            VerifyCSharpHasNoDiagnostics(test);
        }

        [TestMethod]
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
}" + _attributeClass;
            VerifyCSharpHasNoDiagnostics(test);
        }

        [TestMethod]
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
}" + _attributeClass;

            var expected = new DiagnosticResult
            {
                Id = MissingPropertyAttributeAnalyzer.DiagnosticId,
                Message = string.Format("Property '{0}' don't have a Property attribute", "Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ExpressionBodiedPropertyWithoutAttributeShouldTriggerDiagnostic()
        {
            const string test = @"
class TypeName
{
    const int constant = 0;

    public int Id => Test(constant);
}" + _attributeClass;

            var expected = new DiagnosticResult
            {
                Id = MissingPropertyAttributeAnalyzer.DiagnosticId,
                Message = string.Format("Property '{0}' don't have a Property attribute", "Id"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
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


        [TestMethod]
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


        [TestMethod]
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