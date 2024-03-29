﻿namespace DebuggerInterfaces;

public record SymbolInfo(uint Address, string Name, SymbolType SymbolType, object? Context);