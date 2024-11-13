using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProyectoExamenU2.Databases.PrincipalDataBase.Entities
{
    public class UserEntity : IdentityUser
    {
        public string Name { get; set; }
        [Column("refresh_token")]
        [StringLength(450)]
        public string RefreshToken { get; set; }


        [Column("refresh_token_expire")]
        public DateTime RefreshTokenExpire { get; set; }
    }
}
