using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Digitalist.ObjectRecognition
{
  public class Program
  {
    public static void Main(string[] args)
    {      
      /*NativeMethods.test_detector("cfg/coco.data", 
        "cfg/yolov3.cfg", 
        "yolov3.weights", 
        "dog.jpg", 0.5f, 0.5f);*/
        
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            });
  }
}
