﻿using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace UZonMail.Core.SignalRHubs
{
    /// <summary>
    /// 邮件发送进度 Hub
    /// </summary>
    public class UzonMailHub : Hub<IUzonMailClient>
    {
        // 使用一个字典来跟踪用户的连接
        // 有可能同一个用户同时打开多个浏览器窗口
        public static readonly ConcurrentDictionary<string, HashSet<string>> _userConnectionIds = new();

        /// <summary>
        /// 成功连接后
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // 当用户连接时，将他们添加到字典中
            string? userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                if (!_userConnectionIds.TryGetValue(userId, out var connectionIds))
                {
                    connectionIds = [];
                    _userConnectionIds[userId] = connectionIds;
                }
                connectionIds.Add(Context.ConnectionId);
            }

            await base.OnConnectedAsync();

            await Clients.User(userId).Notify(new Notify.NotifyMessage()
            {
                Message = "欢迎使用 UzonMail !",
                Type = Notify.NotifyType.success.ToString()
            });
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // 当用户断开连接时，将他们从字典中移除
            foreach (var item in _userConnectionIds)
            {
                item.Value.Remove(Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
