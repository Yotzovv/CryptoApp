using CryptoApp.Data.dtos;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CryptoApp.API.Common;

public class FileUploadOperation : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(FileUploadDto))
            .ToArray();

        if (fileParams.Length == 0)
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = GenerateSchema(context)
                }
            }
        };
    }

    private OpenApiSchema GenerateSchema(OperationFilterContext context)
    {
        var schema = context.SchemaGenerator.GenerateSchema(typeof(FileUploadDto), context.SchemaRepository);
        return schema;
    }
}