using System;
using FluentValidation;

namespace dcinc.api.entities
{
    public class WebMeetingValidator : AbstractValidator<WebMeeting>
    {
        public WebMeetingValidator()
        {
            // Web会議名が未指定の場合はNGとする。
            RuleFor(webMeeting => webMeeting.Name).NotNull().NotEmpty().WithMessage("name is null or empty");
            // Web会議の開始日時が未指定もしくは今日以前の場合はNGとする。
            RuleFor(webMeeting => webMeeting.StartDateTime).NotNull().NotEmpty().GreaterThan(DateTime.Today).WithMessage("startDateTime is invalid. Please specify today's the date and time.");
            // Web会議のURLが未指定の場合はNGとする。
            RuleFor(webMeeting => webMeeting.Url).NotNull().NotEmpty().WithMessage("slackChannelId is null or empty");
            // 登録者が未指定の場合はNGとする。
            RuleFor(webMeeting => webMeeting.RegisteredBy).NotNull().NotEmpty().WithMessage("slackChannelId is null or empty");
            // 通知先のSlackチャンネルが未指定の場合はNGとする。
            RuleFor(webMeeting => webMeeting.SlackChannelId).NotNull().NotEmpty().WithMessage("slackChannelId is null or empty");
        }
    }
}