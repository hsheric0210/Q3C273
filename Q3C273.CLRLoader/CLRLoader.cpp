#include "pch.h"
#include "CLRLoader.h"

DllExport void NotifyLoad(LPWSTR message)
{
    MessageBoxW(nullptr, message, L"From cmd.exe ...", MB_ICONWARNING);
}

ICLRRuntimeHost *StartCLR(LPCWSTR dotNetVersion)
{
    HRESULT hr;
    ICLRMetaHost *pClrMetaHost = nullptr;
    ICLRRuntimeInfo *pClrRuntimeInfo = nullptr;
    ICLRRuntimeHost *pClrRuntimeHost = nullptr;

    // Get the CLRMetaHost that tells us about .NET on this machine
    hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID *)&pClrMetaHost);
    if (hr == S_OK)
    {
        // Get the runtime information for the particular version of .NET
        hr = pClrMetaHost->GetRuntime(dotNetVersion, IID_PPV_ARGS(&pClrRuntimeInfo));
        if (hr == S_OK)
        {
            // Check if the specified runtime can be loaded into the process. This
            // method will take into account other runtimes that may already be
            // loaded into the process and set pbLoadable to TRUE if this runtime can
            // be loaded in an in-process side-by-side fashion.
            BOOL fLoadable;
            hr = pClrRuntimeInfo->IsLoadable(&fLoadable);
            if ((hr == S_OK) && fLoadable)
            {
                // Load the CLR into the current process and return a runtime interface
                // pointer.
                hr = pClrRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost,
                    IID_PPV_ARGS(&pClrRuntimeHost));
                if (hr == S_OK)
                {
                    // Start it. This is okay to call even if the CLR is already running
                    pClrRuntimeHost->Start();
                    return pClrRuntimeHost;
                }
            }
        }
    }

    // Cleanup if failed
    if (pClrRuntimeHost)
    {
        pClrRuntimeHost->Release();
        pClrRuntimeHost = NULL;
    }

    if (pClrRuntimeInfo)
    {
        pClrRuntimeInfo->Release();
        pClrRuntimeInfo = NULL;
    }

    if (pClrMetaHost)
    {
        pClrMetaHost->Release();
        pClrMetaHost = NULL;
    }

    return NULL;
}

DllExport void LoadManaged(LPCWSTR dotNetVersion, LPCWSTR szDllLocation, LPCWSTR szMainClass, LPCWSTR szMainMethod, LPCWSTR szParameters)
{
    HRESULT hr;

    // Secure a handle to the CLR v4.0
    ICLRRuntimeHost *pClr = StartCLR(dotNetVersion);
    if (pClr)
    {
        DWORD result;
        hr = pClr->ExecuteInDefaultAppDomain(
            szDllLocation,
            szMainClass,
            szMainMethod,
            szParameters,
            &result);
    }
}
