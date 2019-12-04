using System.IO;
using Hangfire.Server;
using Hangfire.Console;
using Microsoft.Extensions.Logging;
using Hangfire;

namespace Digitalist.ObjectRecognition.Jobs
{
  public class GenerateImageListJob
  {
    readonly ILogger _logger;

    public GenerateImageListJob(ILogger<GenerateImageListJob> logger)
    {
      _logger = logger;
    }

    public void Start(string imagesFolder, string labelsFolder, string outputFile, 
      PerformContext context, IJobCancellationToken token)
    {
      var progressBar = context.WriteProgressBar();

      using (var file = File.OpenWrite(outputFile))
      using (var sr = new StreamWriter(file))
      {
        foreach (var fileName in Directory.GetFiles(imagesFolder).WithProgress(progressBar))
        {
          var labelFile = Path.GetFileName(Path.ChangeExtension(fileName, "txt"));
          token.ThrowIfCancellationRequested();

          if (File.Exists(Path.Combine(labelsFolder, labelFile)))
          {
            sr.Write(fileName);
            sr.WriteLine();
          }
          else
          {
            context.WriteLine(ConsoleTextColor.DarkYellow, 
              $"Image {Path.GetFileName(fileName)} does not have corresponding label, skipping");
          }
        }
      }
    }
  }
}