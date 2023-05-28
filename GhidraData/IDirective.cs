namespace GhidraData;

public interface IDirective
{
    bool Unswizzle(TypeStore types); // Return true if any types were resolved
}