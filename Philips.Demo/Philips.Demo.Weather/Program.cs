using Philips.Demo.Text.SpacedReader;
using System.Globalization;

namespace Philips.Demo.Weather
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                await Console.Error.WriteLineAsync("Missing argument with weather file location.");
                return;
            }

            TextFileData textFileData = await TextFileData.CreateFromFile(args[0]);

            IEnumerable<DayTemperatureInfo> dayTemperatureInfos = GetTemperatureInfo(textFileData);
            DayTemperatureInfo minSpreadDay = GetDayWithMinTempSpread(dayTemperatureInfos);

            Console.WriteLine(minSpreadDay.DayNumber);
        }

        private static DayTemperatureInfo GetDayWithMinTempSpread(IEnumerable<DayTemperatureInfo> days)
        {
            return days.MinBy(l => l.MaxTemperature - l.MinTemperature);
        }

        private static IEnumerable<DayTemperatureInfo> GetTemperatureInfo(TextFileData textFileData)
        {
            foreach (var lineData in textFileData.Lines)
            {
                DayTemperatureInfo? dayTemperatureInfo = CreateDayTemperatureInfo(lineData);
                if (dayTemperatureInfo.HasValue)
                {
                    yield return dayTemperatureInfo.Value;
                }
            }
        }
        
        private static DayTemperatureInfo? CreateDayTemperatureInfo(LineData lineData)
        {
            const int dayNumberColumn = 0;
            const int maxTemperatureColumn = 1;
            const int minTemperatureColumn = 2;

            if (!int.TryParse(lineData.LineItems[dayNumberColumn], out int dayNumber))
            {
                // it's not day number
                return null;
            }

            double maxTemp = ParseTemperature(lineData.LineItems[maxTemperatureColumn]);
            double minTemp = ParseTemperature(lineData.LineItems[minTemperatureColumn]);

            return new DayTemperatureInfo(dayNumber, maxTemp, minTemp);
        }

        private static double ParseTemperature(string textValue)
        {
            if (textValue.EndsWith("*"))
            {
                textValue = textValue.Substring(0, textValue.Length - 1);
            }

            return double.Parse(textValue, CultureInfo.InvariantCulture);
        }
    }
}