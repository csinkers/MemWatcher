namespace GhidraData;

public record DTargetChild(string Path, IDirective Directive) : IDirective
{
    public bool Unswizzle(TypeStore types) => false;
}