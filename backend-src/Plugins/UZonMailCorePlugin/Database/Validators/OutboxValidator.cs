﻿using FluentValidation;
using UZonMail.DB.SQL.Emails;

namespace UZonMail.Core.Database.Validators
{
    /// <summary>
    /// Outbox 验证器
    /// </summary>
    public class OutboxValidator : AbstractValidator<Outbox>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OutboxValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("请输入正确的 Email");
            RuleFor(x => x.Password).NotEmpty().WithMessage("请输入 Password");
            RuleFor(x => x.SmtpHost).NotEmpty().WithMessage("请设置 SmtpHost");
            RuleFor(x => x.SmtpPort).GreaterThan(0).LessThan(65535).WithMessage("SmtpPort 必须在 (0, 65535) 之间");
        }
    }
}
