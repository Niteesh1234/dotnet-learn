using InsuranceAnalytics.Core.Filters;
using InsuranceAnalytics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAnalytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KpiController(IKpiService kpiService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] KpiFilter filter, CancellationToken cancellationToken)
    {
        var response = await kpiService.GetDashboardAsync(filter, cancellationToken);
        return Ok(response);
    }
}
