namespace StorageService.Data;

public class StorageItem
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Extension { get; set; }

    public string? OriginalSource { get; set; }

    public DateTime UploadedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public string? OriginalPath { get; set; }
}