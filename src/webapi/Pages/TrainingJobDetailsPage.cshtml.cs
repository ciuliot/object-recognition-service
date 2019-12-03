using Hangfire;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Digitalist.ObjectRecognition.Pages
{
  public class TrainingJobDetailsPageModel : PageModel
  {
    public JobDetailsDto Job;
    readonly ILogger _logger;

    public TrainingJobDetailsPageModel(ILogger<TrainingJobDetailsPageModel> logger)
    {
      _logger = logger;
    }

    public void OnGet(string jobId)
    {
      var monitor = JobStorage.Current.GetMonitoringApi();
      Job = monitor.JobDetails(jobId);
    }
  }
}
