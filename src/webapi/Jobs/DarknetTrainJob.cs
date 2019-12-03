using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Digitalist.ObjectRecognition.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Digitalist.ObjectRecognition.Jobs
{
  public class DarknetTrainJob
  {
    readonly ILogger _logger;
    readonly IHubContext<DarknetJobHub, IDarknetJobHubClient> _darknetJobHub;
    string _hubId;

    public DarknetTrainJob(
      ILogger<DarknetTrainJob> logger,
      IHubContext<DarknetJobHub, IDarknetJobHubClient> darknetJobHub)
    {
      _logger = logger;
      _darknetJobHub = darknetJobHub;
    }

    async void BatchFinishedCallback(ulong batch_number, float loss,
        float avg_loss, float learning_rate, int images)
    {
      _logger.LogInformation($"Batch finished for {_hubId} Epoch: {batch_number}" +
        $" Loss {loss} Avg.loss {avg_loss} Learning rate {learning_rate}"+
        $" iteration {images}");

      _darknetJobHub.Clients.Group(_hubId).UpdateReceived(
                batch_number,
                loss,
                avg_loss,
                learning_rate,
                images);
    }

    public void Start(string hubId, string trainimages, string cfgfile, string weightfile, int[] gpus, bool clear)
    {
      _hubId = hubId;
      var outputdir = Path.Combine(Path.GetTempPath(), hubId.ToString());
      Directory.CreateDirectory(outputdir);

      _logger.LogInformation($"Starting training job for {trainimages} [{cfgfile}");

      NativeMethods.Darknet.train_detector(trainimages, cfgfile, weightfile, outputdir,
        gpus, gpus.Length, clear ? 1 : 0, BatchFinishedCallback);
    }
  }
}