namespace Xilium.CefGlue
{
    using System;
    using Xilium.CefGlue.Interop;

    /// <summary>
    /// One plane of a Linux native pixmap (dmabuf) that an accelerated paint hands over.
    /// </summary>
    public sealed class CefAcceleratedPaintPlane
    {
        /// <summary>The number of bytes from one row of pixels to the next.</summary>
        public uint Stride { get; init; }

        /// <summary>The offset of the plane's first byte within its buffer.</summary>
        public ulong Offset { get; init; }

        /// <summary>The size of the plane in bytes.</summary>
        public ulong Size { get; init; }

        /// <summary>The file descriptor of the plane's dmabuf. It is valid only during the paint call.</summary>
        public int FileDescriptor { get; init; }
    }

    /// <summary>
    /// The GPU texture an accelerated paint hands over, and its frame metadata. The handles are valid only during
    /// the <see cref="CefRenderHandler"/> OnAcceleratedPaint call; a client that keeps the texture must copy or
    /// duplicate it there.
    /// </summary>
    public sealed class CefAcceleratedPaintInfo
    {
        /// <summary>
        /// The shared texture: a D3D11 NT handle on Windows and an IOSurfaceRef on macOS. Zero on Linux, which
        /// hands over <see cref="Planes"/> instead.
        /// </summary>
        public IntPtr SharedTextureHandle { get; init; }

        /// <summary>The native pixmap planes on Linux; empty on other platforms.</summary>
        public CefAcceleratedPaintPlane[] Planes { get; init; } = Array.Empty<CefAcceleratedPaintPlane>();

        /// <summary>The DRM format modifier of the Linux native pixmap; zero on other platforms.</summary>
        public ulong Modifier { get; init; }

        /// <summary>The pixel format of the texture.</summary>
        public CefColorType Format { get; init; }

        /// <summary>The frame's capture timestamp in microseconds.</summary>
        public ulong Timestamp { get; init; }

        /// <summary>The full size of the texture.</summary>
        public CefSize CodedSize { get; init; }

        /// <summary>The visible part of <see cref="CodedSize"/>.</summary>
        public CefRectangle VisibleRect { get; init; }

        /// <summary>The part of the texture that holds content.</summary>
        public CefRectangle ContentRect { get; init; }

        internal static unsafe CefAcceleratedPaintInfo FromNative(cef_accelerated_paint_info_t* ptr)
        {
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));

            switch (CefRuntime.Platform)
            {
                case CefRuntimePlatform.Windows:
                {
                    var info = (cef_accelerated_paint_info_t_windows*)ptr;
                    return Create(info->shared_texture_handle, Array.Empty<CefAcceleratedPaintPlane>(), 0, info->format, &info->extra);
                }

                case CefRuntimePlatform.MacOS:
                {
                    var info = (cef_accelerated_paint_info_t_mac*)ptr;
                    return Create(info->shared_texture_io_surface, Array.Empty<CefAcceleratedPaintPlane>(), 0, info->format, &info->extra);
                }

                case CefRuntimePlatform.Linux:
                {
                    var info = (cef_accelerated_paint_info_t_linux*)ptr;
                    var count = Math.Clamp(info->plane_count, 0, cef_accelerated_paint_info_t_linux.MaxPlanes);
                    var planes = new CefAcceleratedPaintPlane[count];
                    var first = &info->plane0;
                    for (var i = 0; i < count; i++)
                    {
                        planes[i] = new CefAcceleratedPaintPlane
                        {
                            Stride = first[i].stride,
                            Offset = first[i].offset,
                            Size = first[i].size,
                            FileDescriptor = first[i].fd,
                        };
                    }
                    return Create(IntPtr.Zero, planes, info->modifier, info->format, &info->extra);
                }

                default:
                    throw new PlatformNotSupportedException();
            }
        }

        private static unsafe CefAcceleratedPaintInfo Create(IntPtr handle, CefAcceleratedPaintPlane[] planes, ulong modifier,
            CefColorType format, cef_accelerated_paint_info_common_t* extra)
        {
            return new CefAcceleratedPaintInfo
            {
                SharedTextureHandle = handle,
                Planes = planes,
                Modifier = modifier,
                Format = format,
                Timestamp = extra->timestamp,
                CodedSize = new CefSize(extra->coded_size.width, extra->coded_size.height),
                VisibleRect = new CefRectangle(extra->visible_rect),
                ContentRect = new CefRectangle(extra->content_rect),
            };
        }
    }
}
