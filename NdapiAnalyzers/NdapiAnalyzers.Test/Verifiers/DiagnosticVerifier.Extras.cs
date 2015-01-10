namespace TestHelper
{
    public abstract partial class DiagnosticVerifier
    {
        protected void VerifyCSharpHasNoDiagnostics(string source) =>
            VerifyCSharpDiagnostic(source);

        protected void VerifyCSharpHasNoDiagnostics(params string[] sources) =>
            VerifyCSharpDiagnostic(sources);
    }
}
