using System;

namespace worldfetch.Lib
{
    public class FetcherOptions
    {
        public string BaseDataUri { get; set; }

        public TimeSpan FetchInterval { get; set; }
    }
}