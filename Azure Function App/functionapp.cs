#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
//#r "Microsoft.AspNet"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
//using Microsoft.WindowsAzure.Storage.dll;
//using Microsoft.AspNet.WebApi.Client;
 

public static void Run(Stream myBlob, string name, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

    // get name of the file
    var path = "sessionfiles/"+name;
    log.Info(name);

    // execute Azure machine learning experiment
    executeML(log, path);
}

static void executeML(TraceWriter log, string path)
{
    InvokeBatchExecutionService(path).Wait();
}

// Source:  API help page link in the web service DASHBOARD in Machine Learning Studio

public class AzureBlobDataReference
{
    // Storage connection string used for regular blobs. It has the following format:
    // DefaultEndpointsProtocol=https;AccountName=ACCOUNT_NAME;AccountKey=ACCOUNT_KEY
    // It's not used for shared access signature blobs.
    public string ConnectionString { get; set; }

    // Relative uri for the blob, used for regular blobs as well as shared access 
    // signature blobs.
    public string RelativeLocation { get; set; }

    // Base url, only used for shared access signature blobs.
    public string BaseLocation { get; set; }

    // Shared access signature, only used for shared access signature blobs.
    public string SasBlobToken { get; set; }
}

public enum BatchScoreStatusCode
{
    NotStarted,
    Running,
    Failed,
    Cancelled,
    Finished
}

public class BatchScoreStatus
{
    // Status code for the batch scoring job
    public BatchScoreStatusCode StatusCode { get; set; }


    // Locations for the potential multiple batch scoring outputs
    public IDictionary<string, AzureBlobDataReference> Results { get; set; }

    // Error details, if any
    public string Details { get; set; }
}

public class BatchExecutionRequest
{

    public IDictionary<string, AzureBlobDataReference> Inputs { get; set; }
    public IDictionary<string, string> GlobalParameters { get; set; }

    // Locations for the potential multiple batch scoring outputs
    public IDictionary<string, AzureBlobDataReference> Outputs { get; set; }
}

public static async Task WriteFailedResponse(HttpResponseMessage response)
{
    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
    Console.WriteLine(response.Headers.ToString());

    string responseContent = await response.Content.ReadAsStringAsync();
    Console.WriteLine(responseContent);
}

public static async Task InvokeBatchExecutionService(string path)
{

    const string BaseUrl = "https://europewest.services.azureml.net/subscriptions/1543bcfb1cd84ec7929520bb03be04af/services/7a2b3bec258f4203a2c891cee83edb51/jobs";
    const string apiKey = "GSOQaW0ecNuUBs2xSp15/E9w0+6JmCrorHBsNtJcktKPW2IKDpMjDTBD9lH6Zi+Asr6SFENfo5mGmZ13gB3CoQ=="; // Replace this with the API key for the web service

    // set a time out for polling status
    const int TimeOutInMilliseconds = 120 * 4 * 1000; // Set a timeout of 2 minutes


    using (HttpClient client = new HttpClient())
    {
        var request = new BatchExecutionRequest()
        {
            GlobalParameters = new Dictionary<string, string>() {
                {"PathToBlob", path},
            } 

            // Inputs = new Dictionary<string, AzureBlobDataReference> () {
            //     {PathToBlob = path}
            // }             
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
        // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
        // For instance, replace code such as:
        //      result = await DoSomeTask()
        // with the following:
        //      result = await DoSomeTask().ConfigureAwait(false)


        Console.WriteLine("Submitting the job...");

        // submit the job
        var response = await client.PostAsJsonAsync(BaseUrl + "?api-version=2.0", request);
        if (!response.IsSuccessStatusCode)
        {
            await WriteFailedResponse(response);
            return;
        }

        string jobId = await response.Content.ReadAsAsync<string>();
        Console.WriteLine(string.Format("Job ID: {0}", jobId));


        // start the job
        Console.WriteLine("Starting the job...");
        response = await client.PostAsync(BaseUrl + "/" + jobId + "/start?api-version=2.0", null);
        if (!response.IsSuccessStatusCode)
        {
            await WriteFailedResponse(response);
            return;
        }

        string jobLocation = BaseUrl + "/" + jobId + "?api-version=2.0";
        Stopwatch watch = Stopwatch.StartNew();
        bool done = false;
        while (!done)
        {
            Console.WriteLine("Checking the job status...");
            response = await client.GetAsync(jobLocation);
            if (!response.IsSuccessStatusCode)
            {
                await WriteFailedResponse(response);
                return;
            }

            BatchScoreStatus status = await response.Content.ReadAsAsync<BatchScoreStatus>();
            if (watch.ElapsedMilliseconds > TimeOutInMilliseconds)
            {
                done = true;
                Console.WriteLine(string.Format("Timed out. Deleting job {0} ...", jobId));
                await client.DeleteAsync(jobLocation);
            }
            switch (status.StatusCode) {
                case BatchScoreStatusCode.NotStarted:
                    Console.WriteLine(string.Format("Job {0} not yet started...", jobId));
                    break;
                case BatchScoreStatusCode.Running:
                    Console.WriteLine(string.Format("Job {0} running...", jobId));
                    break;
                case BatchScoreStatusCode.Failed:
                    Console.WriteLine(string.Format("Job {0} failed!", jobId));
                    Console.WriteLine(string.Format("Error details: {0}", status.Details));
                    done = true;
                    break;
                case BatchScoreStatusCode.Cancelled:
                    Console.WriteLine(string.Format("Job {0} cancelled!", jobId));
                    done = true;
                    break;
                case BatchScoreStatusCode.Finished:
                    done = true;
                    Console.WriteLine(string.Format("Job {0} finished!", jobId));

                    Console.WriteLine("Response: ");
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                    break;
            }

            if (!done) {
                Thread.Sleep(1000); // Wait one second
            }
        }
    }
}
