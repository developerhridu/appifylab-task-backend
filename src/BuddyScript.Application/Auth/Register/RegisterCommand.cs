using BuddyScript.Application.Auth;
using MediatR;

namespace BuddyScript.Application.Auth.Register;

public record RegisterCommand(string FirstName, string LastName, string Email, string Password)
    : IRequest<AuthResult>;
