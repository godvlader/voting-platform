using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PRBD_Framework;

namespace MyPoll.Model;

public class Choice : EntityBase<MyPollContext> {
    [Required]
    public int Id { get; set; }
    [ForeignKey(nameof(Model.Poll))]
    public int PollId { get; set; }
    public string Label { get; set; }
    public virtual Poll Poll { get; set; }

    public virtual ICollection<Vote> Votes { get; set; } = new HashSet<Vote>();
    
    public Choice(string label, int pollId) {
        Label = label;
        PollId = pollId;

    }

    public int VoteCount {
        get { return Votes.Count; }
    }

    public Choice() { }


    public override bool Equals(object obj) {
        if (obj == null || obj is not Choice) {
            return false;
        }

        Choice other = (Choice)obj;
        return Id == other.Id && Label == other.Label; // Compare relevant properties, such as Id and Name
    }

    // Implement GetHashCode method for performance reasons
    public override int GetHashCode() {
        return HashCode.Combine(Id, Label);
    }

}

