using Microsoft.CodeAnalysis;

namespace UnionGen;

internal readonly struct DiagnosticHelper(Action<Diagnostic> reportDiagnostic)
{
    private const string IdPrefix = "UNIONGEN";
    
    public void ReportError(Error error) => Report(error.Id, error.Message);

    private void Report(int diagId, string message)
    {
        var descriptor = new DiagnosticDescriptor($"{IdPrefix}{diagId:00}",
                                                  "UnionGen Error",
                                                  message,
                                                  "UnionGen Source Generator",
                                                  DiagnosticSeverity.Error,
                                                  true);
        var diagnostic = Diagnostic.Create(descriptor, Location.None);
        reportDiagnostic(diagnostic);
    }

    public readonly record struct Error(int Id, string Message);
    
    public static class ErrorIds
    {
        public const int NestingNotPartial = 11;
        public const int NestingUnknownParent = 12;
        public const int DuplicateType = 21;
        public const int DuplicateMemberName = 22;
        public const int GlobalNamespace = 31;
    }
}
