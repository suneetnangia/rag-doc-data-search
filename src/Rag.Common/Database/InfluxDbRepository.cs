namespace Rag.Common.Database;

using InfluxDB.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class InfluxDbRepository : IDisposable
{
    private readonly ILogger _logger;
    private readonly InfluxDbOptions _influxDbOptions;
    private readonly InfluxDBClient _influxDbClient;
    private readonly QueryApi _influxDbQueryApi;

    // Track whether Dispose has been called.
    private bool disposed = false;

    public InfluxDbRepository(ILogger logger, IOptions<InfluxDbOptions> influxDbOptions)
    {
        // Logger settings are read from appsettings.json or appsettings.Development.json depending on the environment.
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _influxDbOptions = influxDbOptions?.Value ?? throw new ArgumentNullException(nameof(influxDbOptions));

        _influxDbClient = new InfluxDBClient(_influxDbOptions.Url, _influxDbOptions.Token);
        _influxDbQueryApi = _influxDbClient.GetQueryApi();
    }

    // Use C# finalizer syntax for finalization code.
    // This finalizer will run only if the Dispose method
    // does not get called.
    // It gives your base class the opportunity to finalize.
    // Do not provide finalizer in types derived from this class.
    ~InfluxDbRepository()
    {
        // Do not re-create Dispose clean-up code here.
        // Calling Dispose(disposing: false) is optimal in terms of
        // readability and maintainability.
        Dispose(disposing: false);
    }

    public async Task<InfluxDatabaseResponse> QueryAsync(string query, string org)
    {
        _logger.LogTrace($"Executing Influx Db query '{query}'...");
        var flux_Table = await _influxDbQueryApi.QueryAsync(query, org, cancellationToken: default);

        _logger.LogTrace($"Executed Influx Db query, flux table returned had '{flux_Table.Count}' records.");
        return new InfluxDatabaseResponse { Raw = flux_Table.ToArray() };
    }

    public void Dispose()
    {
        Dispose(disposing: true);

        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SuppressFinalize to
        // take this object off the finalization queue
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    // Dispose(bool disposing) executes in two distinct scenarios.
    // If disposing equals true, the method has been called directly
    // or indirectly by a user's code. Managed and unmanaged resources
    // can be disposed.
    // If disposing equals false, the method has been called by the
    // runtime from inside the finalizer and you should not reference
    // other objects. Only unmanaged resources can be disposed.
    protected virtual void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!this.disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                _influxDbClient.Dispose();
            }

            // Note disposing has been done.
            disposed = true;
        }
    }
}
