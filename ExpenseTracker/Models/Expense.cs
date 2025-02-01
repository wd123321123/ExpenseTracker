   
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Models;

public class Expense
{
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa wydatku jest wymagana")]
        [StringLength(100, ErrorMessage = "Maksymalnie 100 znaków")]
        [Display(Name = "Nazwa")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Kwota jest wymagana")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Kwota musi być większa niż 0")]
        [Display(Name = "Kwota")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Data jest wymagana")] 
        [Display(Name = "Data")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

}

