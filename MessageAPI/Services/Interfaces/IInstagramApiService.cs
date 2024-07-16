using MessageAPI.Models;

namespace MessageAPI.Services.Interfaces
{
    public interface IInstagramApiService
    {

        public string Username { get; }
        public Task<ResponseData> PostImageAsync(string imagePath, string caption, IEnumerable<string> userTags);

        public Task<ResponseData> RemoveImageAsync(string uri);

        public Task<ResponseData> LoginAsync(string stateFile = "");

        public Task<ResponseData> CommentMediaAsync(string uri, string text);

        public Task<ResponseData> LikeMediaAsync(string uri);

        public Task<ResponseData> SetUserBiographyAsync(string text);

        public Task<ResponseData> SetUserAvatarAsync(string imagePath);

        public Task<ResponseData> FollowAsync(string username);

        public Task<ResponseData> UnFollowAsync(string username);

        public Task<ResponseData> UploadStoryPhotoAsync(string imagePath, string caption);

        public Task<ResponseData> ShareMediaAsStoryAsync(string imagePath, string mediaUri);

        public Task<ResponseData> SeeAllUserStoriesAsync(string username);
    }
}
