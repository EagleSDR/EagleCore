using EagleWeb.Common.Misc;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Components.FilterBuilder
{
    internal abstract class EagleFilterBuilderBase : EagleNativeObjectWrapper
    {
        protected EagleFilterBuilderBase(IntPtr ptr, float transitionWidth, float attenuation = 60) : base(ptr)
        {
            glue_builder_taps_auto(GetPtr(), transitionWidth, attenuation);
        }

        public IntPtr Ptr => GetPtr();

        public int TapCount
        {
            get => glue_builder_taps_getn(GetPtr());
            set => glue_builder_taps_setn(GetPtr(), value);
        }

        public int CalculateDecimation(out float outputRate)
        {
            return glue_builder_decimation_calc(GetPtr(), out outputRate);
        }

        protected override void DisposeInternal(IntPtr ptr)
        {
            glue_builder_destroy(ptr);
        }

        /* NATIVE */

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern int glue_builder_taps_getn(IntPtr ctx);

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern void glue_builder_taps_setn(IntPtr ctx, int taps);

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern void glue_builder_taps_auto(IntPtr ctx, float transitionWidth, float attenuation);

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern int glue_builder_decimation_calc(IntPtr ctx, out float outputRate);

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern void glue_builder_destroy(IntPtr ctx);
    }
}
