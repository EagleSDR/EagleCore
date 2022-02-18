using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Radio.Components
{
    class EagleMultichannelResampler : IDisposable
    {
        public EagleMultichannelResampler(int channelCount, double minFactor, double maxFactor, bool highQuality)
        {
            //Verify the number of resamples
            if (channelCount < 1)
                throw new ArgumentOutOfRangeException("channelCount");

            //Create the resamplers buffer
            resamplers = new EagleResampler[channelCount];

            //Set up
            Reconfigure(minFactor, maxFactor, highQuality);
        }

        private EagleResampler[] resamplers;

        /// <summary>
        /// Reconfigures the demodulator. Expensive operation.
        /// </summary>
        /// <param name="minFactor"></param>
        /// <param name="maxFactor"></param>
        /// <param name="highQuality"></param>
        public void Reconfigure(double minFactor, double maxFactor, bool highQuality)
        {
            //Dispose of all existing handles
            Dispose();

            //Create the first resampler
            resamplers[0] = new EagleResampler(minFactor, maxFactor, highQuality);

            //Copy this resampler to the other slots
            for (int i = 1; i < resamplers.Length; i++)
                resamplers[i] = resamplers[0].Duplicate();
        }

        /// <summary>
        /// Resamples audio.
        /// </summary>
        /// <param name="factor">The resampling rate.</param>
        /// <param name="inBuffers">The input buffers.</param>
        /// <param name="inBufferLen">The number of samples in the input buffer.</param>
        /// <param name="inBufferUsed">The number of samples consumed in the input buffer. Don't count on this being inBufferLen.</param>
        /// <param name="outBuffers">The output buffers.</param>
        /// <param name="outBufferLen">The maximum size of the output buffer.</param>
        /// <param name="isLast">Set to true if this is the last call. Causes flushing to the output.</param>
        /// <returns></returns>
        public unsafe int Process(double factor, float*[] inBuffers, int inBufferLen, out int inBufferUsed, float*[] outBuffers, int outBufferLen, bool isLast = false)
        {
            return ProcessInternal(factor, inBuffers, inBufferLen, out inBufferUsed, outBuffers, outBufferLen, isLast, 0);
        }

        /// <summary>
        /// Resamples audio, handling recalling for you.
        /// </summary>
        /// <param name="factor">The resampling rate.</param>
        /// <param name="inBuffers">The input buffer.</param>
        /// <param name="inBufferLen">The number of input samples.</param>
        /// <param name="outBuffers">The output buffer.</param>
        /// <param name="outBufferLen">The maximum size of the output buffer.</param>
        /// <param name="isLast">Set to true if this is the last call. Causes flushing to the output.</param>
        /// <param name="result">Called for each output frame with the number of samples outputted. May be called as many times or as few times as needed.</param>
        public unsafe void Process(double factor, float*[] inBuffers, int inBufferLen, float*[] outBuffers, int outBufferLen, bool isLast, Action<int> result)
        {
            int offset = 0;
            while (inBufferLen > 0)
            {
                //Process as usual
                int outputCount = ProcessInternal(factor, inBuffers, inBufferLen, out int inBufferUsed, outBuffers, outBufferLen, isLast, offset);

                //Run callback
                result(outputCount);

                //Advance input buffer
                offset += inBufferUsed;
                inBufferLen -= inBufferUsed;
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < resamplers.Length; i++)
            {
                if (resamplers[i] != null)
                    resamplers[i].Dispose();
                resamplers[i] = null;
            }
        }

        private unsafe int ProcessInternal(double factor, float*[] inBuffers, int inBufferLen, out int inBufferUsed, float*[] outBuffers, int outBufferLen, bool isLast, int inputOffset)
        {
            //Sanity check
            if (inBuffers.Length != resamplers.Length || outBuffers.Length != resamplers.Length)
                throw new Exception("An incorrect number of buffers were passed into a multichannel resampler.");

            //Compute on first
            int result = resamplers[0].Process(factor, inBuffers[0] + inputOffset, inBufferLen, out inBufferUsed, outBuffers[0], outBufferLen, isLast);

            //Compute on the other ones, ensuring consistent results
            for (int i = 1; i < resamplers.Length; i++)
            {
                EnsureConsistency(result, resamplers[i].Process(factor, inBuffers[i] + inputOffset, inBufferLen, out int inBufferUsedTemp, outBuffers[i], outBufferLen, isLast));
                EnsureConsistency(inBufferUsed, inBufferUsedTemp);
            }

            return result;
        }

        private static void EnsureConsistency(int a, int b)
        {
            if (a != b)
                throw new Exception("Inconsistent results from each channel! This may be caused by memory corruption.");
        }
    }
}
