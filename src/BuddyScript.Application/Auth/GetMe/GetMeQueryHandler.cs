using BuddyScript.Application.Common.Exceptions;
using BuddyScript.Application.Common.Interfaces;
using BuddyScript.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuddyScript.Application.Auth.GetMe;

public class GetMeQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMeQuery, UserDto>
{
    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        var user = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email))
            .FirstOrDefaultAsync(cancellationToken);

        return user ?? throw new UnauthorizedException();
    }
}
