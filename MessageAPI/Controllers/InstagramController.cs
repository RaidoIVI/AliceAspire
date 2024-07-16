using MessageAPI.Models;
using MessageAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace MessageAPI.Controllers
{
    [Route("api/instagram")]
    [ApiController]
    public class InstagramController : ControllerBase
    {
        private readonly IInstagramApiService _instagramService;
        private readonly string _imageFolder;

        public InstagramController(IInstagramApiService instagramService, IWebHostEnvironment env)
        {
            _instagramService = instagramService;
            _imageFolder = Path.Combine(env.WebRootPath, "Images");
        }

        [HttpPost("comment")]
        public async Task<IActionResult> PostComment([FromQuery] string uri, [FromQuery] string text)
        {
            ResponseData res = await _instagramService.CommentMediaAsync(uri, text);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("like")]
        public async Task<IActionResult> PostLike([FromQuery] string uri)
        {
            ResponseData res = await _instagramService.LikeMediaAsync(uri);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("image")]
        public async Task<IActionResult> PostImage([FromQuery] string caption, IFormFile image)
        {
            string imagePath = await SaveImage(image);

            ResponseData res = await _instagramService.PostImageAsync(imagePath, caption, []);

            System.IO.File.Delete(imagePath);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpDelete("image")]
        public async Task<IActionResult> RemoveImage([FromQuery] string uri)
        {
            ResponseData res = await _instagramService.RemoveImageAsync(uri);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin()
        {
            //"state.bin"
            ResponseData res = await _instagramService.LoginAsync($"{_instagramService.Username}.bin");

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("biography")]
        public async Task<IActionResult> PostBiography([FromBody] string text)
        {
            ResponseData res = await _instagramService.SetUserBiographyAsync(text);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> PostAvatar(IFormFile image)
        {
            string imagePath = await SaveImage(image);

            ResponseData res = await _instagramService.SetUserAvatarAsync(imagePath);

            System.IO.File.Delete(imagePath);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("follow")]
        public async Task<IActionResult> PostFollow([FromQuery] string username)
        {
            ResponseData res = await _instagramService.FollowAsync(username);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpDelete("follow")]
        public async Task<IActionResult> DeleteFollow([FromQuery] string username)
        {
            ResponseData res = await _instagramService.UnFollowAsync(username);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("story/photo")]
        public async Task<IActionResult> PostStoryPhoto([FromQuery] string caption, IFormFile image)
        {
            string imagePath = await SaveImage(image);

            ResponseData res = await _instagramService.UploadStoryPhotoAsync(imagePath, caption);

            System.IO.File.Delete(imagePath);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("story/media")]
        public async Task<IActionResult> PostMediaStory([FromQuery] string mediaUri, IFormFile image)
        {
            string imagePath = await SaveImage(image);

            ResponseData res = await _instagramService.ShareMediaAsStoryAsync(imagePath, mediaUri);

            System.IO.File.Delete(imagePath);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("story")]
        public async Task<IActionResult> SeeStoriesPost([FromQuery] string username)
        {
            ResponseData res = await _instagramService.SeeAllUserStoriesAsync(username);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
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
