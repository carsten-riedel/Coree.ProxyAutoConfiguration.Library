using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coree.ProxyAutoConfiguration.Library
{
    public class UriSimpleReader
    {

        public class ReadResult
        {
            public Exception? exception { get; set; }
            public string? Content { get; set; }
        }

        private ReadResult ReadPacFtpSync(Uri ftpUri)
        {
            var result = new ReadResult();
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                // If needed, set credentials here
                // request.Credentials = new NetworkCredential("username", "password");

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    result.Content = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                result.exception = ex;
            }
            return result;
        }

        private ReadResult ReadPacFileSync(Uri uri)
        {
            var result = new ReadResult();
            try
            {
                string filePath = uri.LocalPath;

                if (File.Exists(filePath))
                {
                    result.Content = File.ReadAllText(filePath);
                }
            }
            catch (Exception ex)
            {
                result.exception = ex;
            }
            return result;
        }

        private ReadResult ReadPacUrlSync(Uri url)
        {
            var result = new ReadResult();
            using (HttpClient client = new())
            {
                try
                {
                    var response = client.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();
                    result.Content = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {
                    result.exception = ex;
                }
            }
            return result;
        }


        public ReadResult GetContent(Uri? uri)
        {
            var result = new ReadResult();

            if (uri == null)
            {
                result.exception = new ArgumentNullException(nameof(uri), "URI cannot be null.");
                return result;
            }

            switch (uri.Scheme)
            {
                case "file":
                    return ReadPacFileSync(uri);

                case "http":
                case "https":
                    return ReadPacUrlSync(uri);

                case "ftp":
                    return ReadPacFtpSync(uri);

                default:
                    result.exception = new NotSupportedException($"URI scheme '{uri.Scheme}' is not supported.");
                    return result;
            }
        }

    }
}