using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hangfire.Dashboard;

namespace Digitalist.ObjectRecognition.Console
{
  public class ProcessingStateRenderer
  {
    static readonly FieldInfo _page = typeof(HtmlHelper).GetTypeInfo().GetDeclaredField(nameof(_page));
    Func<HtmlHelper, IDictionary<string, string>, NonEscapedString> _originalRenderer;

    public ProcessingStateRenderer(Func<HtmlHelper, IDictionary<string, string>, NonEscapedString> originalRenderer)
    {
      _originalRenderer = originalRenderer;
    }

    public NonEscapedString Render(HtmlHelper helper, IDictionary<string, string> stateData)
    {
      var builder = new StringBuilder();

      builder.Append("<script src=\"https://cdnjs.cloudflare.com/ajax/libs/aspnet-signalr/1.1.4/signalr.min.js\"></script>");
      builder.Append("<script src=\"https://cdn.jsdelivr.net/npm/chart.js@2.9.3/dist/Chart.min.js\"></script>");
      builder.Append("<dl class=\"dl-horizontal\" id='learning-progress'>");

      builder.Append("<dt>Learning progress</dt>");

      builder.Append("<canvas id=\"training-chart\"></canvas>");
      builder.Append("</dl>");

      var page = (RazorPage)_page.GetValue(helper);

      var url = new Uri(page.RequestPath);
      builder.Append($"<input type='hidden' id='job-id' value='{url.Segments.Last()}'></input>");

      builder.Append("<script defer src=\"/objectRecognition.js\"></script>");

      builder.Append(_originalRenderer(helper, stateData));

      return new NonEscapedString(builder.ToString());
    }
  }
}