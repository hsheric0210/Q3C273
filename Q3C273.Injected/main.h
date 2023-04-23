#pragma once
#include "pch.h"
#define DllExport extern "C" __declspec(dllexport)

DllExport void NotifyLoad(LPWSTR message);
