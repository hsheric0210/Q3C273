#pragma once
#include "pch.h"
#pragma comment (lib, "mscoree.lib")
#define DllExport extern "C" __declspec(dllexport)

DllExport void NotifyLoad(LPWSTR message);
DllExport void LoadManaged(LPCWSTR dotNetVersion, LPCWSTR szDllLocation, LPCWSTR szMainClass, LPCWSTR szMainMethod, LPCWSTR szParameters);