#include "pch.h"
#include "main.h"

DllExport void NotifyLoad(LPWSTR message)
{
    MessageBoxW(nullptr, L"TESTING TESTING TESTING\nABCDEFGHIJKLMNOPQRSTUVWXYZ\nabcdefghijklmnopqrstruvwxyz\n0123456789\n�����ٶ󸶹ٻ������īŸ����\n���̻�����ĥ�ȱ���", message, MB_ICONWARNING);
}
