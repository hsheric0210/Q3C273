#include "pch.h"
#include "main.h"

BOOL pipeClientThreadStarted = FALSE;
LPWSTR pipeAddress;

DllExport void NotifyLoad(LPWSTR message)
{
    MessageBoxW(nullptr, L"TESTING TESTING TESTING\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nabcdefghijklmnopqrstruvwxyz\n0123456789\n가나다라마바사아자차카타파하\n일이삼사오육칠팔구십", message, MB_ICONWARNING);
}

DWORD PipeClientThread(PPIPE_CLIENT_THREAD_PARAM param)
{
    BOOL state;
    auto buffer = (LPBYTE)HeapAlloc(GetProcessHeap(), 0, 8192);
    if (!buffer)
        return 1;

    std::vector<BYTE> data;
    do
    {
        DWORD written;
        state = ReadFile(
            param->pipeHandle,
            buffer,
            8192,
            &written,
            nullptr);

        if (!state && GetLastError() != ERROR_MORE_DATA)
            break;

        for (int i = 0; i < written; i++)
            data.push_back(buffer[i]);

        ProcessMessage(param->pipeHandle, data);

        // All message received
        if (!GetLastError())
            data.clear();
    } while (!state);

    HeapFree(GetProcessHeap(), 0, buffer);
}

DllExport DWORD Connect(PCONNECT_PARAM param)
{
    if (pipeClientThreadStarted)
        return 999;

    BOOL state;

    auto pipeHandle = CreateFileW(
        param->pipeAddress,
        GENERIC_READ | GENERIC_WRITE,
        0,
        nullptr,
        OPEN_EXISTING,
        0,
        nullptr);

    if (pipeHandle == INVALID_HANDLE_VALUE)
    {
        DEREF_32(param->errorCode) = GetLastError();
        return 1;
    }

    DWORD pipeMode = PIPE_READMODE_MESSAGE;
    state = SetNamedPipeHandleState(
        pipeHandle,
        &pipeMode,
        nullptr,
        nullptr);
    if (!state)
    {
        DEREF_32(param->errorCode) = GetLastError();
        return 2;
    }

    auto handshakeLen = (DWORD)sizeof(CONNECT_HANDSHAKE);
    auto handshakeMsgBuf = (PCONNECT_HANDSHAKE)HeapAlloc(GetProcessHeap(), 0, handshakeLen);
    if (!handshakeMsgBuf)
    {
        DEREF_32(param->errorCode) = GetLastError();
        return 3;
    }

    handshakeMsgBuf->version = DLL_VERSION;
    CopyMemory(handshakeMsgBuf->identifier, param->identifier, 64);

    DWORD written;
    state = WriteFile(
        pipeHandle,
        handshakeMsgBuf,
        handshakeLen,
        &written,
        nullptr);
    if (!state)
    {
        DEREF_32(param->errorCode) = GetLastError();
        return 4;
    }

    HeapFree(GetProcessHeap(), 0, handshakeMsgBuf);
    pipeAddress = param->pipeAddress;

    auto pipeClientParam = (PPIPE_CLIENT_THREAD_PARAM)HeapAlloc(GetProcessHeap(), 0, sizeof(PIPE_CLIENT_THREAD_PARAM));
    if (!pipeClientParam)
    {
        DEREF_32(param->errorCode) = GetLastError();
        return 5;
    }

    pipeClientParam->pipeHandle = pipeHandle;

    HANDLE threadHandle = CreateThread(
        nullptr,
        0,
        (LPTHREAD_START_ROUTINE)PipeClientThread,
        pipeClientParam,
        0,
        nullptr);
    if (!threadHandle)
    {
        DEREF_32(param->errorCode) = GetLastError();
        return 6;
    }

    return 0;
}
