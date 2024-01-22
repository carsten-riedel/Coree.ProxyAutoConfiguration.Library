using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coree.ProxyAutoConfiguration.Library
{
    public class GenericUriConverter
    {
        public string? Input { get; private set; }
        public Result ConversionResult { get; private set; }

        public GenericUriConverter()
        {
            ConversionResult = new Result { State = ConversionState.Uninitialized };
        }

        public enum ConversionState
        {
            ValidUri,
            ConvertException,
            Uninitialized,
        }

        public class Result
        {
            public Uri? Uri { get; set; } = null;
            public ConversionState State { get; set; }
            public Exception? Exception { get; set; }
        }

        public void ConvertInputToUri(string? input)
        {
            this.Input = input;
            this.ConversionResult = PerformConversion();
        }

        private Result PerformConversion()
        {
            if (Input == null)
            {
                return new Result { State = ConversionState.Uninitialized };
            }
            else
            {
                try
                {
                    Uri resolvedUri;
                    if (Uri.IsWellFormedUriString(Input, UriKind.Absolute))
                    {
                        resolvedUri = new Uri(Input);
                    }
                    else
                    {
                        var absolutePath = Path.GetFullPath(Input);
                        resolvedUri = new Uri(absolutePath);
                    }

                    return new Result { State = ConversionState.ValidUri, Uri = resolvedUri };
                }
                catch (Exception ex)
                {
                    return new Result { State = ConversionState.ConvertException, Exception = ex, Uri = null };
                }
            }
        }
    }
}
