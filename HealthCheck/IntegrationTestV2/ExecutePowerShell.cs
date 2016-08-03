using System;
using System.IO;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace IntegrationTestV2
{
    public class ExecutePowerShell
    {
        public static void ExecutePoweShell(string filePath, long timeStamp)
        {
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript(File.ReadAllText(filePath));
                PowerShellInstance.AddParameter("Timestamp", timeStamp);
                // invoke execution on the pipeline (collecting output)
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();
                // loop through each output object item            
                foreach (PSObject outputItem in PSOutput)
                {
                    Console.WriteLine(outputItem.BaseObject.ToString() + "\n");
                }
            }
        }
    }
}        
      