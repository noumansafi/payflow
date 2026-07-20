using MediatR;
using PayFlow.Application.Auth.Dtos;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;
using PayFlow.Domain.Enums;

namespace PayFlow.Application.Auth.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<RegisterResponseDto>;

public sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IEmailSender emailSender,
    IAuditLogger auditLogger,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterUserCommand, RegisterResponseDto>
{
    private const string DefaultCurrency = "USD";

    public async Task<RegisterResponseDto> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await users.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var now = clock.UtcNow;
        var verificationToken = tokenService.CreateRefreshToken();
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.User,
            IsEmailVerified = false,
            IsActive = true,
            EmailVerificationTokenHash = tokenService.HashToken(verificationToken),
            EmailVerificationTokenExpiresAtUtc = now.AddDays(2),
            CreatedAtUtc = now,
            Wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0m,
                Currency = DefaultCurrency,
                Status = WalletStatus.Active,
                CreatedAtUtc = now
            }
        };

        users.Add(user);

        await auditLogger.WriteAsync(
            AuditAction.Register,
            "User",
            user.Id,
            user.Id,
            """{"event":"user_registered"}""",
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailAsync(
            user.Email,
            "Verify your PayFlow email",
            $"Your verification token is: {verificationToken}",
            cancellationToken);

        return new RegisterResponseDto(
            new AuthUserDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role.ToString(),
                user.IsEmailVerified),
            verificationToken);
    }
}
