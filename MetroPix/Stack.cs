// File that contains a new caching / querying / multi-network stack

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MetroPix
{
    // This class manages access to the network and tries to be intelligent about how
    // network communications happens within the app.
    // 
    // Out of the box this class will support retries to work around 500px SSL bug in Win 8.
    public class NetworkManager
    {
        public const int MAX_PHOTO_SIZE = 100000000; // Surely 100MB is enough for everyone? :)

        private uint _bytesConsumed;

        private static async Task<T> RetryOnFault<T>(Func<Task<T>> function, int maxRetries = 5)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await function();
                }
                catch
                {
                    if (i == maxRetries - 1)
                        throw;
                }
            }
            return default(T);
        }
        
        private int GetContentLength(HttpResponseMessage response)
        {
            int contentLength = MAX_PHOTO_SIZE;
            IEnumerable<string> values;
            if (response.Headers.TryGetValues("content-length", out values))
            {
                int count = 0;
                foreach (var value in values)
                {
                    contentLength = Convert.ToInt32(value);
                    if (contentLength > MAX_PHOTO_SIZE)
                        throw new InvalidOperationException(String.Format("Trying to download an image of that is {0} bytes; exceeds limit of {1}", contentLength, MAX_PHOTO_SIZE));

                    count++;
                }
                Debug.Assert(count == 1);
            }
            return contentLength;
        }

        private async Task<byte[]> GetByteArrayAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage();
                request.RequestUri = uri;
                Stream stream = await client.GetStreamAsync(uri);
                BitmapImage bitmap = new BitmapImage();
                //bitmap.SetSource(stream.AsInputStr);

                //client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead
                var response = await client.GetAsync(uri);
                client.MaxResponseContentBufferSize = GetContentLength(response);
                return await client.GetByteArrayAsync(uri);
            }
        }

        private const uint BUFFER_SIZE = 65536;

        // TODO: promote this to be a regular function
        // TODO: offer the right kind of caching option here -- we need to cache the bytes of the images, not the images themselves
        // we need to aggressively dump images from memory
        public async Task<BitmapImage> AwesomeRead(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var stream = await response.Content.ReadAsStreamAsync();
                var inputStream = stream.AsInputStream();
                var reader = new DataReader(inputStream);
                var ras = new InMemoryRandomAccessStream();
                var writer = new DataWriter(ras.GetOutputStreamAt(0));
                uint bytesLoaded = await reader.LoadAsync(BUFFER_SIZE);
                do
                {
                    _bytesConsumed += bytesLoaded;
                    writer.WriteBuffer(reader.ReadBuffer(bytesLoaded));
                    if (bytesLoaded < BUFFER_SIZE)
                    {
                        break;
                    }
                    bytesLoaded = await reader.LoadAsync(BUFFER_SIZE);
                } while (true);
                await writer.StoreAsync();
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(ras);
                return bitmapImage;
            }
        }

        private async Task<String> GetStringAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(uri);
            }
        }

        private async Task<IRandomAccessStream> CopyByteArrayToRandomAccessStream(byte[] bytes)
        {
            using (var ras = new InMemoryRandomAccessStream())
            {
                
                using (var writer = new DataWriter(ras.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(bytes);
                    await writer.StoreAsync();
                    return ras;
                }
            }
        }

        public async Task<BitmapImage> GetImage(Uri uri)
        {
            var bytes = await RetryOnFault(() => GetByteArrayAsync(uri));
            _bytesConsumed += (uint)bytes.Length;
            var bitmap = new BitmapImage();
            bitmap.SetSource(await CopyByteArrayToRandomAccessStream(bytes));
            return bitmap;
        }

        public async Task<string> GetString(Uri uri)
        {
            return await RetryOnFault(() => GetStringAsync(uri));
        }

        public uint BytesConsumed { get { return _bytesConsumed; } }

        private static NetworkManager _networkManager;

        static NetworkManager() 
        {
            _networkManager = new NetworkManager();    
        }

        public static NetworkManager Current
        {
            get { return _networkManager; }
        }
    }

    // This class manages fetching photos from services. At the moment this will be hard-coded
    // for 1 service: 500px, but will expand over time to support fetching from arbitrary 
    // services.
    public class PhotoManager
    {
    }

    // This class manages querying services like 500px. It returns objects that identify the photo
    // id that we care about.
    public class QueryManager
    {

    }
}