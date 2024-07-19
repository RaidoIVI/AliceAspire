using MessageAPI.Managers.Interfaces;
using MessageAPI.Models.Implementations;
using MessageAPI.Services.Interfaces;

namespace MessageAPI.Managers.Implementations
{
    public class InstagramManager : IInstagramManager
    {
        private IInstagramApiService _instagramService;
        private readonly string _imageFolder;

        public InstagramManager(IInstagramApiService instagramService, IWebHostEnvironment env)
        { 
            _instagramService = instagramService;
            _imageFolder = Path.Combine(env.WebRootPath, "Images");
        }


        public async Task<ResponseData> PostPhotoFromFormfileAsync(Guid sessionId, PostPhoto postPhoto, IFormFile formFile)
        {
            try
            {
                string imagePath = await SaveImage(formFile);

                postPhoto.ImageUri = imagePath;

                ResponseData res = await _instagramService.PostImageAsync(sessionId, postPhoto);

                File.Delete(imagePath);

                return res;
            }
            catch (IOException ex)
            {
                return new() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ResponseData> SetUserAvatarFromFormfileAsync(Guid sessionId, IFormFile formFile)
        {
            try
            {
                string imagePath = await SaveImage(formFile);

                ResponseData res = await _instagramService.SetUserAvatarAsync(sessionId, imagePath);

                File.Delete(imagePath);

                return res;
            }
            catch (IOException ex)
            {
                return new() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ResponseData> ShareMediaAsStoryFormfileAsync(Guid sessionId, MediaStory mediaStory, IFormFile formFile)
        {
            try
            {
                string imagePath = await SaveImage(formFile);

                mediaStory.MediaUri = imagePath;

                ResponseData res = await _instagramService.ShareMediaAsStoryAsync(sessionId, mediaStory);

                File.Delete(imagePath);

                return res;
            }
            catch (IOException ex)
            {
                return new() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ResponseData> SharePhotoAsStoryFormfileAsync(Guid sessionId, PhotoStory photoStory, IFormFile formFile)
        {
            try
            {
                string imagePath = await SaveImage(formFile);

                photoStory.ImageUri = imagePath;

                ResponseData res = await _instagramService.UploadStoryPhotoAsync(sessionId, photoStory);

                File.Delete(imagePath);

                return res;
            }
            catch (IOException ex)
            {
                return new() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<string> SaveImage(IFormFile formFile)
        {
            var ext = Path.GetExtension(formFile.FileName);
            var fName = Path.ChangeExtension(Path.GetRandomFileName(), ext);

            string filePath = Path.Combine(_imageFolder, fName);

            using Stream fileStream = new FileStream(filePath, FileMode.Create);
            await formFile.CopyToAsync(fileStream);

            return filePath;
        }
    }
}
