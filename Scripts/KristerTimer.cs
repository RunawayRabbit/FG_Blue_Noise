using System;
using System.Diagnostics;

public class KristerTimer : IDisposable
{
	private readonly string _name;
	private readonly int _numberOfTests;
	private readonly Stopwatch _stopwatch;

	public KristerTimer( string name, int numberOfTests )
	{
		_name = name;
		_numberOfTests = numberOfTests;
		_stopwatch = Stopwatch.StartNew();
		UnityEngine.Debug.Assert( Stopwatch.IsHighResolution );
	}

	public void Dispose()
	{
		_stopwatch.Stop();
		double totalInMilliseconds = _stopwatch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
		double averageInMilliseconds = totalInMilliseconds / _numberOfTests;
		string average;

		if(averageInMilliseconds < 1.0)
			average = $"{(averageInMilliseconds*1000):0.00}µs";
		else
			average = $"{(averageInMilliseconds):0.00}ms";

		UnityEngine.Debug.Log($"{_name}\n"
							  + $"Average: {average} | "
							  + $"Total: {totalInMilliseconds:0.00}ms ");
	}
}
