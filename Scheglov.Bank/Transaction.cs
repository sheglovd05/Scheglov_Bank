namespace Scheglov.Bank;

public enum TransactionKind
{
    /// <summary>Начальное зачисление при открытии счёта.</summary>
    Opening,

    /// <summary>Пополнение счёта.</summary>
    Credit,

    /// <summary>Списание со счёта.</summary>
    Debit
}

/// <summary>
/// Запись в выписке по счёту. Баланс фиксируется после выполнения операции,
/// поэтому по журналу можно восстановить состояние счёта на любой момент.
/// </summary>
public sealed record Transaction(
    TransactionKind Kind,
    decimal Amount,
    decimal BalanceAfter,
    DateTime CreatedAtUtc)
{
    public DateTime CreatedAtLocal => CreatedAtUtc.ToLocalTime();
}
