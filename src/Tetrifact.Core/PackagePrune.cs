using System;
using System.Collections.Generic;
using System.Text;

namespace Tetrifact.Core
{
    public class PackagePrune : IPackagePrune
    {
        #region FIELDS 

        ITetriSettings _settings;

        IIndexReader _indexReader;

        #endregion

        public PackagePrune(ITetriSettings settings, IIndexReader indexReader)
        {
            _settings = settings;
            _indexReader = indexReader;
        }

        public void Prune()
        { 
            if (!_settings.Prune)
                return;

            IList<string> weeklyPruneQueue = new List<string>();
            IList<string> monthlyPruneQueue = new List<string>();
            IList<string> yearlyPruneQueue = new List<string>();
            IList<string> weeklyPrune= new List<string>();
            IList<string> monthlyPrune = new List<string>();
            IList<string> yearlyPrune = new List<string>();

            IEnumerable<string> packageIds = _indexReader.GetAllPackageIds();
            DateTime? weeklyPruneFloor = _settings.WeeklyPruneThreshold != 0 ? new DateTime().ToUniversalTime() - new TimeSpan(_settings.WeeklyPruneThreshold, 0,0,0) : (DateTime?)null;
            DateTime? monthlyPruneFloor = _settings.MonthPruneThreshold != 0 ? new DateTime().ToUniversalTime() - new TimeSpan(_settings.MonthPruneThreshold, 0, 0, 0) : (DateTime?)null;
            DateTime? yearlyPrunedFloor = _settings.YearlyPruneThreshold != 0 ? new DateTime().ToUniversalTime() - new TimeSpan(_settings.YearlyPruneThreshold, 0, 0, 0) : (DateTime?)null;

            foreach (string packageId in packageIds)
            {
                Manifest manifest = _indexReader.GetManifest(packageId);
                if (manifest == null)
                    continue;

                if (weeklyPruneFloor != null && manifest.CreatedUtc > weeklyPruneFloor)
                    weeklyPruneQueue.Add(packageId);

                if (monthlyPruneFloor != null && manifest.CreatedUtc > monthlyPruneFloor)
                    monthlyPruneQueue.Add(packageId);

                if (yearlyPrunedFloor != null && manifest.CreatedUtc > yearlyPrunedFloor)
                    yearlyPruneQueue.Add(packageId);
            }

            // weekly
            if (weeklyPruneQueue.Count > _settings.WeeklyPruneKeep && _settings.WeeklyPruneKeep  > 0)
            { 
                int period =  7;
                for (int i = 2 ; i < 1000 ; i ++)
                {
                    int periods = period / i;
                    for (int i_period = 0 ; i < periods ; i ++)
                    { 
                        foreach (string id in weeklyPruneQueue)
                        {
                            Manifest manifest = _indexReader.GetManifest(id);
                            if (manifest == null)
                                continue;
                    
                            
                        }
                    }
                    
                }
            }

        }
    }
}
