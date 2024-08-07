using MyNamespace;
using OneOf;
using UnionGen;
using UnionGen.Types;

namespace UnionDemo
{
    [Union<int, double, long[], List<Foo>, Dictionary<string, bool>>]
    internal readonly partial struct DemoObj;

    [Union<Result<int>, NotFound>]
    public readonly partial struct SimpleObj;

    [Union<IList<int>, long, Error>]
    public readonly partial struct WithInterface;
    
    [Union<Success<int>, Failure>]
    public readonly partial struct SuccessWithWellKnownType;
    
    [Union<Success<byte[]>, Failure>]
    public readonly partial struct SuccessWithArrayType;
}

namespace MyNamespace
{
    public class Foo;
}

namespace OneOfComparison
{
    /*[GenerateOneOf]
    public sealed partial class WithInterfaceOneOf : OneOfBase<IList<int>, long, Error>
    {
        public WithInterfaceOneOf(OneOf<IList<int>, long, Error> input) : base(input) { }

        public static OneOf<IList<int>, long, Error> Foo()
        {
            IList<int> bar = [1, 3, 4];
            long baz = 42;
            return baz;
        }
    }*/
}
