using EagleWeb.Common.Radio;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Native
{
    static unsafe class EagleNativeMethods
    {
        public const int DLL_CURRENT_VERSION = 1;
        private const string DLL_NAME = "libeagleradiocore";

        // Notifies user of an error in the radio.
        public delegate void eagleradio_error_cb(IntPtr user_ctx, string message);

        // Processes a read from the source, returning the number of read samples into buffer. Return -1 for error.
        public delegate int eagleradio_source_read_cb(IntPtr user_ctx, IntPtr source_ctx, EagleComplex* buffer, int count);

        // Configures a pipe
        public delegate void eaglesession_pipe_configure_cb(IntPtr user_ctx, int pipe_id, float sampleRate);

        // Pushes samples out of a pipe
        public delegate void eaglesession_pipe_push_cb(IntPtr user_ctx, int pipe_id, EagleComplex* buffer, int count);

        // Configures a demodulator and returns the sample rate. Return 0 to disable audio output. Return -1 to indicate an error.
        public delegate float eaglesession_demodulator_configure_cb(IntPtr user_ctx, IntPtr demodulator_ctx, float input_sample_rate, int buffer_size);

        // Processes a demodulator, returning the number of processed audio samples
        public delegate int eaglesession_demodulator_process_cb(IntPtr user_ctx, IntPtr demodulator_ctx, EagleComplex* iq, float* audioL, float* audioR, int count);

        // Pushes out audio samples
        public delegate void eaglesession_audio_out_cb(IntPtr user_ctx, EagleStereoPair* stereoSamples, int count);



        [DllImport(DLL_NAME)]
        public static extern int eagleradio_get_version();
        [DllImport(DLL_NAME)]
        public static extern IntPtr eagleradio_create(int bufferSize, IntPtr userCtx, eagleradio_source_read_cb cb_source_read, eagleradio_error_cb cb_error);
        [DllImport(DLL_NAME)]
        public static extern bool eagleradio_add_session(IntPtr radio, IntPtr session);
        [DllImport(DLL_NAME)]
        public static extern bool eagleradio_remove_session(IntPtr radio, IntPtr session);
        [DllImport(DLL_NAME)]
        public static extern void eagleradio_change_source(IntPtr radio, IntPtr source_ctx, float source_sample_rate);
        [DllImport(DLL_NAME)]
        public static extern void eagleradio_work(IntPtr radio);
        [DllImport(DLL_NAME)]
        public static extern void eagleradio_destroy(IntPtr radio);



        [DllImport(DLL_NAME)]
        public static extern IntPtr eaglesession_create(int buffer_size, IntPtr user_ctx, eaglesession_pipe_configure_cb pipe_configure_cb, eaglesession_pipe_push_cb pipe_push_cb, eaglesession_demodulator_configure_cb demodulator_configure_cb, eaglesession_demodulator_process_cb demodulator_process_cb);
        [DllImport(DLL_NAME)]
        public static extern void eaglesession_set_demodulator(IntPtr session, IntPtr demodulator_ctx);
        [DllImport(DLL_NAME)]
        public static extern void eaglesession_set_bandwidth(IntPtr session, float bandwidth);
        [DllImport(DLL_NAME)]
        public static extern void eaglesession_set_frequency_offset(IntPtr session, float frequency_offset);
        [DllImport(DLL_NAME)]
        public static extern bool eaglesession_output_create(IntPtr session, IntPtr audio_user_ctx, eaglesession_audio_out_cb callback, float output_rate);
        [DllImport(DLL_NAME)]
        public static extern void eaglesession_output_destroy(IntPtr session, IntPtr audio_user_ctx);
        [DllImport(DLL_NAME)]
        public static extern void eaglesession_destroy(IntPtr session);
    }
}
