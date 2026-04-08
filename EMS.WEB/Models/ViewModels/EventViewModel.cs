using System.ComponentModel.DataAnnotations;

namespace EMS.Web.Models.ViewModels
{
    public class EventViewModel
    {
        public Guid EventId { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Event name must be between 1 and 50 characters")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event category is required")]
        public string EventCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required")]
        [CustomValidation(typeof(EventViewModel), nameof(ValidateFutureDate))]
        public DateTime EventDate { get; set; }

        public string? Description { get; set; }

        public string Status { get; set; } = "Active";

        public static ValidationResult? ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date <= DateTime.Now)
            {
                return new ValidationResult("Event date must be in the future");
            }
            return ValidationResult.Success;
        }
    }
}