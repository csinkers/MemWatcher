namespace MemWatcher.Types;

public class StructHistory : History
{
    public StructHistory((GStructMember, string?, History)[] memberHistories) => MemberHistories = memberHistories ?? throw new ArgumentNullException(nameof(memberHistories));
    public (GStructMember, string?, History)[] MemberHistories { get; }
}