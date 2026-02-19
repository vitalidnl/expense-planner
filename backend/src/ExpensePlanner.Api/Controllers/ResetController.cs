using ExpensePlanner.Application;
using Microsoft.AspNetCore.Mvc;

namespace ExpensePlanner.Api.Controllers;

[ApiController]
[Route("reset")]
public sealed class ResetController : ControllerBase
{
    private readonly DataResetService _dataResetService;

    public ResetController(DataResetService dataResetService)
    {
        _dataResetService = dataResetService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetAsync(CancellationToken cancellationToken = default)
    {
        await _dataResetService.ResetAsync(cancellationToken);
        return NoContent();
    }
}
