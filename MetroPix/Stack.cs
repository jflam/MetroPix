// File that contains a new caching / querying / multi-network stack

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
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

        private ulong _bytesConsumed;

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
                var stream = await client.GetStreamAsync(uri);
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

        // The .NET buffer size in ReadAsync is 4K, so we read into a buffer that grows in 4K increments
        private const int STREAM_BUFFER_SIZE = 4096;

        public async Task<string> GetStringAsync(Uri uri)
        {
            int bufferSize = STREAM_BUFFER_SIZE;
            var buffer = new byte[bufferSize];
            int requestBytesConsumed = 0;
            using (var client = new HttpClient())
            {
                using (var stream = await client.GetStreamAsync(uri))
                {
                    var bytesLoaded = await stream.ReadAsync(buffer, 0, bufferSize);
                    do
                    {
                        requestBytesConsumed += bytesLoaded;
                        if (requestBytesConsumed < bufferSize)
                        {
                            break;
                        }
                        byte[] newBuffer = new byte[bufferSize + STREAM_BUFFER_SIZE];
                        Buffer.BlockCopy(buffer, 0, newBuffer, 0, bufferSize);
                        buffer = newBuffer;
                        bytesLoaded = await stream.ReadAsync(buffer, bufferSize, STREAM_BUFFER_SIZE);
                        bufferSize += STREAM_BUFFER_SIZE;
                    } while (true);
                    _bytesConsumed += (ulong)requestBytesConsumed;
                    // TODO: how do I know for sure what character encoding is being used here? Do we inspect headers for this?
                    return Encoding.UTF8.GetString(buffer, 0, requestBytesConsumed);
                }
            }
        }

        public async Task<BitmapImage> GetBitmapImageAsync(Uri uri)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(await GetRawBitmapIntoInMemoryRandomAccessStream(uri));
            return bitmapImage;
        }

        public ulong BytesConsumed { get { return _bytesConsumed; } }

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