using System.ComponentModel.DataAnnotations;

public class WordBank{
    public int Id { get; set; }
    public required string guessWord { get; set; }
}

public class ValidWords{
    public int Id { get; set; }
    public required string validWord { get; set; }
}

public class WordDTO{
    [Required (ErrorMessage = "Word for validation is required")]
    public required string Word { get; set; }
}

public class ResponseDTO{
    // Example: {'h': 'green', 'e': 'yellow', 'l': 'grey', 'l': 'grey', 'o': 'yellow'}
    public required Dictionary<string, string> Response { get; set; }
}

public class WordComparer{
    public static ResponseDTO CompareWords(string guessWord, string wordOfTheDay, User user, WordleDB db){
        // Compare the guessWord with the wordOfTheDay
        // Return a ResponseDTO object

        Dictionary<string, string> response = new Dictionary<string, string>();
        bool isCorrect = true;

        for (int i = 0; i < guessWord.Length; i++)
        {
            if (guessWord[i] == wordOfTheDay[i])
            {
                response.Add($"{i}_{guessWord[i]}", "green");
            }
            else if (wordOfTheDay.Contains(guessWord[i]))
            {
                response.Add($"{i}_{guessWord[i]}", "yellow");
                isCorrect = false;
            }
            else
            {
                response.Add($"{i}_{guessWord[i]}", "grey");
                isCorrect = false;
            }
        }
        if (isCorrect)
        {
            user.NumberOfWins++;
            user.CurrentStreak++;
            db.UpdateUser(user);
        }
        return new ResponseDTO { Response = response };
    }
}