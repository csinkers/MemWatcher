using System.Text.RegularExpressions;
using MemWatcher.Types;

namespace MemWatcher;

public class DirectiveParser
{
    readonly Func<string, string, IGhidraType> _buildDummyType;

    /* Syntax:

    cast\(([^,]+), (.+)\) - perform typecast on member

    */
    static readonly Regex CastRegex = new(@"cast\(([^,]+), (.+)\)");
    public DirectiveParser(Func<string, string, IGhidraType> buildDummyType) 
        => _buildDummyType = buildDummyType ?? throw new ArgumentNullException(nameof(buildDummyType));

    public IEnumerable<IDirective> TryParse(string comment)
    {
        var directives = comment.Split('#');
        foreach (var directive in directives.Skip(1))
        {
            var cast = TryParseCast(directive);
            if (cast != null)
                yield return cast;
        }
    }

    IDirective? TryParseCast(string comment)
    {
        var castMatch = CastRegex.Match(comment);
        if (!castMatch.Success) 
            return null;

        var path = castMatch.Groups[1].Value;
        var typeName = castMatch.Groups[2].Value;
        var parts = path.Split('.');
        var typeKey = SplitType(typeName);

        var dummyType = _buildDummyType(typeKey.ns, typeKey.name);
        return BuildDirectiveForPath(parts, new DTypeCast(dummyType));
    }

    static IDirective BuildDirectiveForPath(string[] parts, IDirective directive)
    {
        for (int i = parts.Length - 1; i >= 0; i--)
            directive = new DTargetChild(parts[i], directive);
        return directive;
    }

    static (string ns, string name) SplitType(string typeName)
    {
        var index = typeName.LastIndexOf('/');
        if (index == -1)
            return ("/", typeName);

        var ns = typeName[..index];
        var type = typeName[(index + 1)..];
        return (ns, type);
    }
}