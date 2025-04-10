


using System;

public interface IEnemyTarget
{
    public TargetType Type { get; }
    public bool IsActive { get; }

    [Flags]
    public enum TargetType
	{
        
        Player  = 1,
        Tower = 2,
        Wall = 4
	}   
}
