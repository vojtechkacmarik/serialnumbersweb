using System.ComponentModel.DataAnnotations;

namespace SerialNumbers.Web.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}