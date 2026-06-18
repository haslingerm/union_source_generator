using System.Collections.Immutable;
using System.Reflection;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnionGen.InternalUtil;

namespace UnionGen.Test.Infrastructure;

/// <summary>
/// Drives <see cref="UnionSourceGen" /> over a source snippet and (optionally) emits + loads the
/// resulting assembly so tests can exercise the generated unions for real via reflection.
/// </summary>
internal static class GeneratorTestHelper
{
    private static readonly CSharpParseOptions parseOptions = new(LanguageVersion.Latest);

    private static readonly ImmutableArray<PortableExecutableReference> references = BuildReferences();

    public static GeneratorOutput Run(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CSharpCompilation.Create("UnionGenTests",
                                                   [syntaxTree],
                                                   references,
                                                   new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create([new UnionSourceGen().AsSourceGenerator()],
                                                  parseOptions: parseOptions);

        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation,
                                                                     out var outputCompilation,
                                                                     out var generatorDiagnostics);

        return new GeneratorOutput(outputCompilation, generatorDiagnostics, updatedDriver);
    }

    /// <summary>
    /// Compiles the source together with the generated unions, asserts there are no compile errors,
    /// then loads the emitted assembly so its members can be invoked through reflection.
    /// </summary>
    public static Assembly CompileAndLoad(string source)
    {
        var output = Run(source);

        var compileErrors = output.OutputCompilation.GetDiagnostics()
                                  .Where(static d => d.Severity == DiagnosticSeverity.Error)
                                  .ToArray();
        compileErrors.Should()
                     .BeEmpty("the generated union code must compile cleanly\n"
                              + string.Join('\n', compileErrors.Select(static d => d.ToString())));

        using var peStream = new MemoryStream();
        var emitResult = output.OutputCompilation.Emit(peStream);
        emitResult.Success.Should()
                  .BeTrue("the assembly must emit\n"
                          + string.Join('\n', emitResult.Diagnostics.Select(static d => d.ToString())));

        return Assembly.Load(peStream.ToArray());
    }

    public static object? InvokeStatic(this Assembly assembly, string typeName, string methodName, params object?[] args)
    {
        var type = assembly.GetType(typeName)
                   ?? throw new InvalidOperationException($"Type '{typeName}' not found in generated assembly");
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                     ?? throw new InvalidOperationException($"Static method '{methodName}' not found on '{typeName}'");

        try
        {
            return method.Invoke(null, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }

    private static ImmutableArray<PortableExecutableReference> BuildReferences()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trustedPlatformAssemblies)
        {
            foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator))
            {
                if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                {
                    paths.Add(path);
                }
            }
        }

        // Ensure the UnionGen runtime assembly (StateByte, ExceptionHelper, UnionAttribute, ...) is referenced.
        paths.Add(typeof(StateByte).Assembly.Location);

        return paths.Select(static p => MetadataReference.CreateFromFile(p)).ToImmutableArray();
    }
}

internal sealed record GeneratorOutput(
    Compilation OutputCompilation,
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    GeneratorDriver Driver)
{
    public ImmutableArray<Diagnostic> ErrorDiagnostics()
        => OutputCompilation.GetDiagnostics()
                            .Concat(GeneratorDiagnostics)
                            .Where(static d => d.Severity == DiagnosticSeverity.Error)
                            .ToImmutableArray();

    public bool HasGeneratorDiagnostic(string id)
        => GeneratorDiagnostics.Any(d => d.Id == id);

    public ImmutableArray<string> GeneratedFileNames()
        => Driver.GetRunResult()
                 .GeneratedTrees
                 .Select(static t => Path.GetFileName(t.FilePath))
                 .ToImmutableArray();
}
