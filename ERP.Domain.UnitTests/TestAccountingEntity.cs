using Xunit;
using ERP.Domain.Common;
using ERP.Domain.Accounting.Entities;
using ERP.Domain.Accounting.Enums;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for Accounting domain entities
/// </summary>
public class TestAccountingEntity
{
    #region Account Tests

    [Fact]
    public void Account_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var accountCode = "1001";
        var name = "Cash Account";
        var type = AccountType.Asset;
        var accountClass = AccountClass.Debit;

        // Act
        var account = Account.Create(
            organizationId: orgId,
            accountCode: accountCode,
            name: name,
            type: type,
            accountClass: accountClass,
            isBankAccount: true);

        // Assert
        Assert.NotNull(account);
        Assert.Equal(orgId, account.OrganizationId);
        Assert.Equal(accountCode, account.AccountCode);
        Assert.Equal(name, account.Name);
        Assert.Equal(type, account.Type);
        Assert.Equal(accountClass, account.Class);
        Assert.True(account.IsActive);
        Assert.True(account.IsBankAccount);
        Assert.False(account.IsCashAccount);
        Assert.True(account.AllowDirectPosting);
    }

    [Fact]
    public void Account_Create_WithEmptyCode_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Account.Create(
                organizationId: Guid.NewGuid(),
                accountCode: "",
                name: "Test Account",
                type: AccountType.Asset,
                accountClass: AccountClass.Debit));

        Assert.Contains("Account code is required", exception.Message);
    }

    [Fact]
    public void Account_Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Account.Create(
                organizationId: Guid.NewGuid(),
                accountCode: "1001",
                name: "",
                type: AccountType.Asset,
                accountClass: AccountClass.Debit));

        Assert.Contains("Account name is required", exception.Message);
    }

    [Theory]
    [InlineData(AccountType.Asset, AccountClass.Debit)]
    [InlineData(AccountType.Liability, AccountClass.Credit)]
    [InlineData(AccountType.Equity, AccountClass.Credit)]
    [InlineData(AccountType.Revenue, AccountClass.Credit)]
    [InlineData(AccountType.Expense, AccountClass.Debit)]
    public void Account_Create_WithDifferentTypes_ShouldSetCorrectTypeAndClass(AccountType type, AccountClass expectedClass)
    {
        // Act
        var account = Account.Create(
            organizationId: Guid.NewGuid(),
            accountCode: "1001",
            name: "Test Account",
            type: type,
            accountClass: expectedClass);

        // Assert
        Assert.Equal(type, account.Type);
        Assert.Equal(expectedClass, account.Class);
    }

    [Fact]
    public void Account_SetAsBankAccount_ShouldUpdateBankAccountFlags()
    {
        // Arrange
        var account = Account.Create(
            organizationId: Guid.NewGuid(),
            accountCode: "1001",
            name: "Bank BCA",
            type: AccountType.Asset,
            accountClass: AccountClass.Debit);

        // Act
        account.SetAsBankAccount("1234567890", "Bank BCA");

        // Assert
        Assert.True(account.IsBankAccount);
        Assert.Equal("1234567890", account.BankAccountNumber);
        Assert.Equal("Bank BCA", account.BankName);
    }

    [Fact]
    public void Account_SetAsCashAccount_ShouldUpdateCashAccountFlags()
    {
        // Arrange
        var account = Account.Create(
            organizationId: Guid.NewGuid(),
            accountCode: "1001",
            name: "Cash on Hand",
            type: AccountType.Asset,
            accountClass: AccountClass.Debit,
            isBankAccount: true);

        // Act
        account.SetAsCashAccount();

        // Assert
        Assert.True(account.IsCashAccount);
        Assert.False(account.IsBankAccount);
    }

    [Fact]
    public void Account_SetOpeningBalance_ShouldUpdateBalanceAndDate()
    {
        // Arrange
        var account = Account.Create(
            organizationId: Guid.NewGuid(),
            accountCode: "1001",
            name: "Test Account",
            type: AccountType.Asset,
            accountClass: AccountClass.Debit);
        var balanceDate = new DateTime(2024, 1, 1);

        // Act
        account.SetOpeningBalance(5000000m, balanceDate);

        // Assert
        Assert.Equal(5000000m, account.OpeningBalance);
        Assert.Equal(balanceDate, account.OpeningBalanceDate);
    }

    [Fact]
    public void Account_Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var account = Account.Create(
            organizationId: Guid.NewGuid(),
            accountCode: "1001",
            name: "Test Account",
            type: AccountType.Asset,
            accountClass: AccountClass.Debit);

        // Act
        account.Deactivate();

        // Assert
        Assert.False(account.IsActive);
    }

    #endregion

    #region JournalEntry Tests

    [Fact]
    public void JournalEntry_Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var entryNumber = "JE-2024-001";
        var entryDate = DateTime.UtcNow;
        var postingDate = DateTime.UtcNow;
        var title = "Sales Revenue Recognition";

        // Act
        var journalEntry = JournalEntry.Create(
            organizationId: orgId,
            entryNumber: entryNumber,
            entryDate: entryDate,
            postingDate: postingDate,
            title: title);

        // Assert
        Assert.NotNull(journalEntry);
        Assert.Equal(orgId, journalEntry.OrganizationId);
        Assert.Equal(entryNumber, journalEntry.EntryNumber);
        Assert.Equal(entryDate, journalEntry.EntryDate);
        Assert.Equal(postingDate, journalEntry.PostingDate);
        Assert.Equal(title, journalEntry.Title);
        Assert.Equal(JournalEntryStatus.Draft, journalEntry.Status);
        Assert.Equal(0, journalEntry.TotalDebit);
        Assert.Equal(0, journalEntry.TotalCredit);
        Assert.True(journalEntry.IsBalanced);
    }

    [Fact]
    public void JournalEntry_Create_WithEmptyEntryNumber_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            JournalEntry.Create(
                organizationId: Guid.NewGuid(),
                entryNumber: "",
                entryDate: DateTime.UtcNow,
                postingDate: DateTime.UtcNow,
                title: "Test Entry"));

        Assert.Contains("Entry number is required", exception.Message);
    }

    [Fact]
    public void JournalEntry_Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            JournalEntry.Create(
                organizationId: Guid.NewGuid(),
                entryNumber: "JE001",
                entryDate: DateTime.UtcNow,
                postingDate: DateTime.UtcNow,
                title: ""));

        Assert.Contains("Entry title is required", exception.Message);
    }

    [Fact]
    public void JournalEntry_AddLine_ShouldAddLineToEntry()
    {
        // Arrange
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Test Entry");

        var debitLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Debit entry",
            debitAmount: 1000m);

        // Act
        journalEntry.AddLine(debitLine);

        // Assert
        Assert.Single(journalEntry.Lines);
        Assert.Equal(1000m, journalEntry.TotalDebit);
    }

    [Fact]
    public void JournalEntry_IsBalanced_WhenDebitsEqualCredits_ShouldReturnTrue()
    {
        // Arrange
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Balanced Entry");

        var debitLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Debit",
            debitAmount: 500m);

        var creditLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Credit",
            creditAmount: 500m);

        journalEntry.AddLine(debitLine);
        journalEntry.AddLine(creditLine);

        // Assert
        Assert.Equal(500m, journalEntry.TotalDebit);
        Assert.Equal(500m, journalEntry.TotalCredit);
        Assert.True(journalEntry.IsBalanced);
        Assert.Equal(0, journalEntry.Difference);
    }

    [Fact]
    public void JournalEntry_IsNotBalanced_WhenDebitsNotEqualCredits_ShouldReturnFalse()
    {
        // Arrange
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Unbalanced Entry");

        var debitLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Debit",
            debitAmount: 1000m);

        var creditLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Credit",
            creditAmount: 500m);

        journalEntry.AddLine(debitLine);
        journalEntry.AddLine(creditLine);

        // Assert
        Assert.Equal(1000m, journalEntry.TotalDebit);
        Assert.Equal(500m, journalEntry.TotalCredit);
        Assert.False(journalEntry.IsBalanced);
        Assert.Equal(500m, journalEntry.Difference);
    }

    [Fact]
    public void JournalEntry_Submit_WithoutLines_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Empty Entry");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => journalEntry.Submit());
        Assert.Contains("Entry must have at least one line", exception.Message);
    }

    [Fact]
    public void JournalEntry_Submit_WhenNotBalanced_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Unbalanced Entry");

        var debitLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Debit",
            debitAmount: 1000m);

        journalEntry.AddLine(debitLine);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => journalEntry.Submit());
        Assert.Contains("Entry is not balanced", exception.Message);
        Assert.Contains("Difference: 1000", exception.Message);
    }

    [Fact]
    public void JournalEntry_Submit_WhenBalanced_ShouldChangeStatusToSubmitted()
    {
        // Arrange
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Balanced Entry");

        var debitLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Debit",
            debitAmount: 1000m);

        var creditLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Credit",
            creditAmount: 1000m);

        journalEntry.AddLine(debitLine);
        journalEntry.AddLine(creditLine);

        // Act
        journalEntry.Submit();

        // Assert
        Assert.Equal(JournalEntryStatus.Submitted, journalEntry.Status);
    }

    [Fact]
    public void JournalEntry_Submit_WhenAlreadySubmitted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();
        journalEntry.Submit();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => journalEntry.Submit());
        Assert.Contains("Entry already submitted", exception.Message);
    }

    [Fact]
    public void JournalEntry_RemoveLine_ShouldRecalculateTotals()
    {
        // Arrange
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Test Entry");

        var debitLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Debit",
            debitAmount: 1000m);

        journalEntry.AddLine(debitLine);

        // Act
        journalEntry.RemoveLine(debitLine.Id);

        // Assert
        Assert.Empty(journalEntry.Lines);
        Assert.Equal(0, journalEntry.TotalDebit);
        Assert.Equal(0, journalEntry.TotalCredit);
    }

    [Fact]
    public void JournalEntry_Cancel_WhenPosted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();
        journalEntry.Submit();
        journalEntry.Approve();
        journalEntry.Post();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => journalEntry.Cancel());
        Assert.Contains("Posted entries cannot be cancelled", exception.Message);
    }

    [Fact]
    public void JournalEntry_Reverse_WhenPosted_ShouldMarkAsReversed()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();
        journalEntry.Submit();
        journalEntry.Approve();
        journalEntry.Post();

        // Act
        journalEntry.Reverse();

        // Assert
        Assert.True(journalEntry.IsDeleted);
        Assert.Equal(JournalEntryStatus.Reversed, journalEntry.Status);
    }

    [Fact]
    public void JournalEntry_Approve_ShouldMarkAllLinesAsPosted()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();
        journalEntry.Submit();

        // Act
        journalEntry.Approve();

        // Assert
        Assert.Equal(JournalEntryStatus.Approved, journalEntry.Status);
        foreach (var line in journalEntry.Lines)
        {
            Assert.False(line.IsActive);
            Assert.NotNull(line.PostedAt);
        }
    }

    [Fact]
    public void JournalEntry_ClearLines_ShouldRemoveAllLines()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();

        // Act
        journalEntry.ClearLines();

        // Assert
        Assert.Empty(journalEntry.Lines);
        Assert.Equal(0, journalEntry.TotalDebit);
        Assert.Equal(0, journalEntry.TotalCredit);
    }

    [Fact]
    public void JournalEntry_AddLine_WhenNotDraft_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();
        journalEntry.Submit();

        var newLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "New Line",
            debitAmount: 100m);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => journalEntry.AddLine(newLine));
        Assert.Contains("Can only modify draft entries", exception.Message);
    }

    #endregion

    #region JournalLine Tests

    [Fact]
    public void JournalLine_Create_WithValidDebit_ShouldCreateSuccessfully()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var line = JournalLine.Create(
            accountId: accountId,
            description: "Cash Received",
            debitAmount: 500000m);

        // Assert
        Assert.NotNull(line);
        Assert.Equal(accountId, line.AccountId);
        Assert.Equal("Cash Received", line.Description);
        Assert.Equal(500000m, line.DebitAmount);
        Assert.Equal(0, line.CreditAmount);
        Assert.True(line.IsDebit);
        Assert.False(line.IsCredit);
        Assert.Equal(500000m, line.NetAmount);
    }

    [Fact]
    public void JournalLine_Create_WithValidCredit_ShouldCreateSuccessfully()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var line = JournalLine.Create(
            accountId: accountId,
            description: "Revenue",
            creditAmount: 500000m);

        // Assert
        Assert.NotNull(line);
        Assert.Equal(500000m, line.CreditAmount);
        Assert.Equal(0, line.DebitAmount);
        Assert.False(line.IsDebit);
        Assert.True(line.IsCredit);
        Assert.Equal(-500000m, line.NetAmount);
    }

    [Fact]
    public void JournalLine_Create_WithNegativeDebit_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            JournalLine.Create(
                accountId: Guid.NewGuid(),
                description: "Test",
                debitAmount: -100m));

        Assert.Contains("Amounts cannot be negative", exception.Message);
    }

    [Fact]
    public void JournalLine_Create_WithNegativeCredit_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            JournalLine.Create(
                accountId: Guid.NewGuid(),
                description: "Test",
                creditAmount: -100m));

        Assert.Contains("Amounts cannot be negative", exception.Message);
    }

    [Fact]
    public void JournalLine_Create_WithBothDebitAndCredit_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            JournalLine.Create(
                accountId: Guid.NewGuid(),
                description: "Test",
                debitAmount: 100m,
                creditAmount: 50m));

        Assert.Contains("Line cannot have both debit and credit", exception.Message);
    }

    [Fact]
    public void JournalLine_Create_WithZeroAmounts_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            JournalLine.Create(
                accountId: Guid.NewGuid(),
                description: "Test"));

        Assert.Contains("At least one of debit or credit amount is required", exception.Message);
    }

    [Fact]
    public void JournalLine_MarkAsPosted_ShouldSetIsActiveAndPostedAt()
    {
        // Arrange
        var line = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Test",
            debitAmount: 100m);

        // Act
        line.MarkAsPosted();

        // Assert
        Assert.False(line.IsActive);
        Assert.NotNull(line.PostedAt);
    }

    [Fact]
    public void JournalLine_Update_WhenActive_ShouldThrowInvalidOperationException()
    {
        // Arrange - note: Update throws when IsActive=true (not yet posted)
        var line = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Test",
            debitAmount: 100m);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            line.Update(200m, 0));
        Assert.Contains("Cannot update posted line", exception.Message);
    }

    [Fact]
    public void JournalLine_Update_WhenNotActive_ShouldSucceed()
    {
        // Arrange - Update succeeds when IsActive=false (posted)
        var line = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Test",
            debitAmount: 100m);
        line.MarkAsPosted(); // Sets IsActive = false

        // Act
        line.Update(200m, 0);

        // Assert
        Assert.Equal(200m, line.DebitAmount);
    }

    #endregion

    #region Balance Validation Tests

    [Fact]
    public void JournalEntry_BalanceValidation_WithMultipleLines_ShouldCalculateCorrectTotals()
    {
        // Arrange - Create a complex journal entry
        // Cash (Debit)     = 1,000,000
        // Sales Revenue    = 850,000
        // Tax Payable      = 150,000
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Sales Collection");

        var cashDebit = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Cash Received",
            debitAmount: 1000000m);

        var revenueCredit = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Sales Revenue",
            creditAmount: 850000m);

        var taxCredit = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Output Tax",
            creditAmount: 150000m);

        journalEntry.AddLine(cashDebit);
        journalEntry.AddLine(revenueCredit);
        journalEntry.AddLine(taxCredit);

        // Assert
        Assert.Equal(1000000m, journalEntry.TotalDebit);
        Assert.Equal(1000000m, journalEntry.TotalCredit);
        Assert.True(journalEntry.IsBalanced);
    }

    [Theory]
    [InlineData(1000, 1000, true)]
    [InlineData(500, 500, true)]
    [InlineData(1000, 500, false)]
    [InlineData(999.99, 999.99, true)]
    public void JournalEntry_BalanceValidation_ShouldValidateCorrectly(decimal debit, decimal credit, bool expectedBalanced)
    {
        // Arrange
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Test Entry");

        journalEntry.AddLine(JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Debit",
            debitAmount: debit));

        journalEntry.AddLine(JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Credit",
            creditAmount: credit));

        // Assert
        Assert.Equal(expectedBalanced, journalEntry.IsBalanced);
    }

    #endregion

    #region Helper Methods

    private static JournalEntry CreateBalancedJournalEntry()
    {
        var journalEntry = JournalEntry.Create(
            organizationId: Guid.NewGuid(),
            entryNumber: "JE-001",
            entryDate: DateTime.UtcNow,
            postingDate: DateTime.UtcNow,
            title: "Balanced Entry");

        var debitLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Debit Entry",
            debitAmount: 1000m);

        var creditLine = JournalLine.Create(
            accountId: Guid.NewGuid(),
            description: "Credit Entry",
            creditAmount: 1000m);

        journalEntry.AddLine(debitLine);
        journalEntry.AddLine(creditLine);

        return journalEntry;
    }

    #endregion
}
