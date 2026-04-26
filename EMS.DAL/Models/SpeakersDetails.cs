using System.ComponentModel.DataAnnotations;

namespace EMS.DAL.Models
{
    public class SpeakersDetails
    {
        [Key]
        public Guid SpeakerId { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string SpeakerName { get; set; } = string.Empty;

        public ICollection<SessionInfo> Sessions { get; set; } = new List<SessionInfo>();
    }
}