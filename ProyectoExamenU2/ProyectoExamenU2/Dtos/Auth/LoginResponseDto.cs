namespace ProyectoExamenU2.Dtos.Auth
{
    public class LoginResponseDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
        public string RefreshToken { get; set; }
    }
}
