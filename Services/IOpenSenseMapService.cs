using OpenSenseMapApiService.Models;

namespace OpenSenseMapApiService.Services
{
    public interface IOpenSenseMapService
    {
        Task<string> RegisterUserAsync(OpenSenseMapUser user);
        Task<LoginResponseDto> LoginUserAsync(LoginRequestDto loginRequest);

        Task<SenseBoxResponseDto> GetSenseBoxByIdAsync(string boxId);
        Task<NewSenseBoxResponseDto> CreateSenseBoxAsync(NewSenseBoxRequestDto boxRequest, string jwtToken);
        Task<bool> LogoutAsync(string jwtToken);
    }
}
