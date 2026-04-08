using System.ComponentModel.DataAnnotations;

namespace EMS.Web.Models.ViewModels
{
    public class SessionViewModel
    {
        public Guid SessionId { get; set; }

        [Required(ErrorMessage = "Event is required")]
        public Guid EventId { get; set; }

        [Required(ErrorMessage = "Session title is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Session title must be between 1 and 50 characters")]
        public string SessionTitle { get; set; } = string.Empty;

        public Guid? SpeakerId { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Session start time is required")]
        public DateTime SessionStart { get; set; }

        [Required(ErrorMessage = "Session end time is required")]
        [CustomValidation(typeof(SessionViewModel), nameof(ValidateSessionEnd))]
        public DateTime SessionEnd { get; set; }

        public string? SessionUrl { get; set; }

        public static ValidationResult? ValidateSessionEnd(DateTime endTime, ValidationContext context)
        {
            var instance = (SessionViewModel)context.ObjectInstance;
            if (endTime <= instance.SessionStart)
            {
                return new ValidationResult("Session end time must be after start time");
            }
            return ValidationResult.Success;
        }
    }
}