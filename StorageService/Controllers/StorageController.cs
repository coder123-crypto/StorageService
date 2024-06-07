using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StorageService.Data;

namespace StorageService.Controllers;

[Route("[controller]")]
[ApiController]
public class StorageController(IConfiguration configuration) : ControllerBase
{
    [HttpPost("Add")]
    public async Task<ActionResult> Add(IFormFile file)
    {
        try
        {
            var form = HttpContext.Request.Form;
            var newItem = new StorageItem
            {
                Id = Guid.NewGuid(),
                UploadedDate = DateTime.Now,
                OriginalSource = HttpContext.Connection.RemoteIpAddress!.ToString(),
                ModifiedDate = JsonSerializer.Deserialize<DateTime>(form["ModifiedDate"]!),
                OriginalPath = file.FileName,
                Name = Path.GetFileName(file.FileName),
                Extension = Path.GetExtension(file.FileName)
            };

            string path = GetPath(newItem.Id);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using var output = System.IO.File.Open(path, FileMode.Create);
            await file.CopyToAsync(output);

            await using var context = new StorageContext();
            context.Items.Add(newItem);
            await context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.InnerException?.Message ?? e.Message);
        }
    }

    [HttpGet("Get")]
    public async Task<ActionResult> Get(Guid id)
    {
        try
        {
            await using var context = new StorageContext();
            var item = await context.Items.FindAsync(id);

            if (item != null)
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<StorageItem, DownloadedFileData>());
                return Ok(config.CreateMapper().Map<DownloadedFileData>(item));
            }

            return NotFound();
        }
        catch (Exception e)
        {
            return BadRequest(e.InnerException?.Message ?? e.Message);
        }
    }

    [HttpGet("Exists")]
    public ActionResult Exists(Guid id)
    {
        return Ok(System.IO.File.Exists(GetPath(id)));
    }

    [HttpDelete("Delete")]
    public ActionResult Delete(Guid id)
    {
        try
        {
            System.IO.File.Delete(GetPath(id));

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.InnerException?.Message ?? e.Message);
        }
    }

    private string GetPath(Guid id)
    {
        string one = id.ToString()[..2];
        string two = id.ToString().Substring(2, 2);
        string path = Path.Combine(_root, one, two, id.ToString());
        return path;
    }

    private readonly string _root = configuration["Root"]!;
}
