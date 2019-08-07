using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp
{
    static class NotificationDownloader
    {
        static public IEnumerable<string> GetNotificationsFromBlobs(string sasKey, DateTime cutoffDate)
        {
            

            var container = new CloudBlobContainer(new Uri(sasKey));

            var blobs = container.ListBlobs(useFlatBlobListing: true, blobListingDetails: BlobListingDetails.None)
                .OfType<CloudBlockBlob>()
                .Where(b => b.Properties.Created > cutoffDate)
                .OrderByDescending(b => b.Properties.Created);

            foreach (var blob in blobs)
            {
                var content = blob.DownloadTextAsync().Result;
                yield return content;
            }
        }
    }
}
