﻿//
// This file manually written from cef/include/internal/cef_types.h.
// C API name: cef_media_route_connection_state_t.
//
namespace Xilium.CefGlue
{
    /// <summary>
    /// Connection state for a MediaRoute object.
    /// </summary>
    public enum CefMediaRouteConnectionState
    {
        Unknown = -1,
        Connecting = 0,
        Connected = 1,
        Closed = 2,
        Terminated = 3,
    }
}
