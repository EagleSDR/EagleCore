using EagleWeb.Common.Auth;
using EagleWeb.Common.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EagleWeb.Core.Web.FileSystem
{
    class WebFsFileStreamImpl : WebFsFileStream
    {
        public WebFsFileStreamImpl(WebFsManager context, string token, string filename, IEagleAccount account, bool writing)
        {
            //Set
            this.context = context;
            this.filename = filename;
            this.token = token;
            this.account = account;
            this.writing = writing;

            //Open underlying stream
            underlying = new FileStream(filename, writing ? FileMode.Create : FileMode.Open, writing ? FileAccess.ReadWrite : FileAccess.Read);
        }

        private readonly WebFsManager context;
        private readonly string filename;
        private readonly string token;
        private readonly IEagleAccount account;
        private readonly bool writing;
        private readonly FileStream underlying;

        private bool disposed = false;

        public override string FileName => filename;
        public override string Token => token;
        public override IEagleAccount Account => account;
        public override bool CanRead => disposed;
        public override bool CanSeek => disposed;
        public override bool CanWrite => disposed && writing;
        public override long Length => underlying.Length;
        public override long Position { get => underlying.Position; set => underlying.Position = value; }
        public override FileStream Underlying => underlying;

        public override void Flush()
        {
            underlying.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return underlying.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return underlying.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            EnsureWritable();
            underlying.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureWritable();
            underlying.Write(buffer, offset, count);
        }

        public override void Close()
        {
            base.Close();
            underlying.Close();
            context.NotifyFileClosed(token);
        }

        private void EnsureWritable()
        {
            if (!writing)
                throw new WebFsAccessDeniedException();
        }
    }
}
