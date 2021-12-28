using System;

namespace Convey.Logging.Options;

/// <summary>
/// Relevant options for the Serilog Loki sink
/// <para></para>
/// Not all options have been included as some have been covered by Convey.Logging already.
/// The sink has reasonable defaults for the unused options.
/// <para></para>
/// <list type="bullet"><listheader>The included options are:</listheader>
/// <item><term>Uri</term></item>
/// <item><term>Credentials</term></item>
/// <item><term>BatchPostingLimit</term></item>
/// <item><term>QueueLimit</term></item>
/// <item><term>Period</term></item>
/// </list> 
/// <list type="bullet"><listheader>The excluded options are:</listheader>
/// <item><term>Labels</term></item>
/// <item><term>FiltrationMode</term></item>
/// <item><term>FiltrationLabels</term></item>
/// <item><term>OutputTemplate</term></item>
/// <item><term>RestrictedToMinimumLevel</term></item>
/// <item><term>TextFormatter</term></item>
/// <item><term>HttpClient</term></item>
/// <item><term>CreateLevelLabel</term></item>
/// </list> 
/// 
/// </summary>
public class LokiOptions
{
    /// <summary>
    /// Whether or not to enable Loki Logging
    /// </summary>
    public bool Enabled { get; set; }
    /// <summary>
    /// The Uri at which the Loki instance can be found
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// The maximum number of events to post in a single batch. Default value is 1000.
    /// </summary>
    public int? BatchPostingLimit { get; set; }
    /// <summary>
    /// The maximum number of events stored in the queue in memory, waiting to be posted over
    /// </summary>
    public int? QueueLimit { get; set; }
    /// <summary>
    /// The time to wait between checking for event batches. Default value is 2 seconds.
    /// </summary>
    public TimeSpan? Period { get; set; }
    /// <summary>
    /// Username used for Grafana Loki authorization
    /// </summary>
    public string LokiUsername { get; set; }
    /// <summary>
    /// Password used for Grafana Loki authorization
    /// </summary>
    public string LokiPassword { get; set; }
}