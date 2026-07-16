using Lofasi.Application.Accounts.Dtos;
using Lofasi.Application.Transactions.Dtos;

namespace Lofasi.Application.Accounts;

public interface IAccountService
{
    Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken);

    Task<AccountBalanceResponse> GetBalanceAsync(string accountNumber, CancellationToken cancellationToken);

    Task<TransactionResponse> DepositAsync(
        string accountNumber,
        TransactionRequest request,
        CancellationToken cancellationToken);

    Task<TransactionResponse> WithdrawAsync(
        string accountNumber,
        TransactionRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TransactionResponse>> GetTransactionHistoryAsync(
        string accountNumber,
        CancellationToken cancellationToken);
}
