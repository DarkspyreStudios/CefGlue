//
// This file manually written from cef/include/internal/cef_types_component.h.
// C API names: cef_component_state_t, cef_component_update_priority_t, cef_component_update_error_t.
//
namespace Xilium.CefGlue
{
    /// <summary>
    /// The state of a component.
    /// </summary>
    public enum CefComponentState
    {
        New = 0,
        Checking = 1,
        CanUpdate = 2,
        Downloading = 3,
        Decompressing = 4,
        Patching = 5,
        Updating = 6,
        Updated = 7,
        UpToDate = 8,
        UpdateError = 9,
        Run = 10,
    }

    /// <summary>
    /// The priority of a component update request.
    /// </summary>
    public enum CefComponentUpdatePriority
    {
        Background = 0,
        Foreground = 1,
    }

    /// <summary>
    /// The result of a component update request.
    /// </summary>
    public enum CefComponentUpdateError
    {
        None = 0,
        UpdateInProgress = 1,
        UpdateCanceled = 2,
        RetryLater = 3,
        ServiceError = 4,
        UpdateCheckError = 5,
        CrxNotFound = 6,
        InvalidArgument = 7,
        BadCrxDataCallback = 8,
    }
}
