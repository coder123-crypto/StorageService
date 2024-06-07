namespace StorageService.Data;

public class StorageItem : UploadedFileData
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Extension { get; set; }

    public string? OriginalSource { get; set; }

    public DateTime UploadedDate { get; set; }
}