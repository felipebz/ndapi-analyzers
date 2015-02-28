using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace NdapiAnalyzers.Test
{
    public class IncompleteDllImportTests : CodeFixVerifier
    {
        [Fact]
        public void CompleteDllImportWithoutOutStringParameterShouldNotTriggerDiagnostic()
        {
            const string test = @"
using System.Runtime.InteropServices;

class TypeName
{
    [DllImport(""dll"", CallingConvention = CallingConvention.Cdecl)]
    public static extern void method();
}";
            VerifyCSharpHasNoDiagnostics(test);
        }

        [Fact]
        public void CompleteDllImportWithOutStringParameterShouldNotTriggerDiagnostic()
        {
            const string test = @"
using System.Runtime.InteropServices;

class TypeName
{
    [DllImport(""dll"", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern void method(out string str);
}";
            VerifyCSharpHasNoDiagnostics(test);
        }

        [Fact]
        public void IncompleteDllImportWithoutOutStringParameterShouldTriggerDiagnostic()
        {
            const string test = @"
using System.Runtime.InteropServices;

class TypeName
{
    [DllImport(""dll"")]
    public static extern void method();
}";

            var expected = new DiagnosticResult
            {
                Id = IncompleteDllImportAnalyzer.DiagnosticId,
                Message = string.Format("DllImport for method '{0}' is incomplete", "method"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 6) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void IncompleteDllImportWithOutStringParameterShouldTriggerDiagnostic()
        {
            const string test = @"
using System.Runtime.InteropServices;

class TypeName
{
    [DllImport(""dll"", CallingConvention = CallingConvention.Cdecl)]
    public static extern void method(out string str);
}";

            var expected = new DiagnosticResult
            {
                Id = IncompleteDllImportAnalyzer.DiagnosticId,
                Message = string.Format("DllImport for method '{0}' is incomplete", "method"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 6) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void FixDllImportWithoutOutStringParameterShouldTriggerDiagnostic()
        {
            const string test = @"
using System.Runtime.InteropServices;

class TypeName
{
    [DllImport(""dll"")]
    public static extern void method();
}";

            const string fixtest = @"
using System.Runtime.InteropServices;

class TypeName
{
    [DllImport(""dll"", CallingConvention = CallingConvention.Cdecl)]
    public static extern void method();
}";
            VerifyCSharpFix(test, fixtest);
        }

        [Fact]
        public void FixDllImportWithOutStringParameterShouldTriggerDiagnostic()
        {
            const string test = @"
using System.Runtime.InteropServices;

class TypeName
{
    [DllImport(""dll"", CallingConvention = CallingConvention.Cdecl)]
    public static extern void method(out string str);
}";

            const string fixtest = @"
using System.Runtime.InteropServices;

class TypeName
{
    [DllImport(""dll"", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern void method(out string str);
}";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new IncompleteDllImportCodeFix();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new IncompleteDllImportAnalyzer();
        }
    }
}
