#include "pch.h"
#include "main.h"

DllExport void NotifyLoad(LPWSTR message)
{
    MessageBoxW(nullptr, L"TESTING TESTING TESTING\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nabcdefghijklmnopqrstruvwxyz\n0123456789\n가나다라마바사아자차카타파하\n일이삼사오육칠팔구십", message, MB_ICONWARNING);
}
