namespace Philips.Demo.Text.SpacedReader;

public struct LineData
{
    public LineData(IReadOnlyList<string> lineItems)
    {
        LineItems = lineItems;
    }

    public IReadOnlyList<string> LineItems { get; }
}