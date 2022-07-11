using Philips.Demo.Text.SpacedReader;
using System.Linq;

namespace Philips.Demo.Soccer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            const string filePath = "./../../../../soccer.txt";

            TextFileData textFileData = await TextFileData.CreateFromFile(filePath);

            IEnumerable<TeamInfo> teams = GetTeams(textFileData);
            TeamInfo teamInfo = teams.MinBy(t => GetGoalsDifference(t));

            Console.WriteLine(teamInfo.TeamName);
        }

        private static int GetGoalsDifference(TeamInfo teamInfo)
        {
            return Math.Abs(teamInfo.GoalsLost - teamInfo.GoalsScored);
        }

        private static IEnumerable<TeamInfo> GetTeams(TextFileData textFileData)
        {
            int scoredGoalsIndex = FindColumnIndex("F", textFileData);
            int lostGoalsIndex = FindColumnIndex("A", textFileData);
            int teamNameIndex = FindColumnIndex("Team", textFileData);
            foreach (var lineData in textFileData.Lines)
            {
                TeamInfo? teamInfo = GetTeamInfo(teamNameIndex, scoredGoalsIndex, lostGoalsIndex, lineData);
                if (teamInfo.HasValue)
                {
                    yield return teamInfo.Value;
                }
            }
        }

        private static TeamInfo? GetTeamInfo(int teamNameIndex, int scoredGoalsIndex, int lostGoalsIndex, LineData lineData)
        {
            int itemsCount = lineData.LineItems.Count;
            if (teamNameIndex >= itemsCount
                || scoredGoalsIndex >= itemsCount
                || lostGoalsIndex >= itemsCount)
            {
                return null;
            }

            int scoredGoals = GetGoalsValue(lineData.LineItems[scoredGoalsIndex]);
            int lostGoals = GetGoalsValue(lineData.LineItems[lostGoalsIndex]);

            // assumption, that team name is always with ordinal, eg. "15. Everton"
            string teamName = lineData.LineItems[teamNameIndex].Split(' ')[1];

            return new TeamInfo(teamName, scoredGoals, lostGoals);
        }

        private static int GetGoalsValue(string textVal)
        {
            if (textVal.StartsWith("-"))
            {
                textVal = textVal.Substring(1).Trim();
            }

            return int.Parse(textVal);
        }

        private static int FindColumnIndex(string columnText, TextFileData textFileData)
        {
            for (int i = 0; i < textFileData.HeaderItems.Count; i++)
            {
                if (string.Equals(textFileData.HeaderItems[i].ItemText, columnText, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }

            throw new ArgumentException($"Header with column {columnText} was not found");
        }
        
    }
}