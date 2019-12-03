using System.Threading.Tasks;

namespace Digitalist.ObjectRecognition.Hubs
{
  public interface IDarknetJobHubClient
  {
    Task UpdateReceived(ulong batchId, double loss, 
      double avgLoss, double learningRate, int trainingStep);
  }
}