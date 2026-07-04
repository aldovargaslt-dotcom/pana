using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Inventory;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/reorder-rules")]
[Produces("application/json")]
[Authorize]
public class ReorderRulesController : ControllerBase
{
    private readonly IReorderRuleService _reorderRuleService;

    public ReorderRulesController(IReorderRuleService reorderRuleService)
    {
        _reorderRuleService = reorderRuleService;
    }

    /// <summary>
    /// Get all reorder rules.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReorderRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var rules = await _reorderRuleService.GetAllAsync(ct);
        return Ok(rules);
    }

    /// <summary>
    /// Get a specific reorder rule by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReorderRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var rule = await _reorderRuleService.GetByIdAsync(id, ct);
        return rule is null ? NotFound() : Ok(rule);
    }

    /// <summary>
    /// Create a new reorder rule.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReorderRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReorderRuleRequest request, CancellationToken ct)
    {
        var rule = await _reorderRuleService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Deactivate a reorder rule.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await _reorderRuleService.DeactivateAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Get products that need reordering (stock below minimum).
    /// </summary>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(List<ReorderSuggestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuggestions(CancellationToken ct)
    {
        var suggestions = await _reorderRuleService.GetReorderSuggestionsAsync(ct);
        return Ok(suggestions);
    }
}
