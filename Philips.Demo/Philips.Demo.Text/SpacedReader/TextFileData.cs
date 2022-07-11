namespace Philips.Demo.Text.SpacedReader
{
    public class TextFileData
    {
        private readonly List<LineData> _lines;

        private TextFileData(IReadOnlyList<HeaderItem> headerItems)
        {
            HeaderItems = headerItems;
            _lines = new List<LineData>();
        }

        public IReadOnlyList<HeaderItem> HeaderItems { get; }

        public IReadOnlyList<LineData> Lines => _lines;

        public static TextFileData Create(string headerLine)
        {
            List<HeaderItem> headerItems = BuildHeaderItems(headerLine);

            return new TextFileData(headerItems);
        }

        /// <summary>
        /// creates and TextFileData from text file in given path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<TextFileData> CreateFromFile(string filePath)
        {
            using StreamReader streamReader = new(filePath);

            // headerLine
            string headerLine = await streamReader.ReadLineAsync()
                                ?? throw new InvalidOperationException("Header line was null");

            TextFileData data = TextFileData.Create(headerLine);

            while (!streamReader.EndOfStream)
            {
                string? line = await streamReader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                data.AddLine(line);
            }

            return data;
        }

        /// <summary>
        /// Adds line and parses its data according to parsed headers
        /// </summary>
        /// <param name="sourceTextLine"></param>
        public void AddLine(string sourceTextLine)
        {
            List<string> lineItems = new List<string>();

            int startInclusive = -1;

            if (HeaderItems.Count > 0)
            {
                startInclusive = HeaderItems[0].StartIndexInclusive;
            }

            foreach (var headerItem in HeaderItems)
            {
                if (startInclusive >= sourceTextLine.Length)
                {
                    break;
                }

                int endCheck = Math.Min(headerItem.EndIndexInclusive, sourceTextLine.Length - 1);

                bool allWhitespace = Enumerable
                    .Range(startInclusive, endCheck - startInclusive + 1)
                    .All(idx => char.IsWhiteSpace(sourceTextLine[idx]));

                if (allWhitespace)
                {
                    lineItems.Add(string.Empty);
                    continue;
                }

                // sometimes value ends after current column end, find first space to find real end of current value
                while (endCheck < sourceTextLine.Length - 1)
                {
                    if (char.IsWhiteSpace(sourceTextLine[endCheck])
                        || char.IsWhiteSpace(sourceTextLine[endCheck + 1]))
                    {
                        break;
                    }

                    endCheck++;
                }

                string value = sourceTextLine
                    .Substring(startInclusive, endCheck - startInclusive + 1)
                    .Trim();

                lineItems.Add(value);

                startInclusive = endCheck + 1;
            }

            _lines.Add(new LineData(lineItems));
        }

        private static List<HeaderItem> BuildHeaderItems(string headerLine)
        {
            List<HeaderItem> result = new List<HeaderItem>();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return result;
            }

            int currentStart = 0;

            bool readingItem = !char.IsWhiteSpace(headerLine[0]);

            for (int i = 0; i < headerLine.Length; i++)
            {
                char currentChar = headerLine[i];
                if (char.IsWhiteSpace(currentChar))
                {
                    if (!readingItem)
                    {
                        continue;
                    }

                    string text = headerLine.Substring(currentStart, i - currentStart).Trim();
                    HeaderItem headerItem = new HeaderItem(currentStart, i - 1, text);
                    result.Add(headerItem);

                    currentStart = i;
                    readingItem = false;
                }
                else
                {
                    if (!readingItem)
                    {
                        readingItem = true;
                    }
                }
            }

            if (readingItem)
            {
                string text = headerLine.Substring(currentStart).Trim();
                result.Add(new HeaderItem(currentStart, headerLine.Length - 1, text));
            }

            return result;
        }
    }
}
