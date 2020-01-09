using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeDL
{
    class GeoRestrictionException : Exception
    {
        public GeoRestrictionException() : base()
        {
        }

        public GeoRestrictionException(string message) : base(message)
        {
        }

        public GeoRestrictionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    class ExtractorException : Exception
    {
        public ExtractorException() : base()
        {
        }

        public ExtractorException(string message) : base(message)
        {
        }

        public ExtractorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    class MaxDownloadsReachedException : Exception
    {
        public MaxDownloadsReachedException() : base()
        {
        }

        public MaxDownloadsReachedException(string message) : base(message)
        {
        }

        public MaxDownloadsReachedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    class FFMpegException : Exception
    {
        public string FFMpegStdError { get; set; }
        public FFMpegException() : base()
        {
        }

        public FFMpegException(string message) : base(message)
        {
        }

        public FFMpegException(string message, string stderr) : base(message)
        {
            FFMpegStdError = stderr;
        }
    }
}
