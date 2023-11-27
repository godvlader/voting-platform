using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using PRBD_Framework;

namespace MyPoll.Model;

public class User : EntityBase<MyPollContext> {
    [Key]
    public int Id { get; set; }
    public string FullName { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    public string Password { get; set; }

    public virtual ICollection<Poll> Polls { get; set; } = new HashSet<Poll>();

    public virtual ICollection<Poll> PollsCreated { get; set; } = new HashSet<Poll>();

    [ForeignKey("UserId")]
    
    public virtual ICollection<Comment> Comments { get; set; }= new HashSet<Comment>();

    public virtual ICollection<Vote> Votes { get; set; } = new HashSet<Vote>();
    public User() {
    }

    public int AllVotesCount {
        get { return Votes.Count; }
    }
    [NotMapped]
    public int VoteCount { get; set; }

    public int GetVoteCountForPoll(Poll poll) {

        var pollVotes = poll.Choices.Sum(c => c.Votes.Count(v => v.UserId == Id));

        return pollVotes;

    }

    public static User GetByFullName(string fullName) {
        using (var context = new MyPollContext()) {
            var user = context.Users.FirstOrDefault(u => u.FullName == fullName);
            return user;
        }
    }

    public static User[] GetUsers() {
        using (var context = new MyPollContext()) {
            var users = context.Users.ToArray();
            return users;
        }
    }

    public override bool Equals(object obj) {
        if (obj == null || obj is not User) {
            return false;
        }

        User other = (User)obj;
        return Id == other.Id && FullName == other.FullName; //relevant properties, such as Id and Name
    }

    // performance reasons
    public override int GetHashCode() {
        return HashCode.Combine(Id, FullName);
    }
}
