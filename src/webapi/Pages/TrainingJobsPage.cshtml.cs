using Digitalist.ObjectRecognition.Jobs;
using Hangfire;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Digitalist.ObjectRecognition.Pages
{
  public class TrainingJobsPageModel : PageModel
  {
    public KeyValuePair<string, ProcessingJobDto>[] Jobs;
    readonly ILogger _logger;

    public TrainingJobsPageModel(ILogger<TrainingJobsPageModel> logger)
    {
      _logger = logger;
    }

    public void OnGet()
    {
      var monitor = JobStorage.Current.GetMonitoringApi();
      Jobs = (from j in  monitor.ProcessingJobs(0, 9999)
             where j.Value.Job.Type == typeof(DarknetTrainJob)
             select j).ToArray();
    }
  }
}
