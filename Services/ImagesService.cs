
using SyncoraBackend.interfaces;
using SyncoraBackend.Models;
using SyncoraBackend.Models.Common;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class ImagesService(IImagesRepository imagesRepository, UserRequestContext userRequestContext)
{
    private readonly IImagesRepository _imagesRepository = imagesRepository;

    private readonly UserRequestContext _userRequestContext = userRequestContext;



    public async Task<Result<UploadSignature>> GenerateUploadSignature()
    {
        try
        {

            UploadSignature signature = await _imagesRepository.GenerateUploadSignature(_userRequestContext.UserId);

            return Result<UploadSignature>.Success(signature);
        }
        catch (Exception)
        {
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
        catch (Exception)
        {
            return Result<string>.Error("Failed to add photo.", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError);
        }
    }


    public async Task<Result<bool>> ValidateUrlString(string url)
    {
        try
        {
            return Result<bool>.Success(await _imagesRepository.ValidateUrlString(url));
        }
        catch (System.Exception)
        {
            return Result<bool>.Error("Invalid image URL.", ErrorCodes.INVALID_URL);
        }
    }


}
