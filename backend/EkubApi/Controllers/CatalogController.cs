using EkubApi.DTOs;
using EkubApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EkubApi.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : BaseController
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // --- Categories ---

    /// <summary>
    /// List all active Ekub categories (Driver Equb, Trader Equb, etc.).
    /// Available to all authenticated users.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var userId = GetUserId();
        var categories = await _catalogService.GetCategoriesAsync(userId);
        return Ok(categories);
    }

    /// <summary>
    /// Create a new main Ekub category. Admin only.
    /// </summary>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var adminId = GetUserId();
        var category = await _catalogService.CreateCategoryAsync(dto, adminId);
        return CreatedAtAction(nameof(GetCategories), new { }, category);
    }

    // --- Sub-Categories ---

    /// <summary>
    /// List all sub-categories (Ekub plans) under a category.
    /// </summary>
    [HttpGet("categories/{categoryId}/subcategories")]
    [ProducesResponseType(typeof(List<SubCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SubCategoryDto>>> GetSubCategories(int categoryId)
    {
        var userId = GetUserId();
        var subCategories = await _catalogService.GetSubCategoriesAsync(categoryId, userId);
        return Ok(subCategories);
    }

    /// <summary>
    /// Get full details of a sub-category including terms and conditions.
    /// </summary>
    [HttpGet("subcategories/{subCategoryId}")]
    [ProducesResponseType(typeof(SubCategoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubCategoryDetailDto>> GetSubCategory(int subCategoryId)
    {
        var userId = GetUserId();
        var sub = await _catalogService.GetSubCategoryByIdAsync(subCategoryId, userId);
        return sub is null ? NotFound() : Ok(sub);
    }

    /// <summary>
    /// Create a new sub-category (specific Ekub plan) under a category. Admin only.
    /// </summary>
    [HttpPost("subcategories")]
    [ProducesResponseType(typeof(SubCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubCategoryDto>> CreateSubCategory([FromBody] CreateSubCategoryDto dto)
    {
        var adminId = GetUserId();
        var sub = await _catalogService.CreateSubCategoryAsync(dto, adminId);
        return Ok(sub);
    }

    /// <summary>
    /// Join a sub-category. User must agree to the terms and conditions.
    /// </summary>
    [HttpPost("subcategories/{subCategoryId}/join")]
    [ProducesResponseType(typeof(JoinResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JoinResultDto>> JoinSubCategory(int subCategoryId, [FromBody] JoinSubCategoryDto dto)
    {
        var userId = GetUserId();
        var result = await _catalogService.JoinSubCategoryAsync(subCategoryId, dto.AgreedToTerms, userId);
        return Ok(result);
    }

    /// <summary>
    /// Start the Ekub: creates a Circle from all joined members. Admin only.
    /// </summary>
    [HttpPost("subcategories/{subCategoryId}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartEkub(int subCategoryId)
    {
        var adminId = GetUserId();
        await _catalogService.StartEkubAsync(subCategoryId, adminId);
        return Ok(new { message = "Ekub started successfully. A circle has been created for all members." });
    }

    // --- My Ekubs ---

    /// <summary>
    /// List all Ekubs the current user has joined.
    /// </summary>
    [HttpGet("my-ekubs")]
    [ProducesResponseType(typeof(List<MyEkubDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MyEkubDto>>> GetMyEkubs()
    {
        var userId = GetUserId();
        var ekubs = await _catalogService.GetMyEkubsAsync(userId);
        return Ok(ekubs);
    }
}
