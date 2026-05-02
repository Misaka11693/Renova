namespace Renova.Rbac.Roles;

/// <summary>
/// 角色领域服务，负责角色管理与权限分配。
/// </summary>
public class RoleDomainService : ITransientDependency
{
    private readonly SqlSugarRepository<SysRole> _roleRepository;
    private readonly SqlSugarRepository<SysPermission> _permissionRepository;
    private readonly SqlSugarRepository<SysRolePermission> _rolePermissionRepository;
    private readonly CurrentUser _currentUser;

    /// <summary>
    /// 初始化角色领域服务。
    /// </summary>
    public RoleDomainService(
        SqlSugarRepository<SysRole> roleRepository,
        SqlSugarRepository<SysPermission> permissionRepository,
        SqlSugarRepository<SysRolePermission> rolePermissionRepository,
        CurrentUser currentUser)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 分页获取角色列表。
    /// </summary>
    public async Task<SqlSugarPagedList<RoleOutput>> GetPageAsync(RolePageInput input)
    {
        var query = _roleRepository.AsQueryable()
            .OrderBy(x => x.Sort)
            .OrderBy(x => x.CreateTime);

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => x.Code.Contains(input.Code));
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            query = query.Where(x => x.Name.Contains(input.Name));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        var page = await query.ToSqlSugarPagedListAsync(input.PageNumber, input.PageSize);
        var outputs = await BuildRoleOutputsAsync(page.Items.ToList());

        return new SqlSugarPagedList<RoleOutput>
        {
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items = outputs
        };
    }

    /// <summary>
    /// 创建角色。
    /// </summary>
    public async Task<RoleOutput> CreateAsync(CreateRoleInput input)
    {
        var exists = await ExistsByCodeAsync(input.Code);
        if (exists)
        {
            throw new UserFriendlyException("角色编码已存在。");
        }

        var currentUser = _currentUser.GetOrNull();

        var entity = new SysRole
        {
            Code = input.Code,
            Name = input.Name,
            Status = input.Status,
            Sort = input.Sort,
            Remark = input.Remark,
            CreateTime = DateTime.Now,
            CreateUserId = currentUser?.OperatorUserId ?? 0,
            CreateUserName = currentUser?.OperatorUserName ?? RbacDefaults.SystemOperatorName
        };
        entity.GenerateId();
        await _roleRepository.InsertAsync(entity);

        return new RoleOutput
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Status = entity.Status
        };
    }

    /// <summary>
    /// 更新角色。
    /// </summary>
    public async Task<RoleOutput> UpdateAsync(UpdateRoleInput input)
    {
        var entity = await GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("角色不存在。");
        }

        entity.Name = input.Name;
        entity.Status = input.Status;
        entity.Sort = input.Sort;
        entity.Remark = input.Remark;
        await _roleRepository.AsUpdateable(entity).ExecuteCommandAsync();

        return new RoleOutput
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Status = entity.Status
        };
    }

    /// <summary>
    /// 删除角色。
    /// </summary>
    public async Task<string> DeleteAsync(DeleteRoleInput input)
    {
        var entity = await GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("角色不存在。");
        }

        await _roleRepository.SoftDeleteAsync(x => x.Id == input.Id);
        return "角色删除成功。";
    }

    /// <summary>
    /// 设置角色状态。
    /// </summary>
    public async Task<string> SetStatusAsync(SetRoleStatusInput input)
    {
        var entity = await GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("角色不存在。");
        }

        entity.Status = input.Status;
        await _roleRepository.AsUpdateable(entity).ExecuteCommandAsync();
        return "角色状态设置成功。";
    }

    /// <summary>
    /// 重新分配角色权限。
    /// </summary>
    public async Task<string> AssignPermissionsAsync(AssignRolePermissionsInput input)
    {
        var role = await GetByIdAsync(input.RoleId);
        if (role == null)
        {
            throw new UserFriendlyException("角色不存在。");
        }

        var permissionIds = input.PermissionIds.Distinct().ToList();
        var permissions = await GetPermissionsByIdsAsync(permissionIds);

        if (permissions.Count != permissionIds.Count)
        {
            throw new UserFriendlyException("存在无效的权限。");
        }

        await _rolePermissionRepository.Context.Deleteable<SysRolePermission>()
            .Where(x => x.RoleId == input.RoleId)
            .ExecuteCommandAsync();

        if (permissionIds.Count == 0)
        {
            return "权限分配成功。";
        }

        var entities = permissionIds.Select(permissionId =>
        {
            var entity = new SysRolePermission
            {
                RoleId = input.RoleId,
                PermissionId = permissionId
            };
            entity.GenerateId();
            return entity;
        }).ToList();

        await _rolePermissionRepository.Context.Insertable(entities).ExecuteCommandAsync();
        return "权限分配成功。";
    }

    /// <summary>
    /// 确保种子角色存在。
    /// </summary>
    public async Task<SysRole> EnsureSeedRoleAsync(string code, string name, int sort, string? remark)
    {
        var entity = await GetByCodeAsync(code);
        if (entity != null)
        {
            return entity;
        }

        entity = new SysRole
        {
            Code = code,
            Name = name,
            Status = (int)CommonStatus.Enabled,
            Sort = sort,
            Remark = remark,
            CreateTime = DateTime.Now,
            CreateUserId = 0,
            CreateUserName = RbacDefaults.SystemOperatorName
        };
        entity.GenerateId();
        await _roleRepository.InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// 确保角色权限关系存在。
    /// </summary>
    public async Task EnsureRolePermissionAsync(long roleId, long permissionId)
    {
        var exists = await RolePermissionExistsAsync(roleId, permissionId);
        if (exists)
        {
            return;
        }

        var entity = new SysRolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
        entity.GenerateId();
        await _rolePermissionRepository.InsertAsync(entity);
    }

    /// <summary>
    /// 根据角色 Id 获取角色实体。
    /// </summary>
    public async Task<SysRole?> GetByIdAsync(long id)
    {
        return await _roleRepository.AsQueryable().FirstAsync(x => x.Id == id);
    }

    /// <summary>
    /// 根据角色编码获取角色实体。
    /// </summary>
    public async Task<SysRole?> GetByCodeAsync(string code)
    {
        return await _roleRepository.AsQueryable().FirstAsync(x => x.Code == code);
    }

    /// <summary>
    /// 判断角色编码是否已存在。
    /// </summary>
    public Task<bool> ExistsByCodeAsync(string code)
    {
        return _roleRepository.AsQueryable().AnyAsync(x => x.Code == code);
    }

    /// <summary>
    /// 组装角色输出结果。
    /// </summary>
    private async Task<List<RoleOutput>> BuildRoleOutputsAsync(List<SysRole> roles)
    {
        if (roles.Count == 0)
        {
            return [];
        }

        var roleIds = roles.Select(x => x.Id).ToList();
        var rolePermissions = await _rolePermissionRepository.AsQueryable()
            .Where(x => roleIds.Contains(x.RoleId))
            .ToListAsync();

        var permissionIds = rolePermissions.Select(x => x.PermissionId).Distinct().ToList();
        var permissions = await GetPermissionsByIdsAsync(permissionIds);

        var permissionCodeMap = permissions.ToDictionary(x => x.Id, x => x.Code);
        var rolePermissionMap = rolePermissions
            .GroupBy(x => x.RoleId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(item => item.PermissionId)
                    .Where(permissionCodeMap.ContainsKey)
                    .Select(permissionId => permissionCodeMap[permissionId])
                    .Distinct()
                    .ToList());

        return roles.Select(role => new RoleOutput
        {
            Id = role.Id,
            Code = role.Code,
            Name = role.Name,
            Status = role.Status,
            Permissions = rolePermissionMap.TryGetValue(role.Id, out var codes) ? codes : []
        }).ToList();
    }

    /// <summary>
    /// 根据权限 Id 集合获取权限列表。
    /// </summary>
    private Task<List<SysPermission>> GetPermissionsByIdsAsync(List<long> permissionIds)
    {
        return permissionIds.Count == 0
            ? Task.FromResult(new List<SysPermission>())
            : _permissionRepository.AsQueryable().Where(x => permissionIds.Contains(x.Id)).ToListAsync();
    }

    /// <summary>
    /// 判断角色权限关系是否存在。
    /// </summary>
    private Task<bool> RolePermissionExistsAsync(long roleId, long permissionId)
    {
        return _rolePermissionRepository.AsQueryable().AnyAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);
    }
}
