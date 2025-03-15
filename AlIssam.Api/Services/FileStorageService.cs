using AlIssam.API.Services.interFaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AlIssam.API.Services
{
    /// <summary>
    /// Manages file storage operations for application assets
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _uploadPath;

        public FileStorageService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;

            string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            _uploadPath = Path.Combine(webRoot, "Images");

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        /// <summary>
        /// Saves a file to persistent storage
        /// </summary>
        /// <param name="fileBytes">The file contents as byte array</param>
        /// <returns>The generated filename with extension</returns>
        /// <exception cref="IOException">Thrown when file write operations fail</exception>
        public async Task<string> SaveFileAsync(byte[] fileBytes)
        {
            string fileName = GenerateFileName(fileBytes);
            bool isSaved = await SaveFileToStorage(fileBytes, fileName);
            if (!isSaved)
                throw new Exception("Error: Could not save the file!");

            return fileName;
        }

        private async Task<bool> SaveFileToStorage(byte[] fileBytes, string fileName)
        {
            string fileAbsolutePath = Path.Combine(_uploadPath, fileName);

            try
            {
                await using var fileStream = new FileStream(fileAbsolutePath, FileMode.Create);
                await fileStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
                return false;
            }
        }

        private static string GenerateFileName(byte[] fileBytes)
        {
            string fileExtension = FileExtesnionHelper.TryGetExtension(fileBytes);
            string randomString = Guid.NewGuid().ToString().Replace("-", "");
            return $"{randomString}.{fileExtension}";
        }

        public string CombinePathAndFile(string fileName)
        {
            return Path.Combine(_uploadPath, fileName);
        }
    }
}


//using AlIssam.API.Services.interFaces;

//namespace AlIssam.API.Services
//{
//    public class FileStorageService: IFileStorageService
//    {
//        private readonly IWebHostEnvironment _env;
//        private readonly string _uploadPath = "C:\\Users\\H1202\\Desktop\\AlIssam\\AlIssam\\AlIssam.Api\\wwwroot\\Images\\";

//        public FileStorageService(IWebHostEnvironment env, IConfiguration configuration)
//        {
//            _env = env;
//             _uploadPath = Path.Combine(_env.WebRootPath, configuration["FileStorage:UploadPath"]);

//        }
//        public async Task<string> SaveFileAsync(byte[] fileBytes)
//        {
//            string fileName = GenerateFileName(fileBytes);
//            var isSaved = await SaveFileToStorage(fileBytes, fileName);
//            if (!isSaved)
//                throw new Exception("couldn't  do it boss");

//            return fileName;
//        }
//        private async Task<bool> SaveFileToStorage(byte[] fileBytes, string fileName)
//        {
//            string fileAbsolutePath = Path.Combine(_uploadPath, fileName);

//            using var fileStream = new FileStream(fileAbsolutePath, FileMode.Create);

//            await fileStream.WriteAsync(fileBytes);
//            return true;
//        }
//        private static string GenerateFileName(byte[] fileBytes)
//        {
//            string fileExtension = FileExtesnionHelper.TryGetExtension(fileBytes);

//            var randomString = Guid.NewGuid().ToString().Replace("-", "");
//            string FileName = randomString + "." + fileExtension;
//            return FileName;
//        }

//        private string GetStoragePath() => _uploadPath;

//        public string CombinePathAndFile(string fileName)
//        {
//            return GetStoragePath() + fileName;
//        }

//    }
//}
