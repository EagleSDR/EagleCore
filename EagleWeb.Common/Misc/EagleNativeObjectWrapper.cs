using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Common.Misc
{
    public abstract class EagleNativeObjectWrapper : IDisposable
    {
        public EagleNativeObjectWrapper(IntPtr ptr)
        {
            //Validate
            if (ptr == IntPtr.Zero)
                throw new Exception("Object was not properly constructed.");

            //Set
            this.ptr = ptr;

            //Create our own GCHandle
            handle = GCHandle.Alloc(this, GCHandleType.Normal);
        }

        private IntPtr ptr;
        private GCHandle handle;

        protected IntPtr GetManagedPtr()
        {
            return ((IntPtr)handle);
        }

        protected static T ResolveManagedPtr<T>(IntPtr handle)
        {
            return (T)GCHandle.FromIntPtr(handle).Target;
        }

        protected IntPtr GetPtr()
        {
            if (ptr == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().Name);
            return ptr;
        }

        protected abstract void DisposeInternal(IntPtr ptr);

        public void Dispose()
        {
            if (ptr != IntPtr.Zero)
            {
                DisposeInternal(ptr);
                ptr = IntPtr.Zero;
                handle.Free();
            }
        }
    }
}
