namespace ApplicationCore.Logging;

public interface ITrackable
{
    public ITrackingLogger TrackingLogger { get; set; }
}
