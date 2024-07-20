using MessageAPI.Models.Implementations;
using MessageAPI.Models.Interfaces;
using IModelSession = MessageAPI.Models.Interfaces.ISession;

namespace MessageAPI.Services.Interfaces
{
    public interface IInstagramApiService
    {
        public Task<ResponseData> PostImageAsync(Guid sessionId, PostPhoto postPhoto);

        public Task<ResponseData> RemoveImageAsync(Guid sessionId, string uri);

        public Task<ResponseData<IModelSession>> LoginAsync(User user);

        public Task<ResponseData> LogoutAsync(Guid sessionId);

        public Task<ResponseData> CommentMediaAsync(Guid sessionId, CommentMedia comment);

        public Task<ResponseData> LikeMediaAsync(Guid sessionId, string uri);

        public Task<ResponseData> SetUserBiographyAsync(Guid sessionId, string text);

        public Task<ResponseData> SetUserAvatarAsync(Guid sessionId, string imagePath);

        public Task<ResponseData> FollowAsync(Guid sessionId, string username);

        public Task<ResponseData> UnFollowAsync(Guid sessionId, string username);

        public Task<ResponseData> UploadStoryPhotoAsync(Guid sessionId, PhotoStory photoStory);

        public Task<ResponseData> ShareMediaAsStoryAsync(Guid sessionId, MediaStory mediaStory);

        public Task<ResponseData> SeeAllUserStoriesAsync(Guid sessionId, string username);
    }
}
