using BuddyScript.Application.DTOs;
using MediatR;

namespace BuddyScript.Application.Auth.GetMe;

public record GetMeQuery : IRequest<UserDto>;
