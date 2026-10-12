
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using BankingAccountApi.Models;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankAccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BankAccountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/BankAccounts
    // GET: api/BankAccounts?accountHolderName=Arun
    // GET: api/BankAccounts?accountType=Savings
    // GET: api/BankAccounts?minBalance=1000&maxBalance=10000
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BankAccount>>> GetAll(
        [FromQuery] string? accountHolderName,
        [FromQuery] string? accountNumber,
        [FromQuery] string? accountType,
        [FromQuery] decimal? minBalance,
        [FromQuery] decimal? maxBalance)
    {
        var query = _context.BankAccounts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(accountHolderName))
        {
            query = query.Where(a =>
                a.AccountHolderName.Contains(accountHolderName));
        }

        if (!string.IsNullOrWhiteSpace(accountNumber))
        {
            query = query.Where(a =>
                a.AccountNumber.Contains(accountNumber));
        }

        if (!string.IsNullOrWhiteSpace(accountType))
        {
            query = query.Where(a =>
                a.AccountType == accountType);
        }

        if (minBalance.HasValue)
        {
            query = query.Where(a =>
                a.Balance >= minBalance.Value);
        }

        if (maxBalance.HasValue)
        {
            query = query.Where(a =>
                a.Balance <= maxBalance.Value);
        }

        var accounts = await query.ToListAsync();

        return Ok(accounts);
    }

    // GET: api/BankAccounts/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BankAccount>> GetById(int id)
    {
        var account = await _context.BankAccounts.FindAsync(id);

        if (account == null)
        {
            return NotFound(new
            {
                message = $"Account with ID {id} was not found."
            });
        }

        return Ok(account);
    }

    // POST: api/BankAccounts
    [HttpPost]
    public async Task<ActionResult<BankAccount>> Create(
        [FromBody] BankAccount account)
    {
        var accountNumberExists = await _context.BankAccounts
            .AnyAsync(a => a.AccountNumber == account.AccountNumber);

        if (accountNumberExists)
        {
            return Conflict(new
            {
                message = "This account number already exists."
            });
        }

        _context.BankAccounts.Add(account);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = account.Id },
            account);
    }

    // PUT: api/BankAccounts/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] BankAccount updatedAccount)
    {
        var account = await _context.BankAccounts.FindAsync(id);

        if (account == null)
        {
            return NotFound(new
            {
                message = $"Account with ID {id} was not found."
            });
        }

        var accountNumberExists = await _context.BankAccounts
            .AnyAsync(a =>
                a.AccountNumber == updatedAccount.AccountNumber &&
                a.Id != id);

        if (accountNumberExists)
        {
            return Conflict(new
            {
                message = "This account number already belongs to another account."
            });
        }

        account.AccountHolderName = updatedAccount.AccountHolderName;
        account.AccountNumber = updatedAccount.AccountNumber;
        account.AccountType = updatedAccount.AccountType;
        account.Balance = updatedAccount.Balance;

        await _context.SaveChangesAsync();

        return Ok(account);
    }

    // DELETE: api/BankAccounts/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await _context.BankAccounts.FindAsync(id);

        if (account == null)
        {
            return NotFound(new
            {
                message = $"Account with ID {id} was not found."
            });
        }

        _context.BankAccounts.Remove(account);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Bank account deleted successfully."
        });
    }

    // POST: api/BankAccounts/1/deposit
    [HttpPost("{id:int}/deposit")]
    public async Task<IActionResult> Deposit(
        int id,
        [FromBody] BalanceOperationRequest request)
    {
        var account = await _context.BankAccounts.FindAsync(id);

        if (account == null)
        {
            return NotFound(new
            {
                message = $"Account with ID {id} was not found."
            });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Deposit amount must be greater than zero."
            });
        }

        account.Balance += request.Amount;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Deposit successful.",
            account.Id,
            account.AccountNumber,
            account.Balance
        });
    }

    // POST: api/BankAccounts/1/withdraw
    [HttpPost("{id:int}/withdraw")]
    public async Task<IActionResult> Withdraw(
        int id,
        [FromBody] BalanceOperationRequest request)
    {
        var account = await _context.BankAccounts.FindAsync(id);

        if (account == null)
        {
            return NotFound(new
            {
                message = $"Account with ID {id} was not found."
            });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Withdrawal amount must be greater than zero."
            });
        }

        if (account.Balance < request.Amount)
        {
            return BadRequest(new
            {
                message = "Insufficient balance."
            });
        }

        account.Balance -= request.Amount;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Withdrawal successful.",
            account.Id,
            account.AccountNumber,
            account.Balance
        });
    }
}

// Request body used by Deposit and Withdraw.
public class BalanceOperationRequest
{
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}
