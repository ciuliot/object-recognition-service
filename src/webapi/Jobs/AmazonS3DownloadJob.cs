using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;

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

    public void Directory(string bucketName, string s3Directory, string outputDirectory)
    {
      _logger.LogInformation($"Starting directory download {bucketName}/{s3Directory} to {outputDirectory}");

      var objects = _amazonS3Client.ListObjectsV2Async(new ListObjectsV2Request
      {
        BucketName = bucketName,
        Prefix = s3Directory
      }).GetAwaiter().GetResult();

      var tasks = from obj in objects.S3Objects
                  select DownloadFile(bucketName, obj.Key, outputDirectory);

      Task.WaitAll(tasks.ToArray());

      _logger.LogInformation($"Directory downloaded {bucketName}/{s3Directory} to {outputDirectory}");
    }

    public void File(string bucketName, string fileName, string outputDirectory)
    {
      _logger.LogInformation($"Downloading file {bucketName}/{fileName} to {outputDirectory}/{fileName}");
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