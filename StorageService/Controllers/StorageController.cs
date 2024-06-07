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
    public async Task<IActionResult> Add(IFormFile file)
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

            await using var context = new StorageContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            await file.CopyToAsync(output);
            context.Items.Add(newItem);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.InnerException?.Message ?? e.Message);
        }
    }

    [HttpGet("GetInfo")]
    public async Task<IActionResult> GetInfo(Guid id)
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

    [HttpGet("GetFile")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        try
        {
            await using var context = new StorageContext();
            var item = await context.Items.FindAsync(id);

            string path = GetPath(id);
            if (System.IO.File.Exists(path))
            {
                return File(System.IO.File.OpenRead(path), "application/octet-stream", item!.Name);
            }

            return NotFound();
        }
        catch (Exception e)
        {
            return BadRequest(e.InnerException?.Message ?? e.Message);
        }
    }

    [HttpGet("Exists")]
    public async Task<IActionResult> Exists(Guid id)
    {
        await using var context = new StorageContext();
        return Ok(await context.Items.FindAsync(id));
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await using var context = new StorageContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var item = await context.Items.FindAsync(id);

            context.Items.Remove(item!);
            System.IO.File.Delete(GetPath(id));
            
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            
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
