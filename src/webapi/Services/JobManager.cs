using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Digitalist.ObjectRecognition.Services
{
  public class JobManager : IJobManager
  {
    readonly ILogger _logger;
    readonly Dictionary<Guid, Task> _tasks = new Dictionary<Guid, Task>();

    public JobManager(ILogger<JobManager> logger)
    {
      _logger = logger;
    }

    public Guid Add(Task task)
    {
      var guid = Guid.NewGuid();

      _tasks.Add(guid, task);

      return guid;
    }
  }
}