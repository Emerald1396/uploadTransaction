using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UploadTransaction.Helpers
{
    public static class Helper
    {
        #region Exception
        public static string ExceptionLineAndFile(Exception ex)
        {
            var st = new StackTrace(ex, true);
            var frame = st.GetFrame(0);
            var line = frame.GetFileLineNumber();

            return frame.GetFileName() + ": Line No. " + frame.GetFileLineNumber();
        }
        #endregion
        public static void loggingFile(string filename, byte[] filecontent)
        {
            var today = DateTime.UtcNow.ToString("ddMMyyyy");
            string logsDirectory = Path.Combine(Environment.CurrentDirectory, "Logs");

            #region Write File
            string filewritePath = logsDirectory + @"\" + today;
            if (File.Exists(filewritePath + @"\" + filename)) File.Delete(filewritePath + @"\" + filename);

            if (!Directory.Exists(filewritePath))
            {
                Directory.CreateDirectory(filewritePath);
            }
            File.WriteAllBytes(filewritePath + @"\" + filename, filecontent);
            #endregion
        }

        public class FileUploadFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var formParameters = context.ApiDescription.ParameterDescriptions
                    .Where(paramDesc => paramDesc.IsFromForm());

                if (formParameters.Any())
                {
                    // already taken care by swashbuckle. no need to add explicitly.
                    return;
                }
                if (operation.RequestBody != null)
                {
                    // NOT required for form type
                    return;
                }
                if (context.ApiDescription.HttpMethod == HttpMethod.Post.Method)
                {
                    var uploadFileMediaType = new OpenApiMediaType()
                    {
                        Schema = new OpenApiSchema()
                        {
                            Type = "object",
                            Properties =
                    {
                        ["files"] = new OpenApiSchema()
                        {
                            Type = "array",
                            Items = new OpenApiSchema()
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        }
                    },
                            Required = new HashSet<string>() { "files" }
                        }
                    };

                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Content = { ["multipart/form-data"] = uploadFileMediaType }
                    };
                }
            }
        }

        internal static bool IsFromForm(this ApiParameterDescription apiParameter)
        {
            var source = apiParameter.Source;
            var elementType = apiParameter.ModelMetadata?.ElementType;

            return (source == BindingSource.Form || source == BindingSource.FormFile)
                || (elementType != null && typeof(IFormFile).IsAssignableFrom(elementType));
        }
    }

}
