namespace Tetrifact.Core
{
    public enum PruneBracketGrouping
    {
        NextBracket,    // Default. Builds to keep spread over time to next (further back in time) bracket
        Hourly,         // Builds spread out overly an hour interval. Ie, for the given bracket, {Amount} of builds will be kept every hour.
        Daily,          // Builds spread out overly an hour interval. Ie, for the given bracket, {Amount} of builds will be kept every day.
        Weekly,         // Builds spread out overly an hour interval. Ie, for the given bracket, {Amount} of builds will be kept every 7 days.
        Monthly,        // Builds spread out overly an hour interval. Ie, for the given bracket, {Amount} of builds will be kept every month.
        Year            // Builds spread out overly an hour interval. Ie, for the given bracket, {Amount} of builds will be kept every year.
    }
}
