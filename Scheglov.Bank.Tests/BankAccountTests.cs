using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scheglov.Bank;

namespace Scheglov.Bank.Tests;

[TestClass]
public class BankAccountTests
{
    private static BankAccount CreateAccount(decimal balance = 20m) =>
        new("Щеглов Даниил", balance);

    [TestMethod]
    public void Constructor_SavesOwnerAndBalance()
    {
        var account = new BankAccount("Щеглов Даниил", 15.50m);

        Assert.AreEqual("Щеглов Даниил", account.Owner);
        Assert.AreEqual(15.50m, account.Balance);
        Assert.IsFalse(account.IsClosed);
    }

    [TestMethod]
    public void Constructor_WithoutBalance_StartsFromZero()
    {
        var account = new BankAccount("Щеглов Даниил");

        Assert.AreEqual(0m, account.Balance);
        Assert.AreEqual(0, account.Transactions.Count);
    }

    [TestMethod]
    public void Constructor_WithEmptyOwner_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() => new BankAccount("   ", 10m));
    }

    [TestMethod]
    public void Constructor_WithNegativeBalance_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BankAccount("Щеглов Даниил", -1m));
    }

    [TestMethod]
    public void Debit_WithValidAmount_ReducesBalance()
    {
        var account = CreateAccount();

        account.Debit(6.35m);

        Assert.AreEqual(13.65m, account.Balance);
    }

    [TestMethod]
    public void Debit_WithExactBalance_LeavesZero()
    {
        var account = CreateAccount();

        account.Debit(20m);

        Assert.AreEqual(0m, account.Balance);
        Assert.IsTrue(account.CanDebit(0m));
    }

    [TestMethod]
    public void Debit_WhenAmountExceedsBalance_ThrowsExpectedMessage()
    {
        var account = CreateAccount();

        var error = Assert.ThrowsException<ArgumentOutOfRangeException>(() => account.Debit(20.01m));

        StringAssert.Contains(error.Message, BankAccount.DebitAmountExceedsBalanceMessage);
        Assert.AreEqual(20m, account.Balance);
    }

    [TestMethod]
    public void Debit_WhenAmountIsNegative_ThrowsExpectedMessage()
    {
        var account = CreateAccount();

        var error = Assert.ThrowsException<ArgumentOutOfRangeException>(() => account.Debit(-1m));

        StringAssert.Contains(error.Message, BankAccount.DebitAmountLessThanZeroMessage);
    }

    [TestMethod]
    public void Credit_WithValidAmount_IncreasesBalance()
    {
        var account = CreateAccount();

        account.Credit(8.25m);

        Assert.AreEqual(28.25m, account.Balance);
    }

    [TestMethod]
    public void Credit_WhenAmountIsNegative_ThrowsExpectedMessage()
    {
        var account = CreateAccount();

        var error = Assert.ThrowsException<ArgumentOutOfRangeException>(() => account.Credit(-0.01m));

        StringAssert.Contains(error.Message, BankAccount.CreditAmountLessThanZeroMessage);
    }

    [TestMethod]
    public void MoneyOperations_KeepExactDecimalValues()
    {
        var account = CreateAccount(0m);

        account.Credit(0.1m);
        account.Credit(0.2m);

        Assert.AreEqual(0.3m, account.Balance);
    }

    [DataTestMethod]
    [DataRow(5, true)]
    [DataRow(20, true)]
    [DataRow(20.01, false)]
    [DataRow(-1, false)]
    public void CanDebit_ReturnsAvailabilityWithoutChangingBalance(double amount, bool expected)
    {
        var account = CreateAccount();

        var actual = account.CanDebit((decimal)amount);

        Assert.AreEqual(expected, actual);
        Assert.AreEqual(20m, account.Balance);
    }

    [TestMethod]
    public void CanDebit_OnClosedAccount_IsFalse()
    {
        var account = CreateAccount();
        account.Close();

        Assert.IsFalse(account.CanDebit(1m));
    }

    [TestMethod]
    public void Transactions_RecordKindAmountAndBalanceAfter()
    {
        var account = CreateAccount(100m);

        account.Debit(30m);
        account.Credit(10m);

        Assert.AreEqual(3, account.Transactions.Count);

        Assert.AreEqual(TransactionKind.Opening, account.Transactions[0].Kind);
        Assert.AreEqual(100m, account.Transactions[0].BalanceAfter);

        Assert.AreEqual(TransactionKind.Debit, account.Transactions[1].Kind);
        Assert.AreEqual(70m, account.Transactions[1].BalanceAfter);

        Assert.AreEqual(TransactionKind.Credit, account.Transactions[2].Kind);
        Assert.AreEqual(80m, account.Transactions[2].BalanceAfter);
    }

    [TestMethod]
    public void ClosedAccount_RejectsAnyOperation()
    {
        var account = CreateAccount();
        account.Close();

        Assert.ThrowsException<AccountClosedException>(() => account.Debit(1m));
        Assert.ThrowsException<AccountClosedException>(() => account.Credit(1m));
        Assert.ThrowsException<AccountClosedException>(() => account.Close());
    }
}
