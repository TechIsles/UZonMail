﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Web.Service;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Organization;
using UZonMail.Core.SignalRHubs;
using UZonMail.Core.SignalRHubs.Extensions;
using UZonMail.Core.Services.Cache;
using UZonMail.DB.SQL.Permission;

namespace UZonMail.Core.Services.Permission
{
    /// <summary>
    /// 权限服务
    /// </summary>
    public class PermissionService(SqlContext db, CacheService cache, IHubContext<UzonMailHub, IUzonMailClient> hub) : IScopedService
    {
        /// <summary>
        /// 生成权限缓存的 key
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetPermissionCacheKey(long userId) => $"permissions/{userId}";

        /// <summary>
        /// 更新用户的权限缓存
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns>返回权限码</returns>
        public async Task<Dictionary<long, List<string>>> UpdateUserPermissionsCache(List<long> userIds)
        {
            if (userIds.Count == 0) return [];

            var userRoles = await db.UserRole.AsNoTracking()
                .Where(x => userIds.Contains(x.UserId))
                .Include(x => x.Roles)
                .ThenInclude(x => x.PermissionCodes)
                .GroupBy(x => x.UserId)
                .ToListAsync();

            Dictionary<long, List<string>> results = [];
            foreach (var item in userRoles)
            {
                var permissionCodes = item.SelectMany(x => x.Roles).SelectMany(x => x.PermissionCodes).Select(x => x.Code).Distinct().ToList();
                results.Add(item.Key, permissionCodes);
                // 更新缓存
                await cache.SetAsync(GetPermissionCacheKey(item.Key), permissionCodes);
            }
            return results;
        }

        /// <summary>
        /// 更新单个用户的权限缓存
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<string>> UpdateUserPermissionsCache(long userId)
        {
            var results = await UpdateUserPermissionsCache([userId]);
            if (results.TryGetValue(userId, out var value)) return value;
            return [];
        }

        /// <summary>
        /// 获取用户权限码
        /// 会先从缓存中获取，如果缓存中没有则更新缓存
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetUserPermissionCodes(long userId)
        {
            var cacheValues = await cache.GetAsync<List<string>>(GetPermissionCacheKey(userId));
            if (cacheValues != null) return cacheValues;

            // 更新缓存
            cacheValues ??= await UpdateUserPermissionsCache(userId);

            // 添加管理员权限码
            var user = await db.Users.AsNoTracking().FirstAsync(x => x.Id == userId);
            if (user.IsSuperAdmin)
                cacheValues.AddRange(["admin", "*"]);

            // 如果是子账户，添加子账户权限码
            if (user.Type == UserType.SubUser)
                cacheValues.Add("subUser");

            return cacheValues;
        }

        /// <summary>
        /// 判断用户是否有权限
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissionCode"></param>
        /// <returns></returns>
        public async Task<bool> HasPermission(long userId, string permissionCode)
        {
            var permissionCodes = await GetUserPermissionCodes(userId);
            // * 代表所有权限
            if (permissionCode.Contains("*")) return true;

            return permissionCodes.Contains(permissionCode);
        }

        public async Task<bool> HasOrganizationPermission(long userId)
        {
            return await HasPermission(userId, PermissionCode.OrganizationPermissionCode);
        }

        /// <summary>
        /// 向客户端通知用户权限更新
        /// </summary>
        /// <param name="userPermissionCodes"></param>
        /// <returns></returns>
        public async Task NotifyPermissionUpdate(Dictionary<long, List<string>> userPermissionCodes)
        {
            if (userPermissionCodes.Count == 0) return;

            foreach (var item in userPermissionCodes)
            {
                await hub.GetUserClient(item.Key).PermissionUpdated(item.Value);
            }
        }
    }
}
