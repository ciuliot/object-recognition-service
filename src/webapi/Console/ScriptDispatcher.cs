using System;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire.Dashboard;

// ReSharper disable once CheckNamespace
namespace Digitalist.ObjectRecognition.Console
{
  /// <summary>
  /// Alternative to built-in EmbeddedResourceDispatcher, which (for some reasons) is not public.
  /// </summary>
  internal class ScriptDispatcher : IDashboardDispatcher
  {
    public ScriptDispatcher()
    {
    }

    public Task Dispatch(DashboardContext context)
    {
      context.Response.ContentType = "";
      var assembly = typeof(ScriptDispatcher).Assembly;
      var resourceName = $"{typeof(Program).Namespace}.Resources.script.js";

      foreach(var name in assembly.GetManifestResourceNames())
      {
        System.Console.WriteLine(name);
      }

      using (var stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream == null)
          throw new ArgumentException($@"Resource '{resourceName}' not found in assembly {assembly}.");

        return stream.CopyToAsync(context.Response.Body);
      }
    }    
  }
}