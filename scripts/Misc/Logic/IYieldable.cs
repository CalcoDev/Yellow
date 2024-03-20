namespace Yellow.Misc.Logic;

// TODO(calco): Could and should be an interface
public interface IYieldable
{
    bool IsDone { get; }
    void Update(float delta);
}