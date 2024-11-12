using ProyectoExamenU2.Dtos.Auth;
using ProyectoExamenU2.Dtos.Common;

namespace ProyectoExamenU2.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);

    }
}
