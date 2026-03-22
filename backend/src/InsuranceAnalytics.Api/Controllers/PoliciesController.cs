using InsuranceAnalytics.Core.DTOs;
using InsuranceAnalytics.Core.Enums;
using InsuranceAnalytics.Core.Filters;
using InsuranceAnalytics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAnalytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoliciesController(IPolicyService policyService) : ControllerBase
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

        var items = await policyService.GetPoliciesAsync(new PagedRequest { PageNumber = pageNumber, PageSize = pageSize }, filter, cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        var item = await policyService.CreatePolicyAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = item.PolicyId }, item);
    }

    [HttpPost("seed-fake")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SeedFake([FromQuery] int policyCount = 20, [FromQuery] int maxClaimsPerPolicy = 3, CancellationToken cancellationToken = default)
    {
        var inserted = await policyService.SeedFakeDataAsync(policyCount, maxClaimsPerPolicy, cancellationToken);
        return Ok(new { insertedPolicies = inserted });
    }
}
