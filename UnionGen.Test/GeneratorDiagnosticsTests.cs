using AwesomeAssertions;
using UnionGen.Test.Infrastructure;

namespace UnionGen.Test;

public sealed class GeneratorDiagnosticsTests
{
    private const string DuplicateTypeId = "UNIONGEN21";
    private const string DuplicateMemberNameId = "UNIONGEN22";
    private const string GlobalNamespaceId = "UNIONGEN31";
    private const string NestingNotPartialId = "UNIONGEN11";

    [Fact]
    public void QualifiedAttributeName_GeneratesUnion()
    {
        const string Source = """
                              namespace Test;

                              [UnionGen.Union<int, string>]
                              public readonly partial struct Qualified;
                              """;

        var output = GeneratorTestHelper.Run(Source);

        output.GeneratedFileNames().Should().ContainSingle();
        output.ErrorDiagnostics().Should().BeEmpty();
    }

    [Fact]
    public void AliasedAttributeName_GeneratesUnion()
    {
        const string Source = """
                              using UnionAlias = UnionGen.UnionAttribute<int, string>;

                              namespace Test;

                              [UnionAlias]
                              public readonly partial struct Aliased;
                              """;

        var output = GeneratorTestHelper.Run(Source);

        output.GeneratedFileNames().Should().ContainSingle();
        output.ErrorDiagnostics().Should().BeEmpty();
    }

    [Fact]
    public void DuplicateType_ReportsDiagnostic()
    {
        const string Source = """
                              using UnionGen;

                              namespace Test;

                              [Union<int, int>]
                              public readonly partial struct Dup;
                              """;

        var output = GeneratorTestHelper.Run(Source);

        output.HasGeneratorDiagnostic(DuplicateTypeId).Should().BeTrue();
        output.GeneratedFileNames().Should().BeEmpty();
    }

    [Fact]
    public void TypesWithCollidingGeneratedName_ReportsDiagnostic()
    {
        const string Source = """
                              using UnionGen;

                              namespace A { public sealed class Thing; }
                              namespace B { public sealed class Thing; }

                              namespace Test;

                              [Union<A.Thing, B.Thing>]
                              public readonly partial struct Collide;
                              """;

        var output = GeneratorTestHelper.Run(Source);

        output.HasGeneratorDiagnostic(DuplicateMemberNameId).Should().BeTrue();
        output.GeneratedFileNames().Should().BeEmpty();
    }

    [Fact]
    public void GlobalNamespace_ReportsDiagnostic()
    {
        const string Source = """
                              using UnionGen;

                              [Union<int, string>]
                              public readonly partial struct InGlobalNamespace;
                              """;

        var output = GeneratorTestHelper.Run(Source);

        output.HasGeneratorDiagnostic(GlobalNamespaceId).Should().BeTrue();
        output.GeneratedFileNames().Should().BeEmpty();
    }

    [Fact]
    public void NestedInNonPartialType_ReportsDiagnostic()
    {
        const string Source = """
                              using UnionGen;

                              namespace Test;

                              public class Outer
                              {
                                  [Union<int, string>]
                                  public readonly partial struct Nested;
                              }
                              """;

        var output = GeneratorTestHelper.Run(Source);

        output.HasGeneratorDiagnostic(NestingNotPartialId).Should().BeTrue();
        output.GeneratedFileNames().Should().BeEmpty();
    }

    [Fact]
    public void SameNamedNestedUnions_ProduceDistinctHintNames()
    {
        // Regression: hint name now includes parent type names, so equally named nested unions
        // in different parents within one namespace no longer collide.
        const string Source = """
                              using UnionGen;

                              namespace Test;

                              public partial class A
                              {
                                  [Union<int, string>]
                                  public readonly partial struct N;
                              }

                              public partial class B
                              {
                                  [Union<int, string>]
                                  public readonly partial struct N;
                              }
                              """;

        var output = GeneratorTestHelper.Run(Source);

        var fileNames = output.GeneratedFileNames();
        fileNames.Should().HaveCount(2);
        fileNames.Distinct().Should().HaveCount(2);
        output.ErrorDiagnostics().Should().BeEmpty();
    }

    [Fact]
    public void DefaultConstructor_IsObsoleteError()
    {
        const string Source = """
                              using UnionGen;

                              namespace Test;

                              [Union<int, string>]
                              public readonly partial struct U;

                              public static class Consumer
                              {
                                  public static U Make() => new U();
                              }
                              """;

        var output = GeneratorTestHelper.Run(Source);

        // CS0619: member is obsolete with error=true
        output.ErrorDiagnostics().Should().Contain(static d => d.Id == "CS0619");
    }

    [Fact]
    public void TopLevelStructWithoutModifier_CompilesAsInternal()
    {
        // Regression: a top-level struct without an access modifier must not get an illegal
        // `private` modifier in the generated partial.
        const string Source = """
                              using UnionGen;

                              namespace Test;

                              [Union<int, string>]
                              readonly partial struct NoModifier;
                              """;

        var output = GeneratorTestHelper.Run(Source);

        output.GeneratedFileNames().Should().ContainSingle();
        output.ErrorDiagnostics().Should().BeEmpty();
    }
}
