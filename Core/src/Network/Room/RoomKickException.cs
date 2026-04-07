namespace Markwardt;

public class RoomKickException(string? reason) : Exception(reason ?? "Kicked from room");