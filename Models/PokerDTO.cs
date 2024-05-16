namespace BobsBetting.DTOs
{
    public record LoginDto(string Username, string Password);

    public record SignupDto(string Username, string Email, string Password);

    public record LoginResDto(int Id, string Username, string Email, int Chips);
}