namespace DuelApp.Modules.Duels.Infrastructure.Realtime;

public class DuelSession
{
    public Guid DuelId { get; }
    public Guid Player1Id { get; }
    public Guid Player2Id { get; }

    public object Lock { get; } = new();

    public DuelSession(Guid duelId, Guid player1Id, Guid player2Id)
    {
        DuelId = duelId;
        Player1Id = player1Id;
        Player2Id = player2Id;
    }
}