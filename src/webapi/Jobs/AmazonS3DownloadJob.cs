using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
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

    public async Task Directory(string bucketName, string s3Directory, string outputDirectory)
    {
      _logger.LogInformation($"Downloading directory {bucketName}/{s3Directory} to {outputDirectory}");
      var util = new TransferUtility(_amazonS3Client);
      await util.DownloadDirectoryAsync(bucketName, s3Directory, outputDirectory);
    }

    public async Task File(string bucketName, string fileName, string outputDirectory)
    {
      _logger.LogInformation($"Downloading file {bucketName}/{fileName} to {outputDirectory}/{fileName}");
      var util = new TransferUtility(_amazonS3Client);
      await util.DownloadAsync(Path.Combine(outputDirectory, fileName), bucketName, fileName);
    }
  }
}