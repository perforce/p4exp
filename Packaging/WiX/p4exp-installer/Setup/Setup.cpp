// Setup.cpp: Simple bootstrapper for running MSI files.
// 

#include "stdafx.h"
#include "resource.h"
#include <codecvt> // codecvt_utf8, wstring_convert, convert_typeX
#include <fstream> // ofstream
#include <shlobj.h> // SHGetKnownFolderPath
#include <Windows.h>
#include <shellapi.h>
#include <Msi.h> // MSIHANDLE, MsiCloseHandle, MAXPATH
#include <MsiQuery.h> // MsiGetSummaryInformation, MsiSummaryInfoGetProperty

#pragma comment(lib, "msi.lib")

// This is needed so running the app doesn't open a console window.
#pragma comment(linker, "/SUBSYSTEM:windows /ENTRY:wmainCRTStartup")

// This is used to convert to a string the macro VER_PRODUCT that points to the environment
// variable ProductVersion.
#define STRINGIZER(arg)     #arg
#define STR_VALUE(arg)      STRINGIZER(arg)
#define VER_PRODUCT_STRING  STR_VALUE(VER_PRODUCT)

using namespace std;

// Global Variables

PWSTR g_pszLocalAppData = NULL;
BOOL g_fSilent = false;
wstring g_logFileName;
BOOL g_bMSI64bit = false;
const WCHAR g_caption[50] = L"Helix Plugin for File Explorer Setup";

// Function Declarations

void WriteLineToInstallerLog(wstring message);

// Functions

wstring s2ws(const std::string& str)
{
    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;

    return converterX.from_bytes(str);
}

string ws2s(const std::wstring& wstr)
{
    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;

    return converterX.to_bytes(wstr);
}

// The .MSI file is in the .exe as a binary resource, so need to extract it.
void extract_bin_resource(int nResourceId, string strOutputPath)
{
    HGLOBAL hResourceLoaded;   // handle to loaded resource
    HRSRC   hRes;              // handle/ptr to res. info.
    char    *lpResLock;        // pointer to resource data
    DWORD   dwSizeRes;
    string strOutputLocation;
    string strAppLocation;

    hRes = FindResource(NULL, MAKEINTRESOURCE(nResourceId), RT_RCDATA);

    hResourceLoaded = LoadResource(NULL, hRes);
    lpResLock = (char *)LockResource(hResourceLoaded);
    dwSizeRes = SizeofResource(NULL, hRes);

    ofstream outputFile(strOutputPath.c_str(), ios::binary);
    outputFile.write((const char *)lpResLock, dwSizeRes);
    outputFile.close();
}

typedef BOOL(WINAPI *LPFN_ISWOW64PROCESS) (HANDLE, PBOOL);

LPFN_ISWOW64PROCESS fnIsWow64Process;

// Return true if running on a 64-bit version of Windows.
BOOL IsWow64()
{
    BOOL bIsWow64 = FALSE;

    //IsWow64Process is not available on all supported versions of Windows.
    //Use GetModuleHandle to get a handle to the DLL that contains the function
    //and GetProcAddress to get a pointer to the function if available.

    fnIsWow64Process = (LPFN_ISWOW64PROCESS)GetProcAddress(
        GetModuleHandle(TEXT("kernel32")), "IsWow64Process");

    if (NULL != fnIsWow64Process)
    {
        if (!fnIsWow64Process(GetCurrentProcess(), &bIsWow64))
        {
            // TODO: handle error
        }
    }
    return bIsWow64;
}

// Compare 2 MSI product version strings (3 numbers of format "major.minor.build").
// Returns true if 2nd version number is greater than first.
bool IsVer2Greater(WCHAR* ver1, wstring ver2)
{
	int major1, minor1, build1, major2, minor2, build2;
	swscanf_s(ver1, L"%d.%d.%d", &major1, &minor1, &build1);
	swscanf_s(ver2.c_str(), L"%d.%d.%d", &major2, &minor2, &build2);
	if (major2 > major1 ||
        major2 == major1 && minor2 > minor1 ||
        major2 == major1 && minor2 == minor1 && build2 > build1)
	{
		return true;
	}
	return false;
}

// Write to same default log file that installer writes to.
// The file is Unicode when the installer writes to it.
// Simplest to write to file in binary mode.
// TODO: If present, use the log file specified on the command line.
void WriteLineToInstallerLog(wstring message)
{
    message.append(L"\r\n"); // Append newline so write a line.
    std::ofstream logfile(ws2s(g_logFileName.c_str()), std::ios_base::app | std::ios::binary); // app: append
    logfile.write((char *)message.c_str(), message.length() * sizeof(wchar_t));
}

// Gets the version of any installed P4V, or returns "0" if not found.
wstring GetP4VVersion()
{
    TCHAR productCode[255];
    UINT status;
    TCHAR version[14]; // Max: 255.255.65535
    DWORD bufferSize = 14;

    LPCTSTR p4vUpgradeCode = L"{70A9FDC7-885B-4D6D-BAFD-CB2D27AB2963}";

    WCHAR msg[2048] = { 0 };

    // Get productCode of installed P4V, if any.
    // There should only be 1 product code, but there could be cases (one was found internally)
    // where there is more than one (2), AND the additional one is invalid (MsiGetProductInfo
    // doesn't return a valid version, and there isn't an uninstall key with that product code).
    for (int i = 0; i < 6; i++)
    {
        status = MsiEnumRelatedProducts(p4vUpgradeCode, 0, i, productCode);
        swprintf_s(msg, sizeof(msg), L"GetP4VVersion: MsiEnumRelatedProducts returned %d. productCode: %ls", status, productCode);
        WriteLineToInstallerLog(msg);

        if (status == ERROR_NO_MORE_ITEMS)
        {
            return wstring(L"0");
        }
        else
        {
            status = MsiGetProductInfo(productCode, INSTALLPROPERTY_VERSIONSTRING, version, &bufferSize);
            swprintf_s(msg, sizeof(msg), L"GetP4VVersion: MsiGetProductInfo returned %d. version: %ls", status, version);
            WriteLineToInstallerLog(msg);

            if (status == ERROR_SUCCESS)
            {
                return wstring(version);
            }
            else // Use product code to get version from Uninstall key.
            {
                bufferSize = 14;
                HKEY hUninstKey = nullptr;
                wstring regPath = L"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\";
                regPath.append(productCode);
                REGSAM key32or64 = KEY_WOW64_32KEY; // Assume 32-bit installer.
                if (g_bMSI64bit)
                {
                    key32or64 = KEY_WOW64_64KEY; // 64-bit installer uses 64-bit part of registry.
                }
                LONG lResult = RegOpenKeyEx(HKEY_LOCAL_MACHINE, regPath.c_str(), 0, KEY_READ | key32or64, &hUninstKey);

                if (lResult == ERROR_SUCCESS)
                {
                    lResult = RegQueryValueExW(hUninstKey, L"DisplayVersion", 0, 0, (LPBYTE)version, &bufferSize);
                    RegCloseKey(hUninstKey);
                    if (lResult == ERROR_SUCCESS)
                    {
                        swprintf_s(msg, sizeof(msg), L"GetP4VVersion: Version found in Uninstall key: %ls", version);
                        WriteLineToInstallerLog(msg);
                        return wstring(version);
                    }
                    else
                    {
                        WriteLineToInstallerLog(L"GetP4VVersion: Error getting version from Uninstall key.");
                    }
                }
                else
                {
                    WriteLineToInstallerLog(L"GetP4VVersion: Error opening Uninstall key.");
                }
            }
        }
    } // for
    return wstring(L"0"); // Shouldn't get here, but might.
}

// Determine if another installer (MSI) is runnning.
// If it is, we must wait until it completes.
// Also returns true if error.
bool IsAnotherInstallerRunning()
{
    SERVICE_STATUS_PROCESS ssStatus;
    DWORD dwBytesNeeded;
    WCHAR msg[2048];

    // Get a handle to the Service Control Manager database.
    // We're running as an admin so we can open.
    SC_HANDLE schSCManager = OpenSCManager(
        NULL,                    // local computer
        NULL,                    // servicesActive database 
        SC_MANAGER_ALL_ACCESS);  // full access rights 

    if (NULL == schSCManager)
    {
        swprintf_s(msg, sizeof(msg), L"Setup error OpenSCManager failed (%d)", GetLastError());
        WriteLineToInstallerLog(msg);
        return true;
    }

    // Get a handle to the Windows Installer service.

    SC_HANDLE schService = OpenService(
        schSCManager,         // SCM database 
        L"msiserver",            // name of service 
        SERVICE_ALL_ACCESS);  // full access 

    if (schService == NULL)
    {
        swprintf_s(msg, sizeof(msg), L"Setup error OpenService failed (%d)", GetLastError());
        WriteLineToInstallerLog(msg);
        CloseServiceHandle(schSCManager);
        return true;
    }

    // Check the status in case the service is not stopped. 

    if (!QueryServiceStatusEx(
        schService,                     // handle to service 
        SC_STATUS_PROCESS_INFO,         // information level
        (LPBYTE)&ssStatus,             // address of structure
        sizeof(SERVICE_STATUS_PROCESS), // size of structure
        &dwBytesNeeded))              // size needed if buffer is too small
    {
        swprintf_s(msg, sizeof(msg), L"Setup error QueryServiceStatusEx failed (%d)", GetLastError());
        CloseServiceHandle(schService);
        CloseServiceHandle(schSCManager);
        return true;
    }

    // The Windows Installer service is currently running if the value of the dwControlsAccepted 
    // member of the returned SERVICE_STATUS_PROCESS structure is SERVICE_ACCEPT_SHUTDOWN.
    // https://msdn.microsoft.com/en-us/library/windows/desktop/aa372909(v=vs.85).aspx
    // That page is about _MSIExecute Mutex.
    return SERVICE_ACCEPT_SHUTDOWN == ssStatus.dwControlsAccepted;
}

// Get the PackageCode from an MSI file.
UINT GetPackageCode(wstring databasePath, wstring &packageCode)

{
    MSIHANDLE hDatabase = NULL;
    MSIHANDLE hSummaryInfo = NULL;
    UINT uiResult = MsiGetSummaryInformation(hDatabase, databasePath.c_str(), 0, &hSummaryInfo);
    if (ERROR_SUCCESS == uiResult)
    {
        WCHAR inpackageCode[40] = L"";
        DWORD dwSize = 40;
        UINT dType;
        INT  iValue;
        FILETIME ft;
        // PID_REVNUMBER = 9. For an installation package, this is the package code.
        uiResult = MsiSummaryInfoGetProperty(hSummaryInfo, 9, &dType, &iValue, &ft, inpackageCode, &dwSize);
        if (ERROR_SUCCESS == uiResult)
        {
            packageCode.assign(inpackageCode);
        }
        MsiCloseHandle(hDatabase);
    }
    return uiResult;
}

HRESULT CreateAndInitializeFileOperation(REFIID riid, void **ppv)
{
    *ppv = NULL;
    IFileOperation *pfo;
    HRESULT hr = CoCreateInstance(__uuidof(FileOperation), NULL, CLSCTX_ALL, IID_PPV_ARGS(&pfo));
    if (SUCCEEDED(hr))
    {
        // Set the operation flags. Turn off all UI
        // from being shown to the user during the
        // operation. This includes error, confirmation
        // and progress dialogs.
        hr = pfo->SetOperationFlags(FOF_NO_UI);
        if (SUCCEEDED(hr))
        {
            hr = pfo->QueryInterface(riid, ppv);
        }
        pfo->Release();
    }
    return hr;
}

// Check if .NET 4.5 or greater is installed.
// No .NET 5.x released, so don't know how to check for it.
BOOL IsDotNet45OrGreaterInstalled(BOOL bMSIx64)
{
    wstring dotNetRegKey = L"SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full";
    // Release version from https://blogs.msdn.microsoft.com/astebner/2009/06/16/sample-code-to-detect-net-framework-install-state-and-service-pack-level/
    const DWORD dwNetfx45ReleaseVersion = 378389;
    HKEY hDotNetKey = NULL;
    REGSAM key32or64 = KEY_WOW64_32KEY; // Assume 32-bit installer.
    if (bMSIx64)
    {
        key32or64 = KEY_WOW64_64KEY; // 64-bit installer uses 64-bit part of registry.
    }
    if (ERROR_SUCCESS == RegOpenKeyEx(HKEY_LOCAL_MACHINE, dotNetRegKey.c_str(), 0, KEY_READ | key32or64, &hDotNetKey))
    {
        DWORD dwRegValue = 0;
        DWORD dwSize = sizeof(DWORD);
        if (ERROR_SUCCESS == RegQueryValueEx(hDotNetKey, L"Install", 0, 0, (LPBYTE)&dwRegValue, &dwSize))
        {
            // See if Install flag set, which means an install was successful.
            if (1 == dwRegValue)
            {
                RegQueryValueEx(hDotNetKey, L"Release", 0, 0, (LPBYTE)&dwRegValue, &dwSize);
                if (dwRegValue >= dwNetfx45ReleaseVersion)
                {
                    RegCloseKey(hDotNetKey);
                    return true;
                }
            }
        }
        RegCloseKey(hDotNetKey);
    }
    return false;
}

// Extract an installer from this .exe, and "return" path as a reference.
BOOL ExtractInstaller(int resourceID, string fileName, string &pathToInstaller)
{
    // Get a GUID string for the folder name to put the installer in.
    GUID guid;
    CoCreateGuid(&guid);
    WCHAR* pszGUID;
    StringFromCLSID(guid, &pszGUID);

    pathToInstaller.assign(ws2s(g_pszLocalAppData));
    pathToInstaller.append("\\");
    pathToInstaller.append(ws2s(pszGUID));
    pathToInstaller.append("\\");
    CreateDirectoryA(pathToInstaller.c_str(), NULL);

    pathToInstaller.append(fileName);

    extract_bin_resource(resourceID, pathToInstaller);

    return true; // TODO: Detect error and return false if error.
}

// Delete the *folder* that has the temporary installer to also delete the installer.
// The path includes the filename, so need to remove that.
BOOL DeleteInstallerInTemp(string pathToInstallerTemp)
{
    // Delete the folder with the temporary installer.
    // TODO: Simplify this code! Use different interfaces.
    HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
    if (SUCCEEDED(hr))
    {
        IFileOperation *pfo;
        hr = CreateAndInitializeFileOperation(IID_PPV_ARGS(&pfo));
        if (SUCCEEDED(hr))
        {
            size_t found;
            found = pathToInstallerTemp.find_last_of("/\\");
            string pathToFolder = pathToInstallerTemp.substr(0, found);

            IShellItem* folderToDelete = NULL;
            hr = SHCreateItemFromParsingName(s2ws(pathToFolder).c_str(), NULL, IID_PPV_ARGS(&folderToDelete));
            hr = pfo->DeleteItem(folderToDelete, NULL);
            if (SUCCEEDED(hr))
            {
                hr = pfo->PerformOperations();
            }
            pfo->Release();
        }
    }

    if (!SUCCEEDED(hr))
    {
        wstring message = L"Error cleaning up temporary installer file.";
        WriteLineToInstallerLog(message);
        if (!g_fSilent)
        {
            MessageBox(0, message.c_str(), g_caption, MB_OK);
        }
        CoUninitialize();
        return false;
    }
    return true;
}

// Run the .NET 4.5 installer included with this .exe.
BOOL InstallDotNet45()
{
    // Extract .NET installer from this .exe to a temporary location.
    std::string pathToDotNetInstaller; // Returned by ExtractInstaller.
    ExtractInstaller(IDR_DOTNETINSTALLER, "dotNetFx45_Full_setup.exe", pathToDotNetInstaller);

    // Run .NET installer.

    STARTUPINFO si;
    PROCESS_INFORMATION pi;

    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    ZeroMemory(&pi, sizeof(pi));

    if (!CreateProcess(s2ws(pathToDotNetInstaller).c_str(),
        L" /passive /norestart /showfinalerror", // command line (must start with space)
        // passive: don't ask user for input, but display progress.
        // norestart: don't restart system. We will handle that to make sure it's done
        //     for cases where .NET doesn't restart, but restart is necessary for P4EXP
        //     installer to register the DLLs (the registration .exe will fail if system not restarted).
        //     We also want to show a message that reminds the user to start the installer again.
        // showfinalerror: displays errors if the installation is not successful.
        //     This option requires user interaction if the installation is not successful.
        NULL,
        NULL,
        FALSE,
        0,
        NULL,
        NULL,
        &si,
        &pi))
    {
        WriteLineToInstallerLog(L"Error creating process to start .NET 4.5 installer.");
        return false;
    }
    WaitForSingleObject(pi.hProcess, INFINITE); // TODO: Find better timeout?

    DeleteInstallerInTemp(pathToDotNetInstaller);

    return IsDotNet45OrGreaterInstalled(g_bMSI64bit);
}

// So that this installer will run after a system restart,
// add the path to this installer to RunOnce.
// This performs a "nice to have" feature, so if it fails
// don't need to tell user, but best to log the error.
int AddThisInstallerToRunOnce(wchar_t *installerPath)
{
    wstring errorMsg = L"Error adding this installer to registry to run again after restart.";
    DWORD size = wcslen(installerPath) * sizeof(wchar_t);
    HKEY hRunOnceKey = nullptr;
    wstring regPath = L"Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce";
    REGSAM key32or64 = KEY_WOW64_32KEY; // Assume 32-bit installer.
    if (g_bMSI64bit)
    {
        key32or64 = KEY_WOW64_64KEY; // 64-bit installer writes to 64-bit part of registry.
    }

    LONG lResult = RegOpenKeyEx(HKEY_LOCAL_MACHINE, regPath.c_str(), 0, KEY_SET_VALUE | key32or64, &hRunOnceKey);

    if (ERROR_SUCCESS == lResult)
    {
        // TODO: Verify that a path with spaces in it works. May have to quote the path.
        if (ERROR_SUCCESS != RegSetValueEx(hRunOnceKey, L"Perforce P4EXP Installer", 0, REG_SZ, (const BYTE*)(LPCTSTR)installerPath, size))
        {
            WriteLineToInstallerLog(errorMsg);
        }
    }
    else
    {
        WriteLineToInstallerLog(errorMsg);
    }
    RegCloseKey(hRunOnceKey);
    return ERROR_SUCCESS;
}

int wmain(int argc, wchar_t *argv[])
{
    BOOL fIgnoreOtherInstaller = false;
    wstring installOption = L"/i"; // Assume we will be installing.

    WCHAR tempPath[MAX_PATH];
    GetTempPath(MAX_PATH, tempPath); // Get system temp path.

    g_logFileName = tempPath;
    g_logFileName.append(L"PerforceHelixPluginWindowsExplorer.log");

    // Early on, see if user wants this Setup to be silent (/s or /S).
    // That way we know if we shouldn't display error messages.
    // The MSI is silent with /v/qn.
    // Search for option since it can be in any order.
    if (argc > 1)
    {
        for (int index = 1; index < argc; index++)
        {
            if (_wcsicmp(argv[index], L"/s") == 0)
            {
                g_fSilent = true;
            }
        }
    }

    if (S_OK != SHGetKnownFolderPath(FOLDERID_LocalAppData, KF_FLAG_CREATE | KF_FLAG_INIT, NULL, &g_pszLocalAppData))
    {
        wstring message = L"Error getting path to LocalAppData folder. Unable to extract installer.";
        WriteLineToInstallerLog(message);
        if (!g_fSilent)
        {
            MessageBox(0, message.c_str(), g_caption, MB_OK);
        }
        return ERROR_PATH_NOT_FOUND; // TODO: Find better error?
    }

    // Based on environment variable passed to project use appropriate MSI.
#if MSIPLATFORM == 64
    string installer = "p4expinst64.msi";
    g_bMSI64bit = true;
#else
    string installer = "p4expinst.msi";
    g_bMSI64bit = false;
#endif

    if (!IsDotNet45OrGreaterInstalled(g_bMSI64bit))
    {
        wstring message = L"This product requires .NET 4.5 or greater. This installer can install .NET 4.5 from the web. ";
        message.append(L"Install .NET 4.5? If you cancel, you must install .NET 4.5 or greater later, then restart the system.");
        WriteLineToInstallerLog(message);
        if (!g_fSilent)
        {
            int result = MessageBox(NULL, message.c_str(), g_caption, MB_OKCANCEL);
            if (result == IDOK)
            {
                // Launch the .NET installer.
                if (!InstallDotNet45())
                {
                    // The .NET 4.5 installer shows the error, but we also need to log it.
                    WriteLineToInstallerLog(L"Error installing .NET 4.5. The installer can not continue without this prerequisite.");
                    return ERROR_INSTALL_PREREQUISITE_FAILED;
                }
                WriteLineToInstallerLog(L".NET 4.5 successfully installed.");

                result = MessageBox(NULL, L"Restart the system? After restarting the system, run this P4EXP installer again.", g_caption, MB_OKCANCEL);
                if (result == IDOK)
                {
                    DWORD dwResult = ExitWindowsEx(EWX_RESTARTAPPS, SHTDN_REASON_MAJOR_SOFTWARE);

                    if (0 == dwResult) // Return 0 is failure.
                    {
                        WCHAR msg[2048];
                        swprintf_s(msg, sizeof(msg), L"Error attempting to restart the system: %d. The system needs to be restarted before the P4EXP installer is started.", GetLastError());
                        MessageBox(NULL, msg, g_caption, MB_OK);
                        WriteLineToInstallerLog(msg);
                    }
                    else
                    {
                        // There should be enough time after calling ExitWindowsEx to do this.
                        // TODO: Fix problems before using AddThisInstallerToRunOnce:
                        // when the installer starts (it's in RunOnce) it looks like explorer
                        // hasn't started (black background and no taskbar), there's an installer
                        // error that it can't stop explorer if choose to, and after the installer runs,
                        // the p4exp menu isn't present. Explorer needs to be restarted.
                        // AddThisInstallerToRunOnce(argv[0]); // argv[0] has full path to this installer.
                    }
                }
                return ERROR_SYSTEM_SHUTDOWN; // TODO: Find something better related to system restart required.
            }
            else
            {
                WriteLineToInstallerLog(L"User chose not to install .NET 4.5, which is a prerequisite for this installer.");
                return ERROR_INSTALL_PREREQUISITE_FAILED;
            }
        }
        else // Silent install. We can't ask to install .NET, but we wrote to the log that .NET required.
        {
            WriteLineToInstallerLog(L"Installer must be run with a UI so the installer can install .NET 4.5.");
            return ERROR_INSTALL_PREREQUISITE_FAILED;
        }
    }

    // Handle request for help first.
    if (argc > 1 && wcscmp(argv[1], L"/?") == 0)
    {
        string helpMessage = "";
        helpMessage.append("Command line parameters:\n");
        helpMessage.append("/S Hide any initialization dialogs. For silent mode use: /S /v/qn.\n");
        helpMessage.append("/V parameters to MsiExec.exe\n");
        helpMessage.append("/X Uninstall the product installed by this installer.\n");
        helpMessage.append("/A Perform an administrative installation.\n");
        helpMessage.append("/ignore Ignore error if another installer is running.");
        // We don't allow this for now (features in previous version didn't allow advertise):
        // helpMessage.append("/J Perform an advertised installation.");
        MessageBox(NULL, s2ws(helpMessage).c_str(), g_caption, MB_OK);
        return 0;
    }

    // See if user wants to ignore another running installer.
    // Search for option since it can be in any order.
    if (argc > 1)
    {
        for (int index = 1; index < argc; index++)
        {
            if (_wcsicmp(argv[index], L"/ignore") == 0)
            {
                fIgnoreOtherInstaller = true;
            }
        }
    }

    if (!fIgnoreOtherInstaller && IsAnotherInstallerRunning())
    {
        // This is the same text as the Windows error ERROR_INSTALL_ALREADY_RUNNING.
        wstring msg = L"Another installation is already in progress. Complete that installation before proceeding with this install.";
        if (!g_fSilent)
        {
            MessageBox(NULL, msg.c_str(), g_caption, MB_OK | MB_ICONERROR);
        }
        WriteLineToInstallerLog(msg);
        return ERROR_INSTALL_ALREADY_RUNNING;
    }

    // Show a more helpful error message than MSIEXEC's.
    if (g_bMSI64bit && !IsWow64() && !g_fSilent)
    {
        MessageBox(NULL,
            L"This installer can only be run on a 64-bit operating system. A 32-bit installer for Helix Plugin for File Explorer is available from perforce.com/downloads.",
            g_caption, MB_OK | MB_ICONERROR);
        return ERROR_INSTALL_PLATFORM_UNSUPPORTED;
    }

    // Give a warning if installing 32-bit apps on 64-bit OS.
    // We want to encourage customers to use 64-bit apps on 64-bit OS.
    if (!g_bMSI64bit && IsWow64() && !g_fSilent)
    {
        MessageBox(NULL,
            L"This installer is for a 32-bit operating system. We strongly recommend you download and install Helix Plugin for File Explorer using the Windows 64-bit installer, available from perforce.com/downloads.",
            g_caption, MB_OK | MB_ICONWARNING);
    }

    //
    // Setup for comparing installed version to this installer's version.
    //
    HKEY hUninstKey = nullptr;

    wstring regPath = L"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{C32352DF-BD78-438D-B6DA-7AF3226B7D75}";
    REGSAM key32or64 = KEY_WOW64_32KEY; // Assume 32-bit installer.
    if (g_bMSI64bit)
    {
        key32or64 = KEY_WOW64_64KEY; // 64-bit installer writes to 64-bit part of registry.
    }

    string sOurVersion = VER_PRODUCT_STRING; // ProductVersion environment variable used at build time.

    bool fProductInstalled = false;
    WCHAR sInstalledVersion[30]; // Version string is not very long. 30 WCHAR should be more than enough (including '\0' at end).
    DWORD dBufferSize = 30;

    LONG lResult = RegOpenKeyEx(HKEY_LOCAL_MACHINE, regPath.c_str(), 0, KEY_READ | key32or64, &hUninstKey);

    if (lResult == ERROR_SUCCESS)
    {
        fProductInstalled = true;
        lResult = RegQueryValueExW(hUninstKey, L"DisplayVersion", 0, 0, (LPBYTE)sInstalledVersion, &dBufferSize);
        // TODO: Check if sInstalledVersion not empty, or invalid? Likely case?
    }
    RegCloseKey(hUninstKey);

    // See if we have the minimum version of P4V only if we're not installed.
	// Otherwise can't use this setup to start an uninstall if P4V is uninstalled.
    wstring p4vVersion = GetP4VVersion();
    TCHAR msg[2048];
    swprintf_s(msg, L"P4V version found: %ls", p4vVersion.c_str());
    WriteLineToInstallerLog(msg);

    if (!fProductInstalled && (p4vVersion.compare(L"0") == 0 || !IsVer2Greater(L"173.0.0", p4vVersion)))
    {
        wstring msg = L"Helix Plugin for File Explorer requires that you have at least Perforce Visual Client (P4V) 2017.3 installed. Download and install Helix Core Apps (https://www.perforce.com/products/helix-core-apps/helix-visual-client-p4v) then run this installer again.";
        WriteLineToInstallerLog(msg);
        if (!g_fSilent)
        {
            MessageBox(NULL, msg.c_str(), g_caption, MB_OK);
        }
        return ERROR_INSTALL_PREREQUISITE_FAILED;
    }

    //
    // See if customer wants a special installation mode.
    // Options can be in any order so search for them.
    //

    // /a = administrative installation
    if (argc > 1)
    {
        for (int index = 1; index < argc; index++)
        {
            if (_wcsicmp(argv[index], L"/a") == 0)
            {
                installOption = L"/a";
            }
        }
    }

    // /j = advertised installation
    if (argc > 1)
    {
        for (int index = 1; index < argc; index++)
        {
            if (_wcsicmp(argv[index], L"/j") == 0)
            {
                wstring message = L"Setup Error 87: One of the parameters was invalid. /j (advertised install) does not apply to this product.";
                // We don't allow this for now.
                WriteLineToInstallerLog(message);
                if (!g_fSilent)
                {
                    MessageBox(0, message.c_str(), g_caption, MB_OK);
                }
                return ERROR_INVALID_PARAMETER;
            }
        }
    }

    // /x = uninstall
    if (argc > 1)
    {
        for (int index = 1; index < argc; index++)
        {
            if (_wcsicmp(argv[index], L"/x") == 0)
            {
                installOption = L"/x";

                wstring wsInstalledVersion(sInstalledVersion);
                if (!fProductInstalled || sOurVersion.compare(ws2s(wsInstalledVersion)) != 0)
                {
                    // Error: user wants to uninstall, but product not installed by this installer.
                    wstring message = L"Setup Error 87: One of the parameters was invalid. /x (uninstall) only valid after this installer is used to install the product.";
                    WriteLineToInstallerLog(message);
                    if (!g_fSilent)
                    {
                        MessageBox(0, message.c_str(), g_caption, MB_OK);
                    }
                    return ERROR_INVALID_PARAMETER;
                }
            }
        }
    }

    //
    // Extract the bundled installer from this .exe to a cache location.
    //

    string pathToInstallerTempFull;
    if (!ExtractInstaller(IDR_RCDATA1, installer, pathToInstallerTempFull))
    {
        wstring message = L"Error extracting installer.";
        WriteLineToInstallerLog(message);
        if (!g_fSilent)
        {
            MessageBox(0, message.c_str(), g_caption, MB_OK);
        }
        return ERROR_INSTALL_PACKAGE_NOT_FOUND;
    }

    // Next get the package code of the installer to use as the folder name to
    // cache the installer to.
    string cachedInstaller;
    wstring packageCode;
    if (ERROR_SUCCESS != GetPackageCode(s2ws(pathToInstallerTempFull), packageCode))
    {
        wstring message = L"Error getting package code from extracted installer.";
        WriteLineToInstallerLog(message);
        if (!g_fSilent)
        {
            MessageBox(0, message.c_str(), g_caption, MB_OK);
        }
        DeleteInstallerInTemp(pathToInstallerTempFull);
        return ERROR_INSTALL_INVALID_PACKAGE;
    }
    cachedInstaller.assign(ws2s(g_pszLocalAppData));
    cachedInstaller.append("\\");
    cachedInstaller.append(ws2s(packageCode));
    cachedInstaller.append("\\");
    CreateDirectoryA(cachedInstaller.c_str(), NULL);

    cachedInstaller.append(installer);

    // If the file exists, it must be the same file, so true for fail if exists.
    CopyFileA(pathToInstallerTempFull.c_str(), cachedInstaller.c_str(), true);

    if (!DeleteInstallerInTemp(pathToInstallerTempFull))
    {
        CoTaskMemFree(g_pszLocalAppData);
        return ERROR_FILE_NOT_FOUND;
    }

    CoUninitialize();
    CoTaskMemFree(g_pszLocalAppData);

    // Prepare to run the installer.
    SHELLEXECUTEINFO ShExecInfo = { 0 };
    ShExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
    ShExecInfo.fMask = SEE_MASK_NOCLOSEPROCESS;
    ShExecInfo.hwnd = NULL;
    ShExecInfo.lpVerb = NULL;
    ShExecInfo.lpFile = L"msiexec.exe";

    // Construct this command line: <installOption> "<installerfile>" /l+* "<logfile>"
    // /l+ will append the log to the log file. "*" logs all info except verbose (v) and extra debugging information (x).
    // We don't need to remove this if the user passes /v/l on the command line since the
    // last /l on the command line will be used.
    wstring commandLine = installOption;
    commandLine.append(L" \"");
    commandLine.append(s2ws(cachedInstaller));
    commandLine.append(L"\" /l+* \"");

    commandLine.append(g_logFileName);
    commandLine.append(L"\""); // Add quote after name.

    //
    // See if we're doing a minor upgrade (version greater). If so, we have to pass
    // REINSTALL properties to the installer.
    // If versions the same installer will show Modify dialog.
    // TODO: Support small update? Check package code so if different but versions same do small update.
    //

    // TODO: Did previous version allow a 32-bit to upgrade a 64-bit and visa versa?

    if (fProductInstalled && IsVer2Greater(sInstalledVersion, s2ws(sOurVersion)))
    {
        commandLine.append(L" REINSTALL=ALL REINSTALLMODE=vomus"); // TODO: Is vomus what previous installer did? Check saved log file.
        // TODO: Will this work if versions are the same, but package codes different, so only doing a small update?
    }

    // Add to the command line any parameters passed to this setup with /v, like the log file name and location,
    // property settings, silent install, etc.
    // InstallShield documentation says that more than one /v can be used so we need to look for more than one
    // to support customers using that format.
    wstring paramsFromExe = L"";
    if (argc > 1)
    {
        for (int index = 1; index < argc; index++)
        {
            wstring paramsFromExePart = argv[index];
            // Rather than including a function (and imports) to find case insensitive string, just look for both.
            size_t foundLower = paramsFromExePart.find(L"/v");
            size_t foundUpper = paramsFromExePart.find(L"/V");

            if (foundLower != string::npos || foundUpper != string::npos)
            {
                paramsFromExePart = paramsFromExePart.substr(2, paramsFromExePart.length()); // remove /v at the start.
                paramsFromExe.append(L" "); // separate from default commandLine and/or other parameters previously found
                paramsFromExe.append(paramsFromExePart);
            }
        }
    }

    commandLine.append(paramsFromExe);

    //MessageBox(0, commandLine.c_str(), L"DEBUG: commandLine", MB_OK);

    ShExecInfo.lpParameters = commandLine.c_str();

    ShExecInfo.lpDirectory = NULL;
    ShExecInfo.nShow = SW_SHOW; // If SW_SHOW, msiexec's error dialogs are shown. 17.2 (InstallShield) does this.
    ShExecInfo.hInstApp = NULL;
    ShellExecuteEx(&ShExecInfo);
    WaitForSingleObject(ShExecInfo.hProcess, INFINITE); // TODO: Find better timeout?

    // Get the installer's exit code (msiexec code) so we return the same value.
    DWORD installerExitCode = 0;
    GetExitCodeProcess(ShExecInfo.hProcess, &installerExitCode);

    CloseHandle(ShExecInfo.hProcess);

    return installerExitCode;
}

