using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PRBD_Framework;

namespace MyPoll.Model;

public class Participation : EntityBase<MyPollContext> {
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PollId { get; set; }
    public virtual Poll Poll { get; set; }

    [ForeignKey(nameof(User))]
    public int UserId { get; set; }
    [Required]
    public virtual User User { get; set; }

    public Participation() {
       
    }

   
}



