using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider) : ControllerBase
{
  [HttpGet("{fileId}")]
  public async Task<IActionResult> GetFile(string fileId)
  {
    // actually look up file based on ID
    // demo
    var pathToFile = "default.xml";

    if (!System.IO.File.Exists(pathToFile))
    {
      return NotFound();
    }

    if (!fileExtensionContentTypeProvider.TryGetContentType(pathToFile, out var contentType))
    {
      contentType = "application/octet-stream";
    }

    var bytes = await System.IO.File.ReadAllBytesAsync(pathToFile);
    return File(bytes, contentType, Path.GetFileName(pathToFile));
  }

  [HttpPost]
  public async Task<IActionResult> CreateFile( IFormFile file)
  {
    if (file.Length is 0 || file.Length > 20971520 || file.ContentType != "application/pdf")
    {
      return BadRequest("No file or an invalid one was provided.");
    }

    var path = Path.Combine(Directory.GetCurrentDirectory(), $"uploaded_file_{Guid.NewGuid()}.pdf");
    await using var stream = new FileStream(path, FileMode.Create);
    await file.CopyToAsync(stream);
    return Ok("File uploaded successfully.");
  }
}
