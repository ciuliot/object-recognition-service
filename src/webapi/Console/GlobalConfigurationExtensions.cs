using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.States;

namespace Digitalist.ObjectRecognition.Console
{
  public static class GlobalConfigurationExtensions
  {
    public static IGlobalConfiguration UseObjectRecognitionConsole(this IGlobalConfiguration configuration)
    {
      if (configuration == null)
        throw new ArgumentNullException(nameof(configuration));

      var fi = typeof(JobHistoryRenderer).GetField("Renderers", BindingFlags.NonPublic | BindingFlags.Static);

      if (fi != null)
      {
        var val = fi.GetValue(null) as IDictionary<string, Func<HtmlHelper, IDictionary<string, string>, NonEscapedString>>;
        var originalRenderer = val.ContainsKey(ProcessingState.StateName) ? val[ProcessingState.StateName] : JobHistoryRenderer.DefaultRenderer;

        // Chain renderers
        JobHistoryRenderer.Register(ProcessingState.StateName, new ProcessingStateRenderer(originalRenderer).Render);
      }      

      return configuration;
    }
  }
}