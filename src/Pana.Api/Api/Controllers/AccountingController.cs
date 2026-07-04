using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Accounting;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/accounting")]
[Produces("application/json")]
[Authorize]
public class AccountingController : ControllerBase
{
    private readonly IAccountingService _accountingService;

    public AccountingController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    /// <summary>
    /// Get all journal entries for the current tenant, newest first.
    /// </summary>
    [HttpGet("journal-entries")]
    [ProducesResponseType(typeof(List<JournalEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var entries = await _accountingService.GetAllAsync(ct);
        return Ok(entries);
    }

    /// <summary>
    /// Get a specific journal entry by ID with all lines.
    /// </summary>
    [HttpGet("journal-entries/{id:guid}")]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var entry = await _accountingService.GetByIdAsync(id, ct);
        return entry is null ? NotFound() : Ok(entry);
    }

    /// <summary>
    /// Create a new journal entry (double-entry bookkeeping).
    /// </summary>
    [HttpPost("journal-entries")]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateJournalEntryRequest request, CancellationToken ct)
    {
        var entry = await _accountingService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    /// <summary>
    /// Post a journal entry (validates balance and finalizes).
    /// </summary>
    [HttpPost("journal-entries/{id:guid}/post")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(Guid id, CancellationToken ct)
    {
        var result = await _accountingService.PostAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Cancel a journal entry.
    /// </summary>
    [HttpPost("journal-entries/{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _accountingService.CancelAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Get trial balance (summary of all posted accounts).
    /// </summary>
    [HttpGet("trial-balance")]
    [ProducesResponseType(typeof(List<AccountBalanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrialBalance(CancellationToken ct)
    {
        var balances = await _accountingService.GetTrialBalanceAsync(ct);
        return Ok(balances);
    }
}
