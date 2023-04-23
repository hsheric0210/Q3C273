#pragma once
#include "pch.h"
#include <vector>
#include "message.h"
#define DllExport extern "C" __declspec(dllexport)

#define DLL_VERSION 1

#define DEREF_PTR(x) *(UINT_PTR *)(x)
#define DEREF_64(x) *(DWORD64 *)(x)
#define DEREF_32(x) *(DWORD *)(x)
#define DEREF_16(x) *(WORD *)(x)
#define DEREF_8(x) *(BYTE *)(x)

typedef struct _tagCONNECT_PARAM
{
    WCHAR pipeAddress[256];
    WCHAR identifier[64];
    UINT_PTR errorCode;
} CONNECT_PARAM, *PCONNECT_PARAM;

typedef struct _tagCONNECT_HANDSHAKE
{
    ULONGLONG version;
    WCHAR identifier[64];
} CONNECT_HANDSHAKE, *PCONNECT_HANDSHAKE;

typedef struct _tagPIPE_CLIENT_THREAD_PARAM
{
    HANDLE pipeHandle;
} PIPE_CLIENT_THREAD_PARAM, *PPIPE_CLIENT_THREAD_PARAM;

DllExport void NotifyLoad(LPWSTR message);
