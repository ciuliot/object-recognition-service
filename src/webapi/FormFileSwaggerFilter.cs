/*using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger; 
using System.Linq;

namespace Digitalist.ObjectRecognition
{
  /// <summary>
  /// Filter to enable handling file upload in swagger
  /// </summary>
  public class FormFileSwaggerFilter : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      if (operation.OperationId.ToLower() == "apivaluesuploadpost")
      {
        operation.Parameters.Clear();
        operation.Parameters.Add(new NonBodyParameter
        {
          Name = "uploadedFile",
          In = "formData",
          Description = "Upload File",
          Required = true,
          Type = "file"
        });
        operation.Consumes.Add("multipart/form-data");
      }
    }
  }
}*/