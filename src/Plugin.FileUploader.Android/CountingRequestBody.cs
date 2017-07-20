using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OkHttp;
using OkHttp.Okio;

namespace Plugin.FileUploader
{
    public class CountingRequestBody : RequestBody
    {
        protected RequestBody _body;
        protected ICountProgressListener _listener;

        protected CountingSink countingSink;

        public CountingRequestBody(RequestBody body, ICountProgressListener listener)
        {
            _body = body;
            _listener = listener;
        }
        public override MediaType ContentType()
        {
            return _body.ContentType();
        }
        public override long ContentLength()
        {
            return _body.ContentLength();
        }

        public override void WriteTo(OkHttp.Okio.IBufferedSink p0)
        {
            IBufferedSink bufferedSink;

            countingSink = new CountingSink(this, p0);
            bufferedSink = Okio.Buffer(countingSink);

            _body.WriteTo(bufferedSink);

            bufferedSink.Flush();
        }

        public class CountingSink : ForwardingSink
        {
            private long bytesWritten = 0;
            CountingRequestBody _parent;

            public CountingSink(CountingRequestBody parent, ISink sink) : base(sink)
            {
                _parent = parent;
            }

            public override void Write(OkBuffer p0, long p1)
            {
                base.Write(p0, p1);

                bytesWritten += p1;
                _parent?._listener.OnProgress(bytesWritten, _parent.ContentLength());
            }


        }
    }
}