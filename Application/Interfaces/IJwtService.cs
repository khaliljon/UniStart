using UniStart.Domain.Entities;

namespace UniStart.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    int? ValidateToken(string token);
}
