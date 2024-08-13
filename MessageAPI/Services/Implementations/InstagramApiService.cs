using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes.Models;
using MessageAPI.Models.Implementations;
using MessageAPI.Models.Interfaces;
using MessageAPI.Services.Interfaces;
using IModelSession = MessageAPI.Models.Interfaces.ISession;

namespace MessageAPI.Services.Implementations
{
    public class InstagramApiService : IInstagramApiService
    {
        private readonly ISessionService _sessionService;
        private readonly string _registrationApi;
        //private readonly IInstaApi? _api;
        
        public InstagramApiService(ISessionService sessionService,
                                        IConfiguration config)
        {
            _sessionService = sessionService;
            _registrationApi = config["RegistrationApi"] ?? throw new KeyNotFoundException("Ошибка в получении адреса апи для регистрации");
        }

        public async Task<ResponseData> CommentMediaAsync(Guid sessionId, CommentMedia comment)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            string id = await GetMediaIdByUriAsync(api, comment.Uri);

            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Empty id");
                return new() { IsSuccess = false, ErrorMessage = $"Comment media: can't take media id from uri {comment.Uri}" };
            }

            //var media = await _api.MediaProcessor.GetMediaByIdAsync(id);

            var commentResult = await api.CommentProcessor.CommentMediaAsync(id, comment.Text);

            if (commentResult.Succeeded)
            {
                Console.WriteLine($"Comment created: {commentResult.Value.Pk}, text: {commentResult.Value.Text}");
                return new();
            }
            else
            {
                Console.WriteLine($"Unable to create comment: {commentResult.Info.Message}");
                return new() { IsSuccess = false, ErrorMessage = commentResult.Info.Message };
            }
        }

        public async Task<ResponseData> FollowAsync(Guid sessionId, string username)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var user = await api.UserProcessor.GetUserAsync(username);

            if (!user.Succeeded)
            {
                Console.WriteLine($"Error on follow on user {username}: {user.Info.Message}");
                return new() { IsSuccess = false, ErrorMessage = $"Error on follow on user {username}: {user.Info.Message}" };
            }

            var res = await api.UserProcessor.FollowUserAsync(user.Value.Pk);

            if (!res.Succeeded)
            {
                Console.WriteLine("Error on follow on user: " + res.Info.Message);
                return new() { IsSuccess = false, ErrorMessage = "Error on follow on user: " + res.Info.Message };
            }

            return new();
        }

        public async Task<ResponseData> LikeMediaAsync(Guid sessionId, string uri)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            string id = await GetMediaIdByUriAsync(api, uri);

            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Empty id on like media");
                return new() { IsSuccess = false, ErrorMessage = $"Like media: can't take media id from uri {uri}" };
            }

            var content = await api.MediaProcessor.GetMediaByIdAsync(id);

            if (!content.Succeeded)
            {
                return new() { IsSuccess = false, ErrorMessage = content.Info.Message };
            }

            var likeResponse = await api.MediaProcessor.LikeMediaAsync(id);

            if (likeResponse.Succeeded)
            {
                Console.WriteLine($"Successfully liked {content.Value.User.UserName}'s media");
                return new();
            }
            else
            {
                Console.WriteLine($"Failed liked {content.Value.User.UserName}'s media");

                return new() { IsSuccess = false, ErrorMessage = likeResponse.Info.Message };
            }
        }

        public async Task<ResponseData<IModelSession>> LoginAsync(User user)
        {
            IInstaApi api = InstaApiBuilder.CreateBuilder()
                                            .SetUser(new() { UserName = user.InstaLogin, Password = user.InstaPasswordHash })
                                            .Build();

            if (user.State is not null)
            {
                try
                {
                    api.LoadStateDataFromObject(user.State);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (!api.IsUserAuthenticated)
            {
                await api.SendRequestsBeforeLoginAsync();

                // wait 5 seconds
                await Task.Delay(5000);

                var logInResult = await api.LoginAsync();

                if (!logInResult.Succeeded && logInResult.Info.NeedsChallenge)
                {
                    ResponseData mailChallenge = await DoMailChallenge(api, user);

                    if (!mailChallenge.IsSuccess)
                    {
                        return new() { IsSuccess = false, ErrorMessage = mailChallenge.ErrorMessage };
                    }
                }
                else if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                    return new() { IsSuccess = false, ErrorMessage = logInResult.Info.Message };
                }

                if (!api.IsUserAuthenticated)
                {
                    return new() { IsSuccess = false, ErrorMessage = "Login to account: unknown error" };
                }
            }

            await api.SendRequestsAfterLoginAsync();

            user.State = api.GetStateDataAsObject();

            Session session = new(user, api);

            await _sessionService.AddSession(session);

            return new ResponseData<IModelSession>() { Data = session };
        }

        public async Task<ResponseData> LogoutAsync(Guid sessionId)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var res = await api.LogoutAsync();

            if (!res.Succeeded)
            {
                return new() { IsSuccess = false, ErrorMessage = res.Info.Message };
            }

            return new();
        }

        public async Task<ResponseData> PostImageAsync(Guid sessionId, PostPhoto postPhoto)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var mediaImage = new InstaImageUpload
            {
                // leave zero, if you don't know how height and width is it.
                Height = 0,
                Width = 0,
                Uri = postPhoto.ImageUri
            };

            mediaImage.UserTags
                .AddRange(postPhoto.UserTags.Select(u => new InstaUserTagUpload() 
                { 
                    Username = u, 
                    X = 0, 
                    Y = 1 
                }));

            var res = await api.MediaProcessor.UploadPhotoAsync(mediaImage, postPhoto.Caption);

            if (!res.Succeeded)
            {
                Console.WriteLine($"Fail on create image: {res.Info.Message}");
                return new() { IsSuccess = false, ErrorMessage = res.Info.Message };
            }
            else
            {
                Console.WriteLine("Photo successfully created");
                return new();
            }
        }

        public async Task<ResponseData> RemoveImageAsync(Guid sessionId, string uri)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            string id = await GetMediaIdByUriAsync(api, uri);

            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine($"Can't take media id from uri {uri}");
                return new() { IsSuccess = false, ErrorMessage = $"Remove image: can't take media id from uri {uri}" };
            }

            var res = await api.MediaProcessor.DeleteMediaAsync(id, InstaMediaType.Image);

            if (!res.Succeeded)
            {
                Console.WriteLine($"Fail on delete image: {res.Info.Message}");
                return new() { IsSuccess = false, ErrorMessage = res.Info.Message };
            }
            else
            {
                Console.WriteLine("Photo successfully deleted");
                return new();
            }
        }

        public async Task<ResponseData> SeeAllUserStoriesAsync(Guid sessionId, string username)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var user = await api.UserProcessor.GetUserAsync(username);

            if (!user.Succeeded)
            {
                Console.WriteLine("See stories: fail on get user by user name");
                return new() { IsSuccess = false, ErrorMessage = user.Info.Message };
            }   

            var res = await api.StoryProcessor.GetUserStoryAsync(user.Value.Pk);

            if (!res.Succeeded)
            {
                Console.WriteLine("See stories: fail on get user story");
                return new() { IsSuccess = false, ErrorMessage = res.Info.Message };
            }

            foreach (var item in res.Value.Items)
            {
                var isSee = await api.StoryProcessor.MarkStoryAsSeenAsync(item.Id, 0);

                if (!isSee.Succeeded || !isSee.Value)
                {
                    Console.WriteLine("Fail on see story: " + isSee.Info.Message);
                }
            }

            return new();
        }

        public async Task<ResponseData> SetUserBiographyAsync(Guid sessionId, string text)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var setBio = await api.AccountProcessor.SetBiographyAsync(text);

            if (setBio.Succeeded)
            {
                return new();
            }
            else
            {
                Console.WriteLine($"Eror on set bio: {setBio.Info.Message}");
                return new() { IsSuccess = false, ErrorMessage = setBio.Info.Message };
            }
        }

        public async Task<ResponseData> ShareMediaAsStoryAsync(Guid sessionId, MediaStory mediaStory)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            string mediaId = await GetMediaIdByUriAsync(api, mediaStory.MediaUri);

            if (string.IsNullOrEmpty(mediaId))
            {
                Console.WriteLine($"Can't take media id from uri {mediaStory.MediaUri}");
                return new() { IsSuccess = false, 
                        ErrorMessage = $"Share media as story: can't take media id from uri {mediaStory.MediaUri}" };
            }

            var media = await api.MediaProcessor.GetMediaByIdAsync(mediaId);

            if (!media.Succeeded)
            {
                Console.WriteLine("Error on get media by id: " + media.Info.Message);
                return new() { IsSuccess = false, ErrorMessage = media.Info.Message };
            }

            var mediaImage = new InstaImage
            {
                // leave zero, if you don't know how height and width is it.
                Height = 0,
                Width = 0,
                Uri = mediaStory.ImageUri
            };

            var res = await api.StoryProcessor.ShareMediaAsStoryAsync(mediaImage, new InstaMediaStoryUpload() { MediaPk = long.Parse(media.Value.Pk) });

            if (!res.Succeeded)
            {
                Console.WriteLine("Error on post  story photo: " + res.Info.Message);
                return new() { IsSuccess = false, ErrorMessage = $"Error on share media as story: {res.Info.Message}" };
            }

            return new();
        }

        public async Task<ResponseData> UnFollowAsync(Guid sessionId, string username)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var user = await api.UserProcessor.GetUserAsync(username);

            if (!user.Succeeded)
            {
                Console.WriteLine($"Error on follow on user {username}: {user.Info.Message}");
                return new() { IsSuccess = false, ErrorMessage = $"Error on get user by username {username}: {user.Info.Message }" };
            }

            var res = await api.UserProcessor.UnFollowUserAsync(user.Value.Pk);

            if (!res.Succeeded)
            {
                Console.WriteLine("Error on unfollow on user: " + res.Info.Message);
                return new() { IsSuccess = false, ErrorMessage = $"Error on unfollow on user: {res.Info.Message}" };
            }

            return new();
        }

        public async Task<ResponseData> UploadStoryPhotoAsync(Guid sessionId, PhotoStory photoStory)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var mediaImage = new InstaImage
            {
                // leave zero, if you don't know how height and width is it.
                Height = 0,
                Width = 0,
                Uri = photoStory.ImageUri
            };

            var res = await api.StoryProcessor.UploadStoryPhotoAsync(mediaImage, photoStory.Caption);

            if (!res.Succeeded)
            {
                Console.WriteLine("Error on post  story photo: " + res.Info.Message);
                return new() { IsSuccess = false, ErrorMessage = $"Error on post  story photo: {res.Info.Message}" };
            }

            return new();
        }

        public async Task<ResponseData> SetUserAvatarAsync(Guid sessionId,  string imagePath)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            byte[] imageBytes = File.ReadAllBytes(imagePath);

            var res = await api.AccountProcessor.ChangeProfilePictureAsync(imageBytes);

            if (!res.Succeeded)
            {
                return new() { IsSuccess = false, ErrorMessage = $"Error on change user avatar: {res.Info.Message}" };
            }

            return new();
        }

        public async Task<ResponseData<InstaSectionMedia>> GetMediaByTagAsync(Guid sessionId, string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return new() { IsSuccess = false, ErrorMessage = "Tag cannot be empty" };

            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var res = await api.HashtagProcessor.GetRecentHashtagMediaListAsync(tag, PaginationParameters.MaxPagesToLoad(1));

            if (!res.Succeeded)
            {
                return new() { IsSuccess = false, ErrorMessage = res.Info.Message };
            }

            return new() {  Data = res.Value };
        }

        public async Task<ResponseData<InstaMediaList>> GetMediaByUserAsync(Guid sessionId, string username)
        {
            if (string.IsNullOrEmpty(username))
                return new() { IsSuccess = false, ErrorMessage = "Tag cannot be empty" };

            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var res = await api.UserProcessor.GetUserMediaAsync(username, PaginationParameters.MaxPagesToLoad(1));

            if (!res.Succeeded)
            {
                return new() { IsSuccess = false, ErrorMessage = res.Info.Message };
            }

            return new() { Data = res.Value };
        }

        public async Task<ResponseData<User>> RegistrationAsync(MailData mailData)
        {
            User user = new() { InstaLogin = "", InstaPasswordHash = "", MailLogin = mailData.Login, MailPasswordHash = mailData.Password };

            using(HttpClient  httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.PostAsync($"{_registrationApi}/create_account", JsonContent.Create(user));

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    RegistrationResponse? regResponse = await response.Content.ReadFromJsonAsync<RegistrationResponse>();

                    if (regResponse is null || regResponse.Data is null)
                    {
                        return new() { IsSuccess = false, ErrorMessage = "Cannot convert registration response to object" };
                    }

                    user.InstaLogin = regResponse.Data.Username;
                    user.InstaPasswordHash = regResponse.Data.Password;

                    return new() { Data = user };
                }
                else
                {
                    RegistrationResponse? regResponse = await response.Content.ReadFromJsonAsync<RegistrationResponse>();

                    return new() { IsSuccess = false, ErrorMessage = regResponse?.Message ?? "Cannot convert registration response to object" };
                }
            }
        }

        public async Task<ResponseData> TestAsync(Guid sessionId)
        {
            if (!_sessionService.Exist(sessionId))
                return new() { IsSuccess = false, ErrorMessage = $"session with id {sessionId} does not exist" };

            IInstaApi api = _sessionService[sessionId].InstaApi;

            var curUser = await api.GetCurrentUserAsync();

            if (!curUser.Succeeded)
                return new() { IsSuccess = false, ErrorMessage = "Get current user fail" };

            var data = await api.UserProcessor.GetFullUserInfoAsync(curUser.Value.Pk);

            return new();
        }

        private static async Task<string> GetMediaIdByUriAsync(IInstaApi api, string uri)
        {
            if (Uri.TryCreate(uri, new UriCreationOptions(), out Uri? res))
            {
                var media = await api.MediaProcessor.GetMediaIdFromUrlAsync(res);

                if (media.Succeeded)
                {
                    return media.Value;
                }

                Console.WriteLine($"Get media Id by Url error: {media.Info.Message}");
            }

            return string.Empty;
        }

        private static async Task<ResponseData<IModelSession>> DoMailChallenge(IInstaApi api, IUser user)
        {
            IMailVerifyService verifyService = new MailRuService(user.MailLogin, user.MailPasswordHash);

            await Task.Delay(3000);

            var info = await api.GetChallengeRequireVerifyMethodAsync();

            if (!info.Succeeded) 
            {
                return new() { IsSuccess = false, ErrorMessage = info.Info.Message };
            }

            if (info.Value.StepData.Choice != "0" && info.Value.StepData.Choice != "1")
            {
                return new() { IsSuccess = false, ErrorMessage = $"Choise = {info.Value.StepData.Choice}. Possibly, you logged into your account from another device." };
            }

            await Task.Delay(2000);

            var requestVerify = await api.RequestVerifyCodeToEmailForChallengeRequireAsync();//choise: info.Value.StepData.Choice);

            if (!requestVerify.Succeeded)
            {
                return new() { IsSuccess = false, ErrorMessage = $"Error on send request verify code: {requestVerify.Info.Message}" };
            }

            await Task.Delay(3000);

            string code = verifyService.GetVerifyCode();

            var resChalleng = await api.VerifyCodeForChallengeRequireAsync(code);//, choise: info.Value.StepData.Choice);

            if (!resChalleng.Succeeded)
            {
                return new() { IsSuccess = false, ErrorMessage = $"Error on send verify code: {resChalleng.Info.Message}" };
            }

            return new();
        }
    }
}

public class Bob
{
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
}

