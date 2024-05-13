using BobsBetting.DBModels;

namespace BobsBetting.Models;

public class UserConnectionReq
{
    public int UserId { get; set; } = 0;

    public string LobbyId { get; set; } = string.Empty;
}

public class UserConnectionRes(int userId, string username, int chips, string lobbyId)
{
    public int UserId { get; set; } = userId;
    public string Username { get; set; } = username;
    public int Chips { get; set; } = chips;
    public string LobbyId { get; set; } = lobbyId;
}

public class UserConnection(User user, string lobbyId)
{
    public User User { get; set; } = user;
    public string LobbyId { get; set; } = lobbyId;
}