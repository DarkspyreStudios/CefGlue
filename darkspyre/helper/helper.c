// Darkspyre CEF subprocess helper for macOS and Linux.
//
// CEF starts this program for its renderer, GPU and utility processes; the browser process is the .NET
// Chromium service. The helper runs no .NET and no page-facing code of its own: it loads CEF and hands the
// process to CEF's subprocess entry point.
//
// macOS: the helper sits in each helper app bundle at
//   <...>/Chromium Embedded Framework.framework/../<Helper>.app/Contents/MacOS/<Helper>
// It initializes CEF's sandbox library (Libraries/libcef_sandbox.dylib inside the framework) before it
// loads the framework, as CEF's own cef_scoped_sandbox_context_mac.mm and cef_scoped_library_loader_mac.mm
// do. Both are found relative to the executable, three folders up.
//
// Linux: the helper sits in cef/helper/ and loads ../libcef.so. Chromium sandboxes the renderer itself.
//
// The API version is fixed at build time (CEF_API_VERSION) and must equal CefGlue's CEF_API_VERSION, so the
// helper and the browser process select the same CEF API.

#include <dlfcn.h>
#include <limits.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#if defined(__APPLE__)
#include <libgen.h>
#include <mach-o/dyld.h>
#else
#include <libgen.h>
#include <unistd.h>
#endif

#include "include/capi/cef_app_capi.h"
#include "include/cef_api_hash.h"

#if !defined(CEF_API_VERSION)
#error CEF_API_VERSION must be defined, such as -DCEF_API_VERSION=15600
#endif

typedef const char* (*api_hash_fn)(int version, int entry);
typedef int (*execute_process_fn)(const cef_main_args_t* args, cef_app_t* application, void* windows_sandbox_info);

// Writes the folder that holds this executable into dir.
static int executable_dir(char* dir, size_t size) {
    char path[PATH_MAX];
#if defined(__APPLE__)
    uint32_t length = sizeof(path);
    if (_NSGetExecutablePath(path, &length) != 0) {
        return 0;
    }
#else
    ssize_t length = readlink("/proc/self/exe", path, sizeof(path) - 1);
    if (length <= 0) {
        return 0;
    }
    path[length] = '\0';
#endif
    const char* parent = dirname(path);
    if (!parent || strlen(parent) >= size) {
        return 0;
    }
    strcpy(dir, parent);
    return 1;
}

static void* open_library(const char* dir, const char* relative) {
    char path[PATH_MAX];
    if (snprintf(path, sizeof(path), "%s/%s", dir, relative) >= (int)sizeof(path)) {
        fprintf(stderr, "darkspyre-cef-helper: path too long: %s/%s\n", dir, relative);
        return NULL;
    }
#if defined(__APPLE__)
    void* handle = dlopen(path, RTLD_LAZY | RTLD_LOCAL | RTLD_FIRST);
#else
    void* handle = dlopen(path, RTLD_NOW | RTLD_LOCAL);
#endif
    if (!handle) {
        fprintf(stderr, "darkspyre-cef-helper: dlopen %s: %s\n", path, dlerror());
    }
    return handle;
}

static void* symbol(void* handle, const char* name) {
    void* pointer = dlsym(handle, name);
    if (!pointer) {
        fprintf(stderr, "darkspyre-cef-helper: dlsym %s: %s\n", name, dlerror());
    }
    return pointer;
}

int main(int argc, char* argv[]) {
    char dir[PATH_MAX];
    if (!executable_dir(dir, sizeof(dir))) {
        fprintf(stderr, "darkspyre-cef-helper: cannot find the executable's folder\n");
        return 1;
    }

#if defined(__APPLE__)
    void* sandbox_library = open_library(dir, "../../../Chromium Embedded Framework.framework/Libraries/libcef_sandbox.dylib");
    if (!sandbox_library) {
        return 1;
    }
    void* (*sandbox_initialize)(int, char**) = (void* (*)(int, char**))symbol(sandbox_library, "cef_sandbox_initialize");
    void (*sandbox_destroy)(void*) = (void (*)(void*))symbol(sandbox_library, "cef_sandbox_destroy");
    if (!sandbox_initialize || !sandbox_destroy) {
        return 1;
    }
    void* sandbox_context = sandbox_initialize(argc, argv);
    if (!sandbox_context) {
        fprintf(stderr, "darkspyre-cef-helper: the sandbox did not initialize\n");
        return 1;
    }
    void* cef = open_library(dir, "../../../Chromium Embedded Framework.framework/Chromium Embedded Framework");
#else
    void* cef = open_library(dir, "../libcef.so");
#endif
    if (!cef) {
        return 1;
    }

    api_hash_fn api_hash = (api_hash_fn)symbol(cef, "cef_api_hash");
    execute_process_fn execute_process = (execute_process_fn)symbol(cef, "cef_execute_process");
    if (!api_hash || !execute_process) {
        return 1;
    }

    // The first cef_api_hash call selects the API version libcef serves.
    const char* hash = api_hash(CEF_API_VERSION, 0);
    if (!hash || strcmp(hash, CEF_API_HASH_PLATFORM) != 0) {
        fprintf(stderr, "darkspyre-cef-helper: libcef's API hash %s is not this helper's %s\n",
                hash ? hash : "(none)", CEF_API_HASH_PLATFORM);
        return 1;
    }

    cef_main_args_t args = {argc, argv};
    int exit_code = execute_process(&args, NULL, NULL);

#if defined(__APPLE__)
    sandbox_destroy(sandbox_context);
#endif
    return exit_code < 0 ? 1 : exit_code;
}
