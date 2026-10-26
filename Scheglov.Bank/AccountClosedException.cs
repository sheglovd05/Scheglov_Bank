namespace Scheglov.Bank;

/// <summary>
/// Попытка выполнить операцию по закрытому счёту.
/// </summary>
public sealed class AccountClosedException : InvalidOperationException
{
    public AccountClosedException(string owner)
        : base($"Счёт владельца «{owner}» закрыт, операции по нему невозможны.")
    {
        Owner = owner;
    }

    public string Owner { get; }
}
