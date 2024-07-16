using InstagramApiSharp.API;
using InstagramApiSharp.Classes.Models;
using MessageAPI.Models;
using MessageAPI.Services.Interfaces;

namespace MessageAPI.Services.Implementations
{
    public class InstagramApiService : IInstagramApiService
    {
        private readonly IInstaApi _api;

        public string Username => _api.GetLoggedUser().UserName;

        public InstagramApiService(IInstaApi api)
        {
            _api = api;
        }

        public async Task<ResponseData> CommentMediaAsync(string uri, string text)
        {
            string id = await GetMediaIdByUriAsync(uri);

            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Empty id");
                return new() { IsSuccess = false, ErrorMessage = $"Comment media: can't take media id from uri {uri}" };
            }

            //var media = await _api.MediaProcessor.GetMediaByIdAsync(id);

            var commentResult = await _api.CommentProcessor.CommentMediaAsync(id, text);

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

        public async Task<ResponseData> FollowAsync(string username)
        {
            var user = await _api.UserProcessor.GetUserAsync(username);

            if (!user.Succeeded)
            {
                Console.WriteLine($"Error on follow on user {username}: {user.Info.Message}");
                return new() { IsSuccess = false, ErrorMessage = $"Error on follow on user {username}: {user.Info.Message}" };
            }

            var res = await _api.UserProcessor.FollowUserAsync(user.Value.Pk);

            if (!res.Succeeded)
            {
                Console.WriteLine("Error on follow on user: " + res.Info.Message);
                return new() { IsSuccess = false, ErrorMessage = "Error on follow on user: " + res.Info.Message };
            }

            return new();
        }

        public async Task<ResponseData> LikeMediaAsync(string uri)
        {
            string id = await GetMediaIdByUriAsync(uri);

            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Empty id on like media");
                return new() { IsSuccess = false, ErrorMessage = $"Like media: can't take media id from uri {uri}" };
            }

            var content = await _api.MediaProcessor.GetMediaByIdAsync(id);

            if (!content.Succeeded)
            {
                return new() { IsSuccess = false, ErrorMessage = content.Info.Message };
            }

            var likeResponse = await _api.MediaProcessor.LikeMediaAsync(id);

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

        public async Task<ResponseData> LoginAsync(string stateFile = "")
        {
            if (!string.IsNullOrEmpty(stateFile))
            {
                try
                {
                    // load session file if exists
                    if (File.Exists(stateFile))
                    {
                        //Console.WriteLine("Loading state from file");
                        using (var fr = new StreamReader(stateFile))
                        {
                            string data = fr.ReadToEnd();

                            _api.LoadStateDataFromString(data);
                        }
                    }
                    else
                    {
                        File.Create(stateFile);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if (!_api.IsUserAuthenticated)
            {
                await _api.SendRequestsBeforeLoginAsync();

                // wait 5 seconds
                await Task.Delay(5000);

                var logInResult = await _api.LoginAsync();

                if (!logInResult.Succeeded && logInResult.Info.NeedsChallenge)
                {
                    IMailVerifyService verifyService = new MailRuService("kosach_dmitriy@mail.ru", "6bhNM4ZA0rAPjBWCff7V");

                    await Task.Delay(3000);

                    var info = await _api.GetChallengeRequireVerifyMethodAsync();

                    await Task.Delay(2000);

                    var requestVerify = await _api.RequestVerifyCodeToEmailForChallengeRequireAsync(choise: info.Value.StepData.Choice);
                    //_api.RequestVerifyCodeToSMSForChallengeRequireAsync(choise: info.Value.StepData.Choice); 
                    //_api.RequestVerifyCodeToEmailForChallengeRequireAsync(choise: info.Value.StepData.Choice);

                    if (!requestVerify.Succeeded)
                    {
                        return new() { IsSuccess = false, ErrorMessage = $"Error on send request verify code: {requestVerify.Info.Message}" };
                    }

                    await Task.Delay(3000);

                    string code = verifyService.GetVerifyCode();

                    var resChalleng = await _api.VerifyCodeForChallengeRequireAsync(code, choise: info.Value.StepData.Choice);

                    if (!resChalleng.Succeeded)
                    {
                        return new() { IsSuccess = false, ErrorMessage = $"Error on send verify code: {resChalleng.Info.Message}" };
                    }
                }
                else if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                    return new() { IsSuccess = false, ErrorMessage = logInResult.Info.Message };
                }

                if (_api.IsUserAuthenticated)
                {
                    Console.WriteLine("Login is completed");
                }

                if (!string.IsNullOrEmpty(stateFile))
                {
                    var state = _api.GetStateDataAsString();
                    using var sw = new StreamWriter(stateFile);
                    sw.WriteLine(state);
                }
            }

            await _api.SendRequestsAfterLoginAsync();

            return new();
        }

        public async Task<ResponseData> PostImageAsync(string imagePath, string caption, IEnumerable<string> userTags)
        {
            var mediaImage = new InstaImageUpload
            {
                // leave zero, if you don't know how height and width is it.
                Height = 0,
                Width = 0,
                Uri = imagePath
            };

            mediaImage.UserTags
                .AddRange(userTags.Select(u => new InstaUserTagUpload() 
                { 
                    Username = u, 
                    X = 0, 
                    Y = 1 
                }));

            var res = await _api.MediaProcessor.UploadPhotoAsync(mediaImage, caption);

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

        public async Task<ResponseData> RemoveImageAsync(string uri)
        {
            string id = await GetMediaIdByUriAsync(uri);

            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine($"Can't take media id from uri {uri}");
                return new() { IsSuccess = false, ErrorMessage = $"Remove image: can't take media id from uri {uri}" };
            }

            var res = await _api.MediaProcessor.DeleteMediaAsync(id, InstaMediaType.Image);

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

        public async Task<ResponseData> SeeAllUserStoriesAsync(string username)
        {
            var user = await _api.UserProcessor.GetUserAsync(username);

            if (!user.Succeeded)
            {
                Console.WriteLine("See stories: fail on get user by user name");
                return new() { IsSuccess = false, ErrorMessage = user.Info.Message };
            }   

            var res = await _api.StoryProcessor.GetUserStoryAsync(user.Value.Pk);

            if (!res.Succeeded)
            {
                Console.WriteLine("See stories: fail on get user story");
                return new() { IsSuccess = false, ErrorMessage = res.Info.Message };
            }

            foreach (var item in res.Value.Items)
            {
                var isSee = await _api.StoryProcessor.MarkStoryAsSeenAsync(item.Id, 0);

                if (!isSee.Succeeded || !isSee.Value)
                {
                    Console.WriteLine("Fail on see story: " + isSee.Info.Message);
                }
            }

            return new();
        }

        public async Task<ResponseData> SetUserBiographyAsync(string text)
        {
            var setBio = await _api.AccountProcessor.SetBiographyAsync(text);

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

        public async Task<ResponseData> ShareMediaAsStoryAsync(string imagePath, string mediaUri)
        {
            string mediaId = await GetMediaIdByUriAsync(mediaUri);

            if (string.IsNullOrEmpty(mediaId))
            {
                Console.WriteLine($"Can't take media id from uri {mediaUri}");
                return new() { IsSuccess = false, ErrorMessage = $"Share media as story: can't take media id from uri {mediaUri}" };
            }

            var media = await _api.MediaProcessor.GetMediaByIdAsync(mediaId);

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
                Uri = imagePath
            };

            var res = await _api.StoryProcessor.ShareMediaAsStoryAsync(mediaImage, new InstaMediaStoryUpload() { MediaPk = long.Parse(media.Value.Pk) });

            if (!res.Succeeded)
            {
                Console.WriteLine("Error on post  story photo: " + res.Info.Message);
                return new() { IsSuccess = false, ErrorMessage = $"Error on share media as story: {res.Info.Message}" };
            }

            return new();
        }

        public async Task<ResponseData> UnFollowAsync(string username)
        {
            var user = await _api.UserProcessor.GetUserAsync(username);

            if (!user.Succeeded)
            {
                Console.WriteLine($"Error on follow on user {username}: {user.Info.Message}");
                return new() { IsSuccess = false, ErrorMessage = $"Error on get user by username {username}: {user.Info.Message }" };
            }

            var res = await _api.UserProcessor.UnFollowUserAsync(user.Value.Pk);

            if (!res.Succeeded)
            {
                Console.WriteLine("Error on unfollow on user: " + res.Info.Message);
                return new() { IsSuccess = false, ErrorMessage = $"Error on unfollow on user: {res.Info.Message}" };
            }

            return new();
        }

        public async Task<ResponseData> UploadStoryPhotoAsync(string imagePath, string caption)
        {
            var mediaImage = new InstaImage
            {
                // leave zero, if you don't know how height and width is it.
                Height = 0,
                Width = 0,
                Uri = imagePath
            };

            var res = await _api.StoryProcessor.UploadStoryPhotoAsync(mediaImage, caption);

            if (!res.Succeeded)
            {
                Console.WriteLine("Error on post  story photo: " + res.Info.Message);
                return new() { IsSuccess = false, ErrorMessage = $"Error on post  story photo: {res.Info.Message}" };
            }

            return new();
        }

        private async Task<string> GetMediaIdByUriAsync(string uri)
        {
            if (Uri.TryCreate(uri, new UriCreationOptions(), out Uri? res))
            {
                var media = await _api.MediaProcessor.GetMediaIdFromUrlAsync(res);

                if (media.Succeeded)
                {
                    return media.Value;
                }

                Console.WriteLine($"Get media Id by Url error: {media.Info.Message}");
            }

            return string.Empty;
        }

        public async Task<ResponseData> SetUserAvatarAsync(string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);

            var res = await _api.AccountProcessor.ChangeProfilePictureAsync(imageBytes);

            if (!res.Succeeded)
            {
                return new() { IsSuccess = false, ErrorMessage = $"Error on change user avatar: {res.Info.Message}" };
            }

            return new();
        }
    }
}
