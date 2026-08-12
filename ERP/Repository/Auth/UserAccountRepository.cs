using ERP.Data.DTO.Auth;
using ERP.Data.Request;
using ERP.Entity.Auth;
using ERP.Enum;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;

namespace ERP.Repository.Auth;

/// <summary>
/// User Repository interface.
/// </summary>
public interface IUserAccountRepository : IRepository<User>
{
    /// <summary>
    /// Retrieves a user entity based on the specified email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="User"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User> GetUserByEmailAsync(string email);

    /// <summary>
    /// Method of UserAccount Service to get regular user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<User> GetRegularUserByEmailAsync(string email);

    /// <summary>
    /// Checks whether a user exists with the specified email address.
    /// </summary>
    /// <param name="email">The email address to check for an existing user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains <c>true</c> if a user with the given email exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsUserExistByEmailAsync(string email);
    /// <summary>
    /// Determines whether an active user with the specified identifier exists in the system.
    /// </summary>
    /// <remarks>This method checks for the existence of a user with the given identifier who has an active
    /// status.</remarks>
    /// <param name="id">The unique identifier of the user to check.</param>
    /// <returns><see langword="true"/> if an active user with the specified identifier exists; otherwise, <see
    /// langword="false"/>.</returns>
    public Task<bool> IsExistByIdAsync(Guid id);

    /// <summary>
    /// Retrieves a user entity based on the specified tenant ID.
    /// </summary>
    /// <param name="tenantId">The tenant ID associated with the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="User"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User> GetUserByTenantIdAsync(string tenantId);

    /// <summary>
    /// Retrieves the first user that matches the given tenantId.
    /// </summary>
    /// <param name="tenantId">Tenant Id of the user.</param>
    /// <returns>The matching user, or null if not found.</returns>
    Task<User> FindUserByTenantIdAsync(string tenantId);

    /// <summary>
    /// Retrieves a user entity based on the specified user ID.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="User"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User> GetUserByIdAsync(Guid id);

    /// <summary>
    /// Get all users except staff
    /// </summary>
    /// <returns>Returns all users except staff</returns>
    Task<List<User>> GetAllAsync();

    /// <summary>
    /// Export all users except staff
    /// </summary>
    /// <returns>Returns all users except staff</returns>
    Task<List<AdminUserResponse>> ExportAllUsersAsync(ExportAllUsersRequest request);

    /// <summary>
    /// Retrieves a user entity based on the specified email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the <see cref="User"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    Task<User> GetUserByEmailAndRoleAsync(string email, string role);

    /// <summary>
    /// Retrieves a filtered, searchable, and sortable list of admin users.
    /// </summary>
    /// <param name="request">Contains search text, date filters, and sorting options.</param>
    /// <returns>
    /// A queryable list of <see cref="AdminUserResponse"/> objects matching the provided filters.
    /// </returns>
    IQueryable<ExportAllUsersEventResponse> ExportFilteredUsers(ExportAllUsersEventRequest request);
    IQueryable<GetAllUsersEventResponse> GetFilteredUsers(GetAllUsersEventRequest request);

    /// <summary>
    /// Calculates dashboard statistics for tenants, including total count,
    /// current month's new tenants, and percentage increase from previous totals.
    /// </summary>
    /// <returns>A <see cref="GetDashboardCountsResponse"/> containing the counts and percentage increase.</returns>
    Task<GetDashboardCountsEventResponse> GetUserStatsWithMonthlyGrowthAsync();

    /// <summary>
    /// Retrieves tenant registration counts grouped by the specified period type and duration.
    /// </summary>
    /// <param name="periodType">The type of period to group by (Week or Month).</param>
    /// <param name="duration">The number of periods to include in the result.</param>
    /// <returns>A list of <see cref="PeriodicCountDTO"/> with counts for each period.</returns>
    Task<List<GetPeriodicCountsEventResponse>> GetUsersPeriodicStatsAsync(PeriodEventType periodType, int duration);

    /// <summary>
    /// Retrieves the details of a user associated with the specified tenant ID, including 
    /// their personal information, company details, phone, country, and address.
    /// </summary>
    /// <param name="tenantId">The tenant ID of the user whose details are to be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. 
    /// The task result contains a <see cref="GetUserDetailsForEmailDTO"/> object with the user's details, 
    /// or null if no user is found for the given tenant ID.</returns>
    Task<UserDetailsForEmailDTO> GetUserDetailsForEmailByTenantIdAsync(string tenantId);
    /// <summary>
    /// Update Last Activity By TenantId Async
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    Task<bool> UpdateLastActivityByTenantIdAsync(string tenantId);
    Task<bool> UpdateLastActivityByIdAsync(Guid id);
    Task<bool> UpdateLastActivityByEmailAsync(string email);
}

/// <summary>
/// Initializes a new instance of the <see cref="UserAccountRepository"/> class.
/// </summary>
/// <param name="unitOfWork">is an object of IUnitOfWork.</param>
/// <param name="logger">is an object of ILogger.</param>
public class UserAccountRepository(
    IUnitOfWork unitOfWork,
    ILogger<UserAccountRepository> logger
    ) : Repository<User>(unitOfWork, logger), IUserAccountRepository
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<UserAccountRepository> _logger = logger;

    /// <summary>
    /// Method of UserAccount Service to get user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _unitOfWork.Context.Users.Where(user => user.Email.Equals(email) && user.Status == nameof(UserStatus.Active)).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Method of UserAccount Service to get regular user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User> GetRegularUserByEmailAsync(string email)
    {
        return await _unitOfWork.Context.Users.Where(user => user.Email.Equals(email) && user.UserType != nameof(AccessRole.Staff) && user.Status == nameof(UserStatus.Active)).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Method of UserAccount Service to check is user exists by email.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<bool> IsUserExistByEmailAsync(string email)
    {
        return await _unitOfWork.Context.Users.AnyAsync(user => user.Email.Equals(email) && user.Status == nameof(UserStatus.Active));
    }
    /// <summary>
    /// Determines whether an active user with the specified identifier exists in the system.
    /// </summary>
    /// <remarks>This method checks for the existence of a user with the given identifier who has an active
    /// status.</remarks>
    /// <param name="id">The unique identifier of the user to check.</param>
    /// <returns><see langword="true"/> if an active user with the specified identifier exists; otherwise, <see
    /// langword="false"/>.</returns>
    public async Task<bool> IsExistByIdAsync(Guid id)
    {
        return await _unitOfWork.Context.Users.AnyAsync(user => user.Id.Equals(id) && user.Status == nameof(UserStatus.Active));
    }

    /// <summary>
    /// Method of UserAccount Service to get user by tennat Id
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public async Task<User> GetUserByTenantIdAsync(string tenantId)
    {
        return await _unitOfWork.Context.Users.Where(user => user.TenantId.Equals(tenantId) && user.Status == nameof(UserStatus.Active)).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves the first user that matches the given tenantId.
    /// </summary>
    /// <param name="tenantId">Tenant Id of the user.</param>
    /// <returns>The matching user, or null if not found.</returns>
    public async Task<User> FindUserByTenantIdAsync(string tenantId)
    {
        return await _unitOfWork.Context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId);
    }

    /// <summary>
    /// get user by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<User> GetUserByIdAsync(Guid id)
    {
        return await _unitOfWork.Context.Users.Where(user => user.Id.Equals(id) && user.Status == nameof(UserStatus.Active)).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get all users except staff
    /// </summary>
    /// <returns>Returns all users except staff</returns>
    public async Task<List<User>> GetAllAsync()
    {
        return await _unitOfWork.Context.Users.Where(user => user.UserType != (nameof(AccessRole.Staff)) && user.Status == nameof(UserStatus.Active)).ToListAsync();
    }

    /// <summary>
    /// Method of UserAccount Service to get user by email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<User> GetUserByEmailAndRoleAsync(string email, string role)
    {
        return await _unitOfWork.Context.Users.Where(user => user.Email.Equals(email) && user.Status == nameof(UserStatus.Active) && user.UserType == role).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Export all users except staff
    /// </summary>
    /// <returns>Returns all users except staff</returns>
    public async Task<List<AdminUserResponse>> ExportAllUsersAsync(ExportAllUsersRequest request)
    {
        var query = GetAllReadOnly()
            .Where(u => u.UserType != nameof(AccessRole.Staff) && u.EmailConfirmed);

        var start = request.StartDate?.Date;
        var end = request.EndDate?.Date.AddDays(1).AddTicks(-1);

        if (start.HasValue && end.HasValue)
            query = query.Where(u =>
                u.CreatedAt >= request.StartDate.Value &&
                u.CreatedAt <= request.EndDate.Value);

        else if (start.HasValue)
            query = query.Where(u =>
                u.CreatedAt >= request.StartDate.Value);

        else if (end.HasValue)
            query = query.Where(u => u.CreatedAt <= request.EndDate.Value);

        return await query
            .Join(_unitOfWork.Context.Tenants, user => user.TenantId, tenant => tenant.TenantId,
                (user, tenant) => new { user, tenant })
            .Join(_unitOfWork.Context.Companies, x => x.user.TenantId, company => company.TenantId,
                (x, company) => new { x.user, x.tenant, company })
            .Join(_unitOfWork.Context.Settings, x => x.user.TenantId, setting => setting.TenantId,
                (x, setting) => new { x.user, x.tenant, x.company, setting })
            .Join(
                _unitOfWork.Context.Addresses.Where(a => a.Type == AddressCategory.Primary.ToString()),
                x => x.user.TenantId,
                address => address.TenantId,
                (x, address) => new { x.user, x.tenant, x.company, x.setting, address }
            )
            .OrderByDescending(x => x.user.CreatedAt)
            .Select(x => new AdminUserResponse
            {
                TenantId = x.user.TenantId,
                CreatedAt = x.user.CreatedAt,
                Email = x.user.Email,

                TenantName = x.tenant.TenantName,

                Industry = x.company.PrimaryBusinessSector,
                IsPartner = x.company.IsPartner,
                PartnerType = x.company.PartnerType,

                IsSupported = x.setting.IsSupportReq,

                Country = x.address.CountryName
            })
            .ToListAsync();
    }


    /// <summary>
    /// Retrieves a filtered, searchable, and sortable list of admin users.
    /// </summary>
    /// <remarks>
    /// Excludes staff accounts by default and supports multiple layers of filtering:
    /// <list type="bullet">
    /// <item><b>Search Query:</b> Matches against tenant ID, email, or creation date.</item>
    /// <item><b>Date Filters:</b> Supports MTD, last quarter, last half, and YTD ranges.</item>
    /// <item><b>Sorting:</b> Allows sorting by creation date or email in ascending or descending order.</item>
    /// </list>
    /// The method returns an <see cref="IQueryable{T}"/> to allow further composition before execution.
    /// </remarks>
    /// <param name="request">Contains search text, date filters, and sorting options.</param>
    /// <returns>A queryable list of <see cref="AdminUserResponse"/> objects matching the provided filters.</returns>
    public IQueryable<GetAllUsersEventResponse> GetFilteredUsers(GetAllUsersEventRequest request)
    {
        var usersList = GetAllReadOnly().Where(u => u.UserType != nameof(AccessRole.Staff) && u.EmailConfirmed)
            .Join(_unitOfWork.Context.Companies, x => x.TenantId, company => company.TenantId, (user, company) => new { user, company })
            .Join(_unitOfWork.Context.Settings, x => x.user.TenantId, setting => setting.TenantId, (x, setting) => new { x.user, x.company, setting })
            .Join(_unitOfWork.Context.Addresses.Where(x => x.Type == AddressCategory.Primary.ToString()),
                x => x.user.TenantId, address => address.TenantId, (x, address) => new { x.user, x.company, x.setting, address });

        if (!string.IsNullOrEmpty(request.Query))
        {
            var normalizedQuery = request.Query.ToLower();

            bool isDateSearch = DateTime.TryParse(normalizedQuery, out var searchDate);

            usersList = usersList.Where(x =>
                x.user.TenantId.ToLower().Contains(normalizedQuery) ||
                x.user.Email.ToLower().Contains(normalizedQuery) ||
                x.company.CompanyName.ToLower().Contains(normalizedQuery) ||
                x.company.PartnerType.ToLower().Contains(normalizedQuery) ||
                (isDateSearch && x.user.CreatedAt.Value.Date == searchDate.Date)
            );
        }

        if (!string.IsNullOrEmpty(request.Query))
        {
            var normalizedQuery = request.Query.ToLower();

            bool isDateSearch = DateTime.TryParse(normalizedQuery, out var searchDate);

            usersList = usersList.Where(x =>
                x.user.TenantId.ToLower().Contains(normalizedQuery) ||
                x.user.Email.ToLower().Contains(normalizedQuery) ||
                x.company.CompanyName.ToLower().Contains(normalizedQuery) ||
                x.company.PartnerType.ToLower().Contains(normalizedQuery) ||
                (isDateSearch && x.user.CreatedAt.Value.Date == searchDate.Date)
            );
        }

        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            var start = request.StartDate?.Date;
            var end = request.EndDate?.Date.AddDays(1).AddTicks(-1);

            if (start.HasValue && end.HasValue)
                usersList = usersList.Where(x => x.user.CreatedAt >= start && x.user.CreatedAt <= end);
            else if (start.HasValue)
                usersList = usersList.Where(x => x.user.CreatedAt >= start);
            else if (end.HasValue)
                usersList = usersList.Where(x => x.user.CreatedAt <= end);
        }

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            bool isAscending = request.SortDirection?.Trim().ToLower() == "asc";

            usersList = request.SortBy.ToLower() switch
            {
                "createdat" =>
                    isAscending ? usersList.OrderBy(x => x.user.CreatedAt) : usersList.OrderByDescending(x => x.user.CreatedAt),

                "lastactivity" =>
                    isAscending ? usersList.OrderBy(x => x.user.LastActivity) : usersList.OrderByDescending(x => x.user.LastActivity),

                "tenantname" =>
                    isAscending ? usersList.OrderBy(x => x.company.CompanyName) : usersList.OrderByDescending(x => x.company.CompanyName),

                "ispartner" =>
                    isAscending ? usersList.OrderBy(x => x.company.IsPartner) : usersList.OrderByDescending(x => x.company.IsPartner),

                "partnertype" =>
                    isAscending ? usersList.OrderBy(x => x.company.PartnerType) : usersList.OrderByDescending(x => x.company.PartnerType),

                "issupported" =>
                    isAscending ? usersList.OrderBy(x => x.setting.IsSupportReq) : usersList.OrderByDescending(x => x.setting.IsSupportReq),

                _ => usersList.OrderByDescending(x => x.user.CreatedAt)
            };
        }
        else
        {
            usersList = usersList.OrderByDescending(x => x.user.CreatedAt);
        }


        var query = usersList.Select(x => new GetAllUsersEventResponse
        {
            TenantId = x.user.TenantId,
            CreatedAt = x.user.CreatedAt,
            Email = x.user.Email,
            LastActivity = x.user.LastActivity,

            TenantName = x.company.CompanyName,

            Industry = x.company.PrimaryBusinessSector,
            IsPartner = x.company.IsPartner,
            PartnerType = x.company.PartnerType,

            IsSupported = x.setting.IsSupportReq,

            Country = x.address.CountryName
        });

        return query;
    }
    public IQueryable<ExportAllUsersEventResponse> ExportFilteredUsers(ExportAllUsersEventRequest request)
    {
        var usersList = GetAllReadOnly().Where(u => u.UserType != nameof(AccessRole.Staff) && u.EmailConfirmed)
            .Join(_unitOfWork.Context.Tenants, user => user.TenantId, tenant => tenant.TenantId, (user, tenant) => new { user, tenant })
            .Join(_unitOfWork.Context.Companies, x => x.user.TenantId, company => company.TenantId, (x, company) => new { x.user, x.tenant, company })
            .Join(_unitOfWork.Context.Settings, x => x.user.TenantId, setting => setting.TenantId, (x, setting) => new { x.user, x.tenant, x.company, setting })
            .Join(_unitOfWork.Context.Addresses.Where(x => x.Type == AddressCategory.Primary.ToString()),
                x => x.user.TenantId, address => address.TenantId, (x, address) => new { x.user, x.tenant, x.company, x.setting, address });

        if (!string.IsNullOrEmpty(request.Query))
        {
            var normalizedQuery = request.Query.ToLower();

            bool isDateSearch = DateTime.TryParse(normalizedQuery, out var searchDate);

            usersList = usersList.Where(x =>
                x.user.TenantId.ToLower().Contains(normalizedQuery) ||
                x.user.Email.ToLower().Contains(normalizedQuery) ||
                x.company.CompanyName.ToLower().Contains(normalizedQuery) ||
                x.company.PartnerType.ToLower().Contains(normalizedQuery) ||
                (isDateSearch && x.user.CreatedAt.Value.Date == searchDate.Date)
            );
        }

        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            var start = request.StartDate?.Date;
            var end = request.EndDate?.Date.AddDays(1).AddTicks(-1);

            if (start.HasValue && end.HasValue)
                usersList = usersList.Where(x => x.user.CreatedAt >= start && x.user.CreatedAt <= end);
            else if (start.HasValue)
                usersList = usersList.Where(x => x.user.CreatedAt >= start);
            else if (end.HasValue)
                usersList = usersList.Where(x => x.user.CreatedAt <= end);
        }

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            bool isAscending = request.SortDirection?.Trim().ToLower() == "asc";

            usersList = request.SortBy.ToLower() switch
            {
                "createdat" =>
                    isAscending ? usersList.OrderBy(x => x.user.CreatedAt) : usersList.OrderByDescending(x => x.user.CreatedAt),

                "lastactivity" =>
                    isAscending ? usersList.OrderBy(x => x.user.LastActivity) : usersList.OrderByDescending(x => x.user.LastActivity),

                "tenantname" =>
                    isAscending ? usersList.OrderBy(x => x.company.CompanyName) : usersList.OrderByDescending(x => x.company.CompanyName),

                "ispartner" =>
                    isAscending ? usersList.OrderBy(x => x.company.IsPartner) : usersList.OrderByDescending(x => x.company.IsPartner),

                "partnertype" =>
                    isAscending ? usersList.OrderBy(x => x.company.PartnerType) : usersList.OrderByDescending(x => x.company.PartnerType),

                "issupported" =>
                    isAscending ? usersList.OrderBy(x => x.setting.IsSupportReq) : usersList.OrderByDescending(x => x.setting.IsSupportReq),

                _ => usersList.OrderByDescending(x => x.user.CreatedAt)
            };
        }
        else
        {
            usersList = usersList.OrderByDescending(x => x.user.CreatedAt);
        }

        var query = usersList.Select(x => new ExportAllUsersEventResponse
        {
            TenantId = x.user.TenantId,
            CreatedAt = x.user.CreatedAt,
            Email = x.user.Email,
            LastActivity = x.user.LastActivity,

            TenantName = x.company.CompanyName,

            Industry = x.company.PrimaryBusinessSector,
            IsPartner = x.company.IsPartner,
            PartnerType = x.company.PartnerType,

            IsSupported = x.setting.IsSupportReq,

            Country = x.address.CountryName
        });

        return query;
    }

    /// <summary>
    /// Calculates dashboard statistics for tenants, including total count,
    /// current month's new tenants, and percentage increase from previous totals.
    /// </summary>
    /// <returns>A <see cref="GetDashboardCountsEventResponse"/> containing the counts and percentage increase.</returns>
    public async Task<GetDashboardCountsEventResponse> GetUserStatsWithMonthlyGrowthAsync()
    {
        var users = GetAllReadOnly().Where(u => u.UserType != nameof(AccessRole.Staff) && u.EmailConfirmed)
            .Join(_unitOfWork.Context.Companies, x => x.TenantId, company => company.TenantId, (user, company) => new { user, company })
            .Join(_unitOfWork.Context.Settings, x => x.user.TenantId, setting => setting.TenantId, (x, setting) => new { x.user, x.company, setting })
            .Join(_unitOfWork.Context.Addresses.Where(x => x.Type == AddressCategory.Primary.ToString()),
                x => x.user.TenantId, address => address.TenantId, (x, address) => new { x.user, x.company, x.setting, address })
            .Select(x => new { x.user.CreatedAt });

        var totalCounts = await users.CountAsync();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        int currentMonthCounts = await users
                        .Where(x => x.CreatedAt >= monthStart
                                && x.CreatedAt <= now)
                        .CountAsync();

        int previousTotal = totalCounts - currentMonthCounts;

        double percentage = 0;

        if (previousTotal > 0)
            percentage = ((double)currentMonthCounts / previousTotal) * 100;
        else if (currentMonthCounts > 0)
            percentage = 100;

        var response = new GetDashboardCountsEventResponse
        {
            TotalCounts = totalCounts,
            CurrentMonthCounts = currentMonthCounts,
            PercentageIncrease = Math.Round(percentage, 2)
        };

        return response;
    }

    /// <summary>
    /// Retrieves tenant registration counts grouped by the specified period type and duration.
    /// </summary>
    /// <param name="periodType">The type of period to group by (Week or Month).</param>
    /// <param name="duration">The number of periods to include in the result.</param>
    /// <returns>A list of <see cref="PeriodicCountDTO"/> with counts for each period.</returns>
    public async Task<List<GetPeriodicCountsEventResponse>> GetUsersPeriodicStatsAsync(PeriodEventType periodType, int duration)
    {
        var result = new List<GetPeriodicCountsEventResponse>();

        var baseQuery = GetAllReadOnly()
            .Where(u => u.UserType != nameof(AccessRole.Staff) && u.EmailConfirmed)
            .Join(_unitOfWork.Context.Companies, u => u.TenantId, c => c.TenantId, (u, c) => new { u, c })
            .Join(_unitOfWork.Context.Settings, x => x.u.TenantId, s => s.TenantId, (x, s) => new { x.u, x.c, s })
            .Join(_unitOfWork.Context.Addresses.Where(a => a.Type == AddressCategory.Primary.ToString()),
                  x => x.u.TenantId, a => a.TenantId, (x, a) => new { x.u.CreatedAt })
            .Where(x => x.CreatedAt.HasValue);

        DateTime now = DateTime.UtcNow.Date;
        DateTime startDate;

        // Determine date range
        switch (periodType)
        {
            case PeriodEventType.Week:
                startDate = now.AddDays(-7 * duration + 1);
                break;
            case PeriodEventType.Month:
                startDate = now.AddMonths(-duration).AddDays(1);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(periodType));
        }

        // Pull data into memory (SAFE)
        var rawDates = await baseQuery
            .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= now)
            .Select(x => x.CreatedAt.Value)
            .ToListAsync();

        // =========================
        // WEEK (7 DAYS)
        // =========================
        if (periodType == PeriodEventType.Week && duration == 1)
        {
            var grouped = rawDates
                .GroupBy(d => d.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var allDates = Enumerable.Range(0, 7)
                .Select(i => now.AddDays(-i))
                .OrderBy(d => d);

            result = allDates
                .Select(date => new GetPeriodicCountsEventResponse
                {
                    Period = date.ToString("MMM d"),
                    Value = grouped.ContainsKey(date) ? grouped[date] : 0
                })
                .ToList();
        }

        // =========================
        // MONTH (1) → 5-day buckets
        // =========================
        else if (periodType == PeriodEventType.Month && duration == 1)
        {
            const int bucketSize = 5;
            const int totalDays = 30;

            startDate = now.AddDays(-totalDays + 1);

            var grouped = rawDates
                .GroupBy(d => (d.Date - startDate).Days / bucketSize)
                .ToDictionary(g => g.Key, g => g.Count());

            int totalGroups = (int)Math.Ceiling((double)totalDays / bucketSize);

            result = Enumerable.Range(0, totalGroups)
                .Select(i =>
                {
                    var bucketStart = startDate.AddDays(i * bucketSize);
                    var bucketEnd = bucketStart.AddDays(bucketSize - 1);
                    if (bucketEnd > now) bucketEnd = now;

                    return new GetPeriodicCountsEventResponse
                    {
                        Period = bucketEnd.ToString("MMM d"),
                        Value = grouped.ContainsKey(i) ? grouped[i] : 0
                    };
                })
                .ToList();
        }

        // =========================
        // MONTH (3) → 10-day buckets
        // =========================
        else if (periodType == PeriodEventType.Month && duration == 3)
        {
            const int bucketSize = 10;
            const int totalDays = 90;

            startDate = now.AddDays(-totalDays + 1);

            var grouped = rawDates
                .GroupBy(d => (d.Date - startDate).Days / bucketSize)
                .ToDictionary(g => g.Key, g => g.Count());

            int totalBuckets = (int)Math.Ceiling((double)totalDays / bucketSize);

            result = Enumerable.Range(0, totalBuckets)
                .Select(i =>
                {
                    var bucketStart = startDate.AddDays(i * bucketSize);
                    var bucketEnd = bucketStart.AddDays(bucketSize - 1);
                    if (bucketEnd > now) bucketEnd = now;

                    return new GetPeriodicCountsEventResponse
                    {
                        Period = bucketEnd.ToString("MMM d"),
                        Value = grouped.ContainsKey(i) ? grouped[i] : 0
                    };
                })
                .ToList();
        }

        // =========================
        // MONTH (6) → Monthly grouping
        // =========================
        else if (periodType == PeriodEventType.Month && duration == 6)
        {
            var grouped = rawDates
                .GroupBy(d => new { d.Year, d.Month })
                .ToDictionary(g => g.Key, g => g.Count());

            var allMonths = Enumerable.Range(0, duration)
                .Select(i => now.AddMonths(-i))
                .OrderBy(d => d);

            result = allMonths
                .Select(m =>
                {
                    var key = new { m.Year, m.Month };

                    return new GetPeriodicCountsEventResponse
                    {
                        Period = m.ToString("MMM"),
                        Value = grouped.ContainsKey(key) ? grouped[key] : 0
                    };
                })
                .ToList();
        }

        return result;
    }

    /// <summary>
    /// Retrieves the details of a user associated with the specified tenant ID, including 
    /// their personal information, company details, phone, country, and address.
    /// </summary>
    /// <param name="tenantId">The tenant ID of the user whose details are to be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. 
    /// The task result contains a <see cref="GetUserDetailsForEmailDTO"/> object with the user's details, 
    /// or null if no user is found for the given tenant ID.</returns>
    public async Task<UserDetailsForEmailDTO> GetUserDetailsForEmailByTenantIdAsync(string tenantId)
    {
        return await GetAllReadOnly().Where(u => u.TenantId == tenantId)
            .Join(_unitOfWork.Context.Companies, x => x.TenantId, company => company.TenantId, (user, company) => new { user, company })
            .Join(_unitOfWork.Context.Addresses.Where(x => x.Type == AddressCategory.Primary.ToString()),
                x => x.user.TenantId, address => address.TenantId, (x, address) => new { x.user, x.company, address })
        .Select(x => new UserDetailsForEmailDTO
        {
            FullName = $"{x.user.FirstName} {x.user.LastName}",
            Email = x.user.Email,
            Phone = !string.IsNullOrWhiteSpace(x.user.PhoneNumber) ? $"{x.user.CountryCode}.{x.user.PhoneNumber}" : "",
            Company = x.company.CompanyName,
            Country = x.address.CountryName,
            AddressLine = !string.IsNullOrWhiteSpace(x.address.AddressLine) ? x.address.AddressLine : x.address.AddressLine2,
            TownLocality = x.address.TownLocality,
            CityRegion = x.address.CityRegion,
            State = x.address.State,
            PostalZipCode = x.address.PostalZipCode
        }).FirstOrDefaultAsync();
    }
    /// <summary>
    /// UpdateLastActivityByTenantIdAsync
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public Task<bool> UpdateLastActivityByTenantIdAsync(string tenantId)
    {
        return UpdateLastActivityAsync(u => u.TenantId == tenantId);
    }
    /// <summary>
    /// UpdateLastActivityByIdAsync
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<bool> UpdateLastActivityByIdAsync(Guid id)
    {
        return UpdateLastActivityAsync(u => u.Id == id);
    }
    /// <summary>
    /// UpdateLastActivityByEmailAsync added
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public Task<bool> UpdateLastActivityByEmailAsync(string email)
    {
        return UpdateLastActivityAsync(u => u.Email == email);
    }
    /// <summary>
    /// UpdateLastActivityAsync
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>

    private async Task<bool> UpdateLastActivityAsync(Func<User, bool> predicate)
    {
        var user = GetAllReadOnly().FirstOrDefault(predicate);
        if (user == null) return false;

        user.LastActivity = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await Update(user);
        await _unitOfWork.CommitAsync();
        return true;
    }
}
