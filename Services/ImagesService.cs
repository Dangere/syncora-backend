
using SyncoraBackend.interfaces;
using SyncoraBackend.Models;
using SyncoraBackend.Models.Common;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class ImagesService(IImagesRepository imagesRepository, UserRequestContext userRequestContext, ILogger<ImagesService> logger)
{
    private readonly IImagesRepository _imagesRepository = imagesRepository;

    private readonly UserRequestContext _userRequestContext = userRequestContext;

    private readonly ILogger<ImagesService> _logger = logger;


    /// <summary>
    ///     Generates upload signature for singed uploads made by the client
    /// </summary>
    /// <returns></returns>
    public async Task<Result<UploadSignature>> GenerateUploadSignature()
    {
        try
        {

            UploadSignature signature = await _imagesRepository.GenerateUploadSignature(_userRequestContext.UserId);

            return Result<UploadSignature>.Success(signature);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to generate upload signature.");
            return Result<UploadSignature>.Error("Failed to generate upload signature.", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError);
        }

    }


    public async Task<Result<string>> AddPhotoAsync(IFormFile file)
    {
        try
        {
            string imageUrl = await _imagesRepository.AddPhotoAsync(file);

            return Result<string>.Success(imageUrl);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add photo.");
            return Result<string>.Error("Failed to add photo.", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    ///     Checks if the image url is valid and issued by the image repository 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<Result<bool>> ValidateUrlString(string url)
    {
        try
        {
            return Result<bool>.Success(_imagesRepository.ValidateUrlString(url));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Invalid image URL.");
            return Result<bool>.Error("Invalid image URL.", ErrorCodes.INVALID_URL);
        }
    }


}
