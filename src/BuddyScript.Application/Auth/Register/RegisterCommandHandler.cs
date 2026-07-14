using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Application.DTOs;
using BuddyScript.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Auth.Register;

public class RegisterCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IRequestHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        var exists = await db.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = jwtService.CreateToken(user);
        return new AuthResult(token, expiresAt, new UserDto(user.Id, user.FirstName, user.LastName, user.Email));
    }
}
