using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coree.ProxyAutoConfiguration.Library
{
    public class UniversalContentReader
    {
        public ContentResult ReadResult { get; private set; } = new ContentResult();

        /// <summary>
        /// Initializes a new instance of UniversalContentReader and fetches content based on the provided URI.
        /// </summary>
        /// <param name="uri">The URI to fetch content from.</param>
        public UniversalContentReader(Uri? uri)
        {
            ReadResult = FetchContentFromUri(uri);
        }

        /// <summary>
        /// Represents the result of a content reading operation. 
        /// Contains the content read from a URI if successful, or an exception if an error occurred.
        /// </summary>
        public class ContentResult
        {
            /// <summary>
            /// Gets the exception that occurred during the content reading process, if any. 
            /// This property is null if no exception occurred.
            /// </summary>
            public Exception? Exception { get; set; } = null;
            /// <summary>
            /// Gets the content read from the URI. 
            /// This property is null if no content was read or if an exception occurred.
            /// </summary>
            public string? Content { get; set; } = null;
        }

        /// <summary>
        /// Synchronously reads content from an FTP URI.
        /// </summary>
        /// <param name="ftpUri">The FTP URI to read content from.</param>
        /// <returns>A ContenResult object containing the read content or an exception.</returns>
        private static ContentResult ReadFtpSync(Uri ftpUri)
        {
            var result = new ContentResult();
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
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// Synchronously reads content from a file URI.
        /// </summary>
        /// <param name="uri">The file URI to read content from.</param>
        /// <returns>A ContenResult object containing the read content or an exception.</returns>
        private static ContentResult ReadFileSync(Uri uri)
        {
            var result = new ContentResult();
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
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// Synchronously reads content from a HTTP/HTTPS URL.
        /// </summary>
        /// <param name="url">The HTTP/HTTPS URL to read content from.</param>
        /// <returns>A ContenResult object containing the read content or an exception.</returns>
        private static ContentResult ReadUrlSync(Uri url)
        {
            var result = new ContentResult();
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
                    result.Exception = ex;
                }
            }
            return result;
        }

        /// <summary>
        /// Fetches content from the given URI based on its scheme (FTP, file, HTTP/HTTPS).
        /// </summary>
        /// <param name="uri">The URI to fetch content from.</param>
        /// <returns>A ContenResult object containing the fetched content or an exception.</returns>
        public static ContentResult FetchContentFromUri(Uri? uri)
        {
            var result = new ContentResult();

            if (uri == null)
            {
                result.Exception = new ArgumentNullException(nameof(uri), "URI cannot be null.");
                return result;
            }

            switch (uri.Scheme)
            {
                case "file":
                    return UniversalContentReader.ReadFileSync(uri);
                case "http":
                case "https":
                    return UniversalContentReader.ReadUrlSync(uri);
                case "ftp":
                    return UniversalContentReader.ReadFtpSync(uri);
                default:
                    result.Exception = new NotSupportedException($"URI scheme '{uri.Scheme}' is not supported.");
                    return result;
            }
        }
    }
}