using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Windows;
using System.ComponentModel;

namespace KComponents
{
    public static class DebugParam
    {
        public static bool TranslateHandToPenForcely { get; set; }
        //
        public static bool LogRTSyncRegister { get; private set; }
        public static bool LogByodShare { get; private set; }
        public static bool IsGestureTraceEnabled { get; private set; }
        public static bool LogJoinMeeting { get; private set; }
        public static bool TraceMeetingInfoEngine { get; private set; }
        public static bool TracePadDetectionInfo { get; private set; }
        public static bool TraceVoiceCommandInfo { get; private set; }

        static DebugParam()
        {
        }

        public static void Init()
        {
            LogRTSyncRegister = true;// bool.Parse(section["LogRTSyncRegister"]);
            LogJoinMeeting = true;// bool.Parse(section["LogJoinMeeting"]);
            LogByodShare = true;// bool.Parse(section["LogByodShare"]);
            IsGestureTraceEnabled = true;
            TraceMeetingInfoEngine = true;
            TracePadDetectionInfo = true;
            TraceVoiceCommandInfo = true;
        }
    }
}
