using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Components
{
    public unsafe class EagleUnsafeBuffer<T> : IDisposable where T : unmanaged
    {
        public EagleUnsafeBuffer(int count)
        {
            //Validate
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            //Set
            this.count = count;

            //Create
            buffer = new byte[count * sizeof(T)];
            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            valid = true;
        }

        private int count;
        private byte[] buffer;
        private GCHandle handle;
        private bool valid;

        public int Count => count;
        public T* Ptr
        {
            get
            {
                EnsureValid();
                return (T*)handle.AddrOfPinnedObject();
            }
        }

        private void EnsureValid()
        {
            if (!valid)
                throw new ObjectDisposedException("EagleUnsafeBuffer");
        }

        public void Dispose()
        {
            EnsureValid();
            handle.Free();
            valid = false;
        }
    }
}
