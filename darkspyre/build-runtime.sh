#!/usr/bin/env bash
# Builds the Darkspyre CEF runtime artifacts: one zip per runtime identifier, holding
#   cef/                    the CEF minimal Release distribution (libraries, macOS framework, resources, locales)
#   cef/helper/             the native sandboxed helper for that RID (darkspyre/helper/)
#   cefglue/Xilium.CefGlue.dll  built from this commit for net11.0
#   licenses/               CEF LICENSE.txt, Chromium CREDITS.html, CefGlue LICENSE
#
# Usage: darkspyre/build-runtime.sh <cefVersion> (--helpers <dir> | --no-helper) [--out <dir>]
#   <cefVersion>   the full CEF version, such as 156.0.14+g4fa4ab5+chromium-156.0.8078.48. It must equal
#                  CEF_VERSION in CefGlue.Interop.Gen/include/cef_version.h.
#   --helpers DIR  the built helpers, one folder per RID (DIR/osx-arm64/, DIR/win-x64/, ...).
#   --no-helper    builds without helpers, for local testing only. Each zip name ends in -nohelper; these
#                  zips are never published.
#   --out DIR      the output folder; default out/runtime in this repository (ignored by git).
#
# The script builds only what this commit holds. The bindings must equal the generator's output for the
# committed headers; the script regenerates them and stops if the working tree changes. The working tree
# must be clean, because the release name carries the commit.
set -euo pipefail

usage() { sed -n '2,19p' "$0" | sed 's/^# \{0,1\}//' >&2; exit 2; }

repo="$(cd "$(dirname "$0")/.." && pwd)"
cef_version=""
helpers=""
no_helper=0
out="$repo/out/runtime"

while [ $# -gt 0 ]; do
    case "$1" in
        --helpers) helpers="${2:?--helpers needs a folder}"; shift 2 ;;
        --no-helper) no_helper=1; shift ;;
        --out) out="${2:?--out needs a folder}"; shift 2 ;;
        -h|--help) usage ;;
        -*) echo "Unknown option: $1" >&2; usage ;;
        *) [ -z "$cef_version" ] || usage; cef_version="$1"; shift ;;
    esac
done
[ -n "$cef_version" ] || usage
if [ -n "$helpers" ] && [ "$no_helper" = 1 ]; then echo "Give --helpers or --no-helper, not both." >&2; exit 2; fi
if [ -z "$helpers" ] && [ "$no_helper" = 0 ]; then echo "Give --helpers <dir>, or --no-helper for a local test build." >&2; exit 2; fi
if [ -n "$helpers" ]; then helpers="$(cd "$helpers" && pwd)"; fi

for tool in curl jq tar shasum dotnet git; do
    command -v "$tool" >/dev/null || { echo "Missing tool: $tool" >&2; exit 1; }
done

# Writes the zip of the current folder to $1, keeping file modes and symlinks. zip is preferred; 7z and
# Python's zipfile serve hosts without it. Python follows symlinks, which only matters if a future CEF
# distribution ships one (the 156 minimal distributions have none).
zip_folder() {
    if command -v zip >/dev/null; then
        zip -qry -X "$1" .
    elif command -v 7z >/dev/null; then
        7z a -tzip -snl -bso0 -bsp0 "$1" .
    elif command -v python3 >/dev/null; then
        python3 -I -c 'import os, sys, zipfile
with zipfile.ZipFile(sys.argv[1], "w", zipfile.ZIP_DEFLATED) as z:
    for root, dirs, files in os.walk("."):
        dirs.sort()
        for name in sorted(files):
            path = os.path.join(root, name)
            z.write(path, os.path.relpath(path, "."))' "$1"
    else
        echo "Missing a zip tool: zip, 7z or python3." >&2
        return 1
    fi
}

# The Linux distributions ship libcef.so with debug symbols (1.4 GB for x64); the zip carries it stripped.
elf_strip=""
for candidate in llvm-strip /opt/homebrew/opt/llvm/bin/llvm-strip /usr/local/opt/llvm/bin/llvm-strip; do
    if command -v "$candidate" >/dev/null; then elf_strip="$candidate"; break; fi
done
if [ -z "$elf_strip" ] && [ "$(uname -s)" = Linux ] && command -v strip >/dev/null; then elf_strip=strip; fi
[ -n "$elf_strip" ] || { echo "Missing an ELF strip tool: run on a Linux host (the runtime is built on maelstrom)." >&2; exit 1; }

# CEF platform name and .NET runtime identifier, in pairs.
platforms=(macosarm64 osx-arm64 macosx64 osx-x64 windows64 win-x64 windowsarm64 win-arm64 linux64 linux-x64 linuxarm64 linux-arm64)

cd "$repo"

header_version="$(sed -n 's/^#define CEF_VERSION "\(.*\)"$/\1/p' CefGlue.Interop.Gen/include/cef_version.h)"
if [ "$header_version" != "$cef_version" ]; then
    echo "This commit's headers are CEF $header_version, not $cef_version." >&2
    exit 1
fi

if [ -n "$(git status --porcelain)" ]; then
    echo "The working tree has changes; commit them first so the release names a commit." >&2
    exit 1
fi
commit="$(git rev-parse --short=7 HEAD)"
short_version="${cef_version%%+*}"
release="cef-$short_version-cg$commit"

echo "Checking the committed bindings against the generator"
bash CefGlue.Interop.Gen/gen-cef3.sh >/dev/null
if [ -n "$(git status --porcelain)" ]; then
    git status --short >&2
    git checkout -- . && git clean -fdq -- CefGlue
    echo "The committed bindings differ from the generator's output for the committed headers." >&2
    exit 1
fi

echo "Building Xilium.CefGlue.dll"
build="$out/cefglue"
rm -rf "$build"
dotnet build CefGlue/CefGlue.csproj --configuration Release -p:DotnetVersion=net11.0 -p:Version="$short_version" \
    --output "$build" -nologo -v:quiet -clp:ErrorsOnly
[ -f "$build/Xilium.CefGlue.dll" ] || { echo "The build produced no Xilium.CefGlue.dll." >&2; exit 1; }

downloads="$out/downloads"
mkdir -p "$downloads"
index="$downloads/index.json"
curl -fsSL https://cef-builds.spotifycdn.com/index.json -o "$index"

assets='{}'
i=0
while [ $i -lt ${#platforms[@]} ]; do
    cef_platform="${platforms[$i]}"
    rid="${platforms[$((i + 1))]}"
    i=$((i + 2))

    file="$(jq -r --arg p "$cef_platform" --arg v "$cef_version" \
        '.[$p].versions[] | select(.cef_version == $v) | .files[] | select(.type == "minimal") | .name' "$index")"
    sha1="$(jq -r --arg p "$cef_platform" --arg v "$cef_version" \
        '.[$p].versions[] | select(.cef_version == $v) | .files[] | select(.type == "minimal") | .sha1' "$index")"
    [ -n "$file" ] || { echo "CEF $cef_version has no minimal build for $cef_platform." >&2; exit 1; }

    archive="$downloads/$file"
    if [ ! -f "$archive" ] || [ "$(shasum -a 1 "$archive" | cut -d' ' -f1)" != "$sha1" ]; then
        echo "Downloading $file"
        curl -fsSL "https://cef-builds.spotifycdn.com/$(jq -rn --arg f "$file" '$f | @uri')" -o "$archive"
    fi
    [ "$(shasum -a 1 "$archive" | cut -d' ' -f1)" = "$sha1" ] || { echo "SHA-1 mismatch for $file." >&2; exit 1; }

    echo "Packing $rid"
    work="$out/work/$rid"
    rm -rf "$work"
    mkdir -p "$work/extract" "$work/stage/cef" "$work/stage/cefglue" "$work/stage/licenses"
    tar -xjf "$archive" -C "$work/extract"
    dist="$work/extract/${file%.tar.bz2}"

    # Import and static libraries and debug symbols are build inputs, not runtime files.
    (cd "$dist/Release" && find . -type f ! -name '*.lib' ! -name '*.pdb' -print0 | cpio -0pdm --quiet "$work/stage/cef")
    if [ -d "$dist/Resources" ]; then
        (cd "$dist/Resources" && find . -print0 | cpio -0pdm --quiet "$work/stage/cef")
    fi
    if [ -f "$work/stage/cef/libcef.so" ]; then
        "$elf_strip" --strip-unneeded "$work/stage/cef/libcef.so"
        # The browser never uses the setuid sandbox; Linux sandboxing uses user namespaces (an AppArmor
        # profile where the distribution restricts them).
        rm -f "$work/stage/cef/chrome-sandbox"
    fi
    if [ -n "$helpers" ]; then
        [ -d "$helpers/$rid" ] || { echo "No helper for $rid in $helpers." >&2; exit 1; }
        mkdir -p "$work/stage/cef/helper"
        cp -R "$helpers/$rid/." "$work/stage/cef/helper/"
    fi
    cp "$build/Xilium.CefGlue.dll" "$work/stage/cefglue/"
    cp "$dist/LICENSE.txt" "$work/stage/licenses/CEF-LICENSE.txt"
    cp "$dist/CREDITS.html" "$work/stage/licenses/CREDITS.html"
    cp "$repo/LICENSE" "$work/stage/licenses/CefGlue-LICENSE"

    suffix=""
    [ "$no_helper" = 1 ] && suffix="-nohelper"
    zip_name="darkspyre-cef-$short_version-$rid$suffix.zip"
    rm -f "$out/$zip_name"
    (cd "$work/stage" && zip_folder "$out/$zip_name")
    sha256="$(shasum -a 256 "$out/$zip_name" | cut -d' ' -f1)"
    assets="$(jq --arg r "$rid" --arg f "$zip_name" --arg s "$sha256" '. + {($r): {file: $f, sha256: $s}}' <<<"$assets")"
    rm -rf "$work"
done
rm -rf "$out/work"

jq -n --arg v "$cef_version" --arg c "$(git rev-parse HEAD)" --arg r "$release" --argjson a "$assets" \
    '{cefVersion: $v, cefGlueCommit: $c, release: $r, assets: $a}' > "$out/cef-runtime.json"

echo "Wrote $out/cef-runtime.json and six zips for release $release"
