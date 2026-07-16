using Lofasi.Application.Customers;
using Lofasi.Application.Customers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lofasi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
[Produces("application/json")]
public sealed class CustomersController(ICustomerService customerService) : ControllerBase
{
    /// <summary>
    /// Returns the customer profile linked to the authenticated user.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetCurrent(CancellationToken cancellationToken)
    {
        var response = await customerService.GetCurrentAsync(cancellationToken);

        return Ok(response);
    }
}
