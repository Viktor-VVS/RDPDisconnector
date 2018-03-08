#include <Windows.h>
#include <Wtsapi32.h>
#include <Shlwapi.h>
#pragma comment(lib, "Shlwapi.lib")
#pragma comment(lib, "Wtsapi32.lib")
#pragma comment(lib, "Wtsapi32.lib")
extern "C" __declspec(dllexport) BOOL IsUserSessionActive(WCHAR* UserName);
extern "C" __declspec(dllexport) BOOL DisconnectUser(WCHAR*Name);

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{

	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

BOOL IsUserSessionActive(WCHAR* UserName)
{
	PWTS_SESSION_INFO pSessionInfo = 0;
	DWORD dwCount = 0;
	WCHAR* uname = 0;
	DWORD bytes_returned = 0;
	BOOL Res = FALSE;
	WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, &pSessionInfo, &dwCount);

	for (DWORD i = 0; i < dwCount; ++i)
	{
		WTS_SESSION_INFO si = pSessionInfo[i];
		if (WTSActive == si.State)
		{
			WTSQuerySessionInformation(WTS_CURRENT_SERVER_HANDLE, si.SessionId, WTSUserName, &uname, &bytes_returned);
			if (StrCmpW(uname, UserName) == NULL)
			{
				Res = TRUE;
				break;
			}
		}
	}


	WTSFreeMemory(pSessionInfo);
	return Res;
}

BOOL DisconnectUser(WCHAR*Name)
{
	BOOL res = FALSE;
	PWTS_SESSION_INFO pSessionInfo = 0;
	DWORD dwCount = 0;
	WCHAR* uname = 0;
	DWORD bytes_returned = 0;
	BOOL Res = FALSE;

	WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, &pSessionInfo, &dwCount);

	for (DWORD i = 0; i < dwCount; ++i)
	{
		WTS_SESSION_INFO si = pSessionInfo[i];
		if (WTSActive == si.State)
		{
			WTSQuerySessionInformation(WTS_CURRENT_SERVER_HANDLE, si.SessionId, WTSUserName, &uname, &bytes_returned);
			if (StrCmpW(uname, Name) == NULL)
			{
				res = WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, si.SessionId, TRUE);
				break;
			}
		}
	}
	WTSFreeMemory(pSessionInfo);
	return res;
}

