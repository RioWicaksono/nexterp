using Xunit;
using ERP.Domain.Accounting.Entities;
using ERP.Domain.Accounting.Enums;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for JournalEntry entity
/// </summary>
public class JournalEntryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateJournalEntry()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var entryNumber = "JE2024-00001";
        var title = "Test Entry";

        // Act
        var entry = JournalEntry.Create(
            orgId, entryNumber,
            DateTime.UtcNow, DateTime.UtcNow,
            title);

        // Assert
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(orgId, entry.OrganizationId);
        Assert.Equal(entryNumber, entry.EntryNumber);
        Assert.Equal(title, entry.Title);
        Assert.Equal(JournalEntryStatus.Draft, entry.Status);
        Assert.False(entry.IsDeleted);
        Assert.Empty(entry.Lines);
    }

    [Fact]
    public void Create_WithEmptyEntryNumber_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            JournalEntry.Create(Guid.NewGuid(), "", DateTime.UtcNow, DateTime.UtcNow, "Title"));
        Assert.Contains("Entry number is required", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            JournalEntry.Create(Guid.NewGuid(), "JE001", DateTime.UtcNow, DateTime.UtcNow, ""));
        Assert.Contains("Entry title is required", exception.Message);
    }

    [Fact]
    public void AddLine_ShouldAddLineToEntry()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        var line = JournalLine.Create(Guid.NewGuid(), "Test Line", debitAmount: 100);

        // Act
        entry.AddLine(line);

        // Assert
        Assert.Single(entry.Lines);
        Assert.Equal(100, entry.TotalDebit);
    }

    [Fact]
    public void AddLine_WhenNotDraft_ShouldThrowException()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        // Add balanced lines first
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 100));
        entry.Submit();
        entry.Approve();
        var line = JournalLine.Create(Guid.NewGuid(), "Test Line", debitAmount: 100);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => entry.AddLine(line));
    }

    [Fact]
    public void RemoveLine_ShouldRemoveLineFromEntry()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        var line = JournalLine.Create(Guid.NewGuid(), "Test Line", debitAmount: 100);
        entry.AddLine(line);

        // Act
        entry.RemoveLine(line.Id);

        // Assert
        Assert.Empty(entry.Lines);
        Assert.Equal(0, entry.TotalDebit);
    }

    [Fact]
    public void Submit_WithNoLines_ShouldThrowException()
    {
        // Arrange
        var entry = CreateTestJournalEntry();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => entry.Submit());
    }

    [Fact]
    public void Submit_WithUnbalancedEntry_ShouldThrowException()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 50));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => entry.Submit());
    }

    [Fact]
    public void Submit_WithBalancedEntry_ShouldSetStatusToSubmitted()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 100));

        // Act
        entry.Submit();

        // Assert
        Assert.Equal(JournalEntryStatus.Submitted, entry.Status);
    }

    [Fact]
    public void Approve_WithSubmittedStatus_ShouldSetStatusToApproved()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 100));
        entry.Submit();

        // Act
        entry.Approve();

        // Assert
        Assert.Equal(JournalEntryStatus.Approved, entry.Status);
    }

    [Fact]
    public void Approve_WhenNotSubmitted_ShouldThrowException()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 100));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => entry.Approve());
    }

    [Fact]
    public void Post_WhenApproved_ShouldSetStatusToPosted()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 100));
        entry.Submit();
        entry.Approve();

        // Act
        entry.Post();

        // Assert
        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldSetStatusToCancelled()
    {
        // Arrange
        var entry = CreateTestJournalEntry();

        // Act
        entry.Cancel();

        // Assert
        Assert.Equal(JournalEntryStatus.Cancelled, entry.Status);
    }

    [Fact]
    public void Cancel_WhenPosted_ShouldThrowException()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 100));
        entry.Submit();
        entry.Approve();
        entry.Post();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => entry.Cancel());
    }

    [Fact]
    public void Reverse_WhenPosted_ShouldSetStatusToReversed()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 100));
        entry.Submit();
        entry.Approve();
        entry.Post();

        // Act
        entry.Reverse();

        // Assert
        Assert.Equal(JournalEntryStatus.Reversed, entry.Status);
        Assert.True(entry.IsDeleted);
    }

    [Fact]
    public void IsBalanced_WhenDebitsEqualCredits_ShouldReturnTrue()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 100));

        // Act & Assert
        Assert.True(entry.IsBalanced);
    }

    [Fact]
    public void IsBalanced_WhenDebitsNotEqualCredits_ShouldReturnFalse()
    {
        // Arrange
        var entry = CreateTestJournalEntry();
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100));
        entry.AddLine(JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 50));

        // Act & Assert
        Assert.False(entry.IsBalanced);
    }

    private static JournalEntry CreateTestJournalEntry()
    {
        return JournalEntry.Create(
            Guid.NewGuid(),
            "JE2024-00001",
            DateTime.UtcNow,
            DateTime.UtcNow,
            "Test Entry");
    }
}

/// <summary>
/// Unit tests for JournalLine entity
/// </summary>
public class JournalLineTests
{
    [Fact]
    public void Create_WithDebit_ShouldSetDebitAmount()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var line = JournalLine.Create(accountId, "Test", debitAmount: 100);

        // Assert
        Assert.Equal(accountId, line.AccountId);
        Assert.Equal(100, line.DebitAmount);
        Assert.Equal(0, line.CreditAmount);
        Assert.True(line.IsDebit);
        Assert.False(line.IsCredit);
    }

    [Fact]
    public void Create_WithCredit_ShouldSetCreditAmount()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var line = JournalLine.Create(accountId, "Test", creditAmount: 100);

        // Assert
        Assert.Equal(accountId, line.AccountId);
        Assert.Equal(0, line.DebitAmount);
        Assert.Equal(100, line.CreditAmount);
        Assert.False(line.IsDebit);
        Assert.True(line.IsCredit);
    }

    [Theory]
    [InlineData(-100, 0)]
    [InlineData(0, -100)]
    public void Create_WithNegativeAmounts_ShouldThrowException(decimal debit, decimal credit)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            JournalLine.Create(Guid.NewGuid(), "Test", debit, credit));
    }

    [Fact]
    public void Create_WithBothDebitAndCredit_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            JournalLine.Create(Guid.NewGuid(), "Test", debitAmount: 100, creditAmount: 50));
    }

    [Fact]
    public void Create_WithZeroAmounts_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            JournalLine.Create(Guid.NewGuid(), "Test", 0, 0));
    }

    [Fact]
    public void NetAmount_ShouldCalculateDifference()
    {
        // Arrange
        var debitLine = JournalLine.Create(Guid.NewGuid(), "Debit", debitAmount: 100);
        var creditLine = JournalLine.Create(Guid.NewGuid(), "Credit", creditAmount: 50);

        // Act & Assert
        Assert.Equal(100, debitLine.NetAmount);
        Assert.Equal(-50, creditLine.NetAmount);
    }

    [Fact]
    public void MarkAsPosted_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var line = JournalLine.Create(Guid.NewGuid(), "Test", debitAmount: 100);

        // Act
        line.MarkAsPosted();

        // Assert
        Assert.False(line.IsActive);
        Assert.NotNull(line.PostedAt);
    }
}
