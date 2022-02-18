using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Components
{
    unsafe class EagleResampler : IDisposable
    {
        public EagleResampler(double minFactor, double maxFactor, bool highQuality) : this(CreateHandle(minFactor, maxFactor, highQuality))
        {
        }

        private EagleResampler(IntPtr handle)
        {
            this.handle = handle;
        }

        private IntPtr handle;

        /* API */

        public int FilterWidth => resample_get_filter_width(GetHandle());

        /// <summary>
        /// Calculates a resampling factor, given the input and output rate.
        /// </summary>
        /// <param name="inputSampleRate"></param>
        /// <param name="outputSampleRate"></param>
        /// <returns></returns>
        public static double CalculateFactor(double inputSampleRate, double outputSampleRate)
        {
            if (inputSampleRate == 0)
                throw new ArgumentOutOfRangeException("inputSampleRate");
            return outputSampleRate / inputSampleRate;
        }

        /// <summary>
        /// Reconfigures the demodulator. Expensive operation.
        /// </summary>
        /// <param name="minFactor"></param>
        /// <param name="maxFactor"></param>
        /// <param name="highQuality"></param>
        public void Reconfigure(double minFactor, double maxFactor, bool highQuality)
        {
            //Close the current handle
            Dispose();

            //Recreate handle
            handle = CreateHandle(minFactor, maxFactor, highQuality);
        }

        /// <summary>
        /// Resamples audio.
        /// </summary>
        /// <param name="factor">The resampling rate.</param>
        /// <param name="inBuffer">The input buffer.</param>
        /// <param name="inBufferLen">The number of samples in the input buffer.</param>
        /// <param name="inBufferUsed">The number of samples consumed in the input buffer. Don't count on this being inBufferLen.</param>
        /// <param name="outBuffer">The output buffer.</param>
        /// <param name="outBufferLen">The maximum size of the output buffer.</param>
        /// <param name="isLast">Set to true if this is the last call. Causes flushing to the output.</param>
        /// <returns></returns>
        public int Process(double factor, float* inBuffer, int inBufferLen, out int inBufferUsed, float* outBuffer, int outBufferLen, bool isLast = false)
        {
            int inBufferUsedTemp;
            int result = resample_process(
                GetHandle(),
                factor,
                inBuffer,
                inBufferLen,
                isLast ? 1 : 0,
                &inBufferUsedTemp,
                outBuffer,
                outBufferLen
            );
            inBufferUsed = inBufferUsedTemp;
            return result;
        }

        /// <summary>
        /// Resamples audio, handling recalling for you.
        /// </summary>
        /// <param name="factor">The resampling rate.</param>
        /// <param name="inBuffer">The input buffer.</param>
        /// <param name="inBufferLen">The number of input samples.</param>
        /// <param name="outBuffer">The output buffer.</param>
        /// <param name="outBufferLen">The maximum size of the output buffer.</param>
        /// <param name="isLast">Set to true if this is the last call. Causes flushing to the output.</param>
        /// <param name="result">Called for each output frame with the number of samples outputted. May be called as many times or as few times as needed.</param>
        public void Process(double factor, float* inBuffer, int inBufferLen, float* outBuffer, int outBufferLen, bool isLast, Action<int> result)
        {
            while (inBufferLen > 0)
            {
                //Process as usual
                int outputCount = Process(factor, inBuffer, inBufferLen, out int inBufferUsed, outBuffer, outBufferLen, isLast);

                //Run callback
                result(outputCount);

                //Advance input buffer
                inBuffer += inBufferUsed;
                inBufferLen -= inBufferUsed;
            }
        }

        /// <summary>
        /// Creates a deep clone of this resampler.
        /// </summary>
        /// <returns></returns>
        public EagleResampler Duplicate()
        {
            return new EagleResampler(resample_dup(GetHandle()));
        }

        /// <summary>
        /// Frees resampler resources.
        /// </summary>
        public void Dispose()
        {
            resample_close(GetHandle());
            handle = IntPtr.Zero;
        }

        /* UTILITIES */

        private static IntPtr CreateHandle(double minFactor, double maxFactor, bool highQuality)
        {
            return resample_open(highQuality ? 1 : 0, minFactor, maxFactor);
        }

        private IntPtr GetHandle()
        {
            if (handle == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().Name);
            return handle;
        }

        /* NATIVE */

        private const string RESAMPLE_DLL = "libresample";

        [DllImport(RESAMPLE_DLL)]
        private static extern IntPtr resample_open(int highQuality, double minFactor, double maxFactor);

        [DllImport(RESAMPLE_DLL)]
        private static extern IntPtr resample_dup(IntPtr handle);

        [DllImport(RESAMPLE_DLL)]
        private static extern int resample_get_filter_width(IntPtr handle);

        [DllImport(RESAMPLE_DLL)]
        private static extern int resample_process(IntPtr handle, double factor, float* inBuffer, int inBufferLen, int lastFlag, int* inBufferUsed, float* outBuffer, int outBufferLen);

        [DllImport(RESAMPLE_DLL)]
        private static extern void resample_close(IntPtr handle);
    }
}
