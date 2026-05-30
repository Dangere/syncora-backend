using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using SyncoraBackend.interfaces;
using SyncoraBackend.Models.Common;

namespace SyncoraBackend.Repositories;
/// <summary>
///    Implements the IImagesRepository interface to use cloudinary to upload images
/// </summary>
/// <param name="cloudinary"></param>
public class CloudinaryImageRepository(Cloudinary cloudinary, IConfiguration configuration) : IImagesRepository
{
    private readonly Cloudinary _cloudinary = cloudinary;
    private readonly IConfiguration _configuration = configuration;
    public Task<string> AddPhotoAsync(IFormFile file)
    {
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = true,
            AssetFolder = "images",
        };

        ImageUploadResult uploadResult = _cloudinary.Upload(uploadParams);

        return Task.FromResult(uploadResult.Url.ToString());
    }

    // The signature is used to upload directly to cloudinary,
    // all the parameters are gonna be also requested when uploading to be validated against the signature
    public Task<UploadSignature> GenerateUploadSignature(int userId)
    {
        _cloudinary.Api.SignatureAlgorithm = SignatureAlgorithm.SHA256;
        var uploadParams = new ImageUploadParams()
        {
            Timestamp = DateTime.UtcNow,
            AssetFolder = $"profiles/{userId}/",
            Context = { { "user_id", userId.ToString() } },
            UploadPreset = "profile-photos",
        };
        var profileSignature = _cloudinary.Api.SignParameters(uploadParams.ToParamsDictionary());

        return Task.FromResult(new UploadSignature(profileSignature, uploadParams.ToParamsDictionary().ToDictionary()));
    }

    public bool ValidateUrlString(string url)
    {
        string cloudname = _configuration["CloudinaryConfig:CloudName"]!;
        return url.StartsWith($"http://res.cloudinary.com/{cloudname}/image", StringComparison.OrdinalIgnoreCase);
    }
}
