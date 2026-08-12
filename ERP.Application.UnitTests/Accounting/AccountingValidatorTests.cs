using ERP.Application.Accounting.Commands.JournalEntries;
using ERP.Application.Accounting.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace ERP.Application.UnitTests.Accounting;

/// <summary>
/// Unit tests for Accounting domain validators
/// </summary>
public class AccountingValidatorTests
{
    #region CreateJournalEntryCommandValidator Tests

    [Fact]
    public void CreateJournalEntryCommand_ValidBalancedEntry_ShouldPass()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Cash Sale Transaction",
            Notes = "Recording cash sale",
            ReferenceType = "Sales",
            ReferenceNumber = "INV-001",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId1,
                    Description = "Cash received",
                    DebitAmount = 110000,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId2,
                    Description = "Sales revenue",
                    DebitAmount = 0,
                    CreditAmount = 110000
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateJournalEntryCommand_Title_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line",
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateJournalEntryCommand_Title_WhenEmptyOrNull_ShouldFail(string? title)
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = title!,
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line",
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void CreateJournalEntryCommand_Title_WhenExceeds500Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = new string('A', 501),
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line",
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 500 characters");
    }

    [Fact]
    public void CreateJournalEntryCommand_Title_When500Characters_ShouldPass()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = new string('A', 500),
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line",
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateJournalEntryCommand_EntryDate_WhenDefault_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = default,
            PostingDate = DateTime.UtcNow,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line",
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EntryDate)
            .WithErrorMessage("Entry date is required");
    }

    [Fact]
    public void CreateJournalEntryCommand_EntryDate_WhenTooFarInFuture_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow.AddDays(5),
            PostingDate = DateTime.UtcNow,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line",
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EntryDate)
            .WithErrorMessage("Entry date cannot be in the future");
    }

    [Fact]
    public void CreateJournalEntryCommand_PostingDate_WhenDefault_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = default,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line",
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PostingDate)
            .WithErrorMessage("Posting date is required");
    }

    [Fact]
    public void CreateJournalEntryCommand_Lines_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>()
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Lines)
            .WithErrorMessage("Journal entry must have at least one line");
    }

    [Fact]
    public void CreateJournalEntryCommand_Lines_WhenOnlyOneLine_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Single line only",
                    DebitAmount = 100,
                    CreditAmount = 0
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Lines)
            .WithErrorMessage("Journal entry must have at least 2 lines");
    }

    [Fact]
    public void CreateJournalEntryCommand_LineAccountId_WhenEmpty_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = Guid.Empty,
                    Description = "Test line",
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = Guid.NewGuid(),
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Lines[0].AccountId")
            .WithErrorMessage("Account is required for each line");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateJournalEntryCommand_LineDescription_WhenEmptyOrNull_ShouldFail(string? description)
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = description!,
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Lines[0].Description")
            .WithErrorMessage("Description is required for each line");
    }

    [Fact]
    public void CreateJournalEntryCommand_LineDescription_WhenExceeds500Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = new string('A', 501),
                    DebitAmount = 100,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 0,
                    CreditAmount = 100
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Lines[0].Description")
            .WithErrorMessage("Description cannot exceed 500 characters");
    }

    [Fact]
    public void CreateJournalEntryCommand_Line_WhenBothDebitAndCreditAreZero_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Invalid line",
                    DebitAmount = 0,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 100,
                    CreditAmount = 0
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Lines[0]")
            .WithErrorMessage("Each line must have either a debit or credit amount");
    }

    [Fact]
    public void CreateJournalEntryCommand_Line_WhenBothDebitAndCreditHaveValue_ShouldFail()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Valid Title",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Invalid line",
                    DebitAmount = 50,
                    CreditAmount = 50
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Test line 2",
                    DebitAmount = 50,
                    CreditAmount = 0
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Lines[0]")
            .WithErrorMessage("A line cannot have both debit and credit amounts");
    }

    #endregion

    #region Balanced Debits and Credits Tests

    [Fact]
    public void CreateJournalEntryCommand_BalancedDebitsAndCredits_ShouldPass()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var accountId3 = Guid.NewGuid();

        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Multi-line Transaction",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId1,
                    Description = "Cash",
                    DebitAmount = 100000,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId2,
                    Description = "Sales Revenue",
                    DebitAmount = 0,
                    CreditAmount = 90000
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId3,
                    Description = "VAT Payable",
                    DebitAmount = 0,
                    CreditAmount = 10000
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        // Note: The validator itself doesn't validate balance - that's done in the handler
        // But the individual line validations should pass
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(100, 100)]       // Equal - balanced
    [InlineData(1000, 1000)]     // Large equal amounts
    [InlineData(0.01, 0.01)]     // Small equal amounts
    public void CreateJournalEntryCommand_EqualDebitsAndCredits_ShouldPass(decimal debit, decimal credit)
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();

        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Balanced Entry",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Debit entry",
                    DebitAmount = debit,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Credit entry",
                    DebitAmount = 0,
                    CreditAmount = credit
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateJournalEntryCommand_UnbalancedEntry_ShouldFailAtHandler()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId = Guid.NewGuid();

        // This is a valid command per validator rules, but unbalanced
        // The balance check happens in the handler, not the validator
        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Unbalanced Entry",
            Lines = new List<CreateJournalLineDto>
            {
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Debit entry",
                    DebitAmount = 1000,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId,
                    Description = "Credit entry (less than debit)",
                    DebitAmount = 0,
                    CreditAmount = 500
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        // Validator passes because line-level validations are satisfied
        // Balance check is done at the handler level
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateJournalEntryCommand_MultipleDebitLines_BalancedWithCreditLines_ShouldPass()
    {
        // Arrange
        var validator = new CreateJournalEntryCommandValidator();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();
        var accountId3 = Guid.NewGuid();
        var accountId4 = Guid.NewGuid();

        var command = new CreateJournalEntryCommand
        {
            EntryDate = DateTime.UtcNow,
            PostingDate = DateTime.UtcNow,
            Title = "Compound Entry",
            Lines = new List<CreateJournalLineDto>
            {
                // Debit entries totaling 5000
                new CreateJournalLineDto
                {
                    AccountId = accountId1,
                    Description = "Equipment",
                    DebitAmount = 3000,
                    CreditAmount = 0
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId2,
                    Description = "VAT Input",
                    DebitAmount = 2000,
                    CreditAmount = 0
                },
                // Credit entries totaling 5000
                new CreateJournalLineDto
                {
                    AccountId = accountId3,
                    Description = "Bank",
                    DebitAmount = 0,
                    CreditAmount = 4500
                },
                new CreateJournalLineDto
                {
                    AccountId = accountId4,
                    Description = "Supplier",
                    DebitAmount = 0,
                    CreditAmount = 500
                }
            }
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
