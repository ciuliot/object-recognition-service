using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;

namespace Digitalist.ObjectRecognition.Jobs
{
  public class GenerateImageListJob
  {
    readonly ILogger _logger;

    public GenerateImageListJob(ILogger<GenerateImageListJob> logger)
    {
      _logger = logger;
    }

    public void Start(string imagesFolder, string outputFile)
    {
      _logger.LogInformation($"Generating image list for folder ${imagesFolder} to file ${outputFile}");
      
      using (var file = File.OpenWrite(outputFile))
      using (var sr = new StreamWriter(file))
      {
        foreach(var fileName in Directory.GetFiles(imagesFolder))
        {
          sr.Write(fileName);
          sr.WriteLine();
        }        
      }
    }
  }
}