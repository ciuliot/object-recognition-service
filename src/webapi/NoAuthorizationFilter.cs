using Hangfire.Dashboard;

namespace Digitalist.ObjectRecognition
{
public class NoAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow all users to see the Dashboard (potentially dangerous).
        return true; //httpContext.User.Identity.IsAuthenticated;
    }
}
}