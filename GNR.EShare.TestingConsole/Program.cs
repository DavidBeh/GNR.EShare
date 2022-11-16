// See https://aka.ms/new-console-template for more information

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

Console.WriteLine("Hello, World!");


IObservable<long> coldObservable = null!;
Task.Run(() =>
{
    int counter = 0;

    coldObservable = Observable.Interval(TimeSpan.FromSeconds(1)).Do(l => Console.WriteLine($"Do {l}"));
}).Wait();

bool connected = false;

coldObservable.Subscribe(i => Console.WriteLine($"Direct: {i}"));
int count = 0;

var onDemand = coldObservable.Publish().RefCount().Select(l => count += (int)l);

IDisposable? subscription = null;
while (true)
{
    Console.ReadLine();
    if (subscription == null)
    {
        subscription = onDemand.Subscribe(Console.WriteLine);
    }
    else
    {
        subscription.Dispose();
        subscription = null;
    }
}