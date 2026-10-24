#!/usr/bin/env bash
# Regenerates the CefGlue interop from the CEF headers in include/ for one CEF API version.
# CEF_API_VERSION must be a stable version that include/cef_api_versions.h defines, such as 15600.
set -euo pipefail
cd "$(dirname "$0")"
CEF_API_VERSION=15600
/usr/bin/python3 -B cefglue_interop_gen.py --cpp-header-dir include --cefglue-dir ../CefGlue/ --api-version "$CEF_API_VERSION" --no-backup
