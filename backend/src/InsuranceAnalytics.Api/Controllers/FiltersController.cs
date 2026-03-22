using InsuranceAnalytics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAnalytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FiltersController(IFilterService filterService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var response = await filterService.GetFilterOptionsAsync(cancellationToken);
        return Ok(response);
    }
}
