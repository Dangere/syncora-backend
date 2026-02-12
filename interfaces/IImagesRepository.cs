using CloudinaryDotNet.Actions;
using SyncoraBackend.Models.Common;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.interfaces;

public interface IImagesRepository
{
    Task<UploadSignature> GenerateUploadSignature(int userId);
    Task<string> AddPhotoAsync(IFormFile file);
    Task<bool> ValidateUrlString(string url);
}
