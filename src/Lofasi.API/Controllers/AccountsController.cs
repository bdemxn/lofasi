using Lofasi.Application.Accounts;
using Lofasi.Application.Accounts.Dtos;
using Lofasi.Application.Transactions.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lofasi.API.Controllers;

[ApiController]
[Authorize]
[Route("api/accounts")]
[Produces("application/json")]
public sealed class AccountsController(IAccountService accountService) : ControllerBase
{
    /// <summary>
    /// Creates a bank account for the authenticated customer's profile.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> Create(
        CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var response = await accountService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetBalance), new { accountNumber = response.AccountNumber }, response);
    }

    /// <summary>
    /// Returns the current balance for an owned account.
    /// </summary>
    [HttpGet("{accountNumber}/balance")]
    [ProducesResponseType(typeof(AccountBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountBalanceResponse>> GetBalance(
        string accountNumber,
        CancellationToken cancellationToken)
    {
        var response = await accountService.GetBalanceAsync(accountNumber, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Records a deposit for an owned account.
    /// </summary>
    [HttpPost("{accountNumber}/deposits")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> Deposit(
        string accountNumber,
        TransactionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await accountService.DepositAsync(accountNumber, request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Records a withdrawal for an owned account when sufficient funds are available.
    /// </summary>
    [HttpPost("{accountNumber}/withdrawals")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> Withdraw(
        string accountNumber,
        TransactionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await accountService.WithdrawAsync(accountNumber, request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returns the chronological transaction history for an owned account.
    /// </summary>
    [HttpGet("{accountNumber}/transactions")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<TransactionResponse>>> GetTransactions(
        string accountNumber,
        CancellationToken cancellationToken)
    {
        var response = await accountService.GetTransactionHistoryAsync(accountNumber, cancellationToken);

        return Ok(response);
    }
}
