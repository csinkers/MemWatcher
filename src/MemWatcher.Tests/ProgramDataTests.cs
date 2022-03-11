using System.IO;
using System.Text;
using Xunit;

namespace MemWatcher.Tests;

public class ProgramDataTests
{
    const string SampleXml = @"<?xml version=""1.0"" standalone=""yes""?>
<PROGRAM NAME=""SR-Main.exe"" EXE_PATH=""/C:/Depot/bb/ualbion/mods/Repacked/Albion/SR-Main.exe"" EXE_FORMAT=""Portable Executable (PE)"" IMAGE_BASE=""00400000"">
    <INFO_SOURCE USER=""csink"" TOOL=""Ghidra 10.1.1"" TIMESTAMP=""Sun Feb 13 01:00:36 EST 2022"" />
    <PROCESSOR NAME=""x86"" LANGUAGE_PROVIDER=""x86:LE:32:default:watcom"" ENDIAN=""little"" />
    <DATATYPES>
        <STRUCTURE NAME=""AKeyEvent"" NAMESPACE=""/sr-main.enums"" SIZE=""0x14"">
            <MEMBER OFFSET=""0x0"" DATATYPE=""int"" DATATYPE_NAMESPACE=""/"" NAME=""unk0"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x4"" DATATYPE=""int"" DATATYPE_NAMESPACE=""/"" NAME=""unk4"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x8"" DATATYPE=""int"" DATATYPE_NAMESPACE=""/"" NAME=""unk8"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0xc"" DATATYPE=""int"" DATATYPE_NAMESPACE=""/"" NAME=""unkC"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x10"" DATATYPE=""int"" DATATYPE_NAMESPACE=""/"" NAME=""unk10"" SIZE=""0x4"" />
        </STRUCTURE>
        <ENUM NAME=""AsyncFlags"" NAMESPACE=""/sr-main.enums"" SIZE=""0x2"">
            <ENUM_ENTRY NAME=""None"" VALUE=""0x0"" COMMENT="""" />
            <ENUM_ENTRY NAME=""Unk1"" VALUE=""0x1"" COMMENT="""" />
        </ENUM>
        <UNION NAME=""Misc"" NAMESPACE=""/PE"" SIZE=""0x4"">
            <MEMBER OFFSET=""0x0"" DATATYPE=""dword"" DATATYPE_NAMESPACE=""/"" NAME=""PhysicalAddress"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x0"" DATATYPE=""dword"" DATATYPE_NAMESPACE=""/"" NAME=""VirtualSize"" SIZE=""0x4"" />
        </UNION>
        <TYPE_DEF NAME=""__time64_t"" NAMESPACE=""/crtdefs.h"" DATATYPE=""longlong"" DATATYPE_NAMESPACE=""/"" />
        <FUNCTION_DEF NAME=""asyncMethod"" NAMESPACE=""/sr-main.fpn"">
            <REGULAR_CMT>Function Signature Data Type</REGULAR_CMT>
            <RETURN_TYPE DATATYPE=""void"" DATATYPE_NAMESPACE=""/"" SIZE=""0x0"" />
            <PARAMETER ORDINAL=""0x0"" DATATYPE=""async_related_t *"" DATATYPE_NAMESPACE=""/sr-main.structs.h"" NAME=""pAsync"" SIZE=""0x4"" />
        </FUNCTION_DEF>
        <STRUCTURE NAME=""async_related_t"" NAMESPACE=""/sr-main.structs.h"" SIZE=""0x1e"">
            <MEMBER OFFSET=""0x0"" DATATYPE=""AsyncFlags"" DATATYPE_NAMESPACE=""/sr-main.enums"" NAME=""flags"" SIZE=""0x2"" />
            <MEMBER OFFSET=""0x2"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x3"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x4"" DATATYPE=""word"" DATATYPE_NAMESPACE=""/"" NAME=""resultQ"" SIZE=""0x2"" />
            <MEMBER OFFSET=""0x6"" DATATYPE=""asyncMethod *"" DATATYPE_NAMESPACE=""/sr-main.fpn"" NAME=""callback"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0xa"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0xb"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0xc"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0xd"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0xe"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0xf"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x10"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x11"" DATATYPE=""undefined"" DATATYPE_NAMESPACE=""/"" SIZE=""0x1"" />
            <MEMBER OFFSET=""0x12"" DATATYPE=""asyncMethod *"" DATATYPE_NAMESPACE=""/sr-main.fpn"" NAME=""callback2"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x16"" DATATYPE=""asyncMethod *"" DATATYPE_NAMESPACE=""/sr-main.fpn"" NAME=""callback3"" SIZE=""0x4"" />
            <MEMBER OFFSET=""0x1a"" DATATYPE=""asyncMethod *"" DATATYPE_NAMESPACE=""/sr-main.fpn"" NAME=""callback4"" SIZE=""0x4"" />
        </STRUCTURE>
        <STRUCTURE NAME=""npc_t"" NAMESPACE=""/sr-main.structs.h"" SIZE=""0x80"">
            <MEMBER OFFSET=""0x0"" DATATYPE=""word[64]"" DATATYPE_NAMESPACE=""/"" NAME=""misc"" SIZE=""0x80"" />
        </STRUCTURE>
    </DATATYPES>
    <MEMORY_MAP>
        <MEMORY_SECTION NAME=""Headers"" START_ADDR=""00400000"" LENGTH=""0x400"" PERMISSIONS=""r"" COMMENT="""">
            <MEMORY_CONTENTS FILE_NAME=""SR-Main.exe.bytes"" FILE_OFFSET=""0x0"" />
        </MEMORY_SECTION>
    </MEMORY_MAP>
    <REGISTER_VALUES>
        <REGISTER_VALUE_RANGE REGISTER=""addrsize"" VALUE=""0x1"" START_ADDRESS=""004a94e0"" LENGTH=""0x1"" />
    </REGISTER_VALUES>
    <CODE>
        <CODE_BLOCK START=""00401000"" END=""00401001"" />
    </CODE>
    <DATA>
        <DEFINED_DATA ADDRESS=""0043f82c"" DATATYPE=""char[14][20]"" DATATYPE_NAMESPACE=""/"" SIZE=""0x118"" />
        <DEFINED_DATA ADDRESS=""00526c58"" DATATYPE=""npc_t[96]"" DATATYPE_NAMESPACE=""/sr-main.structs.h"" SIZE=""0x3000"" />
        <DEFINED_DATA ADDRESS=""00528c58"" DATATYPE=""npc_t*[32]"" DATATYPE_NAMESPACE=""/sr-main.structs.h"" SIZE=""0x80"" />
        <DEFINED_DATA ADDRESS=""0054b160"" DATATYPE=""pointer"" DATATYPE_NAMESPACE=""/"" SIZE=""0x4"" />
        <DEFINED_DATA ADDRESS=""00579024"" DATATYPE=""string"" DATATYPE_NAMESPACE=""/"" SIZE=""0x2d"" />
        <DEFINED_DATA ADDRESS=""0061a0d4"" DATATYPE=""dword"" DATATYPE_NAMESPACE=""/"" SIZE=""0x4"" />
        <DEFINED_DATA ADDRESS=""0061ac34"" DATATYPE=""TerminatedCString"" DATATYPE_NAMESPACE=""/"" SIZE=""0x7"" />
    </DATA>
    <EQUATES>
        <EQUATE_GROUP>
            <EQUATE NAME=""&apos;9&apos;"" VALUE=""0x39"" />
            <EQUATE NAME=""SimpleMsg"" VALUE=""0x10"" />
        </EQUATE_GROUP>
    </EQUATES>
    <COMMENTS>
        <COMMENT ADDRESS=""00442798"" TYPE=""pre"">Handle offset events</COMMENT>
    </COMMENTS>
    <PROPERTIES>
        <PROPERTY NAME=""Analyzers.Non-Returning Functions - Discovered"" TYPE=""bool"" VALUE=""n"" />
    </PROPERTIES>
    <BOOKMARKS>
        <BOOKMARK ADDRESS=""00401010"" TYPE=""Analysis"" CATEGORY=""Aggressive Intruction Finder"" DESCRIPTION=""Found code"" />
    </BOOKMARKS>
    <PROGRAM_TREES>
        <TREE NAME=""Program Tree"">
            <FRAGMENT NAME=""Headers"">
                <ADDRESS_RANGE START=""00400000"" END=""004003ff"" />
            </FRAGMENT>
        </TREE>
    </PROGRAM_TREES>
    <PROGRAM_ENTRY_POINTS>
        <PROGRAM_ENTRY_POINT ADDRESS=""004014e0"" />
    </PROGRAM_ENTRY_POINTS>
    <RELOCATION_TABLE />
    <SYMBOL_TABLE>
        <SYMBOL ADDRESS=""0044eb75"" NAME=""Handle_Query"" NAMESPACE="""" TYPE=""global"" SOURCE_TYPE=""USER_DEFINED"" PRIMARY=""y"" />
        <SYMBOL ADDRESS=""00526c58"" NAME=""g_Npcs"" NAMESPACE="""" TYPE=""global"" SOURCE_TYPE=""USER_DEFINED"" PRIMARY=""y"" />
    </SYMBOL_TABLE>
    <FUNCTIONS>
        <FUNCTION ENTRY_POINT=""0044eb75"" NAME=""Handle_Query"" LIBRARY_FUNCTION=""n"">
            <RETURN_TYPE DATATYPE=""void"" DATATYPE_NAMESPACE=""/"" SIZE=""0x0"" />
            <ADDRESS_RANGE START=""0044eb75"" END=""0044eb94"" />
            <ADDRESS_RANGE START=""0044ec48"" END=""0044fe3c"" />
            <TYPEINFO_CMT>void __cdecl Handle_Query(void)</TYPEINFO_CMT>
            <STACK_FRAME LOCAL_VAR_SIZE=""0x10c"" PARAM_OFFSET=""0x4"" RETURN_ADDR_SIZE=""0x0"" BYTES_PURGED=""0"">
                <STACK_VAR STACK_PTR_OFFSET=""-0x44"" NAME=""partyMemberIndex"" DATATYPE=""ushort"" DATATYPE_NAMESPACE=""/"" SIZE=""0x2"" />
                <STACK_VAR STACK_PTR_OFFSET=""-0x30"" NAME=""queryType"" DATATYPE=""QueryType"" DATATYPE_NAMESPACE=""/sr-main.enums"" SIZE=""0x1"" />
                <STACK_VAR STACK_PTR_OFFSET=""-0x2c"" NAME=""pEvent"" DATATYPE=""event_t *"" DATATYPE_NAMESPACE=""/sr-main.structs.h"" SIZE=""0x4"" />
                <STACK_VAR STACK_PTR_OFFSET=""-0x28"" NAME=""result"" DATATYPE=""bool4"" DATATYPE_NAMESPACE=""/sr-main.enums"" SIZE=""0x4"" />
            </STACK_FRAME>
        </FUNCTION>
    </FUNCTIONS>
    <MARKUP>
        <MEMORY_REFERENCE ADDRESS=""0041b6f8"" OPERAND_INDEX=""0x0"" USER_DEFINED=""y"" TO_ADDRESS=""0041b6fe"" PRIMARY=""n"" />
        <EQUATE_REFERENCE ADDRESS=""0048389d"" OPERAND_INDEX=""0xffff"" NAME=""&apos;/&apos;"" VALUE=""0x2f"" />
    </MARKUP>
    <EXT_LIBRARY_TABLE>
        <EXT_LIBRARY NAME=""KERNEL32.DLL"" PATH="""" />
    </EXT_LIBRARY_TABLE>
</PROGRAM>
";

    [Fact]
    public void XmlParseTest()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SampleXml));
        var programData = new ProgramData(stream);
        Assert.NotNull(programData);
    }
}


public class TestProgram
{
    /*

    
40000: 



     */

}