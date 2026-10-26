using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scheglov.Bank;

namespace Scheglov.Bank.Tests;

[TestClass]
public class TransferTests
{
    [TestMethod]
    public void Transfer_MovesMoneyBetweenAccounts()
    {
        var sender = new BankAccount("Щеглов Даниил", 500m);
        var receiver = new BankAccount("Петров Иван", 100m);

        sender.TransferTo(receiver, 150m);

        Assert.AreEqual(350m, sender.Balance);
        Assert.AreEqual(250m, receiver.Balance);
    }

    [TestMethod]
    public void Transfer_WritesEntryToBothJournals()
    {
        var sender = new BankAccount("Щеглов Даниил", 500m);
        var receiver = new BankAccount("Петров Иван", 0m);

        sender.TransferTo(receiver, 120m);

        Assert.AreEqual(TransactionKind.Debit, sender.Transactions[^1].Kind);
        Assert.AreEqual(380m, sender.Transactions[^1].BalanceAfter);
        Assert.AreEqual(TransactionKind.Credit, receiver.Transactions[^1].Kind);
        Assert.AreEqual(120m, receiver.Transactions[^1].BalanceAfter);
    }

    [TestMethod]
    public void Transfer_WithoutEnoughMoney_ChangesNothing()
    {
        var sender = new BankAccount("Щеглов Даниил", 50m);
        var receiver = new BankAccount("Петров Иван", 10m);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => sender.TransferTo(receiver, 100m));

        Assert.AreEqual(50m, sender.Balance);
        Assert.AreEqual(10m, receiver.Balance);
        Assert.AreEqual(1, sender.Transactions.Count);
        Assert.AreEqual(1, receiver.Transactions.Count);
    }

    [TestMethod]
    public void Transfer_ToSameAccount_IsRejected()
    {
        var account = new BankAccount("Щеглов Даниил", 50m);

        Assert.ThrowsException<InvalidOperationException>(() => account.TransferTo(account, 10m));
        Assert.AreEqual(50m, account.Balance);
    }

    [TestMethod]
    public void Transfer_WithNegativeAmount_IsRejected()
    {
        var sender = new BankAccount("Щеглов Даниил", 50m);
        var receiver = new BankAccount("Петров Иван", 0m);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => sender.TransferTo(receiver, -5m));
        Assert.AreEqual(0m, receiver.Balance);
    }

    [TestMethod]
    public void Transfer_ToClosedAccount_IsRejected()
    {
        var sender = new BankAccount("Щеглов Даниил", 50m);
        var receiver = new BankAccount("Петров Иван", 0m);
        receiver.Close();

        Assert.ThrowsException<AccountClosedException>(() => sender.TransferTo(receiver, 10m));
        Assert.AreEqual(50m, sender.Balance);
    }
}
