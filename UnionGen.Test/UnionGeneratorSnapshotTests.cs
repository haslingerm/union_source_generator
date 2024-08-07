namespace UnionGen.Test;

public sealed class UnionGeneratorSnapshotTests
{
    [Fact]
    public Task GeneratesUnionStructCorrectly()
    {
        // The source code to test
        var source = """
                     namespace Test1;
                     using UnionGen;
                     
                     [ProtoMember(1)]
                     [UnionGen.Union<int,string>]
                     public readonly partial struct DemoObj;
                     """;

        // Pass the source code to our helper and snapshot test the output
        return SnapshotTestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesUnionStructCorrectly_WithArray()
    {
        // The source code to test
        var source = """
                     namespace Test1;
                     using UnionGen;

                     [ProtoMember(1)]
                     [Union<int,long[]>(UnionAlignment.Aligned8)]
                     public partial struct DemoObj;
                     """;

        // Pass the source code to our helper and snapshot test the output
        return SnapshotTestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesUnionStructCorrectly_NestedType()
    {
        // The source code to test
        var source = """
                     namespace Test1;
                     using UnionGen;
                     
                     public interface INested
                     {
                         [UnionGen.Union<int, double, long>]
                         public readonly partial struct Nested;
                     }
                     """;

        // Pass the source code to our helper and snapshot test the output
        return SnapshotTestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesUnionStructCorrectly_ByteArraySuccess()
    {
        // The source code to test
        var source = """
                     namespace Test1;
                     using UnionGen;

                     [UnionGen.Union<Success<byte[]>, Failure>]
                     public readonly partial struct Nested;
                     """;

        // Pass the source code to our helper and snapshot test the output
        return SnapshotTestHelper.Verify(source);
    }
}
