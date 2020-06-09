using System;
using System.Runtime.InteropServices;

namespace GraphVizNet
{
    public static class GraphViz
    {
        private const string LibGvc = "gvc";
        private const string LibGraph = "cgraph";
        private const int Success = 0;

        public static byte[] LayoutAndRender(string source, string layout, string format)
        {
            var context = IntPtr.Zero;
            var graph = IntPtr.Zero;
            var renderedGraph = IntPtr.Zero;
            byte[] renderedGraphBytes;

            try
            {
                // Create a GraphViz context 
                context = gvContext();
                if (context == IntPtr.Zero)
                    throw new Exception("Failed to create GraphViz context.");

                // Load the data into a graph 
                graph = agmemread(source);
                if (graph == IntPtr.Zero)
                    throw new Exception("Failed to create graph from source. Check for syntax errors.");

                // Apply a layout 
                if (gvLayout(context, graph, layout) != Success)
                    throw new Exception("Layout failed.");

                // Render the graph 
                if (gvRenderData(context, graph, format, out renderedGraph, out var length) != Success)
                    throw new Exception("Render failed.");

                // Create an array to hold the rendered graph
                renderedGraphBytes = new byte[length];

                // Copy from the pointer 
                Marshal.Copy(renderedGraph, renderedGraphBytes, 0, length);
            }
            finally
            {
                // Free up the resources 
                if (context != IntPtr.Zero)
                {
                    if (graph != IntPtr.Zero)
                    {
                        gvFreeLayout(context, graph);
                        agclose(graph);
                    }

                    gvFreeContext(context);

                    if (renderedGraph != IntPtr.Zero)
                    {
                        gvFreeRenderData(renderedGraph);
                    }
                }
            }

            return renderedGraphBytes;
        }

#pragma warning disable IDE1006 // Naming Styles
        // ReSharper disable IdentifierTypo

        [DllImport(LibGvc, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gvContext();

        [DllImport(LibGraph, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr agmemread(string data);

        [DllImport(LibGvc, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gvRenderData(IntPtr gvc, IntPtr g, string format, out IntPtr result, out int length);

        [DllImport(LibGvc, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gvLayout(IntPtr gvc, IntPtr g, string engine);

        [DllImport(LibGvc, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gvFreeLayout(IntPtr gvc, IntPtr g);

        [DllImport(LibGvc, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gvFreeContext(IntPtr gvc);

        [DllImport(LibGraph, CallingConvention = CallingConvention.Cdecl)]
        private static extern int agclose(IntPtr g);

        [DllImport(LibGvc, CallingConvention = CallingConvention.Cdecl)]
        private static extern int gvFreeRenderData(IntPtr result);

        // ReSharper restore IdentifierTypo
#pragma warning restore IDE1006 // Naming Styles
    }
}