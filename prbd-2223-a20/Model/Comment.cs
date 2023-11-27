using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PRBD_Framework;

namespace MyPoll.Model;

public class Comment : EntityBase<MyPollContext> {
    [Key]
    public int Id { get; internal set; }
    [ForeignKey(nameof(User))]
    public int UserId { get; internal set; }
    [ForeignKey(nameof(poll))]
    public int PollId { get; internal set; }
    public string Text { get; internal set; }
    public DateTime Timestamp { get; internal set; }

    public virtual User User { get; internal set; }
    public virtual Poll poll { get; internal set;}

    public Comment( ) {

    }

    public Comment(int userId, int pollId, string text, DateTime timestamp ) {
        UserId = userId;
        PollId = pollId;
        Text = text;
        Timestamp = timestamp;

    }
}

