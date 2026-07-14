using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Auth.Login;

public class LoginCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        // Identical failure for unknown email vs wrong password — no user enumeration.
        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        var (token, expiresAt) = jwtService.CreateToken(user);
        return new AuthResult(token, expiresAt, new UserDto(user.Id, user.FirstName, user.LastName, user.Email));
    }
}
