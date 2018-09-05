using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Azure.Devices;

namespace sensorsgit
{
    public class CloudUpload
    {
        static DeviceClient deviceClient;
        static ServiceClient serviceClient;
        string deviceId;
        string deviceKey;
        string iotHubUri;
        string connectionString;

        public CloudUpload(string deviceId, string deviceKey, string iotHubUri, string connectionString)
        {
            this.deviceId = deviceId;
            this.deviceKey = deviceKey;
            this.iotHubUri = iotHubUri;
            this.connectionString = connectionString;

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
        }

        // source: https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-file-upload
        public async void SendToBlobAsync(string pathname, StreamWriter file, string sessionId)
        {
            file.Dispose();
            Console.WriteLine("Uploading file: {0}", pathname);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("sessionid: " + sessionId);
            using (var sourceData = new FileStream(@pathname, FileMode.Open))
            {
                await deviceClient.UploadToBlobAsync(sessionId, sourceData);
            }

            watch.Stop();
            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }

        // source: https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-file-upload
        public async Task<bool> ReceiveFileUploadNotificationAsync()
        {
            var notificationReceiver = serviceClient.GetFileNotificationReceiver();

            Console.WriteLine("\nReceiving file upload notification from service");
            while (true)
            {
                var fileUploadNotification = await notificationReceiver.ReceiveAsync();
                if (fileUploadNotification == null) continue;

                Console.WriteLine("Received file upload noticiation: {0}", string.Join(", ", fileUploadNotification.BlobName));

                await notificationReceiver.CompleteAsync(fileUploadNotification);
                return true;
            }
        }

    }
}
