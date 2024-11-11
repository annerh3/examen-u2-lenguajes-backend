using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Dtos.Auth
{
    public class LoginDto
    {
        [Display(Name = "Correo Electrónico")]
        [Required(ErrorMessage = "El {0} es obligatorio")]
        [EmailAddress(ErrorMessage = "El campo {0} no es valido.")]
        public string Email { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public string Password { get; set; }
    }
}
