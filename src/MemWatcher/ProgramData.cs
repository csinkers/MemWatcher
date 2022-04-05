using System.Globalization;
using System.Xml;
using MemWatcher.Types;

namespace MemWatcher;

public class ProgramData
{
    readonly (uint Address, string Name)[] _symbols;
    public Dictionary<(string ns, string name), IGhidraType> Types { get; } = new();
    public GNamespace Root { get; }

    public string Describe(uint address)
    {
        if (address == 0)
            return "(null)";

        var index = FindNearest(address);
        var symbol = _symbols[index];
        var delta = (int)(address - symbol.Address);
        var sign = delta < 0 ? '-' : '+';
        var absDelta = Math.Abs(delta);
        return delta > 0 
            ? $"{symbol.Name}{sign}0x{absDelta:X} ({address:X})" 
            : symbol.Name;
    }

    int FindNearest(uint address) => Util.FindNearest(_symbols, address);

    public ProgramData(Stream xmlStream)
    {
        var doc = new XmlDocument();
        doc.Load(xmlStream);

        Types[(GString.Instance.Namespace, GString.Instance.Name)] = GString.Instance;
        foreach (var primitive in GPrimitive.PrimitiveTypes)
            Types[(primitive.Namespace, primitive.Name)] = primitive;

        IGhidraType BuildDummyType(string ns, string name)
        {
            ns = ns.Trim();
            name = name.Trim();

            if (Types.TryGetValue((ns, name), out var existing))
                return existing;

            if (name == "char *")
                return new GPointer(BuildDummyType(ns, "string"));

            if (name.EndsWith('*'))
            {
                var result = new GPointer(BuildDummyType(ns, name[..^1]));
                Types[(ns, name)] = result;
                return result;
            }

            int index = name.IndexOf('[');
            if (index != -1)
            {
                int index2 = name.IndexOf(']');
                var subString = name[(index + 1)..index2];
                var count = uint.Parse(subString);
                var result = new GArray(BuildDummyType(ns, name[..index] + name[(index2 + 1)..]), count);
                Types[(ns, name)] = result;
                return result;
            }

            return new GDummy(ns, name);
        }

        foreach (XmlNode enumDef in doc.SelectNodes("/PROGRAM/DATATYPES/ENUM")!)
        {
            var ns = StrAttrib(enumDef, "NAMESPACE") ?? "";
            var name = StrAttrib(enumDef, "NAME") ?? "";
            var size = UIntAttrib(enumDef, "SIZE");

            var elems = new Dictionary<uint, string>();
            foreach (var elem in enumDef.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "ENUM_ENTRY"))
                elems[UIntAttrib(elem, "VALUE")] = StrAttrib(elem, "NAME") ?? "";

            var ge = new GEnum(ns, name, size, elems);
            Types.Add((ns, name), ge);
        }

        foreach (XmlNode typeDef in doc.SelectNodes("/PROGRAM/DATATYPES/TYPE_DEF")!)
        {
            var ns = StrAttrib(typeDef, "NAMESPACE") ?? "";
            var name = StrAttrib(typeDef, "NAME") ?? "";
            var dtns = StrAttrib(typeDef, "DATATYPE_NAMESPACE") ?? "";
            var type = StrAttrib(typeDef, "DATATYPE") ?? "";
            var td = new GTypeAlias(ns, name, BuildDummyType(dtns, type));
            Types.Add((ns, name), td);
        }

        foreach (XmlNode funcDef in doc.SelectNodes("/PROGRAM/DATATYPES/FUNCTION_DEF")!)
        {
            var ns = StrAttrib(funcDef, "NAMESPACE") ?? "";
            var name = StrAttrib(funcDef, "NAME") ?? "";

            IGhidraType returnType = GPrimitive.Void;
            var returnTypeNode = funcDef.SelectSingleNode("RETURN_TYPE");
            if (returnTypeNode != null)
            {
                var dtns = StrAttrib(returnTypeNode, "DATATYPE_NAMESPACE") ?? "";
                var type = StrAttrib(returnTypeNode, "DATATYPE") ?? "";
                returnType = BuildDummyType(dtns, type);
            }

            List<GFuncParameter> parameters = new();
            foreach (var parameter in funcDef.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "PARAMETER"))
            {
                var ordinal = UIntAttrib(parameter, "ORDINAL");
                var paramName = StrAttrib(parameter, "NAME") ?? "";
                var size = UIntAttrib(parameter, "SIZE");
                var dtns = StrAttrib(parameter, "DATATYPE_NAMESPACE") ?? "";
                var type = StrAttrib(parameter, "DATATYPE") ?? "";
                parameters.Add(new GFuncParameter(ordinal, paramName, size, BuildDummyType(dtns, type)));
            }

            var td = new GFuncPointer(ns, name, returnType, parameters);
            Types.Add((ns, name), td);
        }

        foreach (XmlNode structDef in doc.SelectNodes("/PROGRAM/DATATYPES/STRUCTURE")!)
        {
            var ns = StrAttrib(structDef, "NAMESPACE") ?? "";
            var name = StrAttrib(structDef, "NAME") ?? "";
            var size = UIntAttrib(structDef, "SIZE");

            var members = new List<GStructMember>();
            foreach (var member in structDef.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "MEMBER"))
            {
                var type = BuildDummyType(StrAttrib(member, "DATATYPE_NAMESPACE") ?? "", StrAttrib(member, "DATATYPE") ?? "");
                var memberName = StrAttrib(member, "NAME");
                var offset = UIntAttrib(member, "OFFSET");

                if (string.IsNullOrEmpty(memberName))
                    memberName = $"unk{offset:X}";

                var commentNode = member.ChildNodes.OfType<XmlNode>().FirstOrDefault(x => x.Name == "REGULAR_CMT");
                members.Add(new GStructMember(memberName, type, offset, UIntAttrib(member, "SIZE"), commentNode?.InnerText));
            }
            Types.Add((ns, name), new GStruct(ns, name, size, members));
        }

        foreach (XmlNode unionDef in doc.SelectNodes("/PROGRAM/DATATYPES/UNION")!)
        {
            var ns = StrAttrib(unionDef, "NAMESPACE") ?? "";
            var name = StrAttrib(unionDef, "NAME") ?? "";
            var size = UIntAttrib(unionDef, "SIZE");

            var members = new List<GStructMember>();
            foreach (var member in unionDef.ChildNodes.OfType<XmlNode>().Where(x => x.Name == "MEMBER"))
            {
                var type = BuildDummyType(StrAttrib(member, "DATATYPE_NAMESPACE") ?? "", StrAttrib(member, "DATATYPE") ?? "");
                var memberName = StrAttrib(member, "NAME");
                var offset = UIntAttrib(member, "OFFSET");

                if (string.IsNullOrEmpty(memberName))
                    memberName = $"unk{offset:X}";

                members.Add(new GStructMember(memberName, type, offset, UIntAttrib(member, "SIZE"), StrAttrib(member, "COMMENT")));
            }


            Types.Add((ns, name), new GUnion(ns, name, size, members));
        }

        var dataBlocks = new Dictionary<uint, GGlobal>();
        foreach (XmlNode definedData in doc.SelectNodes("/PROGRAM/DATA/DEFINED_DATA")!)
        {
            var dt = StrAttrib(definedData, "DATATYPE") ?? "";
            var dtns = StrAttrib(definedData, "DATATYPE_NAMESPACE") ?? "";
            var addr = HexAttrib(definedData, "ADDRESS");
            var size = UIntAttrib(definedData, "SIZE");
            dataBlocks[addr] = new GGlobal(addr, size, BuildDummyType(dtns, dt));
        }

        var symbols = new Dictionary<uint, string>();

        foreach (XmlNode fun in doc.SelectNodes("/PROGRAM/FUNCTIONS/FUNCTION")!)
        {
            var addr = HexAttrib(fun, "ENTRY_POINT");
            var name = StrAttrib(fun, "NAME") ?? "";
            symbols[addr] = name;
        }

        Dictionary<(string, string), GGlobal> globalVariables = new();
        foreach (XmlNode sym in doc.SelectNodes("/PROGRAM/SYMBOL_TABLE/SYMBOL")!)
        {
            var addr = HexAttrib(sym, "ADDRESS");
            var name = StrAttrib(sym, "NAME") ?? "";
            var ns = StrAttrib(sym, "NAMESPACE") ?? "";
            if (dataBlocks.TryGetValue(addr, out var data))
            {
                data.Namespace = ns;
                data.Name = name;
                globalVariables[(ns, name)] = data;
            }

            if (!name.StartsWith("case"))
                symbols[addr] = name;
        }

        var parser = new DirectiveParser(BuildDummyType);
        foreach (var key in Types.Keys.ToList())
        {
            var type = Types[key];
            if (type is not GStruct structType) continue;
            foreach (var member in structType.Members)
            {
                if (member.Comment == null) continue;
                member.Directives = parser.TryParse(member.Comment, member.Name).ToList();
                if (member.Directives.Count == 0)
                    member.Directives = null;
            }
        }

        foreach (var key in Types.Keys.ToList())
        {
            var type = Types[key];
            type.Unswizzle(Types);
        }

        foreach (var kvp in globalVariables)
            kvp.Value.Unswizzle(Types);

        _symbols = symbols.Select(x => (x.Key, x.Value)).OrderBy(x => x.Key).ToArray();
        var namespaces = new Dictionary<string, GNamespace>();
        Root = new GNamespace(Constants.RootNamespaceName);
        namespaces[""] = Root;

        GNamespace GetOrAddNamespace(string ns)
        {
            if (namespaces.TryGetValue(ns, out var result))
                return result;

            var parts = ns.Split('/');
            var cur = Root;
            foreach (var part in parts)
                cur = Root.GetOrAddNamespace(part);

            namespaces[ns] = cur;
            return cur;
        }

        foreach (var kvp in globalVariables)
        {
            var ns = GetOrAddNamespace(kvp.Key.Item1);
            ns.Members.Add(kvp.Value);
        }

        Root.Sort();
    }

    static string? StrAttrib(XmlNode node, string name) => node.Attributes![name]?.Value;
    static uint UIntAttrib(XmlNode node, string name)
    {
        var value = node.Attributes![name]?.Value ?? throw new InvalidOperationException($"Attribute {name} was not found on node {node}!");
        return (value.StartsWith("0x"))
            ? uint.Parse(value[2..], NumberStyles.HexNumber)
            : uint.Parse(value);
    }

    static uint HexAttrib(XmlNode node, string name)
    {
        var value = node.Attributes![name]?.Value ?? throw new InvalidOperationException($"Attribute {name} was not found on node {node}!");
        return uint.Parse(value, NumberStyles.HexNumber);
    }
}
