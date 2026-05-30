using CloudinaryDotNet.Actions;
using SyncoraBackend.Models.Common;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.interfaces;
/// <summary>
///     Interface for the images repository
/// </summary>
public interface IImagesRepository
{
    Task<UploadSignature> GenerateUploadSignature(int userId);
    Task<string> AddPhotoAsync(IFormFile file);
    bool ValidateUrlString(string url);
}
