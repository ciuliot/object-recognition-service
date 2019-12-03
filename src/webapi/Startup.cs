using Digitalist.ObjectRecognition.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Amazon.S3;
using Amazon;
using Hangfire;
using Hangfire.MemoryStorage;
using System;
using Hangfire.Dashboard;
using Digitalist.ObjectRecognition.Hubs;
using Digitalist.ObjectRecognition.Jobs;

namespace Digitalist.ObjectRecognition
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddHangfire(config =>
      {
        config.UseMemoryStorage();
      });
      services.AddMvc();
      services.AddSignalR();
      services.AddLogging();
      services.AddControllers();

      services.AddSingleton<DarknetService>();

      var config = new AmazonS3Config
      {
        RegionEndpoint = RegionEndpoint.GetBySystemName(Configuration["AWS_REGION"]),
        ServiceURL = Configuration["AWS_SERVICE_URL"],
        ForcePathStyle = true
      };

      System.Console.WriteLine(config.RegionEndpoint);

      services.AddSingleton(new AmazonS3Client(
        Configuration["AWS_ACCESS_KEY"],
        Configuration["AWS_SECRET_KEY"],
        config));
 
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient backgroundJobs)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseRouting();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapRazorPages();
        endpoints.MapControllers();

        endpoints.MapHub<DarknetJobHub>("/darknetHub");
      });      

      app.UseSwagger();

      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Object recognition API V1");
        c.RoutePrefix = string.Empty;
      });

      app.UseHttpsRedirection();
      

      JobsSidebarMenu.Items.Add(page => new MenuItem("Training details", page.Url.To("/../TrainingJobsPage"))
      {
        Active = page.RequestPath.StartsWith("/jobs/training"),
        Metric = DashboardMetrics.ProcessingCount
      });

      app.UseHangfireDashboard("/dashboard", new DashboardOptions
      {
        Authorization = new [] { new NoAuthorizationFilter() }
      });
      app.UseHangfireServer();      

      app.UseStaticFiles();      
    }
  }
}
