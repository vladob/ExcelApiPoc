using System;
using System.Linq;
using AccountBalanceVerification.Core;
using Xunit;

namespace AccountBalanceVerification.Tests;
public class DecreePlanTests
{
    [Theory]
    [InlineData(PrintPlan.FirstPage, true, false)]
    [InlineData(PrintPlan.Accounts, false, true)]
    [InlineData(PrintPlan.All, true, true)]
    public void MigrationPreservesIndependentPrintSelections(string old, bool title, bool accounts)
    {
        var person = DecreePlan.Migrate("Predseda", "Chair", old);
        Assert.Equal(title, person.OnTitle);
        Assert.Equal(accounts, person.OnAccounts);
        Assert.Equal(title, PrintPlan.Select(new[] { person }, true).Any());
        Assert.Equal(accounts, PrintPlan.Select(new[] { person }, false).Any());
        Assert.Equal(DecreePlan.Recipient, person.DecreeRole);
        Assert.Equal(DecreePlan.Commission, person.RecapRole);
    }
    [Fact]
    public void ResponsiblePersonDoesNotBecomeCommissionMemberOrAutomaticSigner()
    {
        var person = DecreePlan.Migrate("Zodpovedná osoba", "Name", PrintPlan.FirstPage);
        Assert.Empty(person.DecreeRole);
        Assert.Empty(person.RecapRole);
        Assert.True(person.OnTitle);
        Assert.False(person.OnAccounts);
        Assert.Empty(DecreePlan.Migrate("Starosta", "Mayor", PrintPlan.All).DecreeRole);
    }
    [Theory]
    [InlineData("Predseda komisie", "predsedu")]
    [InlineData("Predsedníčka", "predsedníčku")]
    [InlineData("Člen", "člena")]
    [InlineData("Členka", "členku")]
    public void WordingUsesExplicitRoleRatherThanGuessingGender(string role, string word) =>
        Assert.Equal(word, DecreePlan.Appointment(role));
    [Fact]
    public void SamePersonCanSignDecreesAndRecapWithoutReceivingDecree()
    {
        var recipient = DecreePlan.Migrate("Člen", "Recipient", PrintPlan.All);
        var signer = new Signatory { Role = "Starosta", Name = "Signer", OnTitle = false, OnAccounts = false,
            DecreeRole = DecreePlan.Signer, RecapRole = DecreePlan.Signer };
        var rows = new[] { recipient, signer, new Signatory { DecreeRole = DecreePlan.Recipient } };
        Assert.Same(signer, DecreePlan.Validate(rows));
        Assert.Single(DecreePlan.Recipients(rows));
        Assert.Single(PrintPlan.Select(rows, true));
    }
    [Fact]
    public void MissingOrMultipleSignersAreRejected()
    {
        var recipient = DecreePlan.Migrate("Člen", "Recipient", PrintPlan.All);
        var signer = new Signatory { Role = "Starosta", Name = "Signer", DecreeRole = DecreePlan.Signer };
        Assert.Throws<InvalidOperationException>(() => DecreePlan.Validate(new[] { recipient }));
        Assert.Throws<InvalidOperationException>(() => DecreePlan.Validate(new[] { recipient, signer, signer }));
    }
    [Fact]
    public void UnknownRecipientRoleAndInvalidPastedSelectionAreRejected()
    {
        var recipient = new Signatory { Role = "Other", Name = "Recipient", DecreeRole = DecreePlan.Recipient };
        var signer = new Signatory { Role = "Starosta", Name = "Signer", DecreeRole = DecreePlan.Signer };
        Assert.Throws<InvalidOperationException>(() => DecreePlan.Validate(new[] { recipient, signer }));
        recipient.Role = "Člen"; signer.DecreeRole = "Typo";
        Assert.Throws<InvalidOperationException>(() => DecreePlan.Validate(new[] { recipient, signer }));
        Assert.Throws<InvalidOperationException>(() => DecreePlan.Migrate("Člen", "Name", "Typo"));
    }
}
