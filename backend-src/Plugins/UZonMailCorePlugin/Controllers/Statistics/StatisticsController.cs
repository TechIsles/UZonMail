﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Controllers.Statistics.Model;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Emails;
using UZonMail.DB.SQL.EmailSending;
using Uamazing.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Statistics
{
    public class StatisticsController(SqlContext db, TokenService tokenService) : ControllerBaseV1
    {
        /// <summary>
        /// 获取发件箱统计信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("outbox")]
        public async Task<ResponseResult<List<EmailCount>>> GetOutboxEmailCountInfo()
        {
            var userId = tokenService.GetUserDataId();
            var emailCounts = await db.Outboxes.OfType<Outbox>()
                .Where(x => x.UserId == userId)
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.Domain)
                .Select(x => new EmailCount
                {
                    Domain = x.Key,
                    Count = x.Count()
                })
                .ToListAsync();
            return emailCounts.ToSuccessResponse();
        }

        /// <summary>
        /// 获取收件箱统计信息
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("inbox")]
        public async Task<ResponseResult<List<EmailCount>>> GetInboxesEmailCountInfo()
        {
            var userId = tokenService.GetUserDataId();
            var emailCounts = await db.Inboxes
                .Where(x => x.UserId == userId)
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.Domain)
                .Select(x => new EmailCount
                {
                    Domain = x.Key,
                    Count = x.Count()
                })
                .ToListAsync();
            return emailCounts.ToSuccessResponse();
        }

        /// <summary>
        /// 每月发送邮件统计
        /// </summary>
        /// <returns></returns>
        [HttpGet("monthly-sending")]
        public async Task<ResponseResult<List<MonthlySendingInfo>>> GetMonthlySendingCountInfo()
        {
            var userId = tokenService.GetUserDataId();
            var monthlySendingInfos = await db.SendingItems.OfType<SendingItem>()
                .Where(x => x.UserId == userId)
                .Where(x => !x.IsDeleted)
                .GroupBy(x => new { x.CreateDate.Year, x.CreateDate.Month })
                .Select(x => new MonthlySendingInfo
                {
                    Year = x.Key.Year,
                    Month = x.Key.Month,
                    Count = x.Count()
                })
                .ToListAsync();

            return monthlySendingInfos.ToSuccessResponse();
        }
    }
}
