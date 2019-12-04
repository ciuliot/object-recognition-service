using System.IO;
using Digitalist.ObjectRecognition.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Hangfire.Server;
using Hangfire.Console;
using Hangfire;
using System.Threading.Tasks;
using System.Threading;

namespace Digitalist.ObjectRecognition.Jobs
{
  public class DarknetTrainJob
  {
    readonly ILogger _logger;
    readonly IHubContext<DarknetJobHub, IDarknetJobHubClient> _darknetJobHub;
    PerformContext _context;

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
      _context.WriteLine($"Epoch: {batch_number}" +
        $" Loss {loss} Avg.loss {avg_loss} Learning rate {learning_rate}" +
        $" iteration {images}");

      _darknetJobHub.Clients.Group(_context.BackgroundJob.Id).UpdateReceived(
                batch_number,
                loss,
                avg_loss,
                learning_rate,
                images);
    }

    public void Start(string trainimages, string cfgfile,
      string weightfile, int[] gpus, bool clear, PerformContext context,
      IJobCancellationToken token)
    {
      _context = context;
      var outputdir = Path.Combine(Path.GetTempPath(), context.BackgroundJob.Id);
      Directory.CreateDirectory(outputdir);

      int abort = 0;

      var cancellationTokenObserver = Task.Factory.StartNew(() =>
      {
        while(!token.ShutdownToken.IsCancellationRequested)
        {
          Thread.Sleep(100);
        }

        abort = 1;
      });
      
      var trainingJob = Task.Factory.StartNew(() =>
      {
        NativeMethods.Darknet.train_detector(trainimages, cfgfile, weightfile, outputdir,
        gpus, gpus.Length, clear ? 1 : 0, ref abort, BatchFinishedCallback);
      });

      Task.WaitAll(cancellationTokenObserver, trainingJob);
    }
  }
}