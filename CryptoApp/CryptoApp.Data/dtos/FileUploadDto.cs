namespace CryptoApp.Data.dtos;

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class FileUploadDto
{
    [Required]
    [Display(Name = "file")]
    public IFormFile File { get; set; }
}
