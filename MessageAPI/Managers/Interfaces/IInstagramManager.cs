using MessageAPI.Models.Implementations;

namespace MessageAPI.Managers.Interfaces
{
    public interface IInstagramManager
    {
        public Task<ResponseData> PostPhotoFromFormfileAsync(Guid sessionId, PostPhoto postPhoto, IFormFile formFile);

        public Task<ResponseData> ShareMediaAsStoryFormfileAsync(Guid sessionId, MediaStory mediaStory, IFormFile formFile);

        public Task<ResponseData> SharePhotoAsStoryFormfileAsync(Guid sessionId, PhotoStory photoStory, IFormFile formFile);

        public Task<ResponseData> SetUserAvatarFromFormfileAsync(Guid sessionId, IFormFile formFile);
    }
}
