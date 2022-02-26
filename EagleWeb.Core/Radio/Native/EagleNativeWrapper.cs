using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Native
{
    class EagleNativeWrapper<T>
    {
        public EagleNativeWrapper()
        {

        }

        private GCHandle gc;
        private T item;

        public T Object
        {
            get => item;
            set
            {
                //Free old handle, if any
                if (item != null)
                    gc.Free();

                //Set item
                item = value;

                //Open GC on the current item, if any
                if (item != null)
                    gc = GCHandle.Alloc(item);
            }
        }

        public IntPtr Handle
        {
            get
            {
                if (item == null)
                    return IntPtr.Zero;
                else
                    return (IntPtr)gc;
            }
        }

        public void Clear()
        {
            Object = default(T);
        }

        public static T FromHandle(IntPtr handle)
        {
            return (T)GCHandle.FromIntPtr(handle).Target;
        }
    }
}
