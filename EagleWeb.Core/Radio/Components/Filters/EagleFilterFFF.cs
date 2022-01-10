using EagleWeb.Common.Misc;
using EagleWeb.Core.Radio.Components.FilterBuilder;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Components.Filters
{
    internal class EagleFilterFFF : EagleNativeObjectWrapper
    {
        public EagleFilterFFF() : base(glue_filter_fff_create())
        {
        }

        public void Configure(EagleFilterBuilderBase filter, int decimation)
        {
            if (!glue_filter_fff_configure(GetPtr(), filter.Ptr, decimation))
                throw new Exception("Filter parameters are invalid.");
        }

        public unsafe int Process(float* input, float* output, int count)
        {
            return glue_filter_fff_process(GetPtr(), input, output, count);
        }

        protected override void DisposeInternal(IntPtr ptr)
        {
            glue_filter_fff_destroy(ptr);
        }

        /* NATIVE */

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern IntPtr glue_filter_fff_create();

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern bool glue_filter_fff_configure(IntPtr ctx, IntPtr builder, int decimation);

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern unsafe int glue_filter_fff_process(IntPtr ctx, float* input, float* output, int count);

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern unsafe void glue_filter_fff_destroy(IntPtr ctx);
    }
}
