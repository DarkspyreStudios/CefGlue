//
// This file manually written from cef/include/internal/cef_types.h.
// C API name: cef_task_type_t.
//
namespace Xilium.CefGlue
{
    /// <summary>
    /// The kind of process a task manager task runs in.
    /// </summary>
    public enum CefTaskType
    {
        Unknown,
        Browser,
        Gpu,
        Zygote,
        Utility,
        Renderer,
        Extension,
        Guest,
        // CEF 140 and later no longer report plugin tasks; the value keeps the numbering.
        PluginDeprecated,
        SandboxHelper,
        DedicatedWorker,
        SharedWorker,
        ServiceWorker,
    }
}
