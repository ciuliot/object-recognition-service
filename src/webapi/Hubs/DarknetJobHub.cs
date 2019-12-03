using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Digitalist.ObjectRecognition.Hubs
{
  public class DarknetJobHub : Hub<IDarknetJobHubClient>
  {
    readonly ILogger _logger;

    public DarknetJobHub(ILogger<DarknetJobHub> logger)
    {
      _logger = logger;
    }

    public void Subscribe(string darknetId)
    {
      Groups.AddToGroupAsync(Context.ConnectionId, darknetId);
    }
  }
}