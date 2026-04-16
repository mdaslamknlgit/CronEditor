namespace CronEditor
{

    public class CronBuilderService
    {
        private readonly Dictionary<string, (int min, int max, string description)> _fieldDefinitions = new()
    {
        { "minute", (0, 59, "Minute") },
        { "hour", (0, 23, "Hour") },
        { "day", (1, 31, "Day of Month") },
        { "month", (1, 12, "Month") },
        { "dayOfWeek", (0, 6, "Day of Week") },
        { "year", (1970, 2099, "Year") }
    };

        private readonly Dictionary<int, string> _monthNames = new()
    {
        { 1, "January" }, { 2, "February" }, { 3, "March" }, { 4, "April" },
        { 5, "May" }, { 6, "June" }, { 7, "July" }, { 8, "August" },
        { 9, "September" }, { 10, "October" }, { 11, "November" }, { 12, "December" }
    };

        private readonly Dictionary<int, string> _dayNames = new()
    {
        { 0, "Sunday" }, { 1, "Monday" }, { 2, "Tuesday" }, { 3, "Wednesday" },
        { 4, "Thursday" }, { 5, "Friday" }, { 6, "Saturday" }
    };

        public string BuildCronExpression(string minute, string hour, string day, string month, string dayOfWeek, string year)
        {
            return $"{minute} {hour} {day} {month} {dayOfWeek} {year}".Trim();
        }

        public bool ValidateCronExpression(string expression)
        {
            try
            {
                var parts = expression.Split(' ');
                if (parts.Length != 6)
                    return false;

                return ValidateField(parts[0], "minute") &&
                       ValidateField(parts[1], "hour") &&
                       ValidateField(parts[2], "day") &&
                       ValidateField(parts[3], "month") &&
                       ValidateField(parts[4], "dayOfWeek") &&
                       ValidateField(parts[5], "year");
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateField(string field, string fieldType)
        {
            if (!_fieldDefinitions.TryGetValue(fieldType, out var def))
                return false;

            if (field == "*")
                return true;

            if (field.Contains("/"))
            {
                var parts = field.Split('/');
                if (parts.Length != 2)
                    return false;
                return ValidateField(parts[0], fieldType) && int.TryParse(parts[1], out var step) && step > 0;
            }

            if (field.Contains("-"))
            {
                var parts = field.Split('-');
                if (parts.Length != 2)
                    return false;
                return int.TryParse(parts[0], out var start) && int.TryParse(parts[1], out var end) &&
                       start >= def.min && end <= def.max && start <= end;
            }

            if (field.Contains(","))
            {
                return field.Split(',').All(v => ValidateField(v, fieldType));
            }

            return int.TryParse(field, out var value) && value >= def.min && value <= def.max;
        }

        public string GetCronDescription(string expression)
        {
            if (!ValidateCronExpression(expression))
                return "Invalid cron expression";

            var parts = expression.Split(' ');
            var descriptions = new List<string>();

            descriptions.Add($"Minute: {GetFieldDescription(parts[0], "minute")}");
            descriptions.Add($"Hour: {GetFieldDescription(parts[1], "hour")}");
            descriptions.Add($"Day: {GetFieldDescription(parts[2], "day")}");
            descriptions.Add($"Month: {GetFieldDescription(parts[3], "month")}");
            descriptions.Add($"Day of Week: {GetFieldDescription(parts[4], "dayOfWeek")}");
            descriptions.Add($"Year: {GetFieldDescription(parts[5], "year")}");

            return string.Join(" | ", descriptions);
        }

        private string GetFieldDescription(string field, string fieldType)
        {
            if (field == "*")
                return "Every";

            if (field.Contains("/"))
            {
                var parts = field.Split('/');
                var step = parts[1];
                var baseDesc = parts[0] == "*" ? "Every" : parts[0];
                return $"{baseDesc} / {step}";
            }

            if (fieldType == "month" && int.TryParse(field, out var monthNum))
                return _monthNames.TryGetValue(monthNum, out var monthName) ? monthName : field;

            if (fieldType == "dayOfWeek" && int.TryParse(field, out var dayNum))
                return _dayNames.TryGetValue(dayNum, out var dayName) ? dayName : field;

            return field;
        }

        public List<(string value, string label)> GetMinuteOptions()
        {
            var options = new List<(string, string)> { ("*", "Every Minute") };
            for (int i = 0; i < 60; i++)
                options.Add((i.ToString(), $"{i:D2}"));
            return options;
        }

        public List<(string value, string label)> GetHourOptions()
        {
            var options = new List<(string, string)> { ("*", "Every Hour") };
            for (int i = 0; i < 24; i++)
                options.Add((i.ToString(), $"{i:D2}:00"));
            return options;
        }

        public List<(string value, string label)> GetDayOptions()
        {
            var options = new List<(string, string)> { ("*", "Every Day") };
            for (int i = 1; i <= 31; i++)
                options.Add((i.ToString(), $"Day {i}"));
            return options;
        }

        public List<(string value, string label)> GetMonthOptions()
        {
            var options = new List<(string, string)> { ("*", "Every Month") };
            for (int i = 1; i <= 12; i++)
                options.Add((i.ToString(), _monthNames[i]));
            return options;
        }

        public List<(string value, string label)> GetDayOfWeekOptions()
        {
            return new List<(string, string)>
        {
            ("*", "Every Day"),
            ("0", "Sunday"),
            ("1", "Monday"),
            ("2", "Tuesday"),
            ("3", "Wednesday"),
            ("4", "Thursday"),
            ("5", "Friday"),
            ("6", "Saturday")
        };
        }

        public List<(string value, string label)> GetYearOptions()
        {
            var options = new List<(string, string)> { ("*", "Every Year") };
            var currentYear = DateTime.Now.Year;
            for (int i = currentYear; i <= currentYear + 10; i++)
                options.Add((i.ToString(), i.ToString()));
            return options;
        }

        public Dictionary<string, string> GetPresets()
        {
            return new Dictionary<string, string>
        {
            { "Every Minute", "* * * * * *" },
            { "Every 5 Minutes", "*/5 * * * * *" },
            { "Every Hour", "0 * * * * *" },
            { "Every Day at Midnight", "0 0 * * * *" },
            { "Every Day at Noon", "0 12 * * * *" },
            { "Every Monday", "0 0 * * 1 *" },
            { "Every 1st of Month", "0 0 1 * * *" },
            { "Every Quarter", "0 0 1 1,4,7,10 * *" },
            { "Every Year on Jan 1st", "0 0 1 1 * *" }
        };
        }
    }
}
