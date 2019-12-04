using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Digitalist.ObjectRecognition.Jobs;
using Digitalist.ObjectRecognition.Models;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Digitalist.ObjectRecognition.Services
{
  public class DarknetService
  {
    readonly ILogger _logger;
    IntPtr _detectionNetwork;
    readonly IBackgroundJobClient _backgroundJobs;

    #region DetectionNetwork

    IntPtr DetectionNetwork
    {
      get
      {
        lock(this)
        {
          if (_detectionNetwork == IntPtr.Zero)
          {
            _logger.LogInformation("Initializing darknet network");

            _detectionNetwork = NativeMethods.Darknet.initialize("cfg/yolov3.cfg", "yolov3.weights", 0);
          }
        }

        return _detectionNetwork;
      }
    }

    #endregion

    public DarknetService(ILogger<DarknetService> logger,
      IBackgroundJobClient backgroundJobs)
    {
      _logger = logger;
      _backgroundJobs = backgroundJobs;
    }

    public DarknetResult[] Detect(string imagefile, float thresh, float hier_thresh)
    {
      _logger.LogInformation($"Processing image {imagefile}");
      var outputFile = Path.GetTempFileName();

      NativeMethods.Darknet.detect(DetectionNetwork,
        "cfg/coco.data",
        imagefile,
        thresh,
        hier_thresh,
        outputFile);

      var contents = File.ReadAllText(outputFile);
      _logger.LogInformation(contents);
      return JsonConvert.DeserializeObject<DarknetResult[]>(contents);
    }

    public string Train(string trainimages, string cfgfile, string weightfile, int[] gpus, bool clear)
    {
      _logger.LogInformation($"Enqueueing new training job");

      return _backgroundJobs.Enqueue<DarknetTrainJob>(job =>
      job.Start(trainimages, cfgfile, weightfile, gpus, clear, null));
    }
  }
}
