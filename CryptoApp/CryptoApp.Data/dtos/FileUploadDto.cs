namespace CryptoApp.Data.dtos;

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class FileUploadDto
{
    [Required]
    public IFormFile File { get; set; }
}
