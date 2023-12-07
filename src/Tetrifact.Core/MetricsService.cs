using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tetrifact.Core
{
    /// <summary>
    /// Note : for reference, look up "influx line protocol" and "exposition" for data formating etc
    /// </summary>
    public class MetricsService : IMetricsService
    {
        #region FIELDS

        private ISettings _settings;

        private ILogger<IMetricsService> _logger;

        private ISystemCallsService _systemCallsService;

        #endregion

        #region CTORS

        public MetricsService(ISystemCallsService systemCallsService, ISettings settings, ILogger<IMetricsService> logger) 
        {
            _settings = settings;
            _systemCallsService = systemCallsService;
            _logger = logger;
        }

        #endregion

        #region METHODS

        public void Generate() 
        {
            // check last run
            string lastRunPath = Path.Join(_settings.MetricsPath, "last_run");
            if (File.Exists(lastRunPath)) 
            {
                try 
                {
                    DateTime lastRun = DateTime.Parse(File.ReadAllText(lastRunPath));
                    // no need to generate tests yet
                    if (DateTime.Now - lastRun < new TimeSpan(_settings.MetricsGenerationInterval, 0 , 0))
                        return;
                }
                catch (Exception ex)
                {
                    // if we reach here, last_run is corrupt, force delete
                    _logger.LogError($"last_run for metrics generation is corrupt, attempting hard wipe of file", ex);

                    try 
                    {
                        File.Delete(lastRunPath);
                    } 
                    catch(Exception fataException)
                    { 
                        // if we reach here, we can't delete the corrupt last_run error, this should never happen and we should ideally force an app shutdown.
                        // This error will however be thrown within the dae
                        throw new FatalException($"Fatal error - failed to delete corrupt last_run file :", fataException);
                    }
                }
            }

            StringBuilder s = new StringBuilder();

            // add comment at start of file, this is used when reading back the metrics to ensure content is not stale
            s.AppendLine($"##tetrifact:generated:{DateTime.UtcNow}##");

            // nr of packages - nr of direct child dirs in /packages 
            long packageCount = new DirectoryInfo(_settings.PackagePath).GetDirectories().Length;
            s.AppendLine($"tetrifact packages_count={packageCount}u");
            
            // nr of files on disk - nr of files in /repository
            long respositoryFileCount = 0;
            ShellResult result = _systemCallsService.GetRepoFilesCount();

            if (result.ExitCode != 0 || result.StdErr.Count() != 0)
            {
                _logger.LogError($"Repo file count failed, exit code {result.ExitCode}, stderr {string.Join(",", result.StdErr)}");
            }
            else 
            {
                string incomingFileCount = string.Join("", result.StdOut);
                if (!long.TryParse(incomingFileCount, out respositoryFileCount))
                    _logger.LogError($"Repo file count failed, count result \"{incomingFileCount}\" is not a valid long");
            }

            s.AppendLine($"tetrifact repository_files_count={respositoryFileCount}u");


            long respositoryFileSize = 0;
            result = _systemCallsService.GetRepoFilesSize();

            if (result.ExitCode != 0 || result.StdErr.Count() != 0)
            {
                _logger.LogError($"Repo file size failed, exit code {result.ExitCode}, stderr {string.Join(",", result.StdErr)}");
            }
            else 
            {
                Regex bytesLookup = new Regex("^(\\d*)?");
                string stdOut = string.Join("", result.StdOut);
                Match bytesLookupResult = bytesLookup.Match(stdOut);
                if (bytesLookupResult.Success)
                {
                    string bytesRaw = bytesLookupResult.Groups[1].Value;
                    if (!long.TryParse(bytesRaw, out respositoryFileSize))
                        _logger.LogError($"Repo file size failed : could not parse {bytesRaw} to long.");
                } 
                else 
                {
                    _logger.LogError($"Repo file size failed, could not parse bytes from stdOut \"{stdOut}\"");
                }
            }

            s.AppendLine($"tetrifact repository_files_size={respositoryFileSize}u");



            File.WriteAllText(Path.Join(_settings.MetricsPath, "influx"), s.ToString());
            File.WriteAllText(lastRunPath, DateTime.UtcNow.ToString());

            _logger.LogInformation("Generated metrics");
        }

        public string GetInfluxMetrics() 
        { 
            try 
            {
                string metricsFilePath = Path.Join(_settings.MetricsPath, "influx");

                if (!File.Exists(metricsFilePath))
                    throw new MetricsStaleException("Influx metrics file not found. File might not have been generated yet. Check servers logs if you suspect an error has occurred.");

                // Note that there is a (small) chance of a race condition here where we can attempt to read a metrics file while it is being written.
                // As we don't deam metrics to be critical for data integrity, we ignore this possibility. For now.
                string metrics = File.ReadAllText(metricsFilePath);

                // parse date out of file and verify it is not stale
                Regex dateLookup = new Regex("^##tetrifact:generated:(.*)##");
                Match dateLookupREsult = dateLookup.Match(metrics);
                if (dateLookupREsult.Success)
                {
                    DateTime date = DateTime.Parse(dateLookupREsult.Groups[1].Value);
                    if (DateTime.UtcNow - date > new TimeSpan(_settings.MetricsGenerationInterval + 1, 0, 0))
                        throw new MetricsStaleException($"Metrics stale error - last generation ({date}) is more than an hour later than its expected regeneration time");

                    return metrics;
                }
                else
                    throw new MetricsStaleException($"Failed to retrieve generation data from existing influx metrics file. File likely corrupt");

            }
            catch(Exception ex)
            {
                if (ex is MetricsStaleException)
                    throw ex;

                _logger.LogError("Unexpected error on influx metrics get", ex);
                throw new MetricsStaleException("An unexpected error occurred attempting to retrieve influx metrics. See logs for details.");
            }
        }

        #endregion
    }
}
