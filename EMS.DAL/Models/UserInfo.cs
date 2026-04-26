using System.ComponentModel.DataAnnotations;

namespace EMS.DAL.Models
{
    public class UserInfo
    {
        [Key]
        [EmailAddress]
        public string EmailId { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        public ICollection<ParticipantEventDetails> ParticipantEvents { get; set; } = new List<ParticipantEventDetails>();
    }
}