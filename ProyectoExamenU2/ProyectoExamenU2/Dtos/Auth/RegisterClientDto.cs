using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Dtos.Auth
{
    public class RegisterClientDto : RegisterDto
    {
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string ClientName { get; set; }

        [Display(Name = "Id del Tipo de Cliente")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public Guid ClientTypeId { get; set; }
    }
}
