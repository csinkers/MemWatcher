#include <iostream>
#include "TestMap.h"

typedef uint8_t byte;
typedef uint16_t ushort;
typedef uint32_t uint;

enum AreaFlags : byte
{
	AF_Allocated = 1,
	AF_Persistent = 2,
	AF_Unk4 = 4,
	AF_Unk8 = 8
};

struct alloc_t;

struct alloc_type_t
{
	uint(*xldLoader)(void*, uint, uint, uint);
	byte unk4;
	byte unk5;
	byte flags;
	const char* description;
};

struct workspace_area_t
{
	uint unk0;
	uint sizeInBytes;
	uint unk8;
	workspace_area_t* pNext;
	alloc_t* allocation;
};

struct alloc_node_t
{
	void* pData;
	uint memType;
	alloc_node_t* pNext;
	workspace_area_t* pArea;
	ushort workspaceNumber;
};

struct alloc_t
{
	AreaFlags flags;
	byte handle;
	byte lockCount;
	byte length;
	byte memType;
	uint size;
	alloc_type_t* desc;
	alloc_node_t* pHead;
};

typedef alloc_t* (*AllocListConstructor)(uint p1);
struct workspace_t
{
	ushort unk0;
	ushort areaCount;
	uint unk4;
	uint unk8;
	void (*pfnPreAlloc)();
	void (*pfnPostAlloc)();
	void (*pfnFailedAlloc)();
	AllocListConstructor* constructors;
	byte areasBuffer[215];
};

enum MapType : byte
{
	Map2D = 0,
	Map3D
};

enum MapSubMode : byte
{
	MSM_Indoors = 0,
	MSM_Dungeon,
	MSM_Outdoors,
};

enum MapRestMode : byte
{
	MRM_Wait = 0,
	MRM_RestEightHours,
	MRM_RestUntilDawn,
	MRM_NoResting
};

enum Direction : byte
{
	D_North = 0,
	D_East,
	D_South,
	D_West,
	D_Unchanged = 0xff
};

struct map_unk8_t
{
	ushort x;
	ushort y;
	Direction dir;
	ushort unk6;
};

enum MapFlags : ushort
{
	MF_None = 0x0,
	MF_SubType1_Bit1 = 0x1,
	MF_SubType1_Bit2 = 0x2,
	MF_SubType2_Bit1 = 0x4,
	MF_SubType2_Bit2 = 0x8,
	MF_UseSecondaryAutomapTileset = 0x10,
	MF_Unk20 = 0x20,
	MF_Unk40 = 0x40,
	MF_Unk80 = 0x80,
	MF_Unk100 = 0x100,
	MF_Unk200 = 0x200,
	MF_Unk400 = 0x400,
	MF_Unk800 = 0x800,
	MF_Unk1000 = 0x1000,
	MF_Version2Npcs = 0x2000,
	MF_FullNpcCount = 0x4000,
	MF_Unk8000 = 0x8000
};

enum NpcLoadFlags : byte
{
	NLF_None = 0x0,
	NLF_TypeBit1 = 0x1,
	NLF_TypeBit2 = 0x2,
	NLF_TypeBit4_MoveABit1 = 0x4,
	NLF_MoveABit2 = 0x8,
	NLF_SimpleMsg = 0x10,
	NLF_Unk5 = 0x20,
	NLF_NoClip = 0x40,
	NLF_Unused7 = 0x80
};

enum NpcLoadMovement : byte
{
    NLM_Waypoints = 0x0,
    NLM_RandomWander = 0x1,
    NLM_Stationary = 0x2,
    NLM_ChaseParty = 0x3,
    NLM_Waypoints2 = 0x4,
    NLM_Stationary2 = 0x5, // Stationary
    NLM_Stationary3 = 0x6, // Stationary
    NLM_Unk7 = 0x7, // Stationary
    NLM_Unk8 = 0x8,
    NLM_Unk9 = 0x9,
    NLM_UnkA = 0xA,
    NLM_UnkB = 0xB,
    NLM_UnkC = 0xC,
    NLM_UnkD = 0xD,
    NLM_UnkE = 0xE,
    NLM_UnkF = 0xF,
};

enum TriggerTypes : ushort
{
	TT_None = 0x0,
	TT_Normal = 0x1,
	TT_Examine = 0x2,
	TT_Manipulate = 0x4,
	TT_TalkTo = 0x8,
	TT_UseItem = 0x10,
	TT_MapInit = 0x20,
	TT_EveryStep = 0x40,
	TT_EveryHour = 0x80,
	TT_EveryDay = 0x100,
	TT_Default = 0x200,
	TT_Action = 0x400,
	TT_Npc = 0x800,
	TT_Take = 0x1000,
	TT_Unk2000 = 0x2000,
	TT_Unk4000 = 0x4000,
	TT_Unk8000 = 0x8000
};

struct npc_load_t
{
	byte id;
	byte sound;
	ushort eventNumber;
	ushort sprite;
	NpcLoadFlags flags1;
	NpcLoadMovement movement;
	TriggerTypes triggers;
};

struct zone_t
{
	ushort x;
	TriggerTypes triggers;
	ushort eventIndex;
};

struct zone_row_t
{
	ushort zoneCount;
	zone_t rows[1];
};

struct waypoint_t
{
	byte x;
	byte y;
};

enum EventType : byte
{
	None = 0x0,
	MapExit = 0x1,
	Door = 0x2,
	Chest = 0x3,
	Text = 0x4,
	Spinner = 0x5,
	Trap = 0x6,
	ChangeUsedItem = 0x7,
	DataChange = 0x8,
	ChangeIcon = 0x9,
	Encounter = 0xa,
	PlaceAction = 0xb,
	Query = 0xc,
	Modify = 0xd,
	Action = 0xe,
	Signal = 0xf,
	CloneAutomap = 0x10,
	Sound = 0x11,
	StartDialogue = 0x12,
	CreateTransport = 0x13,
	Execute = 0x14,
	RemovePartyMember = 0x15,
	EndDialogue = 0x16,
	Wipe = 0x17,
	PlayAnimation = 0x18,
	Offset = 0x19,
	Pause = 0x1a,
	SimpleChest = 0x1b,
	AskSurrender = 0x1c,
	Script = 0x1d
};

struct event_t
{
	EventType type;
	byte byte1;
	byte byte2;
	byte byte3;
	byte byte4;
	byte byte5;
	ushort word6;
	ushort word8;
	ushort nextIndex;
};

struct map_load_t
{
	MapFlags flags;
	MapType mapType;
	byte songId;
	byte width;
	byte height;
	byte tilesetId;
	byte combatBgId;
	byte paletteId;
	byte frameRate;
	// npc data
	// tile data (3 bytes per tile)
	// zones
	// events
	// waypoints
	// chains
};

struct map_t
{
	uint automapInfoOffset;
	uint waypointsOffset;
	uint eventsOffset;
	uint zoneDataOffset;
	uint tileDataOffset;
	uint tileCount;
	alloc_t* aMapText;
	alloc_t* aMapData;
	void (*unkCallback)();
	ushort frameRate;
	ushort unk26;
	MapSubMode subMode;
	ushort eventCountOffset;
	MapRestMode restMode;
	ushort automapInfoCount;
	ushort height;
	ushort paletteId;
	ushort songId;
	ushort width;
	MapType mapTYpe;
	ushort automapTilesetId;
	alloc_t* aBlockList;
	alloc_t* aTilesetGfx;
	alloc_t* aTilesetData;
	ushort unk48;
	ushort unk4a;
	ushort useBigGraphics;
	ushort unk4e;
	ushort unk50;
	ushort unk52;
	map_unk8_t positionHistory[256];
};

map_t g_Map;
workspace_t g_Workspaces[2];
workspace_area_t g_Areas[750];
alloc_t g_Allocs[256];
alloc_node_t g_Nodes[1024];
alloc_t* g_Handles[];

uint MapLoader(void* pData, uint, uint, uint)
{
	return 0;
}

alloc_type_t c_map_type = { MapLoader, 0, 0, 0, "MapData" };

void ALLOC_InitSystem()
{
	memset(g_Workspaces, 0, sizeof(g_Workspaces));
	memset(g_Areas, 0, sizeof(g_Areas));
	memset(g_Allocs, 0, sizeof(g_Allocs));
	memset(g_Nodes, 0, sizeof(g_Nodes));
}

alloc_node_t* ALLOC_NewNode()
{
	for (int i = 0; i < _countof(g_Nodes); i++)
	{
		auto& pNode = g_Nodes[i];
		if (pNode.pArea != nullptr)
			continue;

		pNode.pArea = &g_Areas[0];
		pNode.memType = 1;
		pNode.pData = nullptr;
		pNode.pNext = nullptr;
		pNode.workspaceNumber = 0;
		return &pNode;
	}

	return nullptr;
}

alloc_t* ALLOC_New(void* data, size_t size, alloc_type_t* type)
{
	for (int i = 0; i < _countof(g_Allocs); i++)
	{
		alloc_t& alloc = g_Allocs[i];
		if (alloc.flags != 0)
			continue;

		alloc.flags = AF_Allocated;
		alloc.size = size;
		alloc.length = 1;
		alloc.desc = type;
		alloc.pHead = ALLOC_NewNode();
		alloc.pHead->pData = data;
		return &alloc;
	}

	return nullptr;
}

void* ALLOC_Lock(alloc_t* pAlloc)
{

	if (pAlloc == (alloc_t*)0x0)
		return nullptr;

	if (pAlloc->lockCount != 0xff)
		pAlloc->lockCount++;

	alloc_node_t* pNode = pAlloc->pHead;
	if (pNode == (alloc_node_t*)0x0)
		return nullptr;

	return pNode->pData;
}

void ALLOC_Unlock(alloc_t* pAlloc)
{
	if (pAlloc != (alloc_t*)0x0 && pAlloc->lockCount != 0)
		pAlloc->lockCount--;
	return;
}

bool NPC_HasWaypoints(const npc_load_t &npc)
{
	return npc.movement == NLM_Waypoints || npc.movement == NLM_Waypoints2;
}

const char* EVENT_TypeName(EventType type)
{
	switch (type)
	{
	case None:              return "None             ";
	case MapExit:           return "MapExit          ";
	case Door:              return "Door             ";
	case Chest:             return "Chest            ";
	case Text:              return "Text             ";
	case Spinner:           return "Spinner          ";
	case Trap:              return "Trap             ";
	case ChangeUsedItem:    return "ChangeUsedItem   ";
	case DataChange:        return "DataChange       ";
	case ChangeIcon:        return "ChangeIcon       ";
	case Encounter:         return "Encounter        ";
	case PlaceAction:       return "PlaceAction      ";
	case Query:             return "Query            ";
	case Modify:            return "Modify           ";
	case Action:            return "Action           ";
	case Signal:            return "Signal           ";
	case CloneAutomap:      return "CloneAutomap     ";
	case Sound:             return "Sound            ";
	case StartDialogue:     return "StartDialogue    ";
	case CreateTransport:   return "CreateTransport  ";
	case Execute:           return "Execute          ";
	case RemovePartyMember: return "RemovePartyMember";
	case EndDialogue:       return "EndDialogue      ";
	case Wipe:              return "Wipe             ";
	case PlayAnimation:     return "PlayAnimation    ";
	case Offset:            return "Offset           ";
	case Pause:             return "Pause            ";
	case SimpleChest:       return "SimpleChest      ";
	case AskSurrender:      return "AskSurrender     ";
	case Script:            return "Script           ";
	default: return "Unk";
	}
}

void print_map_info()
{
	const map_load_t& map = *(map_load_t*)ALLOC_Lock(g_Map.aMapData);
	printf("T:%d W:%d H:%d\nCombatBg:%d Pal:%d Song:%d\nFlags:%x FrameRate:%d Tileset:%d\n\n",
		map.mapType,
		map.width,
		map.height,
		map.combatBgId,
		map.paletteId,
		map.songId,
		map.flags,
		map.frameRate,
		map.tilesetId
	);

	// npc data
	const int npc_count = (map.flags & MF_FullNpcCount) != 0 ? 96 : 32;
	const auto npcs = (npc_load_t*)(&map + 1);

	for(int i = 0; i < npc_count; i++)
	{
		const auto& npc = npcs[i];
		if (npc.id == 0)
			continue;
		printf("    NPC%d:\t%d\tF:%x\tM:%d\tS:%d\tE:%d\tT:%x\tSound:%d\n",
			i,
			(int)npc.id,
			(int)npc.flags1,
			(int)npc.movement,
			(int)npc.sprite,
			(int)npc.eventNumber,
			(int)npc.triggers,
			(int)npc.sound);
	}
	printf("\n");

	// tile data (3 bytes per tile)
	g_Map.tileCount = map.width * map.height;
	byte* tiles = (byte*)(npcs + npc_count);
	g_Map.tileDataOffset = (uint)((const byte*)tiles - (const byte*)&map);

	// zones
	auto row = (zone_row_t*)(tiles + 3 * g_Map.tileCount);
	const auto rows = (zone_row_t**)malloc(sizeof(zone_row_t*) * map.height);
	if (rows == nullptr)
		return;

	g_Map.zoneDataOffset = (uint)((const byte*)row - (const byte*)&map);
	auto global_zones = row;
	row = (zone_row_t*)(&row->rows[0] + row->zoneCount);

	for (int i = 0; i < map.height; i++)
	{
		rows[i] = row;
		row = (zone_row_t*)(&row->rows[0] + row->zoneCount);
	}

	// events
	const ushort event_count = *(ushort*)row;
	const auto events = (event_t*)((ushort*)row + 1);
	g_Map.eventsOffset = (uint)((const byte*)events - (const byte*)&map);

	for (int i = 0; i < event_count; i++)
	{
		const auto& e = events[i];
		printf("    E%04d:\t%s\t1:%d\t2:%d\t3:%d\t4:%d\t5:%d\t6:%d\t8:%d\tNext:%d\n",
			i, EVENT_TypeName(e.type),
			(int)e.byte1,
			(int)e.byte2,
			(int)e.byte3,
			(int)e.byte4,
			(int)e.byte5,
			(int)e.word6,
			(int)e.word8,
			(int)e.nextIndex);
	}
	printf("\n");

	// waypoints
	const auto waypoint_counts = (ushort*)malloc(sizeof(ushort) * npc_count);
	if (waypoint_counts == nullptr)
		return;

	const auto waypoints = (waypoint_t**)malloc(sizeof(waypoint_t*) * npc_count);
	if (waypoints == nullptr)
		return;

	auto npc_waypoints = (waypoint_t*)(events + event_count);
	g_Map.waypointsOffset = (uint)((const byte*)npc_waypoints - (const byte*)&map);

	for(int i = 0; i < npc_count; i++)
	{
		const auto &npc = npcs[i];
		if (npc.id == 0)
		{
			waypoint_counts[i] = 0;
			waypoints[i] = nullptr;
			continue;
		}

		waypoint_counts[i] = NPC_HasWaypoints(npc) ? 0x480 : 1;
		waypoints[i] = npc_waypoints;
		npc_waypoints = npc_waypoints + waypoint_counts[i];
	}

	const byte *automap = (const byte*)npc_waypoints;
	g_Map.automapInfoOffset = (uint)(automap - (const byte*)&map);
	if (map.mapType == Map3D)
		automap = automap + map.width * map.height;

	// chains
	const int chain_count = map.mapType == Map2D ? 250 : 64;
	const auto chains = (ushort*)automap;
	for(int i = 0; i < chain_count; i++)
		printf("     C%d:%d\n", i, (int)chains[i]);

	ALLOC_Unlock(g_Map.aMapData);
}

int main()
{
	ALLOC_InitSystem();
	g_Map.aMapData = ALLOC_New((void*)test_map, sizeof(test_map), &c_map_type);
	print_map_info();
	char buf[128];
	gets_s(buf);
}

