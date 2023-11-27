using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PRBD_Framework;

namespace MyPoll.Model;

public enum PollType {
    Multiple,
    Single,
}

public class Poll : EntityBase<MyPollContext> {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    [ForeignKey(nameof(User))]
    public int CreatorId { get; set; }

    public PollType Type { get; set; }
    public bool Closed { get; set; }

    public virtual User Creator { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

    public virtual ICollection<User> Participants { get; set; } = new HashSet<User>();

    public virtual ICollection<Choice> Choices { get; set; } = new HashSet<Choice>();

    public Poll() {

    }

    public Poll(string name,int creatorId, PollType type , bool closed) {
        Name = name;
        CreatorId = creatorId;
        Type= type;
        Closed = closed;
    }

    public int NumberOfParticipants => Participants.Count;
    public int NumberOfVotes { 
        get {
            return Choices.Sum(choice => choice.Votes.Count);
        }
    }

    public static IQueryable<Poll> GetAdminPolls() {
        return Context.Polls.OrderBy(p => p.Name);
    }

    public static IQueryable<Poll> GetPolls(int userId) {
        int currentUserId = userId;
        return Context.Polls
            .Include(p => p.Participants)
            .Where(p => p.CreatorId == currentUserId || p.Participants.Any(participant => participant.Id == currentUserId))
            .OrderBy(p => p.Name);
    }

    public string BestChoice => GetBestChoice(Id);

    public static string GetBestChoice(int pollId) {
        using (var context = new MyPollContext()) {
            var myPoll = context.Polls
                .Include(p => p.Choices)
                .ThenInclude(c => c.Votes)
                .FirstOrDefault(p => p.Id == pollId);

            if (myPoll == null) {
                return string.Empty;
            }

            var choicesWithScores = myPoll.Choices
                .Select(c => new {
                    Choice = c,
                    Score = c.Votes.Sum(v => Vote.VoteTypeScores[v.Type])
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            var bestChoices = choicesWithScores
                .Where(x => x.Score >= 3)
                .Take(2)
                .ToList();

            if (!bestChoices.Any()) {
                var bestChoice = choicesWithScores.FirstOrDefault(x => x.Score > 0);
                if (bestChoice != null) {
                    return $"{bestChoice.Choice.Label} ({bestChoice.Score})";
                } else {
                    return string.Empty;
                }
            } else {
                return string.Join("\n", bestChoices.Select(bc => $"{bc.Choice.Label} ({bc.Score})"));
            }
        }
    }

    public static IEnumerable<User> GetUsersByPoll(Poll poll) {
        var users = from u in poll.Participants
                    select u;
        return users;
    }

    public static IEnumerable<Choice> GetChoicesByPoll(Poll poll) {
        var choices = from c in Context.Choices
                      where c.PollId == poll.Id
                      select c;

        return choices;
    }

    public static IEnumerable<Comment> GetCommentsByPoll(Poll poll) {
        var comments = from c in poll.Comments
                       orderby c.Timestamp descending
                       select c;
        return comments;
    }


    public static PollType[] GetTypes() {
        return (PollType[])Enum.GetValues(typeof(PollType));
    }

}


