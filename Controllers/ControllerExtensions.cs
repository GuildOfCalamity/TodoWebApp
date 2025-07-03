using System.Diagnostics;

namespace TodoWebApp.Controllers
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Logs the features of the current HTTP context.
        /// </summary>
        public static void ForEach(this Microsoft.AspNetCore.Http.Features.IFeatureCollection features)
        {
            foreach (KeyValuePair<Type, object> feature in features)
            {
                Debug.WriteLine($"[DEBUG] FeatureType: {feature.Key},  Value: {feature.Value}");
            }
        }

        /// <summary>
        /// Parses an endpoint string (e.g., "127.0.0.1:63908") and formats it.
        /// </summary>
        /// <param name="endPointString">The endpoint string to parse.</param>
        /// <returns>A formatted string, or "Invalid endpoint/IP/port format" if parsing fails.</returns>
        public static string FormatEndPoint(this string endPointString)
        {
            if (string.IsNullOrWhiteSpace(endPointString))
                return "Invalid endpoint format";
            try
            {
                if (endPointString.StartsWith("::1"))
                    return "localhost";

                string[] parts = endPointString.Split(':');
                if (parts.Length < 2)
                    return "Invalid endpoint format";

                string ipAddressString = parts[0];
                string portString = parts[1];

                if (!System.Net.IPAddress.TryParse(ipAddressString, out _))
                    return "Invalid IP address format";

                if (!int.TryParse(portString, out int port))
                    return "Invalid port format";

                return $"IP {ipAddressString}, port {port}";
            }
            catch (Exception)
            {
                return "Invalid endpoint format";
            }
        }

        /// <summary>
        /// Display a readable sentence as to when the time will happen.
        /// e.g. "in one second" or "in 2 days"
        /// </summary>
        /// <param name="value"><see cref="TimeSpan"/>the future time to compare from now</param>
        /// <returns>human friendly format</returns>
        public static string ToReadableTime(this TimeSpan? value)
        {
            if (value is null)
                return string.Empty;

            double delta = ((TimeSpan)value).TotalSeconds;
            if (delta < 60) { return Math.Abs(((TimeSpan)value).Seconds) == 1 ? "one second" : Math.Abs(((TimeSpan)value).Seconds) + " seconds"; }
            if (delta < 120) { return "a minute"; }
            if (delta < 3000) { return Math.Abs(((TimeSpan)value).Minutes) + " minutes"; } // 50 * 60
            if (delta < 5400) { return "an hour"; } // 90 * 60
            if (delta < 86400) { return Math.Abs(((TimeSpan)value).Hours) + " hours"; } // 24 * 60 * 60
            if (delta < 172800) { return "one day"; } // 48 * 60 * 60
            if (delta < 2592000) { return Math.Abs(((TimeSpan)value).Days) + " days"; } // 30 * 24 * 60 * 60
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)Math.Abs(((TimeSpan)value).Days) / 30));
                return months <= 1 ? "one month" : months + " months";
            }
            int years = Convert.ToInt32(Math.Floor((double)Math.Abs(((TimeSpan)value).Days) / 365));
            return years <= 1 ? "one year" : years + " years";
        }

        public static bool IsInvalid(this double value)
        {
            if (value == double.NaN || value == double.NegativeInfinity || value == double.PositiveInfinity)
                return true;

            return false;
        }
    }
}
