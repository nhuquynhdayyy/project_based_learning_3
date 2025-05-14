// TourismWeb/Helpers/EnumExtensions.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using TourismWeb.Models; // Đảm bảo using namespace chứa các Enum của bạn

namespace TourismWeb.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null) return string.Empty;

            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?
                            .GetCustomAttribute<DisplayAttribute>()
                            ?.GetName() ?? enumValue.ToString();
        }
    }

    public static class ReportViewHelper
    {
        public static string GetReportTypeClass(ReportType type)
        {
            return type switch
            {
                ReportType.Spam => "red",
                ReportType.InappropriateContent => "orange",
                ReportType.IncorrectInformation => "blue",
                ReportType.CopyrightInfringement => "purple",
                ReportType.Harassment or ReportType.HateSpeech => "darkred", // Cần C# 9.0+ cho or pattern
                // Nếu dùng C# cũ hơn:
                // case ReportType.Harassment:
                // case ReportType.HateSpeech:
                //    return "darkred";
                _ => "gray",
            };
        }

        public static string GetTargetTypeClass(ReportTargetType type)
        {
            return type switch
            {
                ReportTargetType.Post => "green",
                ReportTargetType.Comment => "blue",
                ReportTargetType.User => "purple",
                ReportTargetType.Spot => "teal",
                _ => "gray",
            };
        }

        public static string GetReportStatusClass(ReportStatus status)
        {
            return status switch
            {
                ReportStatus.Pending => "yellow",
                ReportStatus.Resolved => "green",
                ReportStatus.Dismissed => "gray",
                _ => "gray",
            };
        }
    }
}