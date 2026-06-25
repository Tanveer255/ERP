using BuildingBlocks.Application;
using Identity.Application.Auth.Commands;
using Identity.Application.Auth.Queries;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Identity.Application.Auth.Handlers;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    BuildingBlocks.Domain.Repositories.IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IConfiguration configuration) : CommandHandler<LoginCommand, AuthTokenDto>
{
    public override async Task<AuthTokenDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var userWithRoles = await userRepository.GetWithRolesAsync(user.Id, cancellationToken) ?? user;
        var roles = userWithRoles.UserRoles.Select(x => x.Role.Name).ToList();
        var permissions = userWithRoles.UserRoles
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToList();

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user.Id, user.TenantId, user.Email, roles, permissions);
        var refreshTokenValue = tokenService.GenerateRefreshToken();
        var refreshDays = int.Parse(configuration["Jwt:RefreshTokenDays"] ?? "7");
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenValue, DateTime.UtcNow.AddDays(refreshDays));

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokenDto(accessToken, refreshTokenValue, expiresAt);
    }
}

public class RegisterCommandHandler(
    IUserRepository userRepository,
    BuildingBlocks.Domain.Repositories.IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher) : CommandHandler<RegisterCommand, UserDto>
{
    public override async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
            throw new InvalidOperationException("Email already registered.");

        var user = ApplicationUser.Create(
            request.TenantId,
            request.Email,
            passwordHasher.Hash(request.Password),
            request.FirstName,
            request.LastName);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.TenantId, user.Email, user.FullName, [], []);
    }
}

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    BuildingBlocks.Domain.Repositories.IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IConfiguration configuration) : CommandHandler<RefreshTokenCommand, AuthTokenDto>
{
    public override async Task<AuthTokenDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!existing.IsActive)
            throw new UnauthorizedAccessException("Refresh token expired or revoked.");

        var user = await userRepository.GetWithRolesAsync(existing.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        existing.Revoke();
        refreshTokenRepository.Update(existing);

        var roles = user.UserRoles.Select(x => x.Role.Name).ToList();
        var permissions = user.UserRoles.SelectMany(x => x.Role.RolePermissions).Select(x => x.Permission.Code).Distinct().ToList();
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user.Id, user.TenantId, user.Email, roles, permissions);

        var refreshTokenValue = tokenService.GenerateRefreshToken();
        var refreshDays = int.Parse(configuration["Jwt:RefreshTokenDays"] ?? "7");
        var newRefresh = RefreshToken.Create(user.Id, refreshTokenValue, DateTime.UtcNow.AddDays(refreshDays));
        await refreshTokenRepository.AddAsync(newRefresh, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokenDto(accessToken, refreshTokenValue, expiresAt);
    }
}

public class GetCurrentUserQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetWithRolesAsync(request.UserId, cancellationToken);
        if (user is null) return null;

        var roles = user.UserRoles.Select(x => x.Role.Name).ToList();
        var permissions = user.UserRoles.SelectMany(x => x.Role.RolePermissions).Select(x => x.Permission.Code).Distinct().ToList();
        return new UserDto(user.Id, user.TenantId, user.Email, user.FullName, roles, permissions);
    }
}
