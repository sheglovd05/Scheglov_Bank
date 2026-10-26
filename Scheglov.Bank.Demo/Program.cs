using System.Text;
using Scheglov.Bank;

Console.OutputEncoding = Encoding.UTF8;

var account = new BankAccount("Щеглов Даниил", 15.50m);

account.Credit(4.50m);
account.Debit(7.25m);

Console.WriteLine($"Владелец: {account.Owner}");
Console.WriteLine($"Текущий баланс: {account.Balance:F2}");
Console.WriteLine();

Console.WriteLine("Выписка по счёту:");

foreach (var transaction in account.Transactions)
{
    var sign = transaction.Kind == TransactionKind.Debit ? "-" : "+";
    Console.WriteLine(
        $"  {transaction.CreatedAtLocal:HH:mm:ss}  {transaction.Kind,-8} {sign}{transaction.Amount,8:F2}  " +
        $"баланс: {transaction.BalanceAfter,8:F2}");
}

Console.WriteLine();

try
{
    account.Debit(100m);
}
catch (ArgumentOutOfRangeException error)
{
    Console.WriteLine($"Операция отклонена: {error.Message}");
}
