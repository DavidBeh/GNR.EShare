// See https://aka.ms/new-console-template for more information
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

Console.WriteLine("Hello, World!");


IObservable<string> coldObservable = null!;
Task.Run(() =>
{
    int counter = 0;
    
    coldObservable = Observable
        .Interval(TimeSpan.FromSeconds(1)).
        .Select(_ =>
        {
            var eval = (++counter).ToString();
            Console.WriteLine($"Direct {eval}");
            return eval;

        });
}).Wait();

bool connected = false;

//coldObservable.Subscribe(i => Console.WriteLine($"Direct: {i}"));
IDisposable? subscription = null;
while (true)
{

    Console.ReadLine();
    if (subscription == null)
    {
        var refs = coldObservable.Publish().RefCount();
        subscription = refs.Subscribe(Console.WriteLine);
    }
    else
    {
        subscription.Dispose();
        subscription = null;
    }
}