using System;
using System.IO;
using Digitalist.ObjectRecognition.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Digitalist.ObjectRecognition.Services
{
  public class DarknetService
  {
    readonly ILogger _logger;
    readonly IntPtr _network;

    public DarknetService(ILogger<DarknetService> logger)
    {
      _logger = logger;
      _logger.LogInformation("Initializing darknet network");

      _network = NativeMethods.Darknet.initialize("cfg/yolov3.cfg", "yolov3.weights");
    }

    public DarknetResult[] Detect(string imagefile, float thresh, float hier_thresh)
    {
      var outputFile = Path.GetTempFileName();

      NativeMethods.Darknet.detect(_network,
        "cfg/coco.data",
        imagefile,
        thresh,
        hier_thresh,
        outputFile);

      var contents = File.ReadAllText(outputFile);
      return JsonConvert.DeserializeObject<DarknetResult[]>(contents);
    }
  }
}