// GetP4Vars.cpp : This file is for a console application
// that has its Linker > Manifest File > UAC Execution Level
// set to asInvoker. We shouldn't need to run as administrator, but
// if we need to, we can launch this as an administrator by the bootstrapper.
//
// We need to use it with a WiX Burn managed bootstrapper application
// since we need to read 32-bit parts of the registry, but a Burn
// managed bootstrapper can't access the 32-bit part of the
// registry (C# registry functions can't). We could use DllImport, but
// we write this in C++ to easily use Win32 APIs RegOpenKeyEx, RegQueryValueEx, etc.
//
// Note that if we do need to run as administrator, the .exe
// should be signed with a description which will be displayed
// in the UAC dialog.
//
// Copyright (c) 2019 Perforce Software. All Rights Reserved

#include <iostream>
#include "windows.h"

// If we get an error we want to return it.
LONG GetRegValue(HKEY hkeyroot, LPCWSTR lpSubKey, LPCWSTR lpValueName, REGSAM samDesired, LPBYTE lpValue)
{
	DWORD bufferSize = 1024; // Must be same as for lpValue.
	DWORD type = 0;
	HKEY hkey = 0;

	LONG lResult = RegOpenKeyEx(hkeyroot, lpSubKey, 0, KEY_READ | samDesired, &hkey);

	if (lResult == ERROR_SUCCESS)
	{
		lResult = RegQueryValueEx(hkey, lpValueName, NULL, &type, lpValue, &bufferSize);
	}
	RegCloseKey(hkey);
	return lResult;
}

int main()
{
	const int strsize = 1024;
	std::wstring regPath = L"Software\\Perforce\\Environment";
	LONG testResult = 0;
	HKEY testKey = 0;

    // TODO: See if this is really needed.
	// First see if we have access to the 32-bit part of the registry.
	// If not, this app should be run as administrator.
	testResult = RegOpenKeyEx(HKEY_LOCAL_MACHINE, regPath.c_str(), 0, KEY_READ | KEY_WOW64_32KEY, &testKey);
	if (testResult != ERROR_SUCCESS)
	{
		//MessageBox(NULL, L"OpenKeyEx failed.", L"DEBUG", MB_OK);
		return testResult;
	}
	RegCloseKey(testKey);

	// P4INSTROOT

	WCHAR prev_instdir[strsize] = { 0 };
	WCHAR prev_instdir_pm[strsize] = { 0 };
	WCHAR prev_instdir_pu[strsize] = { 0 };

	GetRegValue(HKEY_LOCAL_MACHINE, regPath.c_str(), L"P4INSTROOT", KEY_WOW64_32KEY, (LPBYTE)prev_instdir_pm);
	GetRegValue(HKEY_CURRENT_USER, regPath.c_str(), L"P4INSTROOT", 0, (LPBYTE)prev_instdir_pu);

	if (lstrlen(prev_instdir_pm) > 0) wcscpy_s(prev_instdir, prev_instdir_pm);
	if (lstrlen(prev_instdir_pu) > 0) wcscpy_s(prev_instdir, prev_instdir_pu);

	// P4EDITOR

	WCHAR prev_p4editor[strsize] = { 0 };
	WCHAR prev_p4editor_pm[strsize] = { 0 };
	WCHAR prev_p4editor_pu[strsize] = { 0 };
	WCHAR prev_p4editor_sm[strsize] = { 0 };
	WCHAR prev_p4editor_su[strsize] = { 0 };
	GetRegValue(HKEY_LOCAL_MACHINE, regPath.c_str(), L"P4EDITOR", KEY_WOW64_32KEY, (LPBYTE)prev_p4editor_pm);
	GetRegValue(HKEY_CURRENT_USER, regPath.c_str(), L"P4EDITOR", 0, (LPBYTE)prev_p4editor_pu);
	GetRegValue(HKEY_LOCAL_MACHINE, L"SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", L"P4EDITOR", KEY_WOW64_32KEY, (LPBYTE)prev_p4editor_sm);
	GetRegValue(HKEY_CURRENT_USER, L"Environment", L"P4EDITOR", 0, (LPBYTE)prev_p4editor_su);
	if (lstrlen(prev_p4editor_pm) > 0) wcscpy_s(prev_p4editor, prev_p4editor_pm);
	if (lstrlen(prev_p4editor_pu) > 0) wcscpy_s(prev_p4editor, prev_p4editor_pu);
	if (lstrlen(prev_p4editor_sm) > 0) wcscpy_s(prev_p4editor, prev_p4editor_sm);
	if (lstrlen(prev_p4editor_su) > 0) wcscpy_s(prev_p4editor, prev_p4editor_su);

	// P4PORT

	WCHAR prev_p4port[strsize] = { 0 };
	WCHAR prev_p4port_pm[strsize] = { 0 };
	WCHAR prev_p4port_pu[strsize] = { 0 };
	WCHAR prev_p4port_sm[strsize] = { 0 };
	WCHAR prev_p4port_su[strsize] = { 0 };
	GetRegValue(HKEY_LOCAL_MACHINE, regPath.c_str(), L"P4PORT", KEY_WOW64_32KEY, (LPBYTE)prev_p4port_pm);
	GetRegValue(HKEY_CURRENT_USER, regPath.c_str(), L"P4PORT", 0, (LPBYTE)prev_p4port_pu);
	GetRegValue(HKEY_LOCAL_MACHINE, L"SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", L"P4PORT", KEY_WOW64_32KEY, (LPBYTE)prev_p4port_sm);
	GetRegValue(HKEY_CURRENT_USER, L"Environment", L"P4PORT", 0, (LPBYTE)prev_p4port_su);
	if (lstrlen(prev_p4port_pm) > 0) wcscpy_s(prev_p4port, prev_p4port_pm);
	if (lstrlen(prev_p4port_pu) > 0) wcscpy_s(prev_p4port, prev_p4port_pu);
	if (lstrlen(prev_p4port_sm) > 0) wcscpy_s(prev_p4port, prev_p4port_sm);
	if (lstrlen(prev_p4port_su) > 0) wcscpy_s(prev_p4port, prev_p4port_su);

	// P4USER

	WCHAR prev_p4user[strsize] = { 0 };
	WCHAR prev_p4user_pm[strsize] = { 0 };
	WCHAR prev_p4user_pu[strsize] = { 0 };
	WCHAR prev_p4user_sm[strsize] = { 0 };
	WCHAR prev_p4user_su[strsize] = { 0 };
	GetRegValue(HKEY_LOCAL_MACHINE, regPath.c_str(), L"P4USER", KEY_WOW64_32KEY, (LPBYTE)prev_p4user_pm);
	GetRegValue(HKEY_CURRENT_USER, regPath.c_str(), L"P4USER", 0, (LPBYTE)prev_p4user_pu);
	GetRegValue(HKEY_LOCAL_MACHINE, L"SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", L"P4USER", KEY_WOW64_32KEY, (LPBYTE)prev_p4user_sm);
	GetRegValue(HKEY_CURRENT_USER, L"Environment", L"P4USER", 0, (LPBYTE)prev_p4user_su);
	if (lstrlen(prev_p4user_pm) > 0) wcscpy_s(prev_p4user, prev_p4user_pm);
	if (lstrlen(prev_p4user_pu) > 0) wcscpy_s(prev_p4user, prev_p4user_pu);
	if (lstrlen(prev_p4user_sm) > 0) wcscpy_s(prev_p4user, prev_p4user_sm);
	if (lstrlen(prev_p4user_su) > 0) wcscpy_s(prev_p4user, prev_p4user_su);

	WCHAR p4values[2048] = { 0 };
	// Check sizes so don't copy non-null-terminated string (gets exception).
	// TODO: Find better way to initialize the strings! Tried = L"\0" with same exception.
	if (lstrlen(prev_instdir) > 0)
	{
		wcscpy_s(p4values, prev_instdir);
	}
	wcscat_s(p4values, wcslen(p4values) + 2, L",");
	if (lstrlen(prev_p4editor) > 0)
	{
		wcscat_s(p4values, wcslen(p4values) + 2 * lstrlen(prev_p4editor), prev_p4editor);
	}
	wcscat_s(p4values, wcslen(p4values) + 2, L",");
	if (lstrlen(prev_p4port) > 0)
	{
		wcscat_s(p4values, wcslen(p4values) + 2 * lstrlen(prev_p4port), prev_p4port);
	}
	wcscat_s(p4values, wcslen(p4values) + 2, L",");
	if (lstrlen(prev_p4user) > 0)
	{
		wcscat_s(p4values, wcslen(p4values) + 2 * lstrlen(prev_p4user), prev_p4user);
	}

	// Save the results in the registry for the installer to pick up.

	HKEY hkey = 0;
	HKEY hkeyResult = 0;
	DWORD disposition;
	LONG lResult = RegCreateKeyEx(HKEY_CURRENT_USER, L"Software\\Perforce", 0, NULL, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hkey, &disposition);
	if (lResult != ERROR_SUCCESS)
	{
		//MessageBox(NULL, L"Unable to create or open HKCU\\Software\\Perforce.", L"DEBUG", MB_OK);
		return lResult;
	}

	bool fCreatedPerforceKey = (REG_CREATED_NEW_KEY == disposition);

	lResult = RegSetValueEx(hkey, L"P4ValuesForSetup", 0, REG_SZ, (LPBYTE)p4values, 2 * strsize);
	if (lResult == ERROR_SUCCESS)
	{
		if (fCreatedPerforceKey)
		{
			RegSetValueEx(hkey, L"DeletePerforceKey", 0, REG_SZ, (LPBYTE)L"", 0);
		}
	}
	RegCloseKey(hkey);
	return lResult;
}
