// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using GNR.BruteForcEShare;

Console.WriteLine("Hello, World!");
var brute = new EShareBruteForcer();

brute.StartBruteForce();
/*
var processes = Process.GetProcesses().Where(process => process.ProcessName.ToLower() == "eshare");
var app = Application.Attach(processes.First());
using (var automation = new UIA3Automation())
{
    var windows = app.GetMainWindow(automation);
    var label = windows.FindAllDescendants().FirstOrDefault(element => element.Name.StartsWith("Please enter the password for"));
    var retryButton = windows.FindAllDescendants().FirstOrDefault(element => element.Name == "Please enter again");
    retryButton?.Click();
}

*/