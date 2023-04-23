#pragma once
#include "pch.h"
#include <vector>

enum MessageType
{
    MESSAGE_READ_FILE = 0x45802566390,
    MESSAGE_WRITE_FILE = 0x6894352734,
    MESSAGE_DELETE_FILE = 0x628952771,
    MESSAGE_SHUTDOWN_INSTANCE = 0x690,
};

typedef struct _tagMESSAGE_INFO_BASE
{
    DWORD Identifier;
} MESSAGE_INFO_BASE, *PMESSAGE_INFO_BASE;

typedef struct _tagMESSAGE_READ_FILE_INFO
{
    DWORD Identifier;
    LPWSTR FilePath;
} MESSAGE_READ_FILE_INFO, *PMESSAGE_READ_FILE_INFO;

typedef struct _tagMESSAGE_READ_FILE_RESPONSE
{
    DWORD Identifier;
    LPVOID DataPtr;
} MESSAGE_READ_FILE_RESPONSE, *PMESSAGE_READ_FILE_RESPONSE;

void ProcessMessage(HANDLE pipeHandle, PMESSAGE_INFO_BASE info);