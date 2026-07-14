using BuddyScript.Application.Auth;
using MediatR;

namespace BuddyScript.Application.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
