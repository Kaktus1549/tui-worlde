using System.Text.Json;

public class Attempts{
    public int Id { get; set; }
    public required int UserID { get; set; }
    public required int NumberOfTries { get; set; }
    public bool Won { get; set; }
}

public class AttemptsHistory{
    public int Id { get; set; }
    public required int AttemptID { get; set; }
    public required string Result { get; set; }
    // Not required, since it will be set automatically by the database
    public DateTime TimeStamp { get; set; }
}