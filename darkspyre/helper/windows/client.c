// Darkspyre CEF client DLL for Windows.
//
// CEF 156 sandboxes Windows processes only through its bootstrap executables. The Chromium service ships
// bootstrapc.exe renamed darkspyre-chromium-service.exe; bootstrap loads the DLL of the same base name, this one,
// and calls RunConsoleMain with the sandbox information it created. bootstrapc.exe links chrome_elf.dll, so the
// executable and this DLL sit in the payload's cef\ folder beside libcef.dll; the .NET service sits one folder up.
// The same executable is CEF's browser process and every CEF subprocess:
//
//   - A CEF subprocess (an argument starting with --type=) loads libcef.dll from the executable's folder
//     (LIBCEF below), checks the API hash and hands the process to cef_execute_process with the sandbox
//     information.
//   - The browser process starts the .NET service (..\Darkspyre.Chromium.Service.dll) in this process through
//     hostfxr, appending --darkspyre-sandbox-info=<address>. The service passes that address to
//     CefRuntime.Initialize, which needs it in the process that calls CefInitialize.
//
// The API version is fixed at build time (CEF_API_VERSION) and must equal CefGlue's CEF_API_VERSION.

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shellapi.h>
#include <shlobj.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <wchar.h>

#include "include/capi/cef_app_capi.h"
#include "include/cef_api_hash.h"
#include "include/cef_sandbox_win.h"

#if !defined(CEF_API_VERSION)
#error CEF_API_VERSION must be defined, such as /DCEF_API_VERSION=15600
#endif

// Paths relative to the executable's folder (the payload's cef\ folder).
#define LIBCEF L"libcef.dll"
#define SERVICE_FOLDER L"..\\"
#define SERVICE_ASSEMBLY L"Darkspyre.Chromium.Service.dll"

typedef const char* (*api_hash_fn)(int version, int entry);
typedef int (*execute_process_fn)(const cef_main_args_t* args, cef_app_t* application, void* windows_sandbox_info);

// The subset of hostfxr.h this DLL uses (dotnet/runtime, MIT).
typedef void* hostfxr_handle;
typedef struct {
    size_t size;
    const wchar_t* host_path;
    const wchar_t* dotnet_root;
} hostfxr_initialize_parameters;
typedef int (*hostfxr_initialize_for_dotnet_command_line_fn)(int argc, const wchar_t** argv,
    const hostfxr_initialize_parameters* parameters, hostfxr_handle* host_context_handle);
typedef int (*hostfxr_run_app_fn)(const hostfxr_handle host_context_handle);
typedef int (*hostfxr_close_fn)(const hostfxr_handle host_context_handle);

static void fail(const wchar_t* message, const wchar_t* detail) {
    fwprintf(stderr, L"darkspyre-chromium-service: %ls%ls%ls\n", message, detail ? L": " : L"", detail ? detail : L"");
}

// Writes the folder holding the executable, with a trailing backslash, into dir.
static BOOL executable_dir(wchar_t* dir, DWORD size) {
    DWORD length = GetModuleFileNameW(NULL, dir, size);
    if (length == 0 || length >= size) {
        return FALSE;
    }
    wchar_t* slash = wcsrchr(dir, L'\\');
    if (!slash) {
        return FALSE;
    }
    slash[1] = L'\0';
    return TRUE;
}

static int run_subprocess(const wchar_t* dir, void* sandbox_info) {
    wchar_t path[MAX_PATH];
    if (swprintf(path, MAX_PATH, L"%ls%ls", dir, LIBCEF) < 0) {
        fail(L"path too long", dir);
        return 1;
    }
    HMODULE cef = LoadLibraryExW(path, NULL, LOAD_WITH_ALTERED_SEARCH_PATH);
    if (!cef) {
        fail(L"cannot load", path);
        return 1;
    }
    api_hash_fn api_hash = (api_hash_fn)GetProcAddress(cef, "cef_api_hash");
    execute_process_fn execute_process = (execute_process_fn)GetProcAddress(cef, "cef_execute_process");
    if (!api_hash || !execute_process) {
        fail(L"libcef.dll lacks the expected exports", path);
        return 1;
    }
    // The first cef_api_hash call selects the API version libcef serves.
    const char* hash = api_hash(CEF_API_VERSION, 0);
    if (!hash || strcmp(hash, CEF_API_HASH_PLATFORM) != 0) {
        fail(L"libcef.dll serves another API version", path);
        return 1;
    }
    cef_main_args_t args = {GetModuleHandleW(NULL)};
    int exit_code = execute_process(&args, NULL, sandbox_info);
    return exit_code < 0 ? 1 : exit_code;
}

#if defined(_M_ARM64)
#define DOTNET_ARCH L"arm64"
#else
#define DOTNET_ARCH L"x64"
#endif

// Finds the installed .NET root the way the .NET apphost does, without relying on environment variables the
// LocalServices launcher does not pass: DOTNET_ROOT when set (developer runs), else the InstallLocation the .NET
// installer records in the 32-bit registry view, else the Program Files known folder's dotnet folder.
static BOOL dotnet_root(wchar_t* base, DWORD size) {
    DWORD found = GetEnvironmentVariableW(L"DOTNET_ROOT", base, size);
    if (found > 0 && found < size) {
        return TRUE;
    }
    DWORD bytes = size * sizeof(wchar_t);
    if (RegGetValueW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\dotnet\\Setup\\InstalledVersions\\" DOTNET_ARCH, L"InstallLocation",
                     RRF_RT_REG_SZ | RRF_SUBKEY_WOW6432KEY, NULL, base, &bytes) == ERROR_SUCCESS && base[0] != L'\0') {
        size_t length = wcslen(base);
        if (length > 0 && base[length - 1] == L'\\') {
            base[length - 1] = L'\0';
        }
        return TRUE;
    }
    PWSTR program_files = NULL;
    if (SHGetKnownFolderPath(&FOLDERID_ProgramFiles, 0, NULL, &program_files) != S_OK) {
        CoTaskMemFree(program_files);
        return FALSE;
    }
    int written = swprintf(base, size, L"%ls\\dotnet", program_files);
    CoTaskMemFree(program_files);
    return written > 0;
}

// Finds hostfxr.dll: in the service folder for a self-contained service, else in the installed .NET root at
// host\fxr\<highest version>\. Writes the .NET root used into root.
static HMODULE load_hostfxr(const wchar_t* dir, wchar_t* root, size_t root_size) {
    wchar_t path[MAX_PATH];
    swprintf(path, MAX_PATH, L"%lshostfxr.dll", dir);
    if (GetFileAttributesW(path) != INVALID_FILE_ATTRIBUTES) {
        wcsncpy_s(root, root_size, dir, _TRUNCATE);
        return LoadLibraryW(path);
    }
    wchar_t base[MAX_PATH];
    if (!dotnet_root(base, MAX_PATH)) {
        return NULL;
    }
    wchar_t pattern[MAX_PATH];
    swprintf(pattern, MAX_PATH, L"%ls\\host\\fxr\\*", base);
    WIN32_FIND_DATAW entry;
    HANDLE search = FindFirstFileW(pattern, &entry);
    if (search == INVALID_HANDLE_VALUE) {
        return NULL;
    }
    // Pick the highest version folder by comparing dotted numeric parts.
    wchar_t best[MAX_PATH] = L"";
    int best_parts[4] = {-1, -1, -1, -1};
    do {
        if (!(entry.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) || entry.cFileName[0] == L'.') {
            continue;
        }
        int parts[4] = {0, 0, 0, 0};
        swscanf_s(entry.cFileName, L"%d.%d.%d.%d", &parts[0], &parts[1], &parts[2], &parts[3]);
        int better = 0;
        for (int i = 0; i < 4; i++) {
            if (parts[i] != best_parts[i]) {
                better = parts[i] > best_parts[i];
                break;
            }
        }
        if (better) {
            memcpy(best_parts, parts, sizeof(parts));
            wcsncpy_s(best, MAX_PATH, entry.cFileName, _TRUNCATE);
        }
    } while (FindNextFileW(search, &entry));
    FindClose(search);
    if (best[0] == L'\0') {
        return NULL;
    }
    swprintf(path, MAX_PATH, L"%ls\\host\\fxr\\%ls\\hostfxr.dll", base, best);
    wcsncpy_s(root, root_size, base, _TRUNCATE);
    return LoadLibraryW(path);
}

static int run_service(const wchar_t* dir, int argc, wchar_t** argv, void* sandbox_info) {
    wchar_t root[MAX_PATH];
    wchar_t service_dir[MAX_PATH];
    swprintf(service_dir, MAX_PATH, L"%ls%ls", dir, SERVICE_FOLDER);
    HMODULE fxr = load_hostfxr(service_dir, root, MAX_PATH);
    if (!fxr) {
        fail(L"cannot find hostfxr.dll for .NET", NULL);
        return 1;
    }
    hostfxr_initialize_for_dotnet_command_line_fn initialize =
        (hostfxr_initialize_for_dotnet_command_line_fn)GetProcAddress(fxr, "hostfxr_initialize_for_dotnet_command_line");
    hostfxr_run_app_fn run_app = (hostfxr_run_app_fn)GetProcAddress(fxr, "hostfxr_run_app");
    hostfxr_close_fn close = (hostfxr_close_fn)GetProcAddress(fxr, "hostfxr_close");
    if (!initialize || !run_app || !close) {
        fail(L"hostfxr.dll lacks the expected exports", NULL);
        return 1;
    }

    wchar_t assembly[MAX_PATH];
    wchar_t host[MAX_PATH];
    wchar_t sandbox_argument[64];
    swprintf(assembly, MAX_PATH, L"%ls%ls", service_dir, SERVICE_ASSEMBLY);
    GetModuleFileNameW(NULL, host, MAX_PATH);
    swprintf(sandbox_argument, 64, L"--darkspyre-sandbox-info=%llu", (unsigned long long)(ULONG_PTR)sandbox_info);

    // hostfxr's command line starts with the application assembly; the executable's own argv[0] is dropped.
    const wchar_t** host_argv = (const wchar_t**)calloc((size_t)argc + 2, sizeof(wchar_t*));
    if (!host_argv) {
        return 1;
    }
    int count = 0;
    host_argv[count++] = assembly;
    for (int i = 1; i < argc; i++) {
        host_argv[count++] = argv[i];
    }
    host_argv[count++] = sandbox_argument;

    hostfxr_initialize_parameters parameters = {sizeof(parameters), host, root};
    hostfxr_handle context = NULL;
    int rc = initialize(count, host_argv, &parameters, &context);
    if (rc != 0 || !context) {
        wchar_t code[16];
        swprintf(code, 16, L"0x%08x", rc);
        fail(L"hostfxr could not start the service", code);
        free(host_argv);
        return 1;
    }
    rc = run_app(context);
    close(context);
    free(host_argv);
    return rc;
}

CEF_BOOTSTRAP_EXPORT int RunConsoleMain(int argc, char* argv[], void* sandbox_info, cef_version_info_t* version_info) {
    (void)argc;
    (void)argv;
    (void)version_info;
    wchar_t dir[MAX_PATH];
    if (!executable_dir(dir, MAX_PATH)) {
        fail(L"cannot find the executable's folder", NULL);
        return 1;
    }
    // Use the wide command line: argv is in the console code page and cannot carry every path.
    int wide_argc = 0;
    wchar_t** wide_argv = CommandLineToArgvW(GetCommandLineW(), &wide_argc);
    if (!wide_argv) {
        return 1;
    }
    int subprocess = 0;
    for (int i = 1; i < wide_argc; i++) {
        if (wcsncmp(wide_argv[i], L"--type=", 7) == 0) {
            subprocess = 1;
            break;
        }
    }
    int rc = subprocess ? run_subprocess(dir, sandbox_info) : run_service(dir, wide_argc, wide_argv, sandbox_info);
    LocalFree(wide_argv);
    return rc;
}
