using EagleWeb.Common.Misc;
using EagleWeb.Common.Radio;
using EagleWeb.Core.Radio.Components.FilterBuilder;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Components.Filters
{
    internal class EagleFilterFCC : EagleNativeObjectWrapper
    {
        public EagleFilterFCC() : base(glue_filter_fcc_create())
        {
        }

        public void Configure(EagleFilterBuilderBase filter, int decimation)
        {
            if (!glue_filter_fcc_configure(GetPtr(), filter.Ptr, decimation))
                throw new Exception("Filter parameters are invalid.");
        }

        public unsafe int Process(float* input, EagleComplex* output, int count)
        {
            return glue_filter_fcc_process(GetPtr(), input, output, count);
        }

        protected override void DisposeInternal(IntPtr ptr)
        {
            glue_filter_fcc_destroy(ptr);
        }

        /* NATIVE */

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern IntPtr glue_filter_fcc_create();

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern bool glue_filter_fcc_configure(IntPtr ctx, IntPtr builder, int decimation);

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern unsafe int glue_filter_fcc_process(IntPtr ctx, float* input, EagleComplex* output, int count);

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern unsafe void glue_filter_fcc_destroy(IntPtr ctx);
    }
}
