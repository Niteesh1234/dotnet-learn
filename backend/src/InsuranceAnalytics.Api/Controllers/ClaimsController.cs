using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Filters;
using InsuranceAnalytics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAnalytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClaimsController(IClaimService claimService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? region = null,
        [FromQuery] int? policyType = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new KpiFilter
        {
            FromDate = fromDate,
            ToDate = toDate,
            Region = region,
            PolicyType = policyType is null ? null : (InsuranceAnalytics.Core.Enums.PolicyType?)policyType
        };

        var items = await claimService.GetClaimsAsync(new PagedRequest { PageNumber = pageNumber, PageSize = pageSize }, filter, cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var item = await claimService.CreateClaimAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = item.ClaimId }, item);
    }
}
