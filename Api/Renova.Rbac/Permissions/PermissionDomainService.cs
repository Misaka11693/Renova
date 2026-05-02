namespace Renova.Rbac.Permissions;

/// <summary>
/// 权限领域服务，负责权限定义维护。
/// </summary>
public class PermissionDomainService : ITransientDependency
{
    private readonly SqlSugarRepository<SysPermission> _permissionRepository;
    private readonly CurrentUser _currentUser;

    /// <summary>
    /// 初始化权限领域服务。
    /// </summary>
    public PermissionDomainService(
        SqlSugarRepository<SysPermission> permissionRepository,
        CurrentUser currentUser)
    {
        _permissionRepository = permissionRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 分页获取权限列表。
    /// </summary>
    public Task<SqlSugarPagedList<PermissionOutput>> GetPageAsync(PermissionPageInput input)
    {
        var query = _permissionRepository.AsQueryable()
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

        if (!string.IsNullOrWhiteSpace(input.Type))
        {
            query = query.Where(x => x.Type == input.Type);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        return query.ToSqlSugarPagedListAsync(
            input.PageNumber,
            input.PageSize,
            x => new PermissionOutput
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Type = x.Type,
                Path = x.Path,
                HttpMethod = x.HttpMethod,
                Status = x.Status
            });
    }

    /// <summary>
    /// 创建权限。
    /// </summary>
    public async Task<PermissionOutput> CreateAsync(CreatePermissionInput input)
    {
        var exists = await ExistsByCodeAsync(input.Code);
        if (exists)
        {
            throw new UserFriendlyException("权限编码已存在。");
        }

        var currentUser = _currentUser.GetOrNull();

        var entity = new SysPermission
        {
            Code = input.Code,
            Name = input.Name,
            Type = input.Type,
            Path = input.Path,
            HttpMethod = input.HttpMethod,
            ParentId = input.ParentId,
            Status = input.Status,
            Sort = input.Sort,
            CreateTime = DateTime.Now,
            CreateUserId = currentUser?.OperatorUserId ?? 0,
            CreateUserName = currentUser?.OperatorUserName ?? RbacDefaults.SystemOperatorName
        };
        entity.GenerateId();
        await _permissionRepository.InsertAsync(entity);

        return new PermissionOutput
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Type = entity.Type,
            Path = entity.Path,
            HttpMethod = entity.HttpMethod,
            Status = entity.Status
        };
    }

    /// <summary>
    /// 更新权限。
    /// </summary>
    public async Task<PermissionOutput> UpdateAsync(UpdatePermissionInput input)
    {
        var entity = await GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("权限不存在。");
        }

        entity.Name = input.Name;
        entity.Type = input.Type;
        entity.Path = input.Path;
        entity.HttpMethod = input.HttpMethod;
        entity.ParentId = input.ParentId;
        entity.Status = input.Status;
        entity.Sort = input.Sort;
        await _permissionRepository.AsUpdateable(entity).ExecuteCommandAsync();

        return new PermissionOutput
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Type = entity.Type,
            Path = entity.Path,
            HttpMethod = entity.HttpMethod,
            Status = entity.Status
        };
    }

    /// <summary>
    /// 删除权限。
    /// </summary>
    public async Task<string> DeleteAsync(DeletePermissionInput input)
    {
        var entity = await GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("权限不存在。");
        }

        await _permissionRepository.SoftDeleteAsync(x => x.Id == input.Id);
        return "权限删除成功。";
    }

    /// <summary>
    /// 设置权限状态。
    /// </summary>
    public async Task<string> SetStatusAsync(SetPermissionStatusInput input)
    {
        var entity = await GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException("权限不存在。");
        }

        entity.Status = input.Status;
        await _permissionRepository.AsUpdateable(entity).ExecuteCommandAsync();
        return "权限状态设置成功。";
    }

    /// <summary>
    /// 确保种子权限存在。
    /// </summary>
    public async Task<SysPermission> EnsureSeedPermissionAsync(RbacPermissionDefinition definition)
    {
        var entity = await GetByCodeAsync(definition.Code);
        if (entity != null)
        {
            return entity;
        }

        entity = new SysPermission
        {
            Code = definition.Code,
            Name = definition.Name,
            Type = definition.Type,
            Path = definition.Path,
            HttpMethod = definition.HttpMethod,
            Status = (int)CommonStatus.Enabled,
            Sort = definition.Sort,
            CreateTime = DateTime.Now,
            CreateUserId = 0,
            CreateUserName = RbacDefaults.SystemOperatorName
        };
        entity.GenerateId();
        await _permissionRepository.InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// 根据权限 Id 获取权限实体。
    /// </summary>
    public async Task<SysPermission?> GetByIdAsync(long id)
    {
        return await _permissionRepository.AsQueryable().FirstAsync(x => x.Id == id);
    }

    /// <summary>
    /// 根据权限编码获取权限实体。
    /// </summary>
    public async Task<SysPermission?> GetByCodeAsync(string code)
    {
        return await _permissionRepository.AsQueryable().FirstAsync(x => x.Code == code);
    }

    /// <summary>
    /// 判断权限编码是否已存在。
    /// </summary>
    public Task<bool> ExistsByCodeAsync(string code)
    {
        return _permissionRepository.AsQueryable().AnyAsync(x => x.Code == code);
    }
}
