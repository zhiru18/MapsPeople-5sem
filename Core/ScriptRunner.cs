﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Exceptions;
using Database;
using Models;

namespace Core {
    class ScriptRunner {
        
        public Dictionary<string,string> RunScripts(Dictionary<string,string> scripts, string interpreterPath) {
            Dictionary<string, string> scriptOutput = new Dictionary<string, string>();
            foreach(var script in scripts) {
                scriptOutput.Add(script.Key, RunScript(script.Key, script.Value, interpreterPath));
            }
            return scriptOutput;
        }

        //remember to set the paths to the interpreters
        private string RunScript(string scriptId, string scriptPath, string interpreterPath) {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = interpreterPath;
            start.Arguments = string.Format("\"{0}\" \"{1}\"", scriptPath, null);
            start.UseShellExecute = false;// Do not use OS shell
            start.CreateNoWindow = true; // We don't need new window
            start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
            start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)
            using (Process process = Process.Start(start)) {
                string result = process.StandardOutput.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                string errors = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                if (result.Length != 0) {
                    return result;
                } else {
                    throw new ScriptFailedException(scriptId, errors);
                }
            }
        }        
    }
}
