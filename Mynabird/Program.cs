using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Diagnostics;

namespace Mynabird
{
    class Program
    {
        const string cmd = "cmd";
        const string cmdarg = "/c ";
        const string keyName = @"SOFTWARE\Cboe\Mynabird";

        const string source = "Mynabird";

        static int logRecord(string type, string name, string[] args, string exename)
        {
            string strargs = "";
            foreach (string arg in args)
            {
                strargs += arg + " ";
            }
            Console.WriteLine("{0}: {1} with {2} {3}", type, name, exename, strargs);

            EventLog systemEventLog = new EventLog("System");
            systemEventLog.Source = source;
            systemEventLog.WriteEntry("This is warning from the demo app to the System log", EventLogEntryType.Warning, 150);
            return 0;
        }
        static int Main(string[] args)
        {
            string exename = System.AppDomain.CurrentDomain.FriendlyName;
            RegistryKey rkLM = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey key = rkLM.OpenSubKey(keyName);

            Console.WriteLine(exename);
            RegistryKey regExeKey = null;
            foreach (string subKeyName in key.GetSubKeyNames()) // regHome.GetSubKeyNames())
            {
                if (subKeyName.ToLower() == exename.ToLower())
                {
                    regExeKey = key.OpenSubKey(subKeyName);
                    Console.Write("Found it!:");
                    Console.WriteLine(subKeyName);
                    break;
                }
               Console.WriteLine(subKeyName);
            }

            RegistryKey regBans = regExeKey.OpenSubKey("ban");
            RegistryKey regLogs = regExeKey.OpenSubKey("log");

            foreach (string ruleName in regBans.GetSubKeyNames())
            {
                RegistryKey regCur = regBans.OpenSubKey(ruleName);
                string[] ruleargs = (string[]) regCur.GetValue("args");
                int rulethreshold = (int)regCur.GetValue("threshold");
                int count = 0;
                foreach (string arg in args)
                {
                    if (ruleargs.Contains(arg))
                        count += 1;
                    if (count >= rulethreshold)
                        return logRecord("ban", ruleName, args, exename);
                }
            }

            foreach (string ruleName in regLogs.GetSubKeyNames())
            {
                RegistryKey regCur = regLogs.OpenSubKey(ruleName);
                string[] ruleargs = (string[]) regCur.GetValue("args");
                int rulethreshold = (int)regCur.GetValue("threshold");
                int count = 0;
                foreach (string arg in args)
                {
                    if (ruleargs.Contains(arg))
                        count += 1;
                    if (count >= rulethreshold)
                        return logRecord("log", ruleName, args, exename);
                }
            }



            string command = "";
            foreach (string arg in args)
            {
                //System.Console.WriteLine(arg);
                command = command + arg + " ";
            }


            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(cmd, cmdarg + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                procStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                Console.WriteLine(objException);
                // Log the exception
                return 0;
            }
            return 0;
        }
    }
}
