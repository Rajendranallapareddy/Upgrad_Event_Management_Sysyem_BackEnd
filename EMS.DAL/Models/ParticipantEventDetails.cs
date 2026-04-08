using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMS.DAL.Models
{
    public class ParticipantEventDetails
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [EmailAddress]
        public string ParticipantEmailId { get; set; } = string.Empty;

        [Required]
        public Guid EventId { get; set; }

        public bool IsAttended { get; set; } = false;

        [ForeignKey("ParticipantEmailId")]
        public virtual UserInfo Participant { get; set; } = null!;

        [ForeignKey("EventId")]
        public virtual EventDetails Event { get; set; } = null!;
    }
}