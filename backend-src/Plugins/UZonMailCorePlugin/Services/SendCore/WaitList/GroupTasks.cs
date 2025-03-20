﻿using log4net;
using System.Collections.Concurrent;
using UZonMail.Core.Services.SendCore.Contexts;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.Core.Services.EmailSending.WaitList;

namespace UZonMail.Core.Services.SendCore.WaitList
{
    /// <summary>
    /// 单个用户的发件任务管理
    /// 先添加先发送
    /// </summary>
    /// <remarks>
    /// 构造函数
    /// </remarks>
    /// <param name="userId"></param>
    public class GroupTasks(long userId) : ConcurrentDictionary<long, GroupTask>
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GroupTasks));

        /// <summary>
        /// 用户 id
        /// </summary>
        public long UserId { get; set; } = userId;

        /// <summary>
        /// 添加发件组任务
        /// 若包含 sendingItemIds，则只发送这部分邮件
        /// </summary>
        /// <param name="scopeServices"></param>
        /// <param name="sendingGroupId">传入时请保证组一定存在</param>
        /// <param name="smtpPasswordSecretKeys">smtp密码密钥</param>
        /// <param name="sendingItemIds">待发送的 Id</param>
        /// <returns></returns>
        public async Task<bool> AddSendingGroup(SendingContext scopeServices, long sendingGroupId, List<string> smtpPasswordSecretKeys, List<long>? sendingItemIds = null)
        {
            // 有可能发件组已经存在
            if (!TryGetValue(sendingGroupId, out var existTask))
            {
                // 重新初始化
                // 添加到列表
                var newTask = await GroupTask.Create(scopeServices, sendingGroupId, smtpPasswordSecretKeys);
                if (newTask == null) return false;

                var success = await newTask.InitSendingItems(scopeServices, sendingItemIds);
                if (!success) return false;
                return TryAdd(sendingGroupId, newTask);
            }
            else
            {
                // 复用原来的数据
                return await existTask.InitSendingItems(scopeServices, sendingItemIds);
            }
        }

        /// <summary>
        /// 获取组中可被 outboxId 发送的邮件项
        /// </summary>
        /// <returns></returns>
        public async Task<SendItemMeta?> GetEmailItem(SendingContext context)
        {
            // 依次获取发件项
            foreach (var kv in this)
            {
                var groupTask = kv.Value;
                var result = await groupTask.GetEmailItem(context);
                if (result != null) return result;
            }

            return null;
        }

        public bool MatchEmailItem(OutboxEmailAddress outbox)
        {
            if (outbox.UserId != UserId) return false;

            // 依次获取发件项
            foreach (var kv in this)
            {
                var groupTask = kv.Value;
                var match = groupTask.MatchEmailItem(outbox);
                if (match) return true;
            }

            return false;
        }
    }
}
