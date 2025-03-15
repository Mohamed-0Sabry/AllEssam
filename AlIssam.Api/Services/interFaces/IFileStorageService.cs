namespace AlIssam.API.Services.interFaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(byte[] fileBytes);

        string CombinePathAndFile(string fileName);
    }
}
