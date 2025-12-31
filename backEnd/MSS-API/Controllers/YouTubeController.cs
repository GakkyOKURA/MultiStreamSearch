//using Microsoft.AspNetCore.Mvc;
//using MyApi.Interfaces;

//namespace MyApi.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class YouTubeController : ControllerBase
//{
//    private readonly IYouTubeService _youTubeService;

//    public YouTubeController(IYouTubeService youTubeService)
//    {
//        _youTubeService = youTubeService;
//    }

//    [HttpGet("search")]
//    public async Task<IActionResult> Search([FromQuery] string keyword)
//    {
//        if (string.IsNullOrWhiteSpace(keyword))
//        {
//            return BadRequest(new { message = "keyword は必須です。" });
//        }

//        var result = await _youTubeService.SearchYouTubeVideosAsync(keyword);
//        return Content(result, "application/json");
//    }

//    //[HttpGet("search/details")]
//    //public async Task<IActionResult> SearchWithDetails([FromQuery] string keyword)
//    //{
//    //    if (string.IsNullOrWhiteSpace(keyword))
//    //    {
//    //        return BadRequest(new { message = "keyword は必須です。" });
//    //    }

//    //    var result = await _youTubeService.SearchVideosWithDetailsAsync(keyword);
//    //    return Ok(result);
//    //}

//}
