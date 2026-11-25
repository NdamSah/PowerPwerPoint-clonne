using System.Diagnostics;
namespace PowerPointClone.InterfaceQualite.Aide;
public sealed class AideService
{
    public string Version => "1.0.0";
    public string APropos => "PowerPoint Clone — application de présentation.";
    public void OuvrirDocumentation(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http"))
            throw new ArgumentException("URL de documentation invalide.", nameof(url));
        Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
    }
}