#pragma once
#include "pch.h"
#include <vector>

typedef struct _tagMESSAGE_BASE
{
    DWORD Identifier;
};

typedef struct _tagMESSAGE_READ_FILE
{
    DWORD Identifier;
};



void ProcessMessage(HANDLE pipeHandle, std::vector<BYTE> &msg);