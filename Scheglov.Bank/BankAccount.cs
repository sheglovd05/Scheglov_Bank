namespace Scheglov.Bank;

/// <summary>
/// Банковский счёт с журналом операций.
/// Баланс хранится в decimal: для денежных сумм это важнее скорости,
/// потому что double накапливает двоичную погрешность (0.1 + 0.2 != 0.3).
/// </summary>
public sealed class BankAccount
{
    public const string DebitAmountExceedsBalanceMessage = "Debit amount exceeds balance";
    public const string DebitAmountLessThanZeroMessage = "Debit amount is less than zero";
    public const string CreditAmountLessThanZeroMessage = "Credit amount is less than zero";

    private readonly List<Transaction> _transactions = new();

    public BankAccount(string owner, decimal initialBalance = 0m)
    {
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("Не указан владелец счёта.", nameof(owner));

        if (initialBalance < 0m)
            throw new ArgumentOutOfRangeException(
                nameof(initialBalance),
                initialBalance,
                "Начальный баланс не может быть отрицательным.");

        Owner = owner.Trim();
        Balance = initialBalance;

        if (initialBalance > 0m)
            _transactions.Add(new Transaction(TransactionKind.Opening, initialBalance, initialBalance, DateTime.UtcNow));
    }

    public string Owner { get; }

    public decimal Balance { get; private set; }

    public bool IsClosed { get; private set; }

    public IReadOnlyList<Transaction> Transactions => _transactions;

    /// <summary>
    /// Проверяет, хватит ли средств на списание, не меняя состояние счёта.
    /// </summary>
    public bool CanDebit(decimal amount) => !IsClosed && amount >= 0m && amount <= Balance;

    public void Debit(decimal amount)
    {
        EnsureOpen();

        if (amount < 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, DebitAmountLessThanZeroMessage);

        if (amount > Balance)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, DebitAmountExceedsBalanceMessage);

        Balance -= amount;
        Record(TransactionKind.Debit, amount);
    }

    public void Credit(decimal amount)
    {
        EnsureOpen();

        if (amount < 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, CreditAmountLessThanZeroMessage);

        Balance += amount;
        Record(TransactionKind.Credit, amount);
    }

    /// <summary>
    /// Переводит средства на другой счёт. Сначала проверяются обе стороны,
    /// поэтому при ошибке деньги не «зависают» на промежуточном шаге.
    /// </summary>
    public void TransferTo(BankAccount target, decimal amount)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (ReferenceEquals(target, this))
            throw new InvalidOperationException("Нельзя перевести средства на тот же счёт.");

        EnsureOpen();
        target.EnsureOpen();

        Debit(amount);
        target.Credit(amount);
    }

    public void Close()
    {
        EnsureOpen();
        IsClosed = true;
    }

    private void Record(TransactionKind kind, decimal amount) =>
        _transactions.Add(new Transaction(kind, amount, Balance, DateTime.UtcNow));

    private void EnsureOpen()
    {
        if (IsClosed)
            throw new AccountClosedException(Owner);
    }
}
