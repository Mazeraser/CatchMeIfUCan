using System;

public interface ICommand<T>
{
    void Execute(T workObject);
}

public class SpawnTargetCommand : ICommand<TargetBuilder>
{
    public void Execute(TargetBuilder builder)
    {
        builder.Build();
    }
}
public class AddScoreCommand : ICommand<System.Action<int>>
{
    public void Execute(System.Action<int> scoreModifier)
    {
        scoreModifier?.Invoke(1); // добавляем 1 очко
    }
}