using Hangfire;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Digitalist.ObjectRecognition.Pages
{
  public class TrainingJobsPageModel : PageModel
  {
    public JobList<ProcessingJobDto> Jobs;
    readonly ILogger _logger;

    public TrainingJobsPageModel(ILogger<TrainingJobsPageModel> logger)
    {
      _logger = logger;
    }

    public void OnGet()
    {
      var monitor = JobStorage.Current.GetMonitoringApi();
      Jobs = monitor.ProcessingJobs(0, 10);
    }
  }
}
