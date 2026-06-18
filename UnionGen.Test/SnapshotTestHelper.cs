using UnionGen.Test.Infrastructure;

namespace UnionGen.Test;

public static class SnapshotTestHelper
{
    public static Task Verify(string source)
    {
        var output = GeneratorTestHelper.Run(source);

        return Verifier
               .Verify(output.Driver)
               .UseDirectory("Snapshots");
    }
}
