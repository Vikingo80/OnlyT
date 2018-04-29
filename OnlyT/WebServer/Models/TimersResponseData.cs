﻿using System.Linq;
using OnlyT.Services.Options;
using OnlyT.WebServer.ErrorHandling;

namespace OnlyT.WebServer.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using OnlyT.Models;
    using Services.TalkSchedule;
    using Services.Timer;

    internal class TimersResponseData
    {
        [JsonProperty(PropertyName = "timerStatus")]
        public TimerStatus Status { get; }

        [JsonProperty(PropertyName = "timerInfo")]
        public List<TimerInfo> TimerInfo { get; }

        public TimersResponseData(
            ITalkScheduleService talkService, 
            ITalkTimerService timerService,
            IOptionsService optionsService)
        {
            var talks = talkService.GetTalkScheduleItems();
            Status = timerService.GetStatus();

            TimerInfo = new List<TimerInfo>();

            var countUpByDefault = optionsService.Options.CountUp;
            foreach (var talk in talks)
            {
                TimerInfo.Add(CreateTimerInfo(talk, countUpByDefault));
            }
        }

        public TimersResponseData(
            ITalkScheduleService talkService, 
            ITalkTimerService timerService, 
            IOptionsService optionsService,
            int talkId)
        {
            var talks = talkService.GetTalkScheduleItems();
            var talk = talks.SingleOrDefault(x => x.Id == talkId);
            if (talk == null)
            {
                throw new WebServerException(WebServerErrorCode.TimerDoesNotExist);
            }

            Status = timerService.GetStatus();
            TimerInfo = new List<TimerInfo> { CreateTimerInfo(talk, optionsService.Options.CountUp) };
        }

        private TimerInfo CreateTimerInfo(TalkScheduleItem talk, bool countUpByDefault)
        {
            return new TimerInfo
            {
                TalkId = talk.Id,
                TalkTitle = talk.Name,
                OriginalDurationSecs = (int)talk.OriginalDuration.TotalSeconds,
                ModifiedDurationSecs = talk.ModifiedDuration == null
                    ? null
                    : (int?)talk.ModifiedDuration.Value.TotalSeconds,
                AdaptedDurationSecs = talk.AdaptedDuration == null
                    ? null
                    : (int?)talk.AdaptedDuration.Value.TotalSeconds,
                ActualDurationSecs = (int)talk.ActualDuration.TotalSeconds,
                UsesBell = talk.Bell,
                CompletedTimeSecs = talk.CompletedTimeSecs,
                CountUp = talk.CountUp ?? countUpByDefault
            };
        }
    }
}