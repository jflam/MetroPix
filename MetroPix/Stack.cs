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

        private const uint BUFFER_SIZE = 65536; // TODO: optimize this later -- default max buffer is 64K so this may be reasonable

        // Read the raw bytes from uri into an InMemoryRandomAccessStream. This is a basic building block 
        // function that we will use for the caching infrastructure which will be implemented later.
        private async Task<IRandomAccessStream> GetRawBitmapIntoInMemoryRandomAccessStream(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var stream = await response.Content.ReadAsStreamAsync();
                var inputStream = stream.AsInputStream();
                var ras = new InMemoryRandomAccessStream();
                using (var reader = new DataReader(inputStream)) 
                {
                    using (var writer = new DataWriter(ras.GetOutputStreamAt(0)))
                    {
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
                    }
                }
                return ras;
            }
        }

        public async Task<BitmapImage> GetBitmapImageAsync(Uri uri)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(await GetRawBitmapIntoInMemoryRandomAccessStream(uri));
            return bitmapImage;
        }

        public async Task<string> GetStringAsync(Uri uri)
        {
            return await RetryOnFault(async () =>
            {
                using (var client = new HttpClient())
                {
                    return await client.GetStringAsync(uri);
                }
            });
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