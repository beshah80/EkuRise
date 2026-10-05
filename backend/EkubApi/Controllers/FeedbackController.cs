using EkubApi.DTOs;
using EkubApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EkubApi.Controllers;

[ApiController]
public class FeedbackController : BaseController
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    /// <summary>
    /// Submit a question or feedback.
    /// </summary>
    [HttpPost("api/feedback")]
    [ProducesResponseType(typeof(FeedbackDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeedbackDto>> SubmitFeedback([FromBody] CreateFeedbackDto dto)
    {
        var userId = GetUserId();
        var feedback = await _feedbackService.SubmitFeedbackAsync(userId, dto);
        return CreatedAtAction(nameof(GetMyFeedbacks), new { }, feedback);
    }

    /// <summary>
    /// List all feedback/questions submitted by the current user.
    /// </summary>
    [HttpGet("api/feedback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyFeedbacks()
    {
        var userId = GetUserId();
        var feedbacks = await _feedbackService.GetMyFeedbacksAsync(userId);
        return Ok(feedbacks);
    }

    /// <summary>
    /// List all approved success stories (public to authenticated users).
    /// </summary>
    [HttpGet("api/success-stories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuccessStories()
    {
        var stories = await _feedbackService.GetApprovedSuccessStoriesAsync();
        return Ok(stories);
    }

    /// <summary>
    /// Submit a success story (requires admin approval before it's visible).
    /// </summary>
    [HttpPost("api/success-stories")]
    [ProducesResponseType(typeof(SuccessStoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SuccessStoryDto>> SubmitSuccessStory([FromBody] CreateSuccessStoryDto dto)
    {
        var userId = GetUserId();
        var story = await _feedbackService.SubmitSuccessStoryAsync(userId, dto);
        return Ok(story);
    }
}
