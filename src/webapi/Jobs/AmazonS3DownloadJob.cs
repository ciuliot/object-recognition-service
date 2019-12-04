using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Hangfire.Server;
using Hangfire.Console;
using System.Collections.Generic;

namespace Digitalist.ObjectRecognition.Jobs
{
  public class AmazonS3DownloadJob
  {
    readonly ILogger _logger;
    readonly AmazonS3Client _amazonS3Client;

    public AmazonS3DownloadJob(ILogger<AmazonS3DownloadJob> logger,
      AmazonS3Client amazonS3Client)
    {
      _logger = logger;
      _amazonS3Client = amazonS3Client;
    }

    public void Directory(string bucketName, string s3Directory, string outputDirectory,
      PerformContext context)
    {
      var i = 0.0;
      var startAfter = default(string);
      var response = default(ListObjectsV2Response);
      var progressBar = context.WriteProgressBar();
      var keys = new List<string>();

      do
      {
        response = _amazonS3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
          BucketName = bucketName,
          Prefix = s3Directory,
          StartAfter = startAfter
        }).GetAwaiter().GetResult();

        if (response.KeyCount > 0)
        {
          keys.AddRange(response.S3Objects.Select(o => o.Key));
          startAfter = keys.Last();
        }
      } while (response.KeyCount > 0);

      var tasks = from key in keys
                  select DownloadFile(bucketName, key, outputDirectory).ContinueWith((t) =>
                  {
                    progressBar.SetValue((100.0 * ++i) / (double)keys.Count);
                  });

      Task.WaitAll(tasks.ToArray());


      progressBar.SetValue(100);
      context.WriteLine($"Downloaded {(int)keys.Count} files");
    }

    public void File(string bucketName, string fileName, string outputDirectory)
    {
      DownloadFile(bucketName, fileName, outputDirectory).GetAwaiter().GetResult();
    }

    private async Task DownloadFile(string bucketName, string fileName, string outputDirectory)
    {
      var o = await _amazonS3Client.GetObjectAsync(bucketName, fileName);
      var outputFileName = Path.Combine(outputDirectory, fileName);

      var fi = new FileInfo(outputFileName);
      if (!System.IO.Directory.Exists(fi.DirectoryName))
      {
        System.IO.Directory.CreateDirectory(fi.DirectoryName);
      }

      using (var outputFile = System.IO.File.OpenWrite(Path.Combine(outputFileName)))
      {
        await o.ResponseStream.CopyToAsync(outputFile);
      }
    }
  }
}