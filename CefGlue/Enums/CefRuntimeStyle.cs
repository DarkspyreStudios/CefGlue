//
// This file manually written from cef/include/internal/cef_types_runtime.h.
// C API name: cef_runtime_style_t.
//
namespace Xilium.CefGlue
{
    /// <summary>
    /// CEF supports both a Chrome runtime style (based on the Chrome UI layer) and
    /// an Alloy runtime style (based on the Chromium content layer). The style is
    /// configured for each window or browser at creation time. Windowless rendering
    /// always uses Alloy style.
    /// </summary>
    public enum CefRuntimeStyle
    {
        /// <summary>
        /// Use the default style.
        /// </summary>
        Default,

        /// <summary>
        /// Use Chrome style.
        /// </summary>
        Chrome,

        /// <summary>
        /// Use Alloy style.
        /// </summary>
        Alloy,
    }
}
