﻿namespace Core {
    interface IScriptRunner {
        string RunScript(string scriptId, string scriptPath, string interpreterPath);
    }
}