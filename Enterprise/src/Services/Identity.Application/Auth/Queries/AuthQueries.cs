using BuildingBlocks.Application;
using Identity.Application.DTOs;

namespace Identity.Application.Auth.Queries;

public record GetCurrentUserQuery(Guid UserId) : IQuery<UserDto?>;
