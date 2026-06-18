namespace UnionGen.Test;

public sealed class UnionGeneratorSnapshotTests
{
    [Fact]
    public Task ValueTypesOnly()
    {
        const string Source = """
                              using UnionGen;

                              namespace Test;

                              [Union<int, double>]
                              public readonly partial struct IntOrDouble;
                              """;

        return SnapshotTestHelper.Verify(Source);
    }

    [Fact]
    public Task ReferenceTypesOnly()
    {
        const string Source = """
                              using System;
                              using UnionGen;

                              namespace Test;

                              [Union<string, Exception>]
                              public readonly partial struct StringOrException;
                              """;

        return SnapshotTestHelper.Verify(Source);
    }

    [Fact]
    public Task MixedValueAndReferenceTypes()
    {
        const string Source = """
                              using UnionGen;

                              namespace Test;

                              [Union<int, string>]
                              public readonly partial struct IntOrString;
                              """;

        return SnapshotTestHelper.Verify(Source);
    }

    [Fact]
    public Task WithInterfaceCase()
    {
        const string Source = """
                              using System.Collections.Generic;
                              using UnionGen;

                              namespace Test;

                              [Union<IList<int>, long>]
                              public readonly partial struct ListOrLong;
                              """;

        return SnapshotTestHelper.Verify(Source);
    }

    [Fact]
    public Task GenericCase()
    {
        const string Source = """
                              using UnionGen;
                              using UnionGen.Types;

                              namespace Test;

                              [Union<Success<int>, Failure>]
                              public readonly partial struct SuccessOrFailure;
                              """;

        return SnapshotTestHelper.Verify(Source);
    }

    [Fact]
    public Task ArrayCase()
    {
        const string Source = """
                              using UnionGen;

                              namespace Test;

                              [Union<int, long[]>]
                              public readonly partial struct IntOrLongArray;
                              """;

        return SnapshotTestHelper.Verify(Source);
    }

    [Fact]
    public Task NestedInPartialType()
    {
        const string Source = """
                              using UnionGen;

                              namespace Test;

                              public partial interface IOuter
                              {
                                  [Union<int, double, long>]
                                  public readonly partial struct Nested;
                              }
                              """;

        return SnapshotTestHelper.Verify(Source);
    }

    [Fact]
    public Task QualifiedAttributeName()
    {
        // Regression: the fully qualified attribute form must be recognised just like the short form.
        const string Source = """
                              namespace Test;

                              [UnionGen.Union<int, string>]
                              public readonly partial struct Qualified;
                              """;

        return SnapshotTestHelper.Verify(Source);
    }
}
