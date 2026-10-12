
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankingAccountApi.Models;

public class BankAccount
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string AccountHolderName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    [Required]
    [RegularExpression("^(Savings|Current)$",
        ErrorMessage = "Account type must be Savings or Current.")]
    public string AccountType { get; set; } = string.Empty;
}
