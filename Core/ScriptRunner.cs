﻿using System.Collections.Generic;
using System.Diagnostics;

namespace Core {
    class ScriptRunner {

        public ScriptRunner() {

        }
        
        public List<string> RunScripts(List<string> scriptPaths, string interpreterPath) {
            List<string> scriptOutput = new List<string>();
            foreach(var path in scriptPaths) {
                scriptOutput.Add(RunScript(path, interpreterPath));
            }
            return scriptOutput;
        }

        //remember to set the paths to the interpreters
        private string RunScript(string scriptPath, string interpreterPath) {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = interpreterPath;
            start.Arguments = string.Format("\"{0}\" \"{1}\"", scriptPath, null);
            start.UseShellExecute = false;// Do not use OS shell
            start.CreateNoWindow = true; // We don't need new window
            start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
            start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)
            using (Process process = Process.Start(start)) {
                //using (StreamReader reader = process.StandardOutput) {
                //string stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                string result = process.StandardOutput.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                return result;
                //}
            }
        }        
    }
}
