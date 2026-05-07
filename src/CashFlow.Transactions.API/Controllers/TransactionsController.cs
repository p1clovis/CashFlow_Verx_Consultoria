using CashFlow.Transactions.Application.Commands;
using CashFlow.Transactions.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Transactions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new credit or debit entry.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateTransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        var command = new CreateTransactionCommand(
            request.Amount,
            request.Type,
            request.Description,
            request.OccurredOn);

        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Gets a paginated list of all transactions.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTransactionsQuery(page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Gets transactions for a specific date (yyyy-MM-dd).</summary>
    [HttpGet("by-date/{date:datetime}")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDate(DateTime date, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTransactionsByDateQuery(date), ct);
        return Ok(result);
    }

    /// <summary>Gets a single transaction by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        // Inline – could become a dedicated query
        var all = await mediator.Send(new GetTransactionsQuery(1, int.MaxValue), ct);
        var item = all.FirstOrDefault(t => t.Id == id);
        return item is null ? NotFound() : Ok(item);
    }
}

public record CreateTransactionRequest(
    decimal Amount,
    string Type,
    string Description,
    DateTime? OccurredOn);
