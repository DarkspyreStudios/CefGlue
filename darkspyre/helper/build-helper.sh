#!/usr/bin/env bash
# Compiles the native CEF subprocess helper (helper.c) for one runtime identifier.
#
# Usage: darkspyre/helper/build-helper.sh <rid> <cef distribution folder> <output folder>
#   <rid>   osx-arm64, osx-x64, linux-x64 or linux-arm64. macOS builds run on a Mac; a Linux build runs on
#           a Linux host of the same architecture.
#   <cef distribution folder>  an extracted CEF minimal distribution of the version the fork binds; the
#           helper compiles against its include/ folder.
#   <output folder>  receives <rid>/darkspyre-cef-helper, the layout build-runtime.sh --helpers expects.
#
# The API version comes from CEF_API_VERSION in CefGlue.Interop.Gen/gen-cef3.sh, so the helper and CefGlue
# select the same CEF API. Signing is the app build's job. Windows has no helper here: CEF 156 sandboxes
# Windows subprocesses only through its bootstrap.exe and a client DLL.
set -euo pipefail

[ $# -eq 3 ] || { sed -n '2,13p' "$0" | sed 's/^# \{0,1\}//' >&2; exit 2; }
rid="$1"
dist="$(cd "$2" && pwd)"
out="$3"
here="$(cd "$(dirname "$0")" && pwd)"

api_version="$(sed -n 's/^CEF_API_VERSION=\([0-9]*\)$/\1/p' "$here/../../CefGlue.Interop.Gen/gen-cef3.sh")"
[ -n "$api_version" ] || { echo "CEF_API_VERSION not found in gen-cef3.sh." >&2; exit 1; }
[ -f "$dist/include/capi/cef_app_capi.h" ] || { echo "$dist is not a CEF distribution." >&2; exit 1; }

mkdir -p "$out/$rid"
binary="$out/$rid/darkspyre-cef-helper"
flags=(-std=gnu11 -O2 -Wall -Werror -fstack-protector -DCEF_API_VERSION="$api_version" -I"$dist")

case "$rid" in
    osx-arm64) clang "${flags[@]}" -arch arm64 -mmacosx-version-min=13.0 -o "$binary" "$here/helper.c" ;;
    osx-x64) clang "${flags[@]}" -arch x86_64 -mmacosx-version-min=13.0 -o "$binary" "$here/helper.c" ;;
    linux-x64|linux-arm64) cc "${flags[@]}" -fPIE -pie -o "$binary" "$here/helper.c" -ldl ;;
    *) echo "No helper for $rid." >&2; exit 2 ;;
esac

echo "Built $binary"
