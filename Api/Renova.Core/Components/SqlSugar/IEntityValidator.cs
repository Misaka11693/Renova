using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renova.Core.Components.SqlSugar;

/// <summary>
/// IEntityValidator
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEntityValidator<T> where T : class
{
    /// <summary>
    /// 校验实体
    /// </summary>
    /// <param name="entity">实体对象</param>
    /// <param name="db">数据库上下文</param>
    /// <param name="operationType">操作类型：InsertByObject 或 UpdateByObject</param>
    void Validate(T entity, SqlSugarClient db, DataFilterType operationType);
}


//public class UserValidator : IEntityValidator<User>
//{
//    public void Validate(User user, SqlSugarClient db, DataFilterType operationType)
//    {
//        // 1. 通用校验（新增和更新都要检查）
//        if (string.IsNullOrWhiteSpace(user.Name))
//            throw new ArgumentException("用户名不能为空");

//        if (user.Age is < 0 or > 150)
//            throw new ArgumentOutOfRangeException(nameof(user.Age), "年龄必须在0到150之间");

//        // 2. 区分新增和更新
//        if (operationType == DataFilterType.InsertByObject)
//        {
//            // ===== 新增专属规则 =====
//            // 新增时，手机号必须填
//            if (string.IsNullOrWhiteSpace(user.Phone))
//                throw new ArgumentException("手机号是必填项");

//            // 新增时，手机号绝对不能重复（查全表）
//            if (db.Queryable<User>().Any(u => u.Phone == user.Phone))
//                throw new InvalidOperationException($"手机号 {user.Phone} 已被占用");

//            // 新增时，密码复杂度要求
//            if (string.IsNullOrEmpty(user.Password) || user.Password.Length < 6)
//                throw new ArgumentException("密码长度不能少于6位");
//        }
//        else if (operationType == DataFilterType.UpdateByObject)
//        {
//            // ===== 更新专属规则 =====
//            // 更新时，如果手机号变了，才检查唯一性（排除自身）
//            // 注意：这里无法直接拿到旧值，需要查库获取原始记录
//            var oldUser = db.Queryable<User>().InSingle(user.Id);
//            if (oldUser != null && oldUser.Phone != user.Phone)
//            {
//                if (db.Queryable<User>().Any(u => u.Phone == user.Phone && u.Id != user.Id))
//                    throw new InvalidOperationException($"手机号 {user.Phone} 已被其他用户占用");
//            }

//            // 更新时，禁止修改创建时间（如果有这个字段）
//            if (oldUser != null && oldUser.CreateTime != user.CreateTime)
//                throw new InvalidOperationException("创建时间不允许修改");
//        }
//    }
//}

//// 需要为每个实体类型显式注册
//services.AddScoped<IEntityValidator<User>, UserValidator>();
//services.AddScoped<IEntityValidator<Order>, OrderValidator>();