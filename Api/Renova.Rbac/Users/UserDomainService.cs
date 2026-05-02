namespace Renova.Rbac.Users;

/// <summary>
/// 用户领域服务，负责用户管理与角色分配。
/// </summary>
public class UserDomainService : ITransientDependency
{
    private readonly SqlSugarRepository<SysUser> _userRepository;
    private readonly SqlSugarRepository<SysRole> _roleRepository;
    private readonly SqlSugarRepository<SysUserRole> _userRoleRepository;
    private readonly SqlSugarRepository<SysRolePermission> _rolePermissionRepository;
    private readonly SqlSugarRepository<SysPermission> _permissionRepository;
    private readonly CurrentUser _currentUser;

    /// <summary>
    /// 初始化用户领域服务。
    /// </summary>
    public UserDomainService(
        SqlSugarRepository<SysUser> userRepository,
        SqlSugarRepository<SysRole> roleRepository,
        SqlSugarRepository<SysUserRole> userRoleRepository,
        SqlSugarRepository<SysRolePermission> rolePermissionRepository,
        SqlSugarRepository<SysPermission> permissionRepository,
        CurrentUser currentUser)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _permissionRepository = permissionRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 分页获取用户列表。
    /// </summary>
    public async Task<SqlSugarPagedList<UserOutput>> GetPageAsync(UserPageInput input)
    {
        var query = _userRepository.AsQueryable()
            .OrderByDescending(x => x.CreateTime);

        if (!string.IsNullOrWhiteSpace(input.Account))
        {
            query = query.Where(x => x.Account.Contains(input.Account));
        }

        if (!string.IsNullOrWhiteSpace(input.NickName))
        {
            query = query.Where(x => x.NickName != null && x.NickName.Contains(input.NickName));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        var page = await query.ToSqlSugarPagedListAsync(input.PageNumber, input.PageSize);
        var outputs = await BuildUserOutputsAsync(page.Items.ToList());

        return new SqlSugarPagedList<UserOutput>
        {
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items = outputs
        };
    }

    /// <summary>
    /// 创建用户。
    /// </summary>
    public async Task<UserOutput> CreateAsync(CreateUserInput input)
    {
        var exists = await ExistsByAccountAsync(input.Account);
        if (exists)
        {
            throw new UserFriendlyException("账号已存在。");
        }

        var currentUser = _currentUser.GetOrNull();

        var entity = new SysUser
        {
            Account = input.Account,
            Password = PasswordHasher.Hash(input.Password),
            NickName = input.NickName,
            Status = input.Status,
            CreateTime = DateTime.Now,
            CreateUserId = currentUser?.OperatorUserId ?? 0,
            CreateUserName = currentUser?.OperatorUserName ?? RbacDefaults.SystemOperatorName
        };
        entity.GenerateId();
        await _userRepository.InsertAsync(entity);

        return new UserOutput
        {
            Id = entity.Id,
            Account = entity.Account,
            NickName = entity.NickName,
            Status = entity.Status
        };
    }

    /// <summary>
    /// 更新用户。
    /// </summary>
    public async Task<UserOutput> UpdateAsync(UpdateUserInput input)
    {
        var entity = await GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("用户不存在。");
        }

        entity.NickName = input.NickName;
        entity.Status = input.Status;
        await _userRepository.AsUpdateable(entity).ExecuteCommandAsync();

        return new UserOutput
        {
            Id = entity.Id,
            Account = entity.Account,
            NickName = entity.NickName,
            Status = entity.Status
        };
    }

    /// <summary>
    /// 删除用户。
    /// </summary>
    public async Task<string> DeleteAsync(DeleteUserInput input)
    {
        var entity = await GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("用户不存在。");
        }

        await _userRepository.SoftDeleteAsync(x => x.Id == input.Id);
        return "用户删除成功。";
    }

    /// <summary>
    /// 设置用户状态。
    /// </summary>
    public async Task<string> SetStatusAsync(SetUserStatusInput input)
    {
        var entity = await GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("用户不存在。");
        }

        entity.Status = input.Status;
        await _userRepository.AsUpdateable(entity).ExecuteCommandAsync();
        return "用户状态设置成功。";
    }

    /// <summary>
    /// 重新分配用户角色。
    /// </summary>
    public async Task<string> AssignRolesAsync(AssignUserRolesInput input)
    {
        var user = await GetByIdAsync(input.UserId);
        if (user == null)
        {
            throw new UserFriendlyException("用户不存在。");
        }

        var roleIds = input.RoleIds.Distinct().ToList();
        var roles = await GetRolesByIdsAsync(roleIds);

        if (roles.Count != roleIds.Count)
        {
            throw new UserFriendlyException("存在无效的角色。");
        }

        await _userRoleRepository.Context.Deleteable<SysUserRole>()
            .Where(x => x.UserId == input.UserId)
            .ExecuteCommandAsync();

        if (roleIds.Count == 0)
        {
            return "角色分配成功。";
        }

        var entities = roleIds.Select(roleId =>
        {
            var entity = new SysUserRole
            {
                UserId = input.UserId,
                RoleId = roleId
            };
            entity.GenerateId();
            return entity;
        }).ToList();

        await _userRoleRepository.Context.Insertable(entities).ExecuteCommandAsync();
        return "角色分配成功。";
    }

    /// <summary>
    /// 确保种子用户存在。
    /// </summary>
    public async Task<SysUser> EnsureSeedUserAsync(string account, string password, string nickName)
    {
        var entity = await GetByAccountAsync(account);
        if (entity != null)
        {
            return entity;
        }

        entity = new SysUser
        {
            Account = account,
            Password = PasswordHasher.Hash(password),
            NickName = nickName,
            Status = (int)CommonStatus.Enabled,
            CreateTime = DateTime.Now,
            CreateUserId = 0,
            CreateUserName = RbacDefaults.SystemOperatorName
        };
        entity.GenerateId();
        await _userRepository.InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// 确保用户角色关系存在。
    /// </summary>
    public async Task EnsureUserRoleAsync(long userId, long roleId)
    {
        var exists = await UserRoleExistsAsync(userId, roleId);
        if (exists)
        {
            return;
        }

        var entity = new SysUserRole
        {
            UserId = userId,
            RoleId = roleId
        };
        entity.GenerateId();
        await _userRoleRepository.InsertAsync(entity);
    }

    /// <summary>
    /// 根据用户 Id 获取用户实体。
    /// </summary>
    public async Task<SysUser?> GetByIdAsync(long id)
    {
        return await _userRepository.AsQueryable().FirstAsync(x => x.Id == id);
    }

    /// <summary>
    /// 根据账号获取用户实体。
    /// </summary>
    public async Task<SysUser?> GetByAccountAsync(string account)
    {
        return await _userRepository.AsQueryable().FirstAsync(x => x.Account == account);
    }

    /// <summary>
    /// 判断账号是否已存在。
    /// </summary>
    public Task<bool> ExistsByAccountAsync(string account)
    {
        return _userRepository.AsQueryable().AnyAsync(x => x.Account == account);
    }

    /// <summary>
    /// 获取用户角色编码列表。
    /// </summary>
    public async Task<List<string>> GetRoleCodesAsync(long userId)
    {
        var roleIds = await _userRoleRepository.AsQueryable()
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .ToListAsync();

        if (roleIds.Count == 0)
        {
            return [];
        }

        return await _roleRepository.AsQueryable()
            .Where(x => roleIds.Contains(x.Id) && x.Status == (int)CommonStatus.Enabled)
            .Select(x => x.Code)
            .ToListAsync();
    }

    /// <summary>
    /// 获取用户权限编码列表。
    /// </summary>
    public async Task<List<string>> GetPermissionCodesAsync(long userId)
    {
        var roleIds = await _userRoleRepository.AsQueryable()
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .ToListAsync();

        if (roleIds.Count == 0)
        {
            return [];
        }

        var permissionIds = await _rolePermissionRepository.AsQueryable()
            .Where(x => roleIds.Contains(x.RoleId))
            .Select(x => x.PermissionId)
            .Distinct()
            .ToListAsync();

        if (permissionIds.Count == 0)
        {
            return [];
        }

        return await _permissionRepository.AsQueryable()
            .Where(x => permissionIds.Contains(x.Id) && x.Status == (int)CommonStatus.Enabled)
            .Select(x => x.Code)
            .ToListAsync();
    }

    /// <summary>
    /// 组装用户输出结果。
    /// </summary>
    private async Task<List<UserOutput>> BuildUserOutputsAsync(List<SysUser> users)
    {
        if (users.Count == 0)
        {
            return [];
        }

        var userIds = users.Select(x => x.Id).ToList();
        var userRoles = await _userRoleRepository.AsQueryable()
            .Where(x => userIds.Contains(x.UserId))
            .ToListAsync();

        var roleIds = userRoles.Select(x => x.RoleId).Distinct().ToList();
        var roles = await GetRolesByIdsAsync(roleIds);

        var roleCodeMap = roles.ToDictionary(x => x.Id, x => x.Code);
        var userRoleMap = userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(item => item.RoleId)
                    .Where(roleCodeMap.ContainsKey)
                    .Select(roleId => roleCodeMap[roleId])
                    .Distinct()
                    .ToList());

        return users.Select(user => new UserOutput
        {
            Id = user.Id,
            Account = user.Account,
            NickName = user.NickName,
            Status = user.Status,
            Roles = userRoleMap.TryGetValue(user.Id, out var codes) ? codes : []
        }).ToList();
    }

    /// <summary>
    /// 根据角色 Id 集合获取角色列表。
    /// </summary>
    private Task<List<SysRole>> GetRolesByIdsAsync(List<long> roleIds)
    {
        return roleIds.Count == 0
            ? Task.FromResult(new List<SysRole>())
            : _roleRepository.AsQueryable().Where(x => roleIds.Contains(x.Id)).ToListAsync();
    }

    /// <summary>
    /// 判断用户角色关系是否存在。
    /// </summary>
    private Task<bool> UserRoleExistsAsync(long userId, long roleId)
    {
        return _userRoleRepository.AsQueryable().AnyAsync(x => x.UserId == userId && x.RoleId == roleId);
    }
}
