﻿using UZonMail.Core.Database.Startup;
using UZonMail.Core.Database.Updater;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Services.HostedServices
{
    /// <summary>
    /// 程序启动时，开始中断的发件任务
    /// </summary>
    public class CoreHostedService(IServiceProvider serviceProvider) : IHostedServiceStart
    {
        public int Order => 0;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 数据库启动设置
            var dbStartup = serviceProvider.GetRequiredService<DatabaseReset>();
            await dbStartup.Start();

            // 数据升级
            var dataUpdater = serviceProvider.GetRequiredService<DatabaseUpdateService>();
            await dataUpdater.Update();
        }
    }
}
