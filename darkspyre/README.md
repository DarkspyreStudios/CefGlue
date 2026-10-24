# Darkspyre CefGlue fork

This fork of OutSystems/CefGlue supplies the CEF bindings for Darkspyre's Chromium browser plugin
(AgentDSPlugins `src/Chromium/`). Darkspyre uses only `CefGlue/` (`Xilium.CefGlue.dll`) and its generator,
`CefGlue.Interop.Gen/`. The browser process, the Avalonia control and the browser interface are Darkspyre's own.

## Branches and changes

- `main` follows upstream. Upstream commits are merged from `main` into `darkspyre`.
- `darkspyre` holds Darkspyre's changes. It changes only what Darkspyre's interface needs, so upstream
  merges stay small.
- The upstream Avalonia, WPF, Common, BrowserProcess, demo and test projects are left as upstream has them.
  They are not built on this branch and do not compile against the current bindings.
- Compiler warnings in the fork are not fixed. The Darkspyre repository laws apply to Darkspyre's own
  projects, not to this fork.

## Upgrading CEF (the source step)

The bindings target one CEF version and one CEF API version. An upgrade is a commit on `darkspyre`:

1. Replace `CefGlue.Interop.Gen/include/` with the new CEF distribution's `include/` folder. The distribution
   for one platform lacks the other platforms' `internal/` headers (`cef_types_win.h`, `cef_win.h`,
   `cef_app_win.h`, `cef_sandbox_win.h`, `cef_types_linux.h`, `cef_linux.h`); take those from CEF's source at
   the same commit.
2. Set `CEF_API_VERSION` in `gen-cef3.sh` and `--api-version` in `gen-cef3.cmd` to a stable version that
   `include/cef_api_versions.h` defines.
3. Run `bash CefGlue.Interop.Gen/gen-cef3.sh`. The generator writes the vtable structs, proxies, handlers and
   `Interop/version.g.cs` (the CEF version and the API hashes for each platform).
4. Port the hand-written types to the new headers: `CefGlue/Interop/Structs/` must match the C structs in
   `include/internal/` field for field (a struct that starts with `size_t size` carries a leading
   `UIntPtr size` that CefGlue sets wherever it fills the struct for CEF), `CefGlue/Enums/` must have the C
   enums' members and values, and the proxies and handlers must follow changed method signatures.
5. Build `CefGlue/CefGlue.csproj` and commit the headers, the generated code and the ported types together.

`CefRuntime` passes `CEF_API_VERSION` to CEF's first `cef_api_hash` call, which selects the API version
libcef serves.

## Building the runtime artifacts

```bash
darkspyre/build-runtime.sh <cefVersion> --helpers <dir>
```

`<cefVersion>` is the full CEF version, such as `156.0.14+g4fa4ab5+chromium-156.0.8078.48`, and must equal
`CEF_VERSION` in the committed `include/cef_version.h`. `--helpers` names a folder with the built native
helpers, one subfolder per runtime identifier. `--no-helper` builds zips without helpers for local testing;
their names end in `-nohelper` and they are never published. Output goes to `out/runtime/`.

The script requires a clean working tree. It regenerates the bindings and stops if the committed ones
differ from the generator's output. It builds `Xilium.CefGlue.dll` for `net11.0`, downloads the six CEF
minimal distributions from cef-builds.spotifycdn.com (verifying each SHA-1 from CEF's index), and writes
one zip per runtime identifier:

| Path in the zip | Contents |
| --- | --- |
| `cef/` | The distribution's `Release/` and `Resources/` files, without import libraries and debug symbols |
| `cef/helper/` | The native sandboxed helper for that runtime identifier |
| `cefglue/Xilium.CefGlue.dll` | The bindings built from this commit |
| `licenses/` | `CEF-LICENSE.txt`, `CREDITS.html` (Chromium) and `CefGlue-LICENSE` |

The runtime identifiers are `osx-arm64`, `osx-x64`, `win-x64`, `win-arm64`, `linux-x64` and `linux-arm64`.

## Versions and publication

A release is named `cef-<CEF version>-cg<7-character fork commit>`, such as `cef-156.0.14-cg3839390`. Its
zips are named `darkspyre-cef-<CEF version>-<rid>.zip`. The script writes `out/runtime/cef-runtime.json`
with the full CEF version, the fork commit, the release name and each zip's SHA-256; that file becomes
AgentDSPlugins `src/Chromium/cef-runtime.json`. The zips are published as that release on
DarkspyreStudios/darkspyre-releases. A published release is never replaced; a new fork commit gives a new
release name.
