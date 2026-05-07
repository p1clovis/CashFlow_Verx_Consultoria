using CashFlow.Consolidation.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Consolidation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsolidationController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns the consolidated daily balance for a specific date.</summary>
    [HttpGet("{date:datetime}")]
    [ProducesResponseType(typeof(DailyBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDate(DateTime date, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDailyBalanceQuery(date), ct);
        return result is null ? NotFound(new { message = $"No data found for {date:yyyy-MM-dd}." }) : Ok(result);
    }

    /// <summary>Returns the consolidated balances for a date range.</summary>
    [HttpGet("range")]
    [ProducesResponseType(typeof(IReadOnlyList<DailyBalanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from > to) return BadRequest(new { message = "'from' must be before 'to'." });
        var result = await mediator.Send(new GetBalanceRangeQuery(from, to), ct);
        return Ok(result);
    }
}
