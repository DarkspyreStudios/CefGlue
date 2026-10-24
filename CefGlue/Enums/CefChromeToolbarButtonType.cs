//
// This file manually written from cef/include/internal/cef_types.h.
// C API name: cef_chrome_toolbar_button_type_t.
//
namespace Xilium.CefGlue
{
    using System;

    /// <summary>
    /// Chrome toolbar button types. Should be kept in sync with CEF's internal
    /// ToolbarButtonType type.
    /// </summary>
    public enum CefChromeToolbarButtonType
    {
        CastDeprecated = 0,
        DownloadDeprecated = 1,
        SendTabToSelfDeprecated = 2,
        SidePanelDeprecated = 3,
        Media = 4,
        TabSearchDeprecated = 5,
        BatterySaver = 6,
        Avatar = 7,
    }
}
