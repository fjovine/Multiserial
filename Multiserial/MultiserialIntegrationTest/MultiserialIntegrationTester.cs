namespace MultiserialIntegrationTest
{
    using MultiSerialTestPattern;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Calls Multiserial.exe in simulation mode, sending a stimulus through stdin and reading the result through stdout.
    /// </summary>
    public class MultiserialIntegrationTester
    {
        const string simulatorInC = @"..\..\..\Debug\MultiSerialSimulator.exe";
        const string folderOfSimulation = @"..\..\..\Debug";
        private List<string> testPattern;

        public MultiserialIntegrationTester(string[] stimulus)
        {
            StimulusDescriptor.LoadFrom(stimulus);
            TestPatternGenerator testPatternGenerator = new TestPatternGenerator();
            testPattern = new List<string>();
            testPatternGenerator.ForeachState((w, s) =>
            {
                testPattern.Add(string.Format("{0} {1:X4}", w, s));
            });
        }

        private List<string> LaunchAndGetResult()
        {
            List<string> result = new List<string>();
            ProcessStartInfo startInfo = new ProcessStartInfo(simulatorInC);
            startInfo.Arguments = "";
            startInfo.WorkingDirectory = folderOfSimulation;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = false;
            startInfo.CreateNoWindow = true;
            Process p = Process.Start(startInfo);
            p.OutputDataReceived += new DataReceivedEventHandler((pr, o) =>
            {
                // Receives the child process' standard output
                if (!string.IsNullOrEmpty(o.Data))
                {
                    result.Add(o.Data);
               }
            });
            using (var wr = p.StandardInput)
            {
                foreach (var s in testPattern)
                {
                    wr.WriteLine(s);
                }
            }
            p.BeginOutputReadLine();
            p.WaitForExit();
            p.Close();
            return result;
        }

        public List<string> LaunchAndDecodeResult()
        {
            List<string> result = new List<string>();
            List<string> testOutput = this.LaunchAndGetResult();
            bool started = false;
            foreach (string line in testOutput)
            {
                if (started)
                {
                    if (line == "End simulation")
                    {
                        break;
                    }
                    result.Add(line);
                }
                else if (line == "Start simulation")
                {
                    started = true;
                }
            }

            return result;
        }
    }
}
