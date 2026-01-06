using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApi.Models.DTOs;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilterPresetController : ControllerBase
{
    private readonly IFilterPresetService _filterPresetService;
    private readonly ITodoService _todoService;

    public FilterPresetController(IFilterPresetService filterPresetService, ITodoService todoService)
    {
        _filterPresetService = filterPresetService;
        _todoService = todoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FilterPresetDto>>> GetFilterPresets()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var presets = await _filterPresetService.GetFilterPresetsByUserIdAsync(userId.Value);
        return Ok(presets);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FilterPresetDto>> GetFilterPreset(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var preset = await _filterPresetService.GetFilterPresetByIdAsync(id, userId.Value);
        if (preset == null)
            return NotFound(new { message = "Filter preset not found" });

        return Ok(preset);
    }

    [HttpPost]
    public async Task<ActionResult<FilterPresetDto>> CreateFilterPreset([FromBody] CreateFilterPresetRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Name is required" });

        var preset = await _filterPresetService.CreateFilterPresetAsync(request, userId.Value);
        return CreatedAtAction(nameof(GetFilterPreset), new { id = preset.Id }, preset);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FilterPresetDto>> UpdateFilterPreset(int id, [FromBody] UpdateFilterPresetRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var preset = await _filterPresetService.UpdateFilterPresetAsync(id, request, userId.Value);
        if (preset == null)
            return NotFound(new { message = "Filter preset not found" });

        return Ok(preset);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFilterPreset(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var deleted = await _filterPresetService.DeleteFilterPresetAsync(id, userId.Value);
        if (!deleted)
            return NotFound(new { message = "Filter preset not found" });

        return NoContent();
    }

    [HttpPost("{id}/apply")]
    public async Task<ActionResult<SearchFilterResponse>> ApplyFilterPreset(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var searchRequest = await _filterPresetService.GetSearchFilterRequestFromPresetAsync(id, userId.Value);
            var result = await _todoService.SearchAndFilterTodosAsync(userId.Value, searchRequest);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Filter preset not found" });
        }
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

