namespace UnionGen;

internal static class Extensions
{
    public static string EnsureTitleCase(this string type)
    {
        Span<char> t = type.ToCharArray();
        if (t.Length == 0)
        {
            return type;
        }
        
        t[0] = char.ToUpper(t[0]);
        return t.ToString();
    }
}
