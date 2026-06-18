using System.Collections;
using System.Text;

namespace UnionGen
{
    internal sealed record UnionToGenerate(
        string Name,
        string Namespace,
        string Visibility,
        ValueEqualityArray<TypeParameter> TypeParameters,
        ValueEqualityArray<ParentType> ParentTypes,
        ValueEqualityArray<DiagnosticHelper.Error> Errors)
    {
        /// <summary>
        /// Unique file hint for <c>AddSource</c>. Parent type names are included so that two
        /// equally named unions nested in different types within the same namespace do not collide.
        /// </summary>
        public string HintName
        {
            get
            {
                if (ParentTypes.Count == 0)
                {
                    return $"{Namespace}.{Name}.g.cs";
                }

                var builder = new StringBuilder(Namespace);
                // ParentTypes are stored innermost-first; reverse so the hint reads outermost-to-innermost.
                foreach (var parentType in ParentTypes.Reverse())
                {
                    builder.Append('.').Append(parentType.Name);
                }

                builder.Append('.').Append(Name).Append(".g.cs");

                return builder.ToString();
            }
        }

        public static UnionToGenerate ForError(string name, DiagnosticHelper.Error error)
            => ForError(name, [error]);

        public static UnionToGenerate ForError(string name, IEnumerable<DiagnosticHelper.Error> errors)
            => new(name, string.Empty, string.Empty,
                   new(Array.Empty<TypeParameter>()),
                   new(Array.Empty<ParentType>()),
                   new(errors));

        public bool AnyReferenceType()
        {
            for (var index = 0; index < TypeParameters.Count; index++)
            {
                var type = TypeParameters[index];
                if (type.IsReferenceType)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal readonly record struct ParentType(string Name, string Type, string Visibility)
    {
        public const string Class = "class";
        public const string Struct = "struct";
        public const string Interface = "interface";
    }

    internal sealed record TypeParameter(string Name, string FullName, string GlobalName, bool IsReferenceType, bool IsInterface)
    {
        private string? _titleCaseName;
        private string? _wellKnownName;
        
        public string TitleCaseName => _titleCaseName ??= Name.EnsureTitleCase();
        public string WellKnownName => _wellKnownName ??= WellKnownTypes.AdjustIfWellKnown(TitleCaseName);
        public string CallOperator => IsReferenceType
            ? "?."
            : ".";
    }

    internal sealed class ValueEqualityArray<T>(IEnumerable<T> items) : IReadOnlyList<T>, IEquatable<ValueEqualityArray<T>>
        where T : IEquatable<T>
    {
        private readonly T[] _items = items as T[] ?? items.ToArray();

        public T this[int index] => _items[index];

        public int Count => _items.Length;

        public bool Equals(ValueEqualityArray<T>? other)
        {
            if (other is null || Count != other.Count)
            {
                return false;
            }

            for (var i = 0; i < Count; i++)
            {
                if (!this[i].Equals(other[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj) => Equals(obj as ValueEqualityArray<T>);

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 19;
                foreach (var item in _items)
                {
                    hash = hash * 31 + item.GetHashCode();
                }

                return hash;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return item;
            }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}