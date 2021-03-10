#include "stdafx.h" // If not first get "C4627 warning <fstream> skipped looking for precompiled header use".
#include <fstream> // ifstream
#include <codecvt> // codecvt_utf8, wstring_convert, convert_typeX

const WCHAR caption[50] = L"Helix Core Apps Setup";

/********************************************************************
ExtractBinary - Get binary

********************************************************************/
HRESULT ExtractBinary(
    __in LPCWSTR wzBinaryId,
    __out BYTE** pbData,
    __out DWORD* pcbData
    )
{
    HRESULT hr = S_OK;
    LPWSTR pwzSql = NULL;
    LPWSTR pwzErrMsg = NULL;
    PMSIHANDLE hView;
    PMSIHANDLE hRec;

    // make sure we're not horked from the get-go 
    hr = WcaTableExists(L"Binary");
    if (S_OK != hr)
    {
        if (SUCCEEDED(hr))
        {
            hr = E_UNEXPECTED;
        }
        ExitOnFailure(hr, "There is no Binary table.");
    }

    ExitOnNull(wzBinaryId, hr, E_INVALIDARG, "Binary ID cannot be null.");
    ExitOnNull(*wzBinaryId, hr, E_INVALIDARG, "Binary ID cannot be empty string.");

    hr = StrAllocFormatted(&pwzSql, L"SELECT `Data` FROM `Binary` WHERE `Name`=\'%s\'", wzBinaryId);
    ExitOnFailure(hr, "Failed to allocate Binary table query.");

    hr = WcaOpenExecuteView(pwzSql, &hView);
    ExitOnFailure(hr, "Failed to open view on Binary table.");

    hr = WcaFetchSingleRecord(hView, &hRec);
    ExitOnFailure(hr, "Failed to retrieve request from Binary table.");
    ExitOnNull(hRec, hr, E_INVALIDARG, "ID may not have been found in the binary table.");

    hr = WcaGetRecordStream(hRec, 1, pbData, pcbData);
    ExitOnFailure(hr, "Failed to read Binary.Data.");

LExit:
    ReleaseStr(pwzSql);

    return hr;
}

std::wstring s2ws(const std::string& str)
{
    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;

    return converterX.from_bytes(str);
}

void SetVersionProperty(std::string propertyName, std::string line)
{
    size_t pos;
    std::string version;

    pos = line.find_last_of(' ');
    version = line.substr(pos + 1, line.length());

    // TODO: Get string with "%s" from the MSI so we have the localized string
    // that puts the version in the correct location for the language.
    version = "version: " + version;
    WcaSetProperty(s2ws(propertyName).c_str(), s2ws(version).c_str());
}

UINT __stdcall GetAppVersions(MSIHANDLE hInstall)
{
    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;
    HANDLE hFile = INVALID_HANDLE_VALUE;
    BYTE* pbData = NULL;
    DWORD cbData = 0;
    WCHAR pwzFilename[MAX_PATH];
    WCHAR pwzBinaryID[20] = L"versions_txt";
    LPWSTR szValueBuf = NULL;
    LPWSTR szIsHaspInstalled = NULL;
    LPWSTR szIsSentinelInstalled = NULL;
    std::string line;
    std::ifstream versionsFile; // TODO: Appropriate initialization value?

    hr = WcaInitialize(hInstall, "GetAppVersions");
    ExitOnFailure(hr, "Failed to initialize GetAppVersions.");

    WcaLog(LOGMSG_STANDARD, "Initialized.");

    //
    // Extract Versions.txt from binary table and write to TEMP folder.
    //
    WCHAR tempPath[MAX_PATH];
    GetTempPath(MAX_PATH, tempPath);

    wcscpy_s(pwzFilename, tempPath);
    wcscat_s(pwzFilename, L"Versions.txt");

    // ExtractVersionFile(hInstall, L"versions_txt")

    hr = ExtractBinary(pwzBinaryID, &pbData, &cbData);
    ExitOnFailure(hr, "Failed to extract binary data.");

    if ((hFile = CreateFile((LPCWSTR)pwzFilename, GENERIC_WRITE, FILE_SHARE_WRITE,
        NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL)) == INVALID_HANDLE_VALUE)
    {
        PMSIHANDLE hRecord = MsiCreateRecord(0);
        MsiRecordSetString(hRecord, 0, TEXT("Could not create new temporary file.")); 
            MsiProcessMessage(hInstall,
            INSTALLMESSAGE(INSTALLMESSAGE_ERROR + MB_OK), hRecord);
        return ERROR_INSTALL_USEREXIT;
    }

    DWORD cbWritten = 0;
    if (!WriteFile(hFile, pbData, cbData, &cbWritten, NULL))
    {
        PMSIHANDLE hRecord = MsiCreateRecord(0);
        MsiRecordSetString(hRecord, 0, TEXT("Could not write to file.")); 
            MsiProcessMessage(hInstall,
            INSTALLMESSAGE(INSTALLMESSAGE_ERROR + MB_OK), hRecord);
        return ERROR_INSTALL_USEREXIT;
    }

    CloseHandle(hFile);

    //
    // Read Versions.txt and copy versions to corresponding application properties.
    //
    versionsFile.open(pwzFilename);
    while (std::getline(versionsFile, line))
    {
        if (line.find("(P4EXP)") != std::string::npos)
        {
            SetVersionProperty("P4EXP_Version", line);
        }
    }
    versionsFile.close();
    DeleteFile(pwzFilename);

    return 0;

LExit:
    CloseHandle(hFile);
    er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}

// Show a standard Windows dialog allowing the user to browse for a .exe file. For p4v installer, the file is
// the text editing application.
// Input: P4EDITOR (set with default editor)
// Returns: P4EDITOR
//
// From https://etechgoodness.wordpress.com/2015/04/14/wix-add-browse-for-file-capability-to-installer/
// but modified to also specifically use GetOpenFileNameA when I was using GetOpenFileNameW since I'm using
// UNICODE in the project. 17.2 used GetOpenFileNameA.
// TODO: Use IFileOpenDialog, and if support 32-bit get it to work since when I run the SDK sample with
// IFileOpenDialog on a 32-bit OS I get the error “The ordinal 344 could not be located in the dynamic
// link library COMCTL32.dll.”

UINT __stdcall BrowseForFile(MSIHANDLE hInstall)
{
    UINT er = ERROR_SUCCESS;
    // File name selection variables
    OPENFILENAMEA ofn;
    CHAR szSourceFileName[MAX_PATH] = "";
    CHAR szFoundFileName[MAX_PATH] = "";
    DWORD valueBuf = MAX_PATH;

    // TODO: Use WcaGetProperty and convert to CHAR array.
    UINT result = MsiGetPropertyA(hInstall, "P4EDITOR", szSourceFileName, &valueBuf);

    /* Prepare variables */
    SecureZeroMemory(szFoundFileName, sizeof(szFoundFileName));
    SecureZeroMemory(&ofn, sizeof(ofn));

    StringCchCopyA(szFoundFileName, sizeof(szFoundFileName), szSourceFileName);

    /* Prepare OFN */
    ofn.lStructSize = sizeof(ofn);
    ofn.lpstrTitle = "Choose the application used to open text based forms"; // TODO: Get from Windows Property/String.
    ofn.hwndOwner = GetActiveWindow(); // can also use NULL to be a little bit safer, although the dialog won't be modal in that case
    ofn.lpstrFile = szFoundFileName;
    ofn.nMaxFile = sizeof(szFoundFileName);
    ofn.lpstrInitialDir = NULL;
    ofn.lpstrFilter = "Applications (*.exe)\0*.exe"; // TODO: Get from Windows Property/String.
    ofn.nFilterIndex = 1;
    ofn.Flags = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_HIDEREADONLY | OFN_NOCHANGEDIR | OFN_EXPLORER; // Same as in 17.2.

    // Present dialog to user to select a file.
    if (GetOpenFileNameA(&ofn))
    {
        // User selected a file; populate the property.
        MsiSetPropertyA(hInstall, "P4EDITOR", ofn.lpstrFile);
    }

    return ERROR_SUCCESS;
}

// DllMain - Initialize and cleanup WiX custom action utils.
extern "C" BOOL WINAPI DllMain(
	__in HINSTANCE hInst,
	__in ULONG ulReason,
	__in LPVOID
	)
{
	switch(ulReason)
	{
	case DLL_PROCESS_ATTACH:
		WcaGlobalInitialize(hInst);
		break;

	case DLL_PROCESS_DETACH:
		WcaGlobalFinalize();
		break;
	}

	return TRUE;
}

// Delete a directory and any files and directories in it.
int DeleteDirectory(const std::wstring &refRootDirectory, MSIHANDLE hInstall, bool bDeleteSubdirectories = true)
{
    bool             bSubdirectory = false;       // Flag, indicating whether
    // subdirectories have been found
    HANDLE           hFile;                       // Handle to directory
    std::wstring     strFilePath;                 // Filepath
    std::wstring     strPattern;                  // Pattern
    WIN32_FIND_DATA  fdExistingFileData;          // File information

    /* Prepare variables */
    SecureZeroMemory(&fdExistingFileData, sizeof(fdExistingFileData));

    strPattern.erase();
    strPattern.append(refRootDirectory);
    strPattern.append(L"\\*.*");
    hFile = ::FindFirstFile(strPattern.c_str(), &fdExistingFileData);
    if (hFile != INVALID_HANDLE_VALUE)
    {
        do
        {
            if (fdExistingFileData.cFileName[0] != '.')
            {
                strFilePath.erase();
                strFilePath.append(refRootDirectory);
                strFilePath.append(L"\\");
                strFilePath.append(fdExistingFileData.cFileName);

                if (fdExistingFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
                {
                    if (bDeleteSubdirectories)
                    {
                        // Delete subdirectory
                        int iRC = DeleteDirectory(strFilePath, bDeleteSubdirectories);
                        if (iRC)
                            return iRC;
                    }
                    else
                        bSubdirectory = true;
                }
                else
                {
                    // Set file attributes
                    if (::SetFileAttributes(strFilePath.c_str(),
                        FILE_ATTRIBUTE_NORMAL) == FALSE)
                        return ::GetLastError();

                    // Delete file
                    if (::DeleteFile(strFilePath.c_str()) == FALSE)
                        return ::GetLastError();
                }
            }
        } while (::FindNextFile(hFile, &fdExistingFileData) == TRUE);

        // Close handle
        ::FindClose(hFile);

        DWORD dwError = ::GetLastError();
        if (dwError != ERROR_NO_MORE_FILES)
            return dwError;
        else
        {
            if (!bSubdirectory)
            {
                // Set directory attributes
                if (::SetFileAttributes(refRootDirectory.c_str(),
                    FILE_ATTRIBUTE_NORMAL) == FALSE)
                    return ::GetLastError();

                // Delete directory
                if (::RemoveDirectory(refRootDirectory.c_str()) == FALSE)
                    return ::GetLastError();
            }
        }
    }

    return 0;
}

// Return true if a feature will be uninstalled.
bool FeatureUninstalling(std::wstring featureName, MSIHANDLE hInstall)
{
    // This handles case that MsiGetFeatureState doesn't seem to handle in this installer
    // (works in 17.2). When user chooses REMOVE on Modify dialog, it should be the case that
    // nFeatureAction == INSTALLSTATE_ABSENT.
    // TODO: Find out why MsiGetFeatureState not working in Modify Remove workflow.
    LPWSTR sRemove = NULL;
    WcaGetProperty(L"REMOVE", &sRemove);
    if (wcscmp(sRemove, L"ALL") == 0)
    {
        return TRUE;
    }

    LPWSTR sMajorUpgrade = NULL;
    WcaGetProperty(L"WIX_UPGRADE_DETECTED", &sMajorUpgrade);
    INSTALLSTATE nFeatureState;
    INSTALLSTATE nFeatureAction;
    if (MsiGetFeatureState(hInstall, featureName.c_str(), &nFeatureState, &nFeatureAction) == ERROR_SUCCESS)
    {
        if (wcscmp(sMajorUpgrade, L"") != 0)
        { // Major Upgrade
            if (nFeatureAction != INSTALLSTATE_LOCAL)
            {
                return TRUE;
            }
        }
        else if (nFeatureAction == INSTALLSTATE_ABSENT)
        {
            return TRUE;
        }
    }
    return false;
}

// Deletes the settings and preferences for whatever apps are being uninstalled.
UINT __stdcall DeleteSettings(MSIHANDLE hInstall)
{
    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;

    hr = WcaInitialize(hInstall, "DeleteSettings");
    ExitOnFailure(hr, "Failed to initialize DeleteSettings.");

    WcaLog(LOGMSG_STANDARD, "Initialized.");

    LPWSTR sDeleteSettings = NULL;
    WcaGetProperty(L"DELETE_SETTINGS", &sDeleteSettings);

    if (wcscmp(sDeleteSettings, L"1") == 0)
    {
        char* userProfilePath = nullptr;
        size_t pathSize = 0;
        _dupenv_s(&userProfilePath, &pathSize, "USERPROFILE");
        std::wstring p4SettingsDir;
        std::wstring msg;

        if (FeatureUninstalling(L"P4EXP", hInstall))
        {
            // Read path to config file from registry, then delete file and registry value.
            WCHAR sConfigFileFullPath[MAX_PATH * 2];
            DWORD dBufferSize = sizeof(sConfigFileFullPath);
            HKEY hP4EXPKey = NULL;

            LONG lResult = RegOpenKeyEx(HKEY_CURRENT_USER, L"Software\\Perforce\\P4EXP", 0, KEY_SET_VALUE | KEY_READ, &hP4EXPKey); // Need KEY_SET_VALUE to be able to delete it.

            if (lResult == ERROR_SUCCESS)
            {
                WcaLog(LOGMSG_STANDARD, "Found registry key for value.");

                lResult = RegQueryValueEx(hP4EXPKey, L"UserConfigLocation", 0, 0, (LPBYTE)sConfigFileFullPath, &dBufferSize);

                if (lResult == ERROR_SUCCESS)
                {
                    WcaLog(LOGMSG_STANDARD, "Found path of settings file to delete.");
                    if (DeleteFile(sConfigFileFullPath))
                    {
                        WcaLog(LOGMSG_STANDARD, "Deleted settings file.");
                        lResult = RegDeleteValue(hP4EXPKey, L"UserConfigLocation");
                        if (lResult == ERROR_SUCCESS)
                        {
                            WcaLog(LOGMSG_STANDARD, "Deleted settings registry value.");
                        }
                    }
                }
            }
            RegCloseKey(hP4EXPKey);
        }

        if (FeatureUninstalling(L"P4V", hInstall))
        {
            p4SettingsDir.append(s2ws(userProfilePath));
            p4SettingsDir.append(L"\\.p4qt");
            if (DeleteDirectory(p4SettingsDir, hInstall) != 0)
            {
                // TODO: Make this a function.
                msg.append(L"Could not delete the settings folder: ");
                msg.append(p4SettingsDir);
                MessageBox(NULL, msg.c_str(), caption, MB_OK | MB_ICONERROR); // TODO: 17.2 did this, but what if running silently?
            }

            p4SettingsDir.clear();
            p4SettingsDir.append(s2ws(userProfilePath));
            p4SettingsDir.append(L"\\.p4ob");
            if (DeleteDirectory(p4SettingsDir, hInstall) != 0)
            {
                // TODO: Make this a function.
                msg.append(L"Could not delete the settings folder: ");
                msg.append(p4SettingsDir);
                MessageBox(NULL, msg.c_str(), caption, MB_OK | MB_ICONERROR); // TODO: 17.2 did this, but what if running silently?
            }
        }

        if (FeatureUninstalling(L"P4ADMIN", hInstall))
        {
            p4SettingsDir.clear();
            p4SettingsDir.append(s2ws(userProfilePath));
            p4SettingsDir.append(L"\\.p4admin");
            if (DeleteDirectory(p4SettingsDir, hInstall) != 0)
            {
                // TODO: Make this a function.
                msg.append(L"Could not delete the settings folder: ");
                msg.append(p4SettingsDir);
                MessageBox(NULL, msg.c_str(), caption, MB_OK | MB_ICONERROR); // TODO: 17.2 did this, but what if running silently?
            }
        }

        if (FeatureUninstalling(L"P4MERGE", hInstall))
        {
            p4SettingsDir.clear();
            p4SettingsDir.append(s2ws(userProfilePath));
            p4SettingsDir.append(L"\\.p4merge");
            if (DeleteDirectory(p4SettingsDir, hInstall) != 0)
            {
                // TODO: Make this a function.
                msg.append(L"Could not delete the settings folder: ");
                msg.append(p4SettingsDir);
                MessageBox(NULL, msg.c_str(), caption, MB_OK | MB_ICONERROR); // TODO: 17.2 did this, but what if running silently?
            }
        }
        free(userProfilePath);
    }

    return 0;

LExit:
    er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}