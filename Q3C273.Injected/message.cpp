#include "pch.h"
#include "message.h"

void ProcessMessage(HANDLE pipeHandle, PMESSAGE_INFO_BASE info)
{
    switch (info->Identifier)
    {
        case MESSAGE_READ_FILE:
    }
}