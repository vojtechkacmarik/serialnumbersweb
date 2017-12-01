using System.ComponentModel.DataAnnotations;

namespace SerialNumbers.Web.Models.SerialNumbersViewModels
{
    public class CreateSchemaViewModel
    {
        [Required]
        public string Schema { get; set; }

        [Required]
        public string Customer { get; set; }

        [Required]
        public string Mask { get; set; }

        public int? Seed { get; set; }

        public int? Increment { get; set; }
    }
}