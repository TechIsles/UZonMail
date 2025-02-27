﻿using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Database.Updater
{
    /// <summary>
    /// 数据库更新器
    /// </summary>
    public interface IDatabaseUpdater : IScopedService<IDatabaseUpdater>
    {
        /// <summary>
        /// 版本号
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// 开始更新数据
        /// </summary>
        /// <returns></returns>
        Task Update();
    }
}
