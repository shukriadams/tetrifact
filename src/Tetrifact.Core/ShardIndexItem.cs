using System;
using System.Text.RegularExpressions;

namespace Tetrifact.Core
{
    public class ShardIndexItem
    {
        public string Package { get; set; }
        public long Start { get; set; }
        public long End { get; set; }

        public ShardIndexItem(string line) 
        {
            Regex r = new Regex("/(.*?)_(.*?)_(.*)/");
            Match match = r.Match(line);

            if (match.Groups.Count == 3)
            {
                this.Package = match.Groups[0].Value;
                this.Start = int.Parse(match.Groups[1].Value);
                this.End = int.Parse(match.Groups[2].Value);
            }
            else
                throw new ArgumentException("Invalid index string");


        }
    }
}
