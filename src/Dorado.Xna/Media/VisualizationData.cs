using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Media;

/// <summary>Frequency and waveform data consumed by visualizations.</summary>
public class VisualizationData
{
    private readonly float[] frequencies = new float[256];
    private readonly float[] samples = new float[256];

    public VisualizationData()
    {
        Frequencies = new ReadOnlyCollection<float>(frequencies);
        Samples = new ReadOnlyCollection<float>(samples);
    }

    public ReadOnlyCollection<float> Frequencies { get; }

    public ReadOnlyCollection<float> Samples { get; }
}
