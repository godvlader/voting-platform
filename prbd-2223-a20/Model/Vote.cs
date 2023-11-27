using System.ComponentModel.DataAnnotations.Schema;
using PRBD_Framework;

namespace MyPoll.Model;

public enum VoteType {
    No = -1,
    Maybe = 0,
    Yes = 1,
    NoVote
}

public class Vote : EntityBase<MyPollContext> {

    [ForeignKey(nameof(User))]
    public int UserId { get; internal set; }
    [ForeignKey(nameof(Model.Choice))]
    public int ChoiceId { get; internal set; }
    public VoteType Type { get; set; }

    public virtual User user { get; set; }

    public virtual Choice Choice { get; set; }
    public virtual Poll Poll { get; internal set; }

    public static readonly Dictionary<VoteType, double> VoteTypeScores = new() {
        { VoteType.No, -1 },
        { VoteType.Maybe, 0.5 },
        { VoteType.Yes, 1 },
    };

    public Vote() {
            
    }

}

