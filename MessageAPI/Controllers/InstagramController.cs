using MessageAPI.Managers.Interfaces;
using MessageAPI.Models.Implementations;
using MessageAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

using IModelSession = MessageAPI.Models.Interfaces.ISession;

namespace MessageAPI.Controllers
{
    [Route("api/instagram")]
    [ApiController]
    public class InstagramController : ControllerBase
    {
        private readonly IInstagramApiService _instagramService;
        private readonly IInstagramManager _instagramManager;

        public InstagramController(IInstagramApiService instagramService, 
                                    IInstagramManager instagramManager)
        {
            _instagramService = instagramService;
            _instagramManager = instagramManager;
        }

        [HttpPost("comment")]
        public async Task<IActionResult> PostComment([FromQuery] string sessionId, [FromBody] CommentMedia comment)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramService.CommentMediaAsync(id, comment);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("like")]
        public async Task<IActionResult> PostLike([FromQuery] string sessionId, [FromBody] string uri)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramService.LikeMediaAsync(id, uri);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("image")]
        public async Task<IActionResult> PostImage([FromQuery] string sessionId, [FromForm] PostPhoto photo, IFormFile file)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramManager.PostPhotoFromFormfileAsync(id, photo, file);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpDelete("image")]
        public async Task<IActionResult> RemoveImage([FromQuery] string sessionId, [FromBody] string uri)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramService.RemoveImageAsync(id, uri);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin([FromBody] User user)
        {
            ResponseData<IModelSession> res = await _instagramService.LoginAsync(user);

            if (res.IsSuccess)
            {
                return Ok(res.Data);
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpDelete("logout")]
        public async Task<IActionResult> Logout([FromQuery] string sessionId)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramService.LogoutAsync(id);

            if (res.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("biography")]
        public async Task<IActionResult> PostBiography([FromQuery] string sessionId, [FromBody] string text)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramService.SetUserBiographyAsync(id, text);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> PostAvatar([FromQuery] string sessionId, IFormFile image)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramManager.SetUserAvatarFromFormfileAsync(id, image);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("follow")]
        public async Task<IActionResult> PostFollow([FromQuery] string sessionId, [FromBody] string username)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramService.FollowAsync(id, username);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpDelete("follow")]
        public async Task<IActionResult> DeleteFollow([FromQuery] string sessionId, [FromBody] string username)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramService.UnFollowAsync(id, username);

            if (res.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("story/photo")]
        public async Task<IActionResult> PostStoryPhoto([FromQuery] string sessionId, [FromForm] PhotoStory photo, IFormFile file)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramManager.SharePhotoAsStoryFormfileAsync(id, photo, file);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("story/media")]
        public async Task<IActionResult> PostMediaStory([FromQuery] string sessionId, [FromForm] MediaStory media, IFormFile file)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramManager.ShareMediaAsStoryFormfileAsync(id, media, file);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }

        [HttpPost("story")]
        public async Task<IActionResult> SeeStoriesPost([FromQuery] string sessionId, [FromBody] string username)
        {
            if (!Guid.TryParse(sessionId, out var id))
                return BadRequest($"Impossible convert {sessionId} to Guid");

            ResponseData res = await _instagramService.SeeAllUserStoriesAsync(id, username);

            if (res.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(res.ErrorMessage);
        }
    }
}
